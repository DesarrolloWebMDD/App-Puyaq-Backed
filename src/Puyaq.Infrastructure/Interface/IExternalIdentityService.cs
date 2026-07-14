using Puyaq.Domain.Authentication.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Puyaq.Infrastructure.Interface
{
    public interface IExternalIdentityService
    {
        Task<ExternalIdentity> ValidateAsync(
        string provider,
        string credential,
        CancellationToken cancellationToken = default);
    }
}
