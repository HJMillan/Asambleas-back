# ADR-003: Design Decisions & Known Limitations

## Fecha: 2026-03-21

Este documento recopila decisiones de diseño menores que no ameritan un ADR individual.

---

### #15 — GenerateTokensAsync: Query separada para tokens activos
La generación de tokens ejecuta una query adicional para verificar el límite de tokens
activos por usuario (`MaxActiveRefreshTokens = 5`). Esto es aceptable porque:
- Se ejecuta **una vez por login/refresh**, no en loop
- La query está indexada (userId, IsRevoked, ExpiresAt via FK index)
- Intentar resolver esto con Include generaría materialización innecesaria de todos los tokens

**Monitorear** si el volumen de tokens por usuario crece significativamente.

---

### #19 — Dominio: Entidades sin lógica de negocio
Las features Vacantes, Designaciones, Listados, DeclaracionJurada y Docentes actualmente
solo contienen entidades (modelos de dominio). No tienen controllers, services, DTOs ni
validators porque **el módulo de autenticación se priorizó primero**.

Los controllers y servicios para estas features se implementarán en las siguientes iteraciones.
La estructura de Feature Folders ya está preparada para recibirlos.

---

### #20 — TokenCleanupService: Hard delete intencional
`TokenCleanupService` usa `ExecuteDeleteAsync` (hard delete) en lugar del soft delete
definido en `BaseEntity`. Esto es **intencional** porque:
- Los refresh tokens revocados y expirados no tienen valor de auditoría
- La tabla `refresh_tokens` crecería indefinidamente con soft delete
- La información de auditoría ya se captura en `AuditLogEntry` via `AuditInterceptor`
- El cleanup elimina tokens revocados **o** expirados hace más de 30 días

---

### #21 — LDAP Stub: CUIL hardcodeado
`LdapServiceStub` retorna un CUIL fijo (`20123456789`) para todos los usuarios.
Esto **causará colisiones** si dos usuarios LDAP intentan hacer login en desarrollo.

**Workaround**: En desarrollo, cambiar el stub para generar CUILs basados en el username:
```csharp
Cuil = $"20{username.GetHashCode():D8}0".Substring(0, 11)
```

Esto se resolverá automáticamente cuando se implemente el servicio LDAP real.
