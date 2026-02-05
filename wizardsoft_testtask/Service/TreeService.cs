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

        public async Task<IReadOnlyCollection<TreeNodeRootResponse>> GetRootsWithChildrenIdAsync(CancellationToken cancellationToken)
        {
            var result = await GetRootsAsync(cancellationToken);

            return result
                .Select(x => new TreeNodeRootResponse(
                    x.Id,
                    x.Name, 
                    x.ParentId,
                    x.Children.Select(y => y.Id).ToList()))
                .ToList();
        }

        public async Task<TreeNodeResponse> CreateAsync(TreeNodeCreateRequest request, CancellationToken cancellationToken)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            TreeNode? parent = null;
            if (request.ParentId.HasValue)
            {
                parent = await _db.TreeNodes.FirstOrDefaultAsync(x => x.Id == request.ParentId.Value, cancellationToken);
                if (parent == null)
                {
                    throw new InvalidOperationException("Parent not found");
                }
            }

            var entity = new TreeNode
            {
                Name = request.Name,
                ParentId = parent?.Id
            };

            _db.TreeNodes.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new TreeNodeResponse(entity.Id, entity.Name, entity.ParentId, Array.Empty<TreeNodeResponse>());
        }

        public async Task<TreeNodeResponse?> UpdateAsync(long id, TreeNodeUpdateRequest request, CancellationToken cancellationToken)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            var node = await _db.TreeNodes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (node == null)
            {
                return null;
            }

            if (request.ParentId.HasValue)
            {
                if (request.ParentId.Value == id)
                {
                    throw new InvalidOperationException("Node cannot be its own parent");
                }

                var newParent = await _db.TreeNodes.FirstOrDefaultAsync(x => x.Id == request.ParentId.Value, cancellationToken);
                if (newParent == null)
                {
                    throw new InvalidOperationException("Parent not found");
                }

                if (await IsDescendantAsync(newParent.Id, node.Id, cancellationToken))
                {
                    throw new InvalidOperationException("Cyclic hierarchy is not allowed");
                }

                node.ParentId = newParent.Id;
            }
            else
            {
                node.ParentId = null;
            }

            node.Name = request.Name;

            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var reloaded = await _db.TreeNodes
                .AsNoTracking()
                .Include(x => x.Children)
                .FirstAsync(x => x.Id == id, cancellationToken);

            return await BuildTree(reloaded, cancellationToken);
        }

        public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            var node = await _db.TreeNodes
                .Include(x => x.Children)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (node == null)
            {
                return false;
            }

            await DeleteSubtree(node, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return true;
        }

        private async Task<IReadOnlyCollection<TreeNodeResponse>> GetRootsAsync(CancellationToken cancellationToken)
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

        private async Task<bool> IsDescendantAsync(long nodeId, long potentialAncestorId, CancellationToken cancellationToken)
        {
            var current = await _db.TreeNodes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == nodeId, cancellationToken);
            while (current != null && current.ParentId.HasValue)
            {
                if (current.ParentId.Value == potentialAncestorId)
                {
                    return true;
                }

                current = await _db.TreeNodes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == current.ParentId.Value, cancellationToken);
            }

            return false;
        }
        private async Task DeleteSubtree(TreeNode node, CancellationToken cancellationToken)
        {
            var children = await _db.TreeNodes
                .Where(x => x.ParentId == node.Id)
                .ToListAsync(cancellationToken);

            foreach (var child in children)
            {
                await DeleteSubtree(child, cancellationToken);
            }

            _db.TreeNodes.Remove(node);
        }
    }
}
