using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkflowsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public WorkflowsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Workflow>>> GetWorkflows()
    {
        return await _context.Workflows
            .Include(w => w.Steps.OrderBy(s => s.Order))
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Workflow>> GetWorkflow(int id)
    {
        var workflow = await _context.Workflows
            .Include(w => w.Steps.OrderBy(s => s.Order))
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workflow == null)
            return NotFound();

        return workflow;
    }

    [HttpPost]
    public async Task<ActionResult<Workflow>> CreateWorkflow(WorkflowCreateDto dto)
    {
        var workflow = new Workflow
        {
            Name = dto.Name,
            Description = dto.Description,
            Steps = dto.Steps.Select((step, index) => new WorkflowStep
            {
                Name = step.Name,
                Order = index
            }).ToList()
        };

        _context.Workflows.Add(workflow);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetWorkflow), new { id = workflow.Id }, workflow);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateWorkflow(int id, WorkflowCreateDto dto)
    {
        var workflow = await _context.Workflows
            .Include(w => w.Steps)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workflow == null)
            return NotFound();

        workflow.Name = dto.Name;
        workflow.Description = dto.Description;

        _context.WorkflowSteps.RemoveRange(workflow.Steps);

        workflow.Steps = dto.Steps.Select((step, index) => new WorkflowStep
        {
            Name = step.Name,
            Order = index,
            WorkflowId = id
        }).ToList();

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWorkflow(int id)
    {
        var workflow = await _context.Workflows.FindAsync(id);
        if (workflow == null)
            return NotFound();

        _context.Workflows.Remove(workflow);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class WorkflowCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<WorkflowStepDto> Steps { get; set; } = new();
}

public class WorkflowStepDto
{
    public string Name { get; set; } = string.Empty;
}