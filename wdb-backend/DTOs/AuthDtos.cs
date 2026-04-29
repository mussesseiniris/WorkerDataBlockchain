namespace wdb_backend.DTOs;

public record RegisterRequest(string Email, string UserName, string Password);

public record LoginRequest(string Email, string Password);

public record AuthResult(string AccessToken, string UserName, string Email, Guid UserId);
