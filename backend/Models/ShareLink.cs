public class ShareLink
{
    public required int Id {get;set;}

    public required string ShareKeyHashed {get;set;}

    public required string SharedPath {get;set;}

    public string? Description {get;set;}

    public required bool IsFolder {get;set;}

    public required DateTime CreateAt {get;set;}

    public required DateTime ExpireAt {get;set;}

    
}