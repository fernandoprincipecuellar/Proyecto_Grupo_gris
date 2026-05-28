using System;
using System.IO;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Text;

namespace Proyecto_Grupo_gris.ML.SentimentAnalysis
{
    public class ModelInput
    {
        [LoadColumn(0)]
        public string? SentimentText { get; set; }

        [LoadColumn(1)]
        public string? Sentiment { get; set; }
    }

    public class ModelOutput
    {
        [ColumnName("PredictedLabel")]
        public string? Prediction { get; set; }

        public float[]? Score { get; set; }
    }

    public class SentimentAnalysisTraining
    {
        private static readonly string MLNetModelPath = Path.GetFullPath(
            Path.Combine("ML", "SentimentAnalysis", "SentimentAnalysis.mlnet"));
        private static readonly string TrainDataPath = Path.GetFullPath(
            Path.Combine("ML", "SentimentAnalysis", "trainingdata", "sentiment_data.csv"));

        public static void TrainModel()
        {
            var mlContext = new MLContext(seed: 42);

            // 1. Cargar datos
            var trainingDataView = mlContext.Data.LoadFromTextFile<ModelInput>(
                path: TrainDataPath,
                hasHeader: true,
                separatorChar: ',',
                allowQuoting: true);

            // 2. Pipeline mejorado:
            //    - NormalizeText: quita mayúsculas, puntuación y acentos
            //    - FeaturizeText con word bigrams + char trigrams para mejor captura de contexto
            var textOptions = new TextFeaturizingEstimator.Options
            {
                WordFeatureExtractor = new WordBagEstimator.Options
                {
                    NgramLength = 2,        // unigramas + bigramas
                    UseAllLengths = true,
                    Weighting = NgramExtractingEstimator.WeightingCriteria.TfIdf
                },
                CharFeatureExtractor = new WordBagEstimator.Options
                {
                    NgramLength = 3,        // trigramas de caracteres (captura morfología)
                    UseAllLengths = true,
                    Weighting = NgramExtractingEstimator.WeightingCriteria.TfIdf
                },
                KeepDiacritics = false,     // normaliza acentos (excelente == excelente)
                KeepPunctuations = false,
                KeepNumbers = false,
                CaseMode = TextNormalizingEstimator.CaseMode.Lower
            };

            var dataProcessPipeline = mlContext.Transforms.Conversion
                    .MapValueToKey("Label", nameof(ModelInput.Sentiment))
                .Append(mlContext.Transforms.Text.FeaturizeText("Features", textOptions,
                    nameof(ModelInput.SentimentText)))
                .AppendCacheCheckpoint(mlContext);

            // LbfgsMaximumEntropy converge mejor que SDCA con datasets pequeños
            var trainer = mlContext.MulticlassClassification.Trainers
                    .LbfgsMaximumEntropy("Label", "Features",
                        l1Regularization: 0.05f,
                        l2Regularization: 0.05f)
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel", "Label"));

            var trainingPipeline = dataProcessPipeline.Append(trainer);

            // 3. Entrenar
            Console.WriteLine("[ML] Entrenando modelo de análisis de sentimiento (multiclase con n-gramas)...");
            var trainedModel = trainingPipeline.Fit(trainingDataView);

            // 4. Guardar
            mlContext.Model.Save(trainedModel, trainingDataView.Schema, MLNetModelPath);
            Console.WriteLine($"[ML] Modelo guardado en: {MLNetModelPath}");
        }
    }
}
