using Microsoft.AspNetCore.Mvc;
using Proyecto_Grupo_gris.Services.Agents.Interfaces;

namespace Proyecto_Grupo_gris.Controllers
{
    public class ChatController : Controller
    {
        private readonly IAgentService _agentService;

        public ChatController(IAgentService agentService)
        {
            _agentService = agentService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Send([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Message))
            {
                return BadRequest(new { error = "Message is required." });
            }

            var response = await _agentService.ChatAsync(request.Message);
            return Json(new { response });
        }

        [HttpPost("Chat/ClearHistory")]
        public async Task<IActionResult> ClearHistory()
        {
            await _agentService.ClearHistoryAsync();
            return Json(new { response = "Historial limpiado." });
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}
