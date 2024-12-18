﻿using SDG.Unturned;
using Uncreated.Framework.UI;
using Uncreated.Framework.UI.Patterns;
using Uncreated.Framework.UI.Presets;
using Uncreated.Framework.UI.Reflection;

namespace Uncreated.UI.Tests;

[UnturnedUI(BasePath = "Base")]
public class TestUI : UnturnedUI
{
    //public readonly UnturnedLabel Label;// = new UnturnedLabel("~/test/with/path");
    //public readonly TestGroup SingleGroup = ElementPatterns.Create<TestGroup>("./single/group/name");
    //public readonly TestGroup[] MultipleGroups = ElementPatterns.CreateArray<TestGroup>("path/with/{0}/index/test_{1}_group_{0}", 1, to: 3);
    //public readonly TestGroup[] MultipleGroups2 = ElementPatterns.CreateArray<TestGroup>("path/with/{0}/index/test_{1}", 1, to: 3);
    //public readonly NestedArray[] NestedArrayGroup = ElementPatterns.CreateArray<NestedArray>("no_path_{0}", 1, to: 8);
    //public readonly LabeledButton LabeledButton = new LabeledButton("~/test/path/1/btn", "./lbl");
#nullable disable
    //public readonly FOBListElement[] FOBs = ElementPatterns.CreateArray<FOBListElement>("Canvas/FOB_Item_{0}", 0, to: 9);
    //public TestClass[] Elements { get; }

    public LabelTest[] PresetLabelTests = ElementPatterns.CreateArray<LabelTest>("A/B/C/Test_{0}_{1}_Value", 0, to: 2);

    public TestUI() : base(0, debugLogging: true)
    {
        //Elements = ElementPatterns.CreateArray<TestClass>("Test/{0}/Class_{1}", 1, to: 10);
        //
        //LateRegisterElement(Label = new UnturnedLabel("test"));
        //LateRegisterElement(Elements);
    }

    public class LabelTest
    {
        [Pattern("", CleanJoin = '_', Root = true)]
        public UnturnedUIElement Root { get; set; }

        [Pattern("ButtonNoArgs", Mode = FormatMode.Format)]
        public LabeledRightClickableStateButton ButtonNoArgs { get; set; }

        [Pattern("ButtonLessArgs", Mode = FormatMode.Format, PresetPaths = [ "./Label", "ButtonLessArgs/RClickListener", "../State" ])]
        public LabeledRightClickableStateButton RelativeButtonLessArgs { get; set; }

        [ArrayPattern(0, To = 3)]
        [Pattern("Btn{0}", Mode = FormatMode.Format, PresetPaths = [ "./{0}_l", "ll/{0}_r", "../{0}_s"])]
        public LabeledRightClickableStateButton[] ArrayTest { get; set; }
    }


    #region hide
    public class TestClass
    {
        // marking an element as Root will put all other objects below this one. Only one object can be a root.

        // actual path Test/#/Class
        //   setting the CleanJoin character fixes the extra underscore from formatting in an empty string
        [Pattern("", Mode = FormatMode.Format, CleanJoin = '_')]
        public UnturnedUIElement Root { get; set; }

        // actual path Test/#/Class_Name
        [Pattern("Name", Mode = FormatMode.Format)]
        public UnturnedLabel Name { get; set; }

        // actual path Test/#/Class_SteamID
        [Pattern("SteamID", Mode = FormatMode.Format)]
        public UnturnedLabel SteamId { get; set; }
    }
    public struct FOBListElement
    {
        [Pattern(Root = true)]
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
    #endregion
#nullable restore

    private void OnRootPressed(UnturnedButton button, Player player)
    {
        
    }
}
