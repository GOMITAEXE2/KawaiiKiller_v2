# Flujo de Datos Tienda-Inventario-Jugador

## Estado de Tienda
1. `ShopAccessTrigger` detecta entrada/salida del jugador en zona válida.
2. `ShopPanelController` mantiene estado `ClosedState`/`OpenState`.
3. Al abrir: pausa tiempo, desbloquea cursor, bloquea input del jugador, refresca tienda e inventario.
4. Al cerrar: reanuda tiempo, bloquea cursor, restaura input del jugador.

## Generación de Tienda
1. `ShopDatabaseSO` expone pools de `WeaponDataSO` y `ModifierDataSO`.
2. `ShopManager.GenerateShopItems()` limpia `shopGrid`.
3. Instancia `ShopItemUI` con datos y precio desde `IStorable`.
4. `AttemptReroll()` descuenta costo en `PlayerEconomy` y regenera listado.

## Compra de Armas
1. Click izquierdo en `ShopItemUI` de tipo arma.
2. `ShopPanelController.AttemptDirectWeaponPurchase` delega a `PlayerInventoryPanelUI`.
3. `PlayerInventoryPanelUI` usa `ShopInventoryService.TryBuyWeapon`.
4. `ShopInventoryService` valida economía e inserta `WeaponInstance` en `PlayerWeaponController`.
5. UI de inventario se reconstruye y el item visual se consume.

## Compra de Mejoras
1. Drag de `ShopItemUI` (modificador) hacia `InventorySlotUI`.
2. `InventorySlotUI.OnDrop` valida slot vacío, tipo compatible y recursos.
3. `WeaponInstance.TryEquipModifier` equipa y recalcula estadísticas.
4. `InventorySlotUI` refresca icono del slot y elimina el item de tienda.

## Venta de Mejoras
1. Drag de `InventorySlotUI` hacia `SellZoneUI`.
2. `SellZoneUI` calcula valor de venta por multiplicador.
3. `PlayerEconomy.AddMoney` acredita recursos.
4. `InventorySlotUI.ConsumeEquippedForSale` desequipa y fuerza recálculo.

## Venta de Armas
1. Click en botón de venta dentro de `WeaponInventoryEntryUI`.
2. `PlayerInventoryPanelUI` delega en `ShopInventoryService.TrySellWeapon`.
3. `PlayerWeaponController.TryRemoveWeaponAt` elimina arma.
4. Si inventario queda vacío, `PlayerWeaponController` inserta arma fallback `fistsWeapon`.
5. UI se reconstruye para reflejar selección y slots del arma activa.

## Integración de Slots
1. `PlayerInventoryPanelUI` lista armas en panel lateral con scroll vertical.
2. Cada `WeaponInventoryEntryUI` pinta nombre, precio, icono y estado de slots.
3. Al seleccionar arma, `PlayerWeaponController.SelectWeaponByIndex` cambia arma activa.
4. `InventorySlotUI` se vuelve a enlazar al `WeaponInstance` actual.
