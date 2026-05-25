using Microsoft.AspNetCore.Mvc;

namespace Cyrena.Shell.Controllers
{
    public class OpenController : Controller
    {
        private readonly App _app;
        public OpenController(App app)
        {
            _app = app;
        }

        [HttpGet("/api/is-alive")]
        public IActionResult IsAlive()
        {
            _app.ShowWindow();
            return Ok();
        }
    }
}
