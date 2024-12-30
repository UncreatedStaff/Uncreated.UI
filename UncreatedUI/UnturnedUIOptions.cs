using System;

namespace Uncreated.Framework.UI;

[Flags]
public enum UnturnedUIOptions
{
    // bool hasElements, bool keyless, bool reliable, bool debugLogging, bool staticKey

    HasElements = 1 << 0,
    Keyless = 1 << 1,
    Reliable = 1 << 2,
    DebugLogging = 1 << 3,
    StaticKey = 1 << 4,

    Default = HasElements | Reliable
}
