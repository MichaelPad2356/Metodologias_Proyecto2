using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            var usuarios = new List<object>
            {
                new { nombre = "Juan" },
                new { nombre = "Ana" },
                new { nombre = "Luis" }
            };
            return Ok(usuarios);
        }
    }
}
