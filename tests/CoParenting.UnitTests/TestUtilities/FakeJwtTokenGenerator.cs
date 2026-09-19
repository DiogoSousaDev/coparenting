using CoParenting.BuildingBlocks.Core.Common.Interfaces;
using CoParenting.BuildingBlocks.Core.Common.Models;

namespace CoParenting.UnitTests.TestUtilities;

public class FakeJwtTokenGenerator : IJwtTokenGenerator
{
    public JwtToken CreateToken(Guid userId, string email) => new($"fake-jwt-{userId}", DateTime.UtcNow.AddDays(7));
}
