using System.ComponentModel.DataAnnotations;

namespace wizardsoft_testtask.Dtos
{
    public record TreeNodeCreateRequest([Length(1, 1000)] string Name, long? ParentId);

    public record TreeNodeUpdateRequest([Length(1, 1000)] string Name, long? ParentId);

    public record TreeNodeResponse(long Id, string Name, long? ParentId, IReadOnlyCollection<TreeNodeResponse> Children);
    public record TreeNodeRootResponse(long Id, string Name, long? ParentId, IReadOnlyCollection<long> ChildrenId);
}
