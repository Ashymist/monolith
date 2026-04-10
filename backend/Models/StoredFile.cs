public class StoredFile
{
    public int Id {get;set;}
    public required string FilePath {get; set;}
    public required string Name {get; set;}
    public required string Type {get; set;}
    public required long ByteSize {get; set;}
    public required DateTime LastUpdated {get; set;}

    public required DateTime CreatedAt {get;set;}

}