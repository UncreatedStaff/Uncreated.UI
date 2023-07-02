using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Steamworks;

namespace Uncreated.Framework.UI.Data;

public interface IUnturnedUIData
{
    CSteamID Player { get; }
    UnturnedUI Owner { get; }
    UnturnedUIElement Element { get; }
}