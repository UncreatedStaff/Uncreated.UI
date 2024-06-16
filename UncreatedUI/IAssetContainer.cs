using SDG.Unturned;
using System;

namespace Uncreated.Framework.UI;
public interface IAssetContainer
{
    Guid Guid { get; }
    ushort Id { get; }
    Asset? Asset { get; }
}
