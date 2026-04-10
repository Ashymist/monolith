public class Session
{
    public int Id {get;set;}
    
    public required string SessionKeyHashed {get;set;}

    public required string UserAgent {get;set;}

    public required DateTime CreatedAt {get;set;}

    public required DateTime ExpireAt {get;set;}

    public required string IP {get;set;}

    public required DateTime LastUsedAt {get;set;}

}