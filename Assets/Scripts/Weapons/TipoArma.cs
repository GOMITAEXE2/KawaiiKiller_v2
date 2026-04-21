using System;

namespace KawaiiKiller.Weapons
{
    [Flags]
    public enum TipoArma
    {
        Pistol = 1 << 0,
        Rifle = 1 << 1,
        Shotgun = 1 << 2,
        Sniper = 1 << 3,
        RPG = 1 << 4,
        Special = 1 << 5
    }
}
