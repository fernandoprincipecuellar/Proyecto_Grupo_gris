using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Proyecto_Grupo_gris.Services
{
    public class ChatService
    {
        private readonly IChatCompletionService _chatCompletionService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _systemPrompt;
        private const string SessionKey = "ChatHistory";

        public ChatService(IChatCompletionService chatCompletionService, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _chatCompletionService = chatCompletionService;
            _httpContextAccessor = httpContextAccessor;
            _systemPrompt = configuration["OllamaChat:SystemPrompt"] ?? string.Empty;
        }

        public async Task<string> SendMessageAsync(string userMessage)
        {
            var history = GetHistory();

            // Agregar mensaje del usuario al historial
            history.AddUserMessage(userMessage);

            // Obtener la respuesta del modelo
            var response = await _chatCompletionService.GetChatMessageContentAsync(history);

            // Agregar la respuesta del bot al historial
            history.AddAssistantMessage(response.Content ?? string.Empty);

            SaveHistory(history);

            return response.Content ?? string.Empty;
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
                        var history = new ChatHistory(_systemPrompt);
                        foreach (var item in savedHistory)
                        {
                            if (item.Role == AuthorRole.User.Label)
                                history.AddUserMessage(item.Content);
                            else if (item.Role == AuthorRole.Assistant.Label)
                                history.AddAssistantMessage(item.Content);
                        }
                        return history;
                    }
                }
                catch
                {
                    // Ignorar error de deserialización y crear uno nuevo
                }
            }

            return new ChatHistory(_systemPrompt);
        }

        private void SaveHistory(ChatHistory history)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                var historyToSave = history
                    .Where(m => m.Role != AuthorRole.System)
                    .Select(m => new ChatHistoryItem { Role = m.Role.Label, Content = m.Content ?? string.Empty })
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
