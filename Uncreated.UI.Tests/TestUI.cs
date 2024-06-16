using SDG.Unturned;
using Uncreated.Framework.UI;
using Uncreated.Framework.UI.Patterns;
using Uncreated.Framework.UI.Presets;

namespace Uncreated.UI.Tests;

public class TestUI : UnturnedUI
{
    public readonly UnturnedLabel Label = new UnturnedLabel("test/with/path");
    public readonly TestGroup SingleGroup = ElementPatterns.Create<TestGroup>("single/group/name");
    public readonly TestGroup[] MultipleGroups = ElementPatterns.CreateArray<TestGroup>("path/with/{0}/index/test_{1}_group_{0}", 1, to: 3);
    public readonly NestedArray[] NestedArrayGroup = ElementPatterns.CreateArray<NestedArray>("no_path_{0}", 1, to: 8);
    public readonly LabeledButton LabeledButton = new LabeledButton("test/path/1/btn", "../btn_label");
#nullable disable
    public readonly FOBListElement[] FOBs = ElementPatterns.CreateArray<FOBListElement>("Canvas/{0}", 0, to: 9);
    public struct FOBListElement
    {
        [Pattern("", AdditionalPath = "../Canvas2/c2/", Mode = FormatMode.Prefix, Root = true)]
        public UnturnedUIElement Root { get; set; }

        [Pattern("N{0}", Mode = FormatMode.Replace)]
        public UnturnedLabel Name { get; set; }

        [Pattern("R{0}", Mode = FormatMode.Replace)]
        public UnturnedLabel Resources { get; set; }
    }
    public class TestGroup
    {
        [Pattern("", Mode = FormatMode.Format, CleanJoin = '_')]
        public UnturnedButton Root { get; set; }

        [Pattern("Type", Mode = FormatMode.Format)]
        public UnturnedLabel Type { get; set; }

        [Pattern("Reputation", Mode = FormatMode.Format)]
        public UnturnedLabel Reputation { get; set; }

        [Pattern("Duration", Mode = FormatMode.Format)]
        public UnturnedLabel Duration { get; set; }

        [Pattern("Icon", Mode = FormatMode.Format)]
        public UnturnedLabel Icon { get; set; }

        [Pattern("Message", Mode = FormatMode.Format)]
        public UnturnedLabel Message { get; set; }

        [Pattern("AdminPfp", Mode = FormatMode.Format)]
        public UnturnedImage AdminProfilePicture { get; set; }

        [Pattern("Admin", Mode = FormatMode.Format)]
        public UnturnedLabel Admin { get; set; }

        [Pattern("Timestamp", Mode = FormatMode.Format)]
        public UnturnedLabel Timestamp { get; set; }

        [Pattern("Group({1})", Mode = FormatMode.Format)]
        public NestedGroup Group2 { get; set; }

        [Pattern("Group({1})", AdditionalPath = "Empty", Mode = FormatMode.Format)]
        public NestedGroup GroupUnder { get; set; }

        [ArrayPattern(1, To = 4)]
        [Pattern("Item_{1}_{0}", AdditionalPath = "NestGroup", Mode = FormatMode.Replace)]
        public NestedArray[] NestedArrayGroup { get; set; }
    }
    public class NestedGroup
    {
        [Pattern("", Mode = FormatMode.Format, CleanJoin = '_')]
        public UnturnedButton Root { get; set; }

        [Pattern("Label", Mode = FormatMode.Format)]
        public UnturnedLabel Label { get; set; }
    }
    public class NestedArray
    {
        [Pattern("", Mode = FormatMode.Format, CleanJoin = '_')]
        public UnturnedButton Root { get; set; }

        [Pattern("Label", Mode = FormatMode.Format)]
        public UnturnedLabel Label { get; set; }

        [Pattern("_ThisGoesAfter", Mode = FormatMode.Suffix)]
        public UnturnedLabel LabelAfter { get; set; }

        [Pattern("ThisGoesBefore_", Mode = FormatMode.Prefix)]
        public UnturnedLabel LabelPrefix { get; set; }
    }
#nullable restore

    public TestUI() : base(0, debugLogging: true)
    {
        //ElementPatterns.SubscribeAll(MultipleGroups, group => group.Root, OnRootPressed);
    }
    private void OnRootPressed(UnturnedButton button, Player player)
    {
        
    }
}
