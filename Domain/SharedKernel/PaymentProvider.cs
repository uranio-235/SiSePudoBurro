namespace Domain.SharedKernel;
public enum PaymentProvider
{
    /// <summary>
    /// Operación de débite, local.
    /// </summary>
    Debit,

    /// <summary>
    /// La plataforma de Erich.
    /// </summary>
    QvaPay
}
