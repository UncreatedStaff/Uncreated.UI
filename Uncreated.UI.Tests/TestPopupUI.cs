using System;
using SDG.Unturned;
using Uncreated.Framework.UI;
using Uncreated.Framework.UI.Patterns;
using Uncreated.Framework.UI.Presets;
using Uncreated.Framework.UI.Reflection;

namespace Uncreated.UI.Tests;

[UnturnedUI(BasePath = "Container/MainBox")]
public class TestPopupUI : UnturnedUI
{
    public LabeledStateButton[] Buttons { get; } =
    [
        new LabeledStateButton("ButtonContainer/Button1", "./Button1Label", "./Button1State"),
        new LabeledStateButton("ButtonContainer/Button2", "./Button2Label", "./Button2State"),
        new LabeledStateButton("ButtonContainer/Button3", "./Button3Label", "./Button3State"),
        new LabeledStateButton("ButtonContainer/Button4", "./Button4Label", "./Button4State")
    ];

    public UnturnedImage Image { get; } = new UnturnedImage("Image");
    public TestPopupUI() : base((EffectAsset?)null)
    {
        ElementPatterns.SubscribeAll(Buttons, OnButtonClicked);
    }

    private void OnButtonClicked(UnturnedButton button, Player player)
    {
        throw new NotImplementedException();
    }
}