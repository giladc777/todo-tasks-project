using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tasks.Api.Contracts;
using Tasks.Api.Services;

namespace Tasks.Api.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize(Policy = "tasks.read")]
public sealed class TasksController(ITaskService taskService) : ControllerBase
{
    /// <summary>Lists tasks owned by current user.</summary>
    [HttpGet(Name = "ListTasks")]
    [ProducesResponseType(typeof(IReadOnlyList<TaskResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TaskResponse>>> List(CancellationToken cancellationToken) =>
        Ok(await taskService.ListAsync(cancellationToken));

    /// <summary>Gets task by ID for current user.</summary>
    [HttpGet("{id:guid}", Name = "GetTask")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var task = await taskService.GetAsync(id, cancellationToken);
        return task is null ? NotFound() : Ok(task);
    }

    /// <summary>Creates a task for current user.</summary>
    [HttpPost(Name = "CreateTask")]
    [Authorize(Policy = "tasks.write")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<TaskResponse>> Create(
        CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var task = await taskService.CreateAsync(request, cancellationToken);
        return Created($"/api/tasks/{task.Id}", task);
    }

    /// <summary>Updates a task owned by current user.</summary>
    [HttpPut("{id:guid}", Name = "UpdateTask")]
    [Authorize(Policy = "tasks.write")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> Update(
        Guid id,
        UpdateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var task = await taskService.UpdateAsync(id, request, cancellationToken);
        return task is null ? NotFound() : Ok(task);
    }

    /// <summary>Deletes a task owned by current user.</summary>
    [HttpDelete("{id:guid}", Name = "DeleteTask")]
    [Authorize(Policy = "tasks.write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await taskService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}