public record FileDto
{
    public string Reference {get; init;} = null!;
    public string Type {get; init;} = null!;
    public double ByteSize {get; init;}
    public DateTime LastUpdated {get; init;}
    public string Name {get; init;} = null!;

}