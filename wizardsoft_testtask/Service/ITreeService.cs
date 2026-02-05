using wizardsoft_testtask.Dtos;
using wizardsoft_testtask.Models;

namespace wizardsoft_testtask.Service
{
    public interface ITreeService
    {
        Task<TreeNodeResponse?> GetAsync(long id, CancellationToken cancellationToken);
        Task<IReadOnlyCollection<TreeNodeResponse>> ExportAsync(CancellationToken cancellationToken);
        Task<IReadOnlyCollection<TreeNodeRootResponse>> GetRootsWithChildrenIdAsync(CancellationToken cancellationToken);
        Task<TreeNodeResponse> CreateAsync(TreeNodeCreateRequest request, CancellationToken cancellationToken);
        Task<TreeNodeResponse?> UpdateAsync(long id, TreeNodeUpdateRequest request, CancellationToken cancellationToken);
        Task<bool> DeleteAsync(long id, CancellationToken cancellationToken);
    }
}
