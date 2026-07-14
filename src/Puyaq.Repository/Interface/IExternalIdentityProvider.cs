using Puyaq.Domain.Authentication.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Puyaq.Repository.Interface
{
    public interface IExternalIdentityProvider
    {
        Task<ExternalIdentity> ValidateAsync(
            string provider,
            string credential,
            CancellationToken cancellationToken = default);
    }
}
