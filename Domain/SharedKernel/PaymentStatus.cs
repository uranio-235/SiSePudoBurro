namespace Domain.SharedKernel;

/// <summary>
/// Estado del pago, 1 solicitado y 2 bien recibido.
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// 0 - Estado por defect, alguien metió la pata y no puso el valor.
    /// </summary>
    Unknown,

    /// <summary>
    /// 1 - El pago se ha solicitado, estamos esperando por el proveedor.
    /// </summary>
    Requested,

    /// <summary>
    /// 2 - El pago se terminó satisfactoriamente.
    /// </summary>
    Completed,


    /// <summary>
    /// El usuario o nuestro lado, a cancelado el pago de manera intencional.
    /// </summary>
    Canceled,

    /// <summary>
    /// El proveedor (la otra parte) a rechazado el pago.
    /// </summary>
    Rejected,

}
