# Fix Summary: LoginUseCase Compilation Errors

## Errors Fixed

The following errors from `PlexAutoplay.ArchitectureTests` were resolved:

```
CS1729: 'LoginUseCase' does not contain a constructor that takes 3 arguments
CS1061: 'LoginUseCase' does not contain a definition for 'TryGenerateAsync'
```

## Changes Made

### 1. `src/PlexAutoplay.Application/UseCases/LoginUseCase.cs`
- Changed constructor from `(ITokenService, string adminPassword)` to `(ITokenService, IAuthCredentialStore, IPasswordHasher)`
- Added `LoginResult` class with `Success` and `Token` properties
- Replaced `TryGenerate(string)` → `TryGenerateAsync(string)` returning `Task<LoginResult>`

### 2. `src/PlexAutoplay.Web/Program.cs`
- Updated DI registration to inject `IAuthCredentialStore` and `IPasswordHasher` instead of `adminPassword`

### 3. `src/PlexAutoplay.Web/Endpoints/AuthEndpoints.cs`
- Marked lambda as `async`
- Uses `await loginUseCase.TryGenerateAsync(password)` 
- Accesses `result.Success` and `result.Token` instead of deconstructing old tuple

## Result
- ✅ Build succeeds (0 errors, 4 warnings)
- ✅ All 12 ArchitectureTests pass
- ✅ Login/auth flow works with proper credential store and password hasher