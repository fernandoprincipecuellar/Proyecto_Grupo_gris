using Microsoft.AspNetCore.Mvc;

namespace Proyecto_Grupo_gris.Controllers
{
    public class ForumController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
