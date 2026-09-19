using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CoParenting.BuildingBlocks.Core.Common.Interfaces;
using CoParenting.BuildingBlocks.Core.Common.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CoParenting.BuildingBlocks.Core.Identity;

public class JwtTokenGenerator(IOptions<JwtOptions> jwtOptions) : IJwtTokenGenerator
{
    public JwtToken CreateToken(Guid userId, string email)
    {
        var options = jwtOptions.Value;
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(options.ExpiryMinutes);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: signingCredentials);

        return new JwtToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }
}
