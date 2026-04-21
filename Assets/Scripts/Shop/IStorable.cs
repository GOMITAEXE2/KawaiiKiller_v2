using UnityEngine;

namespace KawaiiKiller.UI.Shop
{
    public interface IStorable
    {
        Sprite Icon { get; }
        int Price { get; }
    }
}
