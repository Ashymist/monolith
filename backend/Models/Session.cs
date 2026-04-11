public class Session
{
    public int Id {get;set;}
    
    public required string SessionKeyHashed {get;set;}

    public required string UserAgent {get;set;}

    public required DateTimeOffset CreatedAt {get;set;}

    public required DateTimeOffset ExpireAt {get;set;}

    public required string IP {get;set;}

    public required DateTimeOffset LastUsedAt {get;set;}

    public required byte[] Value {get;set;}

}