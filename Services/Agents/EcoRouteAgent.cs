using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Proyecto_Grupo_gris.Services.Agents.Interfaces;
using Proyecto_Grupo_gris.Services.Agents.Plugins;

namespace Proyecto_Grupo_gris.Services.Agents
{
    public class EcoRouteAgent : IAgentService
    {
        private readonly WeatherPlugin _weatherPlugin;
        private readonly AirQualityPlugin _airQualityPlugin;
        private readonly ForumPlugin _forumPlugin;
        private readonly IChatCompletionService _chatCompletionService;
        private readonly Kernel _kernel;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string SessionKey = "AgentChatHistory";

        private static readonly string SystemPrompt = string.Join("\n", new[]
        {
            "Eres EcoRoute AI, un asistente experto en medio ambiente y rutas ecologicas.",
            "",
            "Tienes acceso a informacion en tiempo real sobre:",
            "- CLIMA de ciudades (temperatura, condicion, humedad, viento)",
            "- CALIDAD DEL AIRE de ciudades (AQI, PM2.5, PM10, contaminantes)",
            "- FORO de la comunidad ecologica (posts, comentarios, likes)",
            "",
            "Para obtener esta informacion, responde con un JSON en la siguiente linea despues de [TOOL_CALL]:",
            "[TOOL_CALL]\"nombre_h-parametro1-parametro2\"",
            "",
            "Herramientas disponibles:",
            "- WEATHER\"ciudad\" - Consulta el clima actual de una ciudad",
            "- FORECAST\"ciudad-dias\" - Pronostico del clima",
            "- AIR\"ciudad\" - Calidad del aire de una ciudad",
            "- AIRCOORD\"lat-lon\" - Calidad del aire por coordenadas",
            "- FORUMSEARCH\"query\" - Buscar posts en el foro",
            "- FORUMRECENT\"cantidad\" - Posts mas recientes",
            "- FORUMCOMMENTS\"postId\" - Comentarios de un post",
            "- FORUMTOP - Posts mas populares",
            "",
            "Ejemplos:",
            "Usuario: Como esta el clima en Madrid?",
            "Respuesta: Voy a consultar el clima de Madrid para ti.",
            "[TOOL_CALL]\"WEATHER-Madrid\"",
            "",
            "Usuario: Que hay en el foro sobre reciclaje?",
            "Respuesta: Dejame buscar en el foro informacion sobre reciclaje.",
            "[TOOL_CALL]\"FORUMSEARCH-reciclaje\"",
            "",
            "Si la pregunta NO requiere datos en tiempo real, responde directamente sin [TOOL_CALL].",
            "Responde SIEMPRE en espanol, se amigable y da consejos ecologicos."
        });

        public EcoRouteAgent(
            WeatherPlugin weatherPlugin,
            AirQualityPlugin airQualityPlugin,
            ForumPlugin forumPlugin,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _weatherPlugin = weatherPlugin;
            _airQualityPlugin = airQualityPlugin;
            _forumPlugin = forumPlugin;
            _httpContextAccessor = httpContextAccessor;

            var model = configuration["OllamaChat:Model"] ?? "gemma3:4b";
            var endpoint = configuration["OllamaChat:Endpoint"] ?? "http://localhost:11434/v1";
            var apiKey = configuration["OllamaChat:ApiKey"] ?? "ollama";

            var builder = Kernel.CreateBuilder();
            builder.AddOpenAIChatCompletion(modelId: model, endpoint: new Uri(endpoint), apiKey: apiKey);
            _kernel = builder.Build();

            _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
        }

        public async Task<string> ChatAsync(string userMessage)
        {
            try
            {
                var history = GetHistory();
                history.AddSystemMessage(SystemPrompt);
                history.AddUserMessage(userMessage);

                var settings = new PromptExecutionSettings();
                var response = await _chatCompletionService.GetChatMessageContentAsync(
                    history, settings, _kernel);

                var responseText = response.Content ?? string.Empty;

                var toolMatch = Regex.Match(responseText, @"\[TOOL_CALL\][\""](.+?)[\""]");
                if (!toolMatch.Success)
                {
                    toolMatch = Regex.Match(responseText, @"\[TOOL_CALL\]\s*([A-Z]+-[^\s\""]+)");
                }

                if (toolMatch.Success)
                {
                    var toolCall = toolMatch.Groups[1].Value;
                    var toolResult = await ExecuteToolAsync(toolCall);

                    var cleanResponse = Regex.Replace(responseText, @"\s*\[TOOL_CALL\][\""][^\""]*[\""]", "").Trim();

                    history.AddAssistantMessage(responseText);
                    history.AddUserMessage("Resultado de la herramienta:\n" + toolResult + "\n\nAhora responde al usuario de forma amigable y clara usando esta informacion.");

                    var finalResponse = await _chatCompletionService.GetChatMessageContentAsync(
                        history, settings, _kernel);

                    var finalText = finalResponse.Content ?? cleanResponse;
                    history.AddAssistantMessage(finalText);
                    SaveHistory(history);
                    return finalText;
                }

                history.AddAssistantMessage(responseText);
                SaveHistory(history);
                return responseText;
            }
            catch (Exception ex)
            {
                return "Error al procesar tu mensaje: " + ex.Message;
            }
        }

        private async Task<string> ExecuteToolAsync(string toolCall)
        {
            try
            {
                var parts = toolCall.Split(new[] { '-' }, 2);
                var toolName = parts[0];
                var paramStr = parts.Length > 1 ? parts[1] : "";
                var toolParams = paramStr.Split('-');

                return toolName switch
                {
                    "WEATHER" => await _weatherPlugin.GetWeatherAsync(toolParams[0]),
                    "FORECAST" => await _weatherPlugin.GetForecastAsync(
                        toolParams[0],
                        toolParams.Length > 1 ? int.Parse(toolParams[1]) : 5),
                    "AIR" => await _airQualityPlugin.GetAirQualityAsync(toolParams[0]),
                    "AIRCOORD" => await _airQualityPlugin.GetAirQualityByCoordsAsync(
                        double.Parse(toolParams[0]),
                        double.Parse(toolParams[1])),
                    "FORUMSEARCH" => await _forumPlugin.SearchForumPostsAsync(toolParams[0]),
                    "FORUMRECENT" => await _forumPlugin.GetRecentPostsAsync(
                        toolParams.Length > 0 ? int.Parse(toolParams[0]) : 5),
                    "FORUMCOMMENTS" => await _forumPlugin.GetPostCommentsAsync(int.Parse(toolParams[0])),
                    "FORUMTOP" => await _forumPlugin.GetTopPostsAsync(),
                    _ => "Herramienta no reconocida"
                };
            }
            catch (Exception ex)
            {
                return "Error ejecutando herramienta: " + ex.Message;
            }
        }

        public async Task ClearHistoryAsync()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            session?.Remove(SessionKey);
            await Task.CompletedTask;
        }

        private ChatHistory GetHistory()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null && session.TryGetValue(SessionKey, out var data))
            {
                try
                {
                    var savedHistory = JsonSerializer.Deserialize<List<ChatHistoryItem>>(data);
                    if (savedHistory != null)
                    {
                        var history = new ChatHistory();
                        foreach (var item in savedHistory)
                        {
                            if (item.Role == "user")
                                history.AddUserMessage(item.Content);
                            else if (item.Role == "assistant")
                                history.AddAssistantMessage(item.Content);
                        }
                        return history;
                    }
                }
                catch
                {
                }
            }

            return new ChatHistory();
        }

        private void SaveHistory(ChatHistory history)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                var historyToSave = history
                    .Where(m => m.Role == AuthorRole.User || m.Role == AuthorRole.Assistant)
                    .Select(m => new ChatHistoryItem
                    {
                        Role = m.Role.Label,
                        Content = m.Content ?? string.Empty
                    })
                    .ToList();

                session.SetString(SessionKey, JsonSerializer.Serialize(historyToSave));
            }
        }

        private class ChatHistoryItem
        {
            public string Role { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
        }
    }
}
