using Exiled.API.Features.Items;
using Exiled.CustomModules.API.Features.CustomItems;
using PlayerRoles;
using ExCustomItem = Exiled.CustomModules.API.Features.CustomItems.CustomItem;
using ExItemExtensions = Exiled.CustomModules.API.Extensions.ItemExtensions;

namespace ToolForExiled;

public record struct ItemInformation(ItemTypeSystem ItemSystem, uint ItemId)
{
    public const uint NONE_ID = uint.MaxValue;

    // WTF! YOU DO NOT USE EXILED CUSTOM ITEMS AND YOU WHANT TO ADD YOUR CUSTOM ITEMS MANAGEMENT !!!
    // LUCK

    public ItemInformation() : this(ItemTypeSystem.Vanilla, 0) { }

    public ItemInformation(ItemType itemTypeId) : this(ItemTypeSystem.Vanilla, unchecked((uint)itemTypeId)) { }


    public bool IsValid(Item item, bool? isCustomItem = null)
    {
        if (item == null) return false;

        switch (ItemSystem)
        {
            case ItemTypeSystem.Vanila when !(isCustomItem ?? item.IsCustom()):
                if (item.Type < 0) return false;
                return unchecked((uint)item.Type) == ItemId;

            case ItemTypeSystem.CustomItemsExiled when isCustomItem ?? true:
                if (ExItemExtensions.TryGet(item, out var customItem) && customItem.Id != ItemId)
                    return true;
                goto default;

            // where add you code to check if the item is use or not...

            default:
                return false;
        }
    }

}

public static class CustomItemExtension
{
    // JUSTE EDIT THIS TO SAY IF YES OR NO THE ITEM IS A CUSTOM ITEM.
    public static bool IsCustom(this Item item)
    {
        var hasExile = ExItemExtensions.TryGet(item, out _);
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

public enum ItemTypeSystem
{
    Vanila,
    // lol i forget an l
    Vanilla = Vanila,
    CustomItemExiled,
    // lol i forget an s
    CustomItemsExiled = CustomItemExiled,

    // the system that you use
}