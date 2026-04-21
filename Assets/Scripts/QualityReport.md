# Informe de Calidad (Automático)

## Alcance
- Directorio analizado: `Assets/Scripts`
- Fuentes: revisión estática por patrones + ajustes de ensamblados + correcciones de código.

## Métricas
- Errores de compilación (diagnóstico IDE): 0
- Archivos modificados: 3
- Archivos añadidos: 0
- Advertencias corregidas: 1

## Hallazgos y correcciones

### 1) Dependencia prohibida / mala práctica
- Hallazgo: `using Unity.VisualScripting;` presente en `PlayerCharacter.cs`.
- Impacto: dependencia innecesaria y contraria a reglas del proyecto; potencial de conflictos por paquetes.
- Corrección: eliminación de `using` no utilizado.
- Archivo: `Assets/Scripts/Player/PlayerCharacter.cs`

### 2) Rendimiento / GC por frame
- Hallazgo: asignación por disparo de `new List<string>()` dentro de `PlayerWeaponController.FireProjectile`.
- Impacto: presión de GC bajo fuego automático, micro-stutters.
- Corrección: buffer reutilizable `_activatedEffectsBuffer` con `Clear()` por disparo.
- Archivo: `Assets/Scripts/Weapons/PlayerWeaponController.cs`

### 3) Seguridad lógica (economía)
- Hallazgo: `AddMoney(int amount)` aceptaba valores negativos (inyección de saldo por venta negativa / scripts externos).
- Impacto: inconsistencias de economía por entrada inválida.
- Corrección: guard clause `if (amount <= 0) return;`
- Archivo: `Assets/Scripts/Player/PlayerEconomy.cs`

## Estado final
- Compilación (diagnóstico IDE): OK
