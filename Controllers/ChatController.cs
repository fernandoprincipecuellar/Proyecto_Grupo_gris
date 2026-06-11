using Microsoft.AspNetCore.Mvc;
using Proyecto_Grupo_gris.Services;

namespace Proyecto_Grupo_gris.Controllers
{
    public class ChatController : Controller
    {
        private readonly ChatService _chatService;

        public ChatController(ChatService chatService)
        {
            _chatService = chatService;
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

            var response = await _chatService.SendMessageAsync(request.Message);
            return Json(new { response });
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}
