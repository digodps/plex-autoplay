namespace PlexAutoplay.ArchitectureTests;

using PlexAutoplay.Application;
using PlexAutoplay.Application.UseCases;
using PlexAutoplay.Infrastructure.Auth;
using PlexAutoplay.Infrastructure.Persistence.Repositories;

public class PasswordAndAuthTests
{
    private sealed class FakeCredentialStore : IAuthCredentialStore
    {
        public string? Hash { get; set; }
        public bool IsConfiguredAsyncResult => Hash is not null;

        public Task<bool> IsConfiguredAsync() => Task.FromResult(IsConfiguredAsyncResult);
        public Task<string?> GetHashAsync() => Task.FromResult(Hash);
        public Task<bool> CreateAsync(string passwordHash)
        {
            if (Hash is not null) return Task.FromResult(false);
            Hash = passwordHash;
            return Task.FromResult(true);
        }

        public Task<bool> UpdateAsync(string passwordHash)
        {
            if (Hash is null) return Task.FromResult(false);
            Hash = passwordHash;
            return Task.FromResult(true);
        }
    }

    private sealed class FakeTokenValidator : ISetupTokenValidator
    {
        public string Token { get; set; } = "setup-token";
        public int Invalidations { get; private set; }

        public bool TryValidate(string token) => token == Token;

        public void Invalidate()
        {
            Invalidations++;
            Token = "";
        }
    }

    private sealed class FakeTokenService : ITokenService
    {
        public string GenerateToken() => "jwt-token";
        public bool ValidateToken(string token) => token == "jwt-token";
    }

    private static readonly IPasswordHasher Hasher = new Pbkdf2PasswordHasher(1_000);

    [Fact]
    public void PasswordHasher_hashes_and_verifies_with_distinct_salts()
    {
        var first = Hasher.Hash("senha-forte");
        var second = Hasher.Hash("senha-forte");

        Assert.NotEqual(first, second);
        Assert.StartsWith("pbkdf2-sha256$", first);
        Assert.True(Hasher.Verify("senha-forte", first));
        Assert.False(Hasher.Verify("senha-errada", first));
        Assert.False(Hasher.Verify("senha-forte", "formato-invalido"));
    }

    [Fact]
    public void PasswordPolicy_enforces_limits()
    {
        Assert.False(PasswordPolicy.IsValid(new string('a', 7)));
        Assert.True(PasswordPolicy.IsValid(new string('a', 8)));
        Assert.True(PasswordPolicy.IsValid(new string('a', 128)));
        Assert.False(PasswordPolicy.IsValid(new string('a', 129)));
        Assert.False(PasswordPolicy.IsValid(null));
    }

    [Fact]
    public async Task Login_uses_sqlite_hash_and_rejects_unknown_password()
    {
        var store = new FakeCredentialStore { Hash = Hasher.Hash("senha-nova") };
        var useCase = new LoginUseCase(new FakeTokenService(), store, Hasher);

        Assert.True((await useCase.TryGenerateAsync("senha-nova")).Success);
        Assert.False((await useCase.TryGenerateAsync("senha-antiga")).Success);
    }

    [Fact]
    public async Task Login_fails_when_setup_is_not_configured()
    {
        var useCase = new LoginUseCase(new FakeTokenService(), new FakeCredentialStore(), Hasher);

        Assert.False((await useCase.TryGenerateAsync("qualquer")).Success);
    }

    [Fact]
    public async Task CompleteSetup_creates_once_and_invalidates_token()
    {
        var store = new FakeCredentialStore();
        var tokens = new FakeTokenValidator();
        var useCase = new CompleteSetupUseCase(store, Hasher, tokens);

        var result = await useCase.ExecuteAsync("setup-token", "senha-forte");

        Assert.Equal(CompleteSetupResult.Success, result);
        Assert.NotNull(store.Hash);
        Assert.Equal(1, tokens.Invalidations);
        Assert.Equal(
            CompleteSetupResult.AlreadyConfigured,
            await useCase.ExecuteAsync("setup-token", "outra-senha"));
    }

    [Fact]
    public async Task CompleteSetup_rejects_invalid_token_and_password()
    {
        var store = new FakeCredentialStore();
        var tokens = new FakeTokenValidator();
        var useCase = new CompleteSetupUseCase(store, Hasher, tokens);

        Assert.Equal(
            CompleteSetupResult.InvalidToken,
            await useCase.ExecuteAsync("token-errado", "senha-forte"));
        Assert.Equal(
            CompleteSetupResult.InvalidPassword,
            await useCase.ExecuteAsync("setup-token", "curta"));
        Assert.Null(store.Hash);
    }

    [Fact]
    public async Task ChangePassword_requires_current_password_and_changes_hash()
    {
        var store = new FakeCredentialStore { Hash = Hasher.Hash("senha-atual") };
        var useCase = new ChangePasswordUseCase(store, Hasher);

        Assert.Equal(
            ChangePasswordResult.WrongCurrentPassword,
            await useCase.ExecuteAsync("senha-errada", "senha-nova-forte"));
        Assert.True(Hasher.Verify("senha-atual", store.Hash!));

        Assert.Equal(
            ChangePasswordResult.Success,
            await useCase.ExecuteAsync("senha-atual", "senha-nova-forte"));
        Assert.True(Hasher.Verify("senha-nova-forte", store.Hash!));
        Assert.False(Hasher.Verify("senha-atual", store.Hash!));
    }

    [Fact]
    public async Task SqliteAuthCredentialStore_creates_once_and_updates()
    {
        var path = Path.Combine(Path.GetTempPath(), $"auth-test-{Guid.NewGuid():N}.db");
        try
        {
            var store = new SqliteAuthCredentialStore($"Data Source={path}");
            using (var conn = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={path}"))
            {
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "CREATE TABLE AuthCredential (UserId TEXT NOT NULL PRIMARY KEY, PasswordHash TEXT NOT NULL, UpdatedAt TEXT NOT NULL DEFAULT (datetime('now')));";
                cmd.ExecuteNonQuery();
            }

            Assert.False(await store.IsConfiguredAsync());
            Assert.True(await store.CreateAsync("hash-1"));
            Assert.False(await store.CreateAsync("hash-2"));
            Assert.Equal("hash-1", await store.GetHashAsync());
            Assert.True(await store.UpdateAsync("hash-3"));
            Assert.Equal("hash-3", await store.GetHashAsync());
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void SetupToken_is_single_use_after_invalidation()
    {
        var previous = Environment.GetEnvironmentVariable("SETUP_TOKEN");
        try
        {
            Environment.SetEnvironmentVariable("SETUP_TOKEN", "token-de-teste");
            var service = new SetupTokenService();
            var token = service.EnsureGenerated();

            Assert.Equal("token-de-teste", token);
            Assert.True(service.TryValidate(token));

            service.Invalidate();

            Assert.False(service.TryValidate(token));
        }
        finally
        {
            Environment.SetEnvironmentVariable("SETUP_TOKEN", previous);
        }
    }
}
