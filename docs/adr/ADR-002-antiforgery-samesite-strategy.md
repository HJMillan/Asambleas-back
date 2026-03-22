# ADR-002: Anti-Forgery — SameSite Cookie Strategy

## Estado: Aceptada

## Fecha: 2026-03-21

## Contexto
El proyecto usa JWT almacenado en cookies HTTP-only con `SameSite=Strict`. Se configuró
Anti-Forgery (`AddAntiforgery`) pero no se aplica `[ValidateAntiForgeryToken]` en ningún
endpoint. Se debatió si es necesario dado el esquema de cookies.

## Decisión
**Se remueve Anti-Forgery del pipeline.** La protección CSRF se logra mediante las cookies
con `SameSite=Strict`.

### Razones:
1. **SameSite=Strict es suficiente**: El navegador NO envía cookies Strict en requests
   cross-origin, lo que previene CSRF por diseño
2. **API REST pura**: No se sirven páginas HTML desde el backend; el frontend es una SPA
   separada que se comunica via API JSON
3. **Anti-Forgery agrega complejidad innecesaria**: Requiere que el frontend obtenga y
   envíe el token XSRF en cada request mutante, lo cual es redundante con SameSite
4. **Doble protección de cookie**: HttpOnly + Secure + SameSite=Strict ya cubren los
   vectores de ataque CSRF

## Consecuencias
- (+) Simplifica el frontend (no necesita manejar tokens XSRF)
- (+) Menos código y configuración
- (+) Las cookies Strict ya previenen CSRF en navegadores modernos
- (-) Navegadores muy antiguos sin soporte SameSite quedarían desprotegidos (riesgo aceptable)

## Referencias
- [OWASP SameSite Cookie Attribute](https://owasp.org/www-community/SameSite)
- [MDN SameSite cookies](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Set-Cookie/SameSite)
