using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

public class SessionStore : ITicketStore
{
    private readonly IServiceScopeFactory _factory;

    public SessionStore(IServiceScopeFactory factory)
    {
        _factory = factory;
    }

    public async Task RemoveAsync(string key)
    {
        using (var scope = _factory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<FileStorageContext>();

            string hashedSessionKey = GetSHA256(key);

            var session = await context.Sessions.SingleOrDefaultAsync(x => string.Equals(x.SessionKeyHashed, hashedSessionKey));
            if(session != null)
            {
                context.Sessions.Remove(session);
                context.SaveChanges();
            }

        }
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        using (var scope = _factory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<FileStorageContext>();
            string hashedSessionKey = GetSHA256(key);
            var session = await context.Sessions.FirstOrDefaultAsync(x => string.Equals(x.SessionKeyHashed, hashedSessionKey));
            if(session != null)
            {
                session.Value = SerializeToBytes(ticket);
                session.LastUsedAt = DateTimeOffset.UtcNow;
                session.ExpireAt = (DateTimeOffset)ticket.Properties.ExpiresUtc;

            }

            context.SaveChanges();
        }
    }

    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        using (var scope = _factory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<FileStorageContext>();
            string hashedSessionKey = GetSHA256(key);
            var session = await context.Sessions.FirstOrDefaultAsync(x => string.Equals(x.SessionKeyHashed, hashedSessionKey));

            if(session != null)
            {
                session.LastUsedAt = DateTimeOffset.UtcNow;
                context.SaveChanges();

                return DeserializeFromBytes(session.Value);
            }
        }
        return null;
    }


public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        using (var scope = _factory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<FileStorageContext>();
            string sessionKey = GetSecureString();
            ticket.Properties.Items.TryGetValue("IP", out string IPAdress);
            ticket.Properties.Items.TryGetValue("User-Agent", out string UserAgentFetched);
            var session = new Session
            {
                SessionKeyHashed = GetSHA256(sessionKey),
                IP = IPAdress,
                UserAgent = UserAgentFetched,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpireAt = (DateTimeOffset)ticket.Properties.ExpiresUtc,
                LastUsedAt = DateTimeOffset.UtcNow,
                Value = SerializeToBytes(ticket)
            };

            context.Sessions.Add(session);
            context.SaveChanges();

            return sessionKey;
        }
    }
    public static string GetSHA256(string key)
    {
        SHA256 sha256 = SHA256.Create();

        byte[] bytes = Encoding.Unicode.GetBytes(key);
        return Convert.ToHexString(sha256.ComputeHash(bytes));
    }

    public static string GetSecureString(int size = 32)
    {
        byte[] bytes = new byte[size];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    private byte[] SerializeToBytes(AuthenticationTicket ticket) => TicketSerializer.Default.Serialize(ticket);

    private AuthenticationTicket DeserializeFromBytes(byte[] source) => source == null ? null : TicketSerializer.Default.Deserialize(source);
}