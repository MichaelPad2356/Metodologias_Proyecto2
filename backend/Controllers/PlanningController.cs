using Microsoft.AspNetCore.Mvc;
using backend.Models;
using System.Collections.Generic;
using System.Linq;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlanningController : ControllerBase
    {
        // Simulamos una base de datos en memoria (static) para que los datos no se borren entre peticiones
        private static List<Iteracion> _iteraciones = new List<Iteracion>
        {
            // Datos de ejemplo para que veas algo en el dashboard
            new Iteracion { Id = 1, Nombre = "Iteración 1", FaseOpenUP = "Elaboración", FechaInicio = DateTime.Now.AddDays(-14), FechaFin = DateTime.Now.AddDays(-1), Objetivo = "Base del proyecto", PuntosCompletados = 20, CapacidadEquipoHoras = 100 },
            new Iteracion { Id = 2, Nombre = "Iteración 2", FaseOpenUP = "Construcción", FechaInicio = DateTime.Now, FechaFin = DateTime.Now.AddDays(14), Objetivo = "Implementar HU-015", PuntosEstimados = 15, CapacidadEquipoHoras = 80 }
        };

        // HU-015: Obtener iteraciones (Dashboard)
        [HttpGet]
        public IActionResult GetIteraciones()
        {
            return Ok(_iteraciones);
        }

        // HU-015: Crear iteración
        [HttpPost]
        public IActionResult CrearIteracion([FromBody] Iteracion nuevaIteracion)
        {
            nuevaIteracion.Id = _iteraciones.Count + 1;
            _iteraciones.Add(nuevaIteracion);
            return Ok(nuevaIteracion);
        }

        // HU-016: Registrar capacidad y calcular velocidad
        // Este endpoint devuelve la velocidad promedio histórica
        [HttpGet("velocidad-historica")]
        public IActionResult GetVelocidadHistorica()
        {
            // Filtramos iteraciones pasadas que tengan puntos completados
            var iteracionesPasadas = _iteraciones.Where(i => i.PuntosCompletados > 0).ToList();
            
            if (!iteracionesPasadas.Any())
                return Ok(new { velocidadPromedio = 0, mensaje = "No hay datos históricos suficientes" });

            double velocidadPromedio = iteracionesPasadas.Average(i => i.PuntosCompletados);
            return Ok(new { velocidadPromedio });
        }
    }
}