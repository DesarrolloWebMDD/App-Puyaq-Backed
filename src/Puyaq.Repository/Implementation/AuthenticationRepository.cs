using Microsoft.Data.SqlClient;
using Puyaq.Domain.Models.Authentication;
using Puyaq.Infrastructure.Interface;
using Puyaq.Repository.Interface;
using Puyaq.Repository.Resources;
using System.Data;

namespace Puyaq.Repository.Implementation;

public sealed class AuthenticationRepository(IConnectionBase connectionBase)
    : IAuthenticationRepository
{

    public Task<AuthenticationUserRecord?> GetByNormalizedEmailAsync(
    string normalizedEmail,
    CancellationToken cancellationToken = default)
    {
        List<SqlParameter> parameters = new()
    {
        new SqlParameter(
            "@NormalizedEmail",SqlDbType.NVarChar,256)
        {
            Value = normalizedEmail
        }
    };

        return connectionBase.ExecuteSingleAsync<AuthenticationUserRecord>(
            StoreProcedures.AuthenticationGetByNormalizedEmail,
            parameters,
            cancellationToken: cancellationToken);
    }

    public async Task<AuthenticationUserRecord> RegisterAsync(
      RegisterUserCommand command,
      CancellationToken cancellationToken = default)
    {
        List<SqlParameter> parameters = new()
    {
        new SqlParameter("@Id", command.Id),
        new SqlParameter("@Email", command.Email),
        new SqlParameter("@NormalizedEmail", command.NormalizedEmail),
        new SqlParameter("@Status", command.Status),
        new SqlParameter("@PasswordHash", command.PasswordHash),
        new SqlParameter("@DisplayName", command.DisplayName),
        new SqlParameter("@CreatedAt", command.CreatedAt),
    };

        var result =
            await connectionBase.ExecuteSingleAsync<AuthenticationUserRecord>(
                StoreProcedures.AuthenticationRegister,
                parameters,
                cancellationToken: cancellationToken);

        return result
            ?? throw new InvalidOperationException(
                "El registro no devolvió el usuario creado.");
    }

    public async Task UpdateExternalLoginAsync(
     string provider,
     string providerUserId,
     string? email,
     string? displayName,
     string? profileImageUrl,
     bool emailVerified,
     DateTimeOffset updatedAt,
     DateTimeOffset lastLoginAt,
     CancellationToken cancellationToken = default)
    {
        var parameters = new[]
        {
        new SqlParameter(
            "@Provider",
            SqlDbType.NVarChar,
            30)
        {
            Value = provider.Trim().ToUpperInvariant()
        },

        new SqlParameter(
            "@ProviderUserId",
            SqlDbType.NVarChar,
            255)
        {
            Value = providerUserId
        },

        new SqlParameter(
            "@Email",
            SqlDbType.NVarChar,
            256)
        {
            Value = (object?)email ?? DBNull.Value
        },

        new SqlParameter(
            "@DisplayName",
            SqlDbType.NVarChar,
            150)
        {
            Value = (object?)displayName ?? DBNull.Value
        },

        new SqlParameter(
            "@ProfileImageUrl",
            SqlDbType.NVarChar,
            1000)
        {
            Value = (object?)profileImageUrl ?? DBNull.Value
        },

        new SqlParameter(
            "@EmailVerified",
            SqlDbType.Bit)
        {
            Value = emailVerified
        },

        new SqlParameter(
            "@UpdatedAt",
            SqlDbType.DateTimeOffset)
        {
            Value = updatedAt
        },

        new SqlParameter(
            "@LastLoginAt",
            SqlDbType.DateTimeOffset)
        {
            Value = lastLoginAt
        }
    };

        await connectionBase.ExecuteNonQueryAsync(
            StoreProcedures.AuthenticationUpdateExternalLogin,
            parameters,
            cancellationToken: cancellationToken);
    }

    public async Task SaveRefreshTokenAsync(
        SaveRefreshTokenCommand command,
        CancellationToken cancellationToken = default)
    {
        var parameters = new[]
        {
            new SqlParameter("@SessionId", SqlDbType.UniqueIdentifier) { Value = command.SessionId },
            new SqlParameter("@RefreshTokenId", SqlDbType.UniqueIdentifier) { Value = command.RefreshTokenId },
            new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = command.UserId },
            new SqlParameter("@TokenFamilyId", SqlDbType.UniqueIdentifier) { Value = command.TokenFamilyId },
            new SqlParameter("@TokenHash", SqlDbType.NVarChar, 128) { Value = command.TokenHash },
            new SqlParameter("@SessionExpiresAt", SqlDbType.DateTimeOffset) { Value = command.SessionExpiresAt },
            new SqlParameter("@TokenExpiresAt", SqlDbType.DateTimeOffset) { Value = command.TokenExpiresAt },
            new SqlParameter("@CreatedAt", SqlDbType.DateTimeOffset) { Value = command.CreatedAt }
        };

        await connectionBase.ExecuteNonQueryAsync(
            StoreProcedures.AuthenticationSaveRefreshToken,
            parameters,
            cancellationToken: cancellationToken);
    }

    public Task<AuthenticationUserRecord?> GetByExternalLoginAsync(
    string provider,
    string providerUserId,
    CancellationToken cancellationToken = default)
    {
        List<SqlParameter> parameters = new()
    {
        new("@Provider", SqlDbType.NVarChar, 30)
        {
            Value = provider
        },
        new("@ProviderUserId", SqlDbType.NVarChar, 255)
        {
            Value = providerUserId
        }
    };

        return connectionBase
            .ExecuteSingleAsync<AuthenticationUserRecord>(
                StoreProcedures.AuthenticationGetByExternalLogin,
                parameters,
                cancellationToken: cancellationToken);
    }

    public async Task<AuthenticationUserRecord> RegisterExternalUserAsync(
    RegisterExternalUserCommand command,
    CancellationToken cancellationToken = default)
    {
        List<SqlParameter> parameters = new()
    {
        new("@UserId", command.UserId),
        new("@ExternalLoginId", command.ExternalLoginId),
        new("@Email", command.Email),
        new("@NormalizedEmail", command.NormalizedEmail),
        new("@DisplayName", command.DisplayName),
        new("@Status", command.Status),
        new("@Provider", command.Provider),
        new("@ProviderUserId", command.ProviderUserId),
        new("@ProfileImageUrl",
            (object?)command.ProfileImageUrl ?? DBNull.Value),
        new("@EmailVerified", command.EmailVerified),
        new("@CreatedAt", command.CreatedAt)
    };

        var result =
            await connectionBase
                .ExecuteSingleAsync<AuthenticationUserRecord>(
                    StoreProcedures.AuthenticationRegisterExternalUser,
                    parameters,
                    cancellationToken: cancellationToken);

        return result
            ?? throw new InvalidOperationException(
                "El registro social no devolvió el usuario creado.");
    }

    public async Task UpdateExternalLoginAsync(
    UpdateExternalLoginCommand command,
    CancellationToken cancellationToken = default)
    {
        List<SqlParameter> parameters = new()
    {
        new("@Provider", command.Provider),
        new("@ProviderUserId", command.ProviderUserId),
        new("@Email",
            (object?)command.Email ?? DBNull.Value),
        new("@DisplayName",
            (object?)command.DisplayName ?? DBNull.Value),
        new("@ProfileImageUrl",
            (object?)command.ProfileImageUrl ?? DBNull.Value),
        new("@EmailVerified", command.EmailVerified),
        new("@UpdatedAt", command.UpdatedAt),
        new("@LastLoginAt", command.LastLoginAt)
    };

        await connectionBase.ExecuteNonQueryAsync(
            StoreProcedures.AuthenticationUpdateExternalLogin,
            parameters,
            cancellationToken: cancellationToken);
    }

    public async Task UpdateLastLoginAsync(
    Guid userId,
    DateTimeOffset lastLoginAt,
    CancellationToken cancellationToken = default)
    {
        var parameters = new[]
        {
        new SqlParameter(
            "@UserId",
            SqlDbType.UniqueIdentifier)
        {
            Value = userId
        },
        new SqlParameter(
            "@LastLoginAt",
            SqlDbType.DateTimeOffset)
        {
            Value = lastLoginAt
        }
    };

        await connectionBase.ExecuteNonQueryAsync(
            StoreProcedures.AuthenticationUpdateLastLogin,
            parameters,
            cancellationToken: cancellationToken);
    }
    public async Task UpdateExternalLoginLastLoginAsync(
    Guid userId,
    string provider,
    string providerUserId,
    DateTimeOffset lastLoginAt,
    CancellationToken cancellationToken = default)
    {
        var parameters = new[]
        {
        new SqlParameter(
            "@UserId",
            SqlDbType.UniqueIdentifier)
        {
            Value = userId
        },
        new SqlParameter(
            "@Provider",
            SqlDbType.NVarChar,
            30)
        {
            Value = provider
        },
        new SqlParameter(
            "@ProviderUserId",
            SqlDbType.NVarChar,
            255)
        {
            Value = providerUserId
        },
        new SqlParameter(
            "@LastLoginAt",
            SqlDbType.DateTimeOffset)
        {
            Value = lastLoginAt
        }
    };

        await connectionBase.ExecuteNonQueryAsync(
            StoreProcedures.AuthenticationUpdateExternalLoginLastLogin,
            parameters,
            cancellationToken: cancellationToken);
    }


}
