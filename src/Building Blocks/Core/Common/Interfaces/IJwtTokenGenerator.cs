using CoParenting.BuildingBlocks.Core.Common.Models;

namespace CoParenting.BuildingBlocks.Core.Common.Interfaces;

public interface IJwtTokenGenerator
{
    JwtToken CreateToken(Guid userId, string email);
}
