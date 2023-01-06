using AspNetCore.Authentication.ApiKey;

using System.Security.Claims;

namespace View.Middlewares;

internal class ApiKeyProvider : IApiKeyProvider
{
    private readonly Dictionary<string, string> _kies;
    public ApiKeyProvider(IConfiguration configuration)
    {
        _kies = configuration!.GetSection("ApiKey")
            .GetChildren()
            .ToDictionary(x => x.Value ?? throw new NullReferenceException(), x => x.Key); // ejasí al revés, no lo toques
    }

#pragma warning disable CA1841 // null es porque no encuentra el dato y devolverá acceso denegado
    public async Task<IApiKey> ProvideAsync(string key) =>
            _kies.Keys.Contains(key) ? new ApiKey(key, _kies[key]) : null;
#pragma warning restore CA1841
}


internal class ApiKey : IApiKey
{
    public ApiKey(string key, string owner, List<Claim> claims = null)
    {
        Key = key;
        OwnerName = owner;
        Claims = claims ?? new List<Claim>();
    }

    public string Key { get; }
    public string OwnerName { get; }
    public IReadOnlyCollection<Claim> Claims { get; }
}
