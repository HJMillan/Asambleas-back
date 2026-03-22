namespace Asambleas.Features.Auth;

// ── Requests ──

/// <summary>
/// Request de login por CUIL + contraseña.
/// </summary>
/// <param name="Cuil">CUIL del usuario (11 dígitos numéricos).</param>
/// <param name="Password">Contraseña del usuario.</param>
public record LoginCuilRequest(string Cuil, string Password);

/// <summary>
/// Request de login por LDAP (usuario de red).
/// </summary>
/// <param name="Username">Nombre de usuario de red.</param>
/// <param name="Password">Contraseña de red.</param>
public record LoginLdapRequest(string Username, string Password);

/// <summary>
/// Request de registro de nuevo usuario docente.
/// </summary>
/// <param name="Cuil">CUIL del docente (11 dígitos numéricos).</param>
/// <param name="Nombre">Nombre de pila.</param>
/// <param name="Apellido">Apellido.</param>
/// <param name="Email">Email institucional o personal.</param>
/// <param name="Password">Contraseña (min 8 chars, mayúscula, minúscula, dígito, especial).</param>
/// <param name="ConfirmPassword">Confirmación de contraseña (debe coincidir).</param>
public record RegisterRequest(
    string Cuil,
    string Nombre,
    string Apellido,
    string Email,
    string Password,
    string ConfirmPassword
);

// ── Responses ──

/// <summary>
/// Respuesta estándar de autenticación con datos del usuario y mensaje.
/// </summary>
/// <param name="User">Datos públicos del usuario autenticado.</param>
/// <param name="Message">Mensaje descriptivo del resultado de la operación.</param>
public record AuthResponse(
    UserInfoResponse User,
    string Message
);

/// <summary>
/// Datos públicos del usuario autenticado.
/// No expone datos sensibles como PasswordHash o tokens.
/// </summary>
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
