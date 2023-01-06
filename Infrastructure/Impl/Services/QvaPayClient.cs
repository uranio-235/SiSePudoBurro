﻿using Domain.Abstract.Services;
using Domain.Models.QvaPay;

using Flurl;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

using System.Net.Http.Json;

namespace Infrastructure.Impl.Services;
public class QvapayClient : IQvapayClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<QvapayClient> _logger;
    private readonly QvapayConfig _config;

    private const string _urlPrefix = @"https://qvapay.com";

    public QvapayClient(
        QvapayConfig config,
        HttpClient httpClient,
        ILogger<QvapayClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
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
