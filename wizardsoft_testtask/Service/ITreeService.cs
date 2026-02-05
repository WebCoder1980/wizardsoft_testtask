using wizardsoft_testtask.Dtos;
using wizardsoft_testtask.Models;

namespace wizardsoft_testtask.Service
{
    public interface ITreeService
    {
        Task<TreeNodeResponse?> GetAsync(long id, CancellationToken cancellationToken);
        Task<IReadOnlyCollection<TreeNodeResponse>> ExportAsync(CancellationToken cancellationToken);
        Task<IReadOnlyCollection<TreeNodeResponse>> GetRootsAsync(CancellationToken cancellationToken);
    }
}
