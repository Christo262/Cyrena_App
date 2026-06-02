using Cyrena.Contracts;
using Cyrena.Shell.Models;
using Microsoft.AspNetCore.Mvc;

namespace Cyrena.Shell.Controllers
{
    public class OpenController : Controller
    {
        private readonly App _app;
        private readonly ISettingsService _settings;
        public OpenController(App app, ISettingsService settings)
        {
            _app = app;
            _settings = settings;
        }

        [HttpGet("/api/is-alive")]
        public IActionResult IsAlive(string? squawk)
        {
            if (string.IsNullOrEmpty(squawk))
                return BadRequest();
            var ext = _settings.Read<Squawk>(Squawk.Key);
            if (ext == null || ext.Value != squawk)
                return BadRequest();
            _app.ShowWindow();
            return Ok();
        }

        [HttpGet("/api/kill")]
        public IActionResult Kill(string? squawk)
        {
            if (string.IsNullOrEmpty(squawk))
                return Unauthorized();
            var ext = _settings.Read<Squawk>(Squawk.Key);
            if (ext == null)
                return StatusCode(500);
            if (ext.Value != squawk)
                return Forbid();
            _app.Exit();
            return Ok();
        }
    }
}
