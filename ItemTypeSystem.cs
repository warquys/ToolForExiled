#if NEW_EXILED
using Exiled.CustomModules.API.Features.CustomItems;
using ExCustomItem = Exiled.CustomModules.API.Features.CustomItems.CustomItem;
using ExItemExtensions = Exiled.CustomModules.API.Extensions.ItemExtensions;
#else
#endif

namespace ToolForExiled;

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