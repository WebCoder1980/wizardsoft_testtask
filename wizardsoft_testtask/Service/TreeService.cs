using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using wizardsoft_testtask.Data;
using wizardsoft_testtask.Dtos;
using wizardsoft_testtask.Exceptions;
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
            TreeNode? node = await _db.TreeNodes
                .AsNoTracking()
                .Include(treeNode => treeNode.Children)
                .FirstOrDefaultAsync(treeNode => treeNode.Id == id, cancellationToken);

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
            IReadOnlyCollection<TreeNodeResponse> roots = await GetRootsAsync(cancellationToken);

            return roots
                .Select(treeNode => new TreeNodeRootResponse(
                    treeNode.Id,
                    treeNode.Name,
                    treeNode.ParentId,
                    treeNode.Children.Select(child => child.Id).ToList()))
                .ToList();
        }

        public async Task<TreeNodeResponse> CreateAsync(TreeNodeCreateRequest request, CancellationToken cancellationToken)
        {
            await using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            TreeNode? parent = null;
            if (request.ParentId.HasValue)
            {
                parent = await _db.TreeNodes.FirstOrDefaultAsync(x => x.Id == request.ParentId.Value, cancellationToken);
                if (parent == null)
                {
                    throw new InvalidDataException("Parent not found");
                }
            }

            TreeNode entity = new TreeNode
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
            await using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            TreeNode? node = await _db.TreeNodes.FirstOrDefaultAsync(treeNode => treeNode.Id == id, cancellationToken);
            if (node == null)
            {
                return null;
            }

            if (request.ParentId.HasValue)
            {
                if (request.ParentId.Value == id)
                {
                    throw new CircleInTreeException();
                }

                TreeNode? newParent = await _db.TreeNodes.FirstOrDefaultAsync(treeNode => treeNode.Id == request.ParentId.Value, cancellationToken);
                if (newParent == null)
                {
                    throw new InvalidDataException("Parent not found");
                }

                if (await IsDescendantAsync(newParent.Id, node.Id, cancellationToken))
                {
                    throw new CircleInTreeException();
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

            TreeNode reloaded = await _db.TreeNodes
                .AsNoTracking()
                .Include(treeNode => treeNode.Children)
                .FirstAsync(treeNode => treeNode.Id == id, cancellationToken);

            return await BuildTree(reloaded, cancellationToken);
        }

        public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken)
        {
            await using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            TreeNode? node = await _db.TreeNodes
                .Include(treeNode => treeNode.Children)
                .FirstOrDefaultAsync(treeNode => treeNode.Id == id, cancellationToken);

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
            List<TreeNode> roots = await _db.TreeNodes
                .Where(root => root.ParentId == null)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            List<TreeNodeResponse> rootsResponse = new List<TreeNodeResponse>();
            foreach (TreeNode root in roots)
            {
                rootsResponse.Add(await BuildTree(root, cancellationToken));
            }

            return rootsResponse;
        }

        private async Task<TreeNodeResponse> BuildTree(TreeNode node, CancellationToken cancellationToken)
        {
            List<TreeNode> children = await _db.TreeNodes
                .AsNoTracking()
                .Where(child => child.ParentId == node.Id)
                .ToListAsync(cancellationToken);

            List<TreeNodeResponse> childResponses = new List<TreeNodeResponse>();
            foreach (TreeNode child in children)
            {
                childResponses.Add(await BuildTree(child, cancellationToken));
            }

            return new TreeNodeResponse(node.Id, node.Name, node.ParentId, childResponses);
        }

        private async Task<bool> IsDescendantAsync(long nodeId, long potentialAncestorId, CancellationToken cancellationToken)
        {
            TreeNode? current = await _db.TreeNodes.AsNoTracking().FirstOrDefaultAsync(treeNode => treeNode.Id == nodeId, cancellationToken);
            while (current != null && current.ParentId.HasValue)
            {
                if (current.ParentId.Value == potentialAncestorId)
                {
                    return true;
                }

                current = await _db.TreeNodes.AsNoTracking().FirstOrDefaultAsync(treeNode => treeNode.Id == current.ParentId.Value, cancellationToken);
            }

            return false;
        }
        private async Task DeleteSubtree(TreeNode node, CancellationToken cancellationToken)
        {
            List<TreeNode> children = await _db.TreeNodes
                .Where(child => child.ParentId == node.Id)
                .ToListAsync(cancellationToken);

            foreach (TreeNode child in children)
            {
                await DeleteSubtree(child, cancellationToken);
            }

            _db.TreeNodes.Remove(node);
        }
    }
}
