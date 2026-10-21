using Tasks.Api.Contracts;

namespace Tasks.Api.Services;

public interface ITaskService
{
    Task<IReadOnlyList<TaskResponse>> ListAsync(CancellationToken ct);
    Task<TaskResponse?> GetAsync(Guid id, CancellationToken ct);
    Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken ct);
    Task<TaskResponse?> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}