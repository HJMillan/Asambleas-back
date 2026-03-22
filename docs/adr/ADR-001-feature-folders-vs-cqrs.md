# ADR-001: Feature Folders vs CQRS/MediatR

## Estado: Aceptada

## Fecha: 2026-03-21

## Contexto
Se evaluó implementar CQRS con MediatR pipeline behaviors como patrón arquitectónico.
El proyecto Asambleas actualmente tiene:
- 1 controller (AuthController)
- 1 servicio de negocio (AuthService)
- ~14 entidades de dominio sin lógica de negocio implementada aún
- Equipo pequeño

## Decisión
**NO se implementa MediatR/CQRS en esta etapa.** Se mantiene Feature Folders con servicios inyectados.

### Razones:
1. **Tamaño del proyecto**: Con un solo controller/servicio, MediatR agrega complejidad sin beneficio real
2. **Feature Folders cumple el objetivo**: La separación por features (Auth, Asambleas, Vacantes, etc.) ya provee la modularidad necesaria
3. **Inversión de dependencias lograda**: Se implementan interfaces (`IAuthService`, `IUserRepository`, `IRefreshTokenRepository`) que logran el mismo desacoplamiento
4. **Pipeline behaviors reemplazados**: `ValidationFilter` (IAsyncActionFilter) cumple el rol del ValidationBehavior de MediatR
5. **Overhead operacional**: MediatR agrega una capa de indirección que dificulta la navegación del código para equipos pequeños

## Criterio de re-evaluación
Considerar MediatR cuando:
- Haya más de 5 controllers con lógica compleja
- Se necesiten cross-cutting concerns más sofisticados (caching per-query, authorization per-handler)
- Aparezcan domain events que requieran pub/sub
- El equipo crezca y la trazabilidad pipeline sea más valiosa que la simplicidad

## Consecuencias
- (+) Código más simple y directo
- (+) Menor curva de aprendizaje
- (-) Sin pipeline behaviors nativos (compensado con ActionFilters)
- (-) Sin separación explícita Command/Query (compensado con métodos claramente nombrados)
