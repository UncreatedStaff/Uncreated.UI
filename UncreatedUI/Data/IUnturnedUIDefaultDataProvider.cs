using Steamworks;

namespace Uncreated.Framework.UI.Data;

public interface IUnturnedUIDefaultDataProvider<out TData> where TData : class, IUnturnedUIData
{
    TData GetDefaults(CSteamID player);
}