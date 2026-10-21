using Skins.Api.Contracts;

namespace Skins.Api.Services;

public interface ISkinService
{
    Task<IReadOnlyList<SkinResponse>> ListAsync(CancellationToken ct);
    Task<SkinResponse> ActiveAsync(CancellationToken ct);
    Task<SkinResponse?> SaveAsync(Guid? id, SkinRequest request, CancellationToken ct);
    Task<SkinResponse?> ActivateAsync(Guid id, CancellationToken ct);
}