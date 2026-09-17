using CoParenting.Application.Common.Interfaces;
using CoParenting.Application.Common.Models;

namespace CoParenting.UnitTests.TestUtilities;

public class FakeJwtTokenGenerator : IJwtTokenGenerator
{
    public JwtToken CreateToken(Guid userId, string email) => new($"fake-jwt-{userId}", DateTime.UtcNow.AddDays(7));
}
