namespace JobTracker.Api.Auth;

public class JwtOptions
{
    public string Issuer { get; set; } = "JobTracker";
    public string Audience { get; set; } = "JobTracker";
    public string Secret { get; set; } = "dev-only-change-me";
    public int ExpMinutes { get; set; } = 60;
}
