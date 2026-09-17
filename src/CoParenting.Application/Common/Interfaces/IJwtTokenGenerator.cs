using CoParenting.Application.Common.Models;

namespace CoParenting.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    JwtToken CreateToken(Guid userId, string email);
}
