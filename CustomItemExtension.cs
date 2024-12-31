using Exiled.API.Features.Items;
using Exiled.CustomItems.API.Features;

namespace ToolForExiled;

public static class CustomItemExtension
{
    // JUSTE EDIT THIS TO SAY IF YES OR NO THE ITEM IS A CUSTOM ITEM.
    public static bool IsCustom(this Item item)
    {
        var hasExile = CustomItem.Registered.Any(p => p.Check(item));
      
        if (hasExile) return true;

        return false;
    }

    public static bool IsValid(this IEnumerable<ItemInformation> items, Item item)
    {
        var isCustom = item.IsCustom();
        return items.Any(p => p.IsValid(item, isCustom));
    }

    public static bool IsValid(this IEnumerable<ItemInformation> items, Item item, out ItemInformation itemInfo)
    {
        var isCustom = item.IsCustom();
        itemInfo = items.FirstOrDefault(p => p.IsValid(item, isCustom));
        return itemInfo != default;
    }
}
