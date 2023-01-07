using Domain.Abstract.Services;
using Domain.Models.QvaPay;

using Flurl;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Net.Http.Json;

namespace Infrastructure.Impl.Services;
public class QvapayClient : IQvapayClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<QvapayClient> _logger;
    private readonly IHostEnvironment _env;
    private readonly QvapayConfig _config;

    private const string _urlPrefix = @"https://qvapay.com";

    public QvapayClient(
        IHostEnvironment env, // por el amor de dios, no hagas esto en la vida real
        QvapayConfig config,
        HttpClient httpClient,
        ILogger<QvapayClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _env = env;
        _config = config;
    }

    public async Task<InvoiceCreated> CreateInvoiceAsync(decimal amount, string description, string remoteId)
    {
        var url = _urlPrefix
            .AppendPathSegment("/api/v1/create_invoice")
            .SetQueryParams(new
            {
                app_id = _config.ClientId,
                app_secret = _config.ClientSecret,
                amount = Math.Round(amount, 2),
                description = description,
                remote_id = remoteId,
                signed = 0
            });

        if (!_env.IsProduction())
            return new InvoiceCreated
            {
                RemoteId = remoteId,
                Amount = amount,
                QvapayId = Guid.NewGuid(),
                Url = "http://localhost/testing"
            };

        var result = await _httpClient.GetAsync(url.ToString());

        if (!result.IsSuccessStatusCode)
        {
            _logger.LogError("fallo contactando a qvapay: {E}", result.ReasonPhrase);
            throw new OperationCanceledException();
        }

        return await result.Content.ReadFromJsonAsync<InvoiceCreated>();

    } // CreateInvoice


    public async Task<InvoiceGot> GetInvoiceAsync(string qvapayId)
    {
        var url = _urlPrefix
            .AppendPathSegment($"/api/v1/transactions/{qvapayId}")
            .SetQueryParams(new
            {
                app_id = _config.ClientId,
                app_secret = _config.ClientSecret
            });

        if (!_env.IsProduction())
            return new InvoiceGot
            {
                Amount = 10,
                QvapayId = Guid.NewGuid(),
                Status = "paid"
            };

        var result = await _httpClient.GetAsync(url.ToString());

        if (!result.IsSuccessStatusCode)
        {
            _logger.LogError("fallo contactando a qvapay: {E}", result.ReasonPhrase);
            throw new Exception();
        }

        return await result.Content.ReadFromJsonAsync<InvoiceGot>();

    } // GetInvoice



    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        => await HealthCheck() ?
            HealthCheckResult.Healthy("respuesta de qvapay") :
            HealthCheckResult.Unhealthy("falló qvapay");

    public async Task<bool> HealthCheck()
    {
        if (!_env.IsProduction())
            return true;

        var url = _urlPrefix
            .AppendPathSegment($"/api/v1/info")
            .SetQueryParams(new
            {
                app_id = _config.ClientId,
                app_secret = _config.ClientSecret
            });

        var result = await _httpClient.GetAsync(url.ToString());

        return result.IsSuccessStatusCode;
    }
}
