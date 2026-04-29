namespace wdb_backend.Services;

public class JwtOptions
{
    // from appsettings.json
    // "Jwt": {
    //     "Issuer": "WorkerDataBlockchain",
    //     "Audience": "WorkerDataBlockchain.Client",
    //     "Key": "CHANGE_THIS_TO_A_LONG_RANDOM_SECRET_KEY_AT_LEAST_32_CHARS",
    //     "AccessTokenMinutes": 60
    // }

    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public int AccessTokenMinutes = 60;

}
