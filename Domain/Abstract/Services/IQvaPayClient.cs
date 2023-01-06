using Domain.Models.QvaPay;

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Domain.Abstract.Services;
public interface IQvapayClient : IHealthCheck
{
    /// <summary>
    /// Genera una factura de qvapay.
    /// </summary>
    /// <param name="amount">cuando le quieres cobrar al tipo</param>
    /// <param name="description">obligado tiene que decir algo</param>
    /// <param name="remoteId">el campo ad-hoc que el webhook lo tira pa atrá</param>
    /// <returns>El modelo que contiene los datos.</returns>
    Task<InvoiceCreated> CreateInvoiceAsync(decimal amount, string description, string remoteId);

    /// <summary>
    /// Encuesta los datos de una factura en la api.
    /// </summary>
    /// <param name="qvapayId">la id de qvapay, conocida como uuid</param>
    /// <returns>El modelo que contiene los datos.</returns>
    Task<InvoiceGot> GetInvoiceAsync(string qvapayId);

    /// <summary>
    /// Respuesta para el endpoint de health check
    /// </summary>
    /// <returns></returns>
    Task<bool> HealthCheck();
}
