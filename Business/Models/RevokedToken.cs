namespace DataLabelProject.Business.Models;

public class RevokedToken
{
    public string Jti { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime RevokedAt { get; set; }
}
