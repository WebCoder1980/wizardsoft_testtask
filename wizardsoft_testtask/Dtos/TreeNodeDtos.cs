namespace wizardsoft_testtask.Dtos
{
    public record TreeNodeCreateRequest(string Name, long? ParentId);

    public record TreeNodeUpdateRequest(string Name, long? ParentId);

    public record TreeNodeResponse(long Id, string Name, long? ParentId, IReadOnlyCollection<TreeNodeResponse> Children);
    public record TreeNodeRootResponse(long Id, string Name, long? ParentId, IReadOnlyCollection<long> ChildrenId);
}
