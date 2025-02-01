using Exiled.API.Features.Items;
using Exiled.CustomItems.API.Features;
using ExCustomItem = Exiled.CustomItems.API.Features.CustomItem;

namespace ToolForExiled;

public record struct ItemInformation(ItemTypeSystem ItemSystem, uint ItemId)
{
    public const uint NONE_ID = uint.MaxValue;

    // WTF! YOU DO NOT USE EXILED CUSTOM ITEMS AND YOU WHANT TO ADD YOUR CUSTOM ITEMS MANAGEMENT !!!
    // LUCK

    public ItemInformation() : this(ItemTypeSystem.Vanilla, 0) { }

    public ItemInformation(ItemType itemTypeId) : this(ItemTypeSystem.Vanilla, unchecked((uint)itemTypeId)) { }

    public void GiveItem(Player player, params object[] complementaryInfo)
    {
        try
        {
            if (!player.IsValid()) return;

            var extractor = new Extractor<object>();

            switch (ItemSystem)
            {
                case ItemTypeSystem.Vanilla:

                    player.AddItem(unchecked((ItemType)ItemId));
                    break;

                case ItemTypeSystem.CustomItemsExiled:
                    if (!ExCustomItem.TryGet(ItemId, out var item))
                    {
                        Log.Warn($"Item {ItemId} not found.");
                        return;
                    }

                    extractor.AddSource(complementaryInfo)
                        .AddExtraction<DisplayMessage>(out var displayMessage, DisplayMessage.No)
                        .Execute();

                    item.Give(player, DisplayMessage.Yes == displayMessage);
                    break;

                // where add you code to give the item...

                default:
                    break;
            }
        }
        catch (Exception)
        {
            Log.Error($@"Exception raised when trying to give an item:
Type {ItemSystem}
Id {ItemId}");
            throw;
        }
    }


    public bool IsValid(Item item, bool? isCustomItem = null)
    {
        if (item == null) return false;

        switch (ItemSystem)
        {
            case ItemTypeSystem.Vanila:

                if (isCustomItem == true)
                    return false;

                if (isCustomItem == null && item.IsCustom())
                    return false;

                if (item.Type < 0) return false;
                return unchecked((uint)item.Type) == ItemId;

            case ItemTypeSystem.CustomItemsExiled when isCustomItem ?? true:
                
                if (CustomItem.TryGet(ItemId, out var customItem) && customItem != null)
                    return customItem.Check(item);
      
                goto default;

            // where add you code to check if the item is use or not...

            default:
                return false;
        }
    }

    enum DisplayMessage
    {
        No = 0,
        Yes = 1
    }
}