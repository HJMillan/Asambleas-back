namespace Asambleas.Features.Auth;

// ── Requests ──

public record LoginCuilRequest(string Cuil, string Password);

public record LoginLdapRequest(string Username, string Password);

public record RegisterRequest(
    string Cuil,
    string Nombre,
    string Apellido,
    string Email,
    string Password,
    string ConfirmPassword
);

// ── Responses ──

public record AuthResponse(
    UserInfoResponse User,
    string Message
);

public record UserInfoResponse(
    Guid Id,
    string Dni,
    string Cuil,
    string Nombre,
    string Apellido,
    string Email,
    string Role,
    bool IsDomelecVerified
);
