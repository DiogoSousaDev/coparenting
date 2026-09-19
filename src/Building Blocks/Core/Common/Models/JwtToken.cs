namespace CoParenting.BuildingBlocks.Core.Common.Models;

public record JwtToken(string Value, DateTime ExpiresAtUtc);
