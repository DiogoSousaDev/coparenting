using CoParenting.Application.Common.Interfaces;
using CoParenting.Application.Common.Models;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace CoParenting.Infrastructure.Identity;

public class GoogleTokenValidator(IOptions<GoogleOptions> options) : IGoogleTokenValidator
{
    public async Task<GoogleUserInfo?> ValidateAsync(string idToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [options.Value.ClientId]
            });

            return new GoogleUserInfo(payload.Email, payload.GivenName ?? string.Empty, payload.FamilyName ?? string.Empty, payload.EmailVerified);
        }
        catch (InvalidJwtException)
        {
            return null;
        }
    }
}
