using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlanningController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PlanningController(ApplicationDbContext context)
        {
            _context = context;
        }

        // HU-015: Obtener iteraciones con sus tareas
        [HttpGet]
        public async Task<IActionResult> GetIteraciones([FromQuery] int? projectId)
        {
            var query = _context.Set<Iteracion>().AsQueryable();
            
            if (projectId.HasValue)
            {
                query = query.Where(i => i.ProjectId == projectId.Value);
            }
            
            var iteraciones = await query.ToListAsync();
            
            // Deserializar las tareas desde JSON
            foreach (var iteracion in iteraciones)
            {
                if (!string.IsNullOrEmpty(iteracion.TareasJson))
                {
                    iteracion.Tareas = JsonSerializer.Deserialize<List<Tarea>>(iteracion.TareasJson) ?? new List<Tarea>();
                }
            }
            
            return Ok(iteraciones);
        }

        // HU-015: Crear iteración
        [HttpPost]
        public async Task<IActionResult> CrearIteracion([FromBody] Iteracion nuevaIteracion)
        {
            // Serializar tareas a JSON
            if (nuevaIteracion.Tareas != null && nuevaIteracion.Tareas.Any())
            {
                nuevaIteracion.TareasJson = JsonSerializer.Serialize(nuevaIteracion.Tareas);
            }
            
            _context.Set<Iteracion>().Add(nuevaIteracion);
            await _context.SaveChangesAsync();
            
            return Ok(nuevaIteracion);
        }

        // Actualizar iteración (incluyendo tareas)
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarIteracion(int id, [FromBody] Iteracion iteracionActualizada)
        {
            var iteracion = await _context.Set<Iteracion>().FindAsync(id);
            
            if (iteracion == null)
                return NotFound(new { mensaje = "Iteración no encontrada" });
            
            iteracion.Nombre = iteracionActualizada.Nombre;
            iteracion.FechaInicio = iteracionActualizada.FechaInicio;
            iteracion.FechaFin = iteracionActualizada.FechaFin;
            iteracion.Objetivo = iteracionActualizada.Objetivo;
            iteracion.FaseOpenUP = iteracionActualizada.FaseOpenUP;
            iteracion.CapacidadEquipoHoras = iteracionActualizada.CapacidadEquipoHoras;
            iteracion.PuntosEstimados = iteracionActualizada.PuntosEstimados;
            iteracion.PuntosCompletados = iteracionActualizada.PuntosCompletados;
            
            // Actualizar tareas
            if (iteracionActualizada.Tareas != null)
            {
                iteracion.TareasJson = JsonSerializer.Serialize(iteracionActualizada.Tareas);
            }
            
            await _context.SaveChangesAsync();
            
            return Ok(iteracion);
        }

        // Eliminar iteración
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarIteracion(int id)
        {
            var iteracion = await _context.Set<Iteracion>().FindAsync(id);
            
            if (iteracion == null)
                return NotFound(new { mensaje = "Iteración no encontrada" });
            
            _context.Set<Iteracion>().Remove(iteracion);
            await _context.SaveChangesAsync();
            
            return Ok(new { mensaje = "Iteración eliminada correctamente" });
        }

        // HU-016: Calcular velocidad histórica
        [HttpGet("velocidad-historica")]
        public async Task<IActionResult> GetVelocidadHistorica([FromQuery] int? projectId)
        {
            var query = _context.Set<Iteracion>().AsQueryable();
            
            if (projectId.HasValue)
            {
                query = query.Where(i => i.ProjectId == projectId.Value);
            }
            
            var iteracionesPasadas = await query
                .Where(i => i.PuntosCompletados > 0 && i.FechaFin < DateTime.Now)
                .ToListAsync();
            
            if (!iteracionesPasadas.Any())
                return Ok(new { velocidadPromedio = 0, mensaje = "No hay datos históricos suficientes" });

            double velocidadPromedio = iteracionesPasadas.Average(i => i.PuntosCompletados);
            return Ok(new { velocidadPromedio });
        }

        // Obtener progreso del proyecto basado en tareas
        [HttpGet("progreso/{projectId}")]
        public async Task<IActionResult> GetProgresoProyecto(int projectId)
        {
            var iteraciones = await _context.Set<Iteracion>()
                .Where(i => i.ProjectId == projectId)
                .ToListAsync();
            
            int totalTareas = 0;
            int tareasCompletadas = 0;
            var progresosPorFase = new Dictionary<string, ProgresoFase>();
            
            foreach (var iteracion in iteraciones)
            {
                if (!string.IsNullOrEmpty(iteracion.TareasJson))
                {
                    var tareas = JsonSerializer.Deserialize<List<Tarea>>(iteracion.TareasJson) ?? new List<Tarea>();
                    
                    foreach (var tarea in tareas)
                    {
                        totalTareas++;
                        if (tarea.Estado == "Completada")
                        {
                            tareasCompletadas++;
                        }
                        
                        // Agrupar por fase
                        if (!string.IsNullOrEmpty(tarea.FaseProyecto))
                        {
                            if (!progresosPorFase.ContainsKey(tarea.FaseProyecto))
                            {
                                progresosPorFase[tarea.FaseProyecto] = new ProgresoFase { Fase = tarea.FaseProyecto };
                            }
                            
                            progresosPorFase[tarea.FaseProyecto].Total++;
                            if (tarea.Estado == "Completada")
                            {
                                progresosPorFase[tarea.FaseProyecto].Completadas++;
                            }
                        }
                    }
                }
            }
            
            var progresoTotal = totalTareas > 0 ? (tareasCompletadas * 100.0 / totalTareas) : 0;
            
            return Ok(new
            {
                progresoTotal,
                totalTareas,
                tareasCompletadas,
                progresosPorFase = progresosPorFase.Values.Select(p => new
                {
                    fase = p.Fase,
                    porcentaje = p.Total > 0 ? (p.Completadas * 100.0 / p.Total) : 0,
                    completadas = p.Completadas,
                    total = p.Total
                })
            });
        }
    }
    
    public class ProgresoFase
    {
        public string Fase { get; set; } = string.Empty;
        public int Total { get; set; }
        public int Completadas { get; set; }
    }
}