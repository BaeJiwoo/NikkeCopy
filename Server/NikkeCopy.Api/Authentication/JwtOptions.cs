namespace NikkeCopy.Api.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "NikkeCopy.Server";
    public string Audience { get; init; } = "NikkeCopy.Client";
    public string Secret { get; init; } = string.Empty;
    public int AccessTokenMinutes { get; init; } = 60;
}
