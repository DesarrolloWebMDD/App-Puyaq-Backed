namespace Puyaq.Application.Authentication.DTOs
{
    /// <summary>
    /// Solicitud para vincular un proveedor externo
    /// a una cuenta PUYAQ autenticada.
    /// </summary>
    public sealed record LinkExternalProviderRequest(
     string Provider,
     string Credential);
}
