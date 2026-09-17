namespace CoParenting.Application.Common.Models;

public record JwtToken(string Value, DateTime ExpiresAtUtc);
