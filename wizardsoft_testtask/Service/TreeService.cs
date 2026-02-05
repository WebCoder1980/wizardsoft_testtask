using Microsoft.EntityFrameworkCore;
using wizardsoft_testtask.Data;
using wizardsoft_testtask.Dtos;
using wizardsoft_testtask.Models;

namespace wizardsoft_testtask.Service
{
    public class TreeService : ITreeService
    {
        private readonly AppDbContext _db;

        public TreeService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<TreeNodeResponse?> GetAsync(long id, CancellationToken cancellationToken)
        {
            var node = await _db.TreeNodes
                .AsNoTracking()
                .Include(x => x.Children)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (node == null)
            {
                return null;
            }

            return await BuildTree(node, cancellationToken);
        }

        public async Task<IReadOnlyCollection<TreeNodeResponse>> ExportAsync(CancellationToken cancellationToken)
        {
            return await GetRootsAsync(cancellationToken);
        }

        public async Task<IReadOnlyCollection<TreeNodeResponse>> GetRootsAsync(CancellationToken cancellationToken)
        {
            var roots = await _db.TreeNodes
                .Where(x => x.ParentId == null)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var result = new List<TreeNodeResponse>();
            foreach (var root in roots)
            {
                result.Add(await BuildTree(root, cancellationToken));
            }

            return result;
        }

        private async Task<TreeNodeResponse> BuildTree(TreeNode node, CancellationToken cancellationToken)
        {
            var children = await _db.TreeNodes
                .AsNoTracking()
                .Where(x => x.ParentId == node.Id)
                .ToListAsync(cancellationToken);

            var childResponses = new List<TreeNodeResponse>();
            foreach (var child in children)
            {
                childResponses.Add(await BuildTree(child, cancellationToken));
            }

            return new TreeNodeResponse(node.Id, node.Name, node.ParentId, childResponses);
        }
    }
}
