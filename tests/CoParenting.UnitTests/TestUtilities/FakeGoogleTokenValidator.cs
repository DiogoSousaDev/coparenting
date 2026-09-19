using CoParenting.BuildingBlocks.Core.Common.Interfaces;

namespace CoParenting.UnitTests.TestUtilities;

public class FakeGoogleTokenValidator : IGoogleTokenValidator
{
    private readonly Dictionary<string, GoogleUserInfo> _validTokens = [];

    public void SetValidToken(string idToken, GoogleUserInfo userInfo) => _validTokens[idToken] = userInfo;

    public Task<GoogleUserInfo?> ValidateAsync(string idToken, CancellationToken cancellationToken = default) =>
        Task.FromResult(_validTokens.TryGetValue(idToken, out var userInfo) ? userInfo : null);
}
