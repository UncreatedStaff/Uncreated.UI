using Uncreated.Framework.UI;
using Uncreated.Framework.UI.Reflection;

namespace Uncreated.UI.Tests;
[UnturnedUI("Test")]
public class TestUI : UnturnedUI
{
    public readonly UnturnedUIElement Element = new UnturnedUIElement("Element");
    
    public readonly TestStruct2 Element1 = new TestStruct2
    {
        Value = new UnturnedTextBox("Element1_Value"),
        Label = new UnturnedLabel[] { new UnturnedLabel("Element1_Label"), new UnturnedLabel("Element1_Label2") }
    };
    
    public readonly TestStruct2[] Pattern = UnturnedUIPatterns.CreateArray<TestStruct2>("Pattern_{0}", 10, 4);
    public readonly UnturnedImage[] Pattern2 = UnturnedUIPatterns.CreateArray<UnturnedImage>("Image_{0}", 4, 2);
    public readonly TestStruct1[] ArrayPattern = UnturnedUIPatterns.CreateArray<TestStruct1>("Array{1}_{0}", 2, 3);
    public TestUI() : base(1) { }

    public struct TestStruct1
    {
        [UIPattern("_Labels_{0}", Mode = FormatMode.Format, FormatIndex = 1)]
        [UIPatternArray(4, To = 6)]
        public TestStruct2[] Label { get; set; }

        [UIPattern("_Value", Mode = FormatMode.Format, FormatIndex = 1)]
        public UnturnedTextBox Value { get; set; }
    }
    public struct TestStruct2
    {
        [UIPattern("_Labels_{0}", Mode = FormatMode.Suffix)]
        [UIPatternArray(10, To = 12)]
        public UnturnedLabel[] Label { get; set; }

        [UIPattern("_TextBox", Mode = FormatMode.Suffix)]
        public UnturnedTextBox Value { get; set; }
    }
}
