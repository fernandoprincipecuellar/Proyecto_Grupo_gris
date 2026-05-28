using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML;

namespace Proyecto_Grupo_gris.ML.SentimentAnalysis
{
    public class SentimentResult
    {
        public string Sentiment { get; set; } = "Neutro";
        public float ConfidencePercentage { get; set; }
    }

    public class SentimentAnalysisConsumption
    {
        private static readonly object _lock = new object();
        private static PredictionEngine<ModelInput, ModelOutput>? _engine = null;

        private static readonly string MLNetModelPath = Path.GetFullPath(
            Path.Combine("ML", "SentimentAnalysis", "SentimentAnalysis.mlnet"));

        // ── Palabras positivas con pesos ─────────────────────────────────────
        // (sin duplicados para evitar excepcion en Dictionary)
        private static readonly (string word, int weight)[] PositiveKeywords =
        {
            ("excelente",    3), ("genial",        3), ("increible",     3),
            ("increíble",    3), ("fantastico",    3), ("fantástico",    3),
            ("esplendido",   3), ("espléndido",    3), ("maravilloso",   3),
            ("maravillosa",  3), ("hermoso",       2), ("hermosa",       2),
            ("perfecto",     3), ("perfecta",      3), ("bravo",         2),
            ("feliz",        3), ("felices",       3), ("alegria",       3),
            ("alegría",      3), ("orgullo",       2), ("orgulloso",     2),
            ("orgullosa",    2), ("contento",      2), ("contenta",      2),
            ("encanta",      3), ("encanto",       3), ("adoro",         2),
            ("limpio",       2), ("limpia",        2), ("limpias",       2),
            ("limpios",      2), ("mejoro",        2), ("mejoró",        2),
            ("mejorado",     2), ("mejora",        2), ("logro",         2),
            ("logramos",     2), ("exito",         3), ("éxito",         3),
            ("gran trabajo", 3), ("muy bien",      3), ("bueno",         2),
            ("buena",        2), ("buen",          1), ("grande",        1),
            ("funciona",     1), ("funcionando",   1), ("gracias",       2),
            ("felicitaciones",3),("felicitacion",  3), ("felicitación",  3),
            ("participacion",1), ("participación", 1), ("voluntario",    1),
            ("voluntarios",  1), ("comprometidos", 2), ("compromiso",    2),
            ("reciclamos",   2), ("reciclando",    2), ("reciclar",      1),
            ("reforestacion",1), ("reforestación", 1), ("plantados",     1),
            ("creciendo",    1), ("compostaje",    1), ("compostera",    1),
            ("reutilizar",   1), ("reduccion",     1), ("reducción",     1),
            ("limpieza",     1), ("qué alegría",   3), ("que alegria",   3),
            ("bien hecho",   3), ("buen trabajo",  3), ("excelente trabajo", 3),
            ("me alegra",    3), ("nos alegra",    3), ("qué bueno",     2),
            ("que bueno",    2), ("muy bueno",     3), ("muy buena",     3),
        };

        // ── Palabras negativas con pesos ─────────────────────────────────────
        private static readonly (string word, int weight)[] NegativeKeywords =
        {
            ("terrible",    3), ("horrible",     3), ("pesimo",        3),
            ("pésimo",      3), ("pesima",        3), ("pésima",        3),
            ("desastre",    3), ("desastroso",    3), ("verguenza",     3),
            ("vergüenza",   3), ("vergonzoso",    3), ("vergonzosa",    3),
            ("indignante",  3), ("inaceptable",   3), ("asco",          3),
            ("odio",        3), ("molesta",       2), ("molesto",       2),
            ("enojado",     2), ("enojada",       2), ("triste",        2),
            ("indignado",   3), ("indignada",     3), ("preocupante",   2),
            ("basura",      2), ("suciedad",      2), ("sucio",         2),
            ("sucia",       2), ("contaminacion", 2), ("contaminación", 2),
            ("contaminado", 2), ("contamina",     2), ("toxico",        3),
            ("tóxico",      3), ("toxica",        3), ("tóxica",        3),
            ("huele",       2), ("hedor",         3), ("olores",        2),
            ("acumulada",   2), ("acumulado",     2), ("acumulan",      2),
            ("rebalsa",     2), ("queman",        3), ("quemando",      3),
            ("botan",       2), ("tiran",         2), ("botando",       2),
            ("tirando",     2), ("peligro",       2), ("riesgo",        2),
            ("enfermedad",  2), ("falla",         2), ("fallo",         2),
            ("sin control", 3), ("no pasa",       3), ("no recogen",    3),
            ("no hay",      1), ("sin multas",    2), ("desmonte",      2),
            ("escombros",   2), ("espuma",        2), ("insoportable",  3),
            ("problema",    1), ("nadie respeta", 3), ("siguen tirando",3),
            ("siguen botando",3),("mal olor",     3), ("muy mal",       2),
            ("pésima gestion",3),("pesima gestion",3),("residuos industriales",3),
        };

        // ── Motor ML (singleton thread-safe) ─────────────────────────────────
        private static PredictionEngine<ModelInput, ModelOutput>? TryGetEngine()
        {
            try
            {
                if (_engine == null)
                {
                    lock (_lock)
                    {
                        if (_engine == null)
                        {
                            if (!File.Exists(MLNetModelPath))
                                SentimentAnalysisTraining.TrainModel();

                            var mlContext = new MLContext();
                            var mlModel = mlContext.Model.Load(MLNetModelPath, out _);
                            _engine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);
                        }
                    }
                }
                return _engine;
            }
            catch
            {
                return null; // Si el modelo falla, usamos solo keywords
            }
        }

        // ── Método principal ──────────────────────────────────────────────────
        /// <summary>
        /// Devuelve exactamente "Positivo", "Negativo" o "Neutro" con % de confianza.
        /// Sistema híbrido: keywords en español (primario) + modelo ML.NET (respaldo).
        /// </summary>
        public static SentimentResult Predict(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return new SentimentResult { Sentiment = "Neutro", ConfidencePercentage = 0f };

            string lower = texto.ToLowerInvariant();

            // 1. Análisis de keywords (siempre se ejecuta, nunca falla)
            int posScore = 0;
            int negScore = 0;

            foreach (var (word, weight) in PositiveKeywords)
                if (lower.Contains(word)) posScore += weight;

            foreach (var (word, weight) in NegativeKeywords)
                if (lower.Contains(word)) negScore += weight;

            // 2. Predicción ML
            string mlLabel = "Neutro";
            float mlConfidence = 0f;

            var engine = TryGetEngine();
            if (engine != null)
            {
                try
                {
                    var result = engine.Predict(new ModelInput { SentimentText = texto });
                    mlLabel = MapToLabel(result.Prediction);
                    mlConfidence = result.Score?.Length > 0 ? result.Score.Max() : 0f;
                }
                catch { /* ignorar errores de prediccion */ }
            }

            // 3. Decisión híbrida — keywords tienen prioridad cuando tienen señal
            string finalLabel;
            float finalConfidence;

            int kwDiff = Math.Abs(posScore - negScore);
            string kwLabel = posScore > negScore ? "Positivo"
                           : negScore > posScore ? "Negativo"
                           : "Neutro";

            if (kwDiff >= 2)
            {
                // ✅ Keywords tienen señal clara → KEYWORDS GANAN siempre
                finalLabel = kwLabel;
                float kwConfidence = Math.Min(0.60f + kwDiff * 0.04f, 0.96f);

                // Bonus si el modelo coincide
                if (mlLabel == kwLabel && mlConfidence > 0)
                    kwConfidence = Math.Min(kwConfidence + 0.05f, 0.97f);

                finalConfidence = kwConfidence;
            }
            else if (kwDiff == 1)
            {
                // Señal débil de keywords: ML decide si tiene confianza, sino keywords
                if (mlConfidence >= 0.65f && mlLabel != "Neutro")
                {
                    finalLabel = mlLabel;
                    finalConfidence = mlConfidence;
                }
                else
                {
                    finalLabel = kwLabel;
                    finalConfidence = 0.58f;
                }
            }
            else
            {
                // Keywords empate (ambigüo) → confiar en ML
                if (mlConfidence >= 0.60f)
                {
                    finalLabel = mlLabel;
                    finalConfidence = mlConfidence;
                }
                else
                {
                    finalLabel = "Neutro";
                    finalConfidence = 0.50f;
                }
            }

            return new SentimentResult
            {
                Sentiment = finalLabel,
                ConfidencePercentage = (float)Math.Round(finalConfidence * 100f, 1)
            };
        }

        private static string MapToLabel(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "Neutro";
            string n = raw.Trim().ToLowerInvariant();
            if (n.Contains("positiv")) return "Positivo";
            if (n.Contains("negativ")) return "Negativo";
            return "Neutro";
        }
    }
}
