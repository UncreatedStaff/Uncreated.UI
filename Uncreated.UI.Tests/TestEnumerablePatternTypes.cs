using System.Collections;
using Uncreated.Framework.UI;
using Uncreated.Framework.UI.Patterns;
using Uncreated.Framework.UI.Reflection;

namespace Uncreated.UI.Tests;

public class TestEnumerablePatternTypes
{
    private static readonly Guid TestGuid = new Guid("b3f526c6-dfd9-427f-ba91-5b76925d4236");

    [SetUp]
    public void Setup()
    {
        TestUIProvider.Setup();
    }

    [Test]
    public void PathsCorrect()
    {
        TestUI ui = new TestUI();

        Assert.That(ui.Pattern.ArrayNull.Length, Is.EqualTo(5));
        Assert.That(ui.Pattern.ArrayNotNull.Length, Is.EqualTo(6));

        Assert.That(ui.Pattern.ListNull.Count, Is.EqualTo(5));
        Assert.That(ui.Pattern.ListNotNull.Count, Is.EqualTo(6));

        Assert.That(ui.Pattern.ArrayListNull.Count, Is.EqualTo(5));
        Assert.That(ui.Pattern.ArrayListNotNull.Count, Is.EqualTo(6));

        int ct1 = 0;
        IEnumerator enumerator = ui.Pattern.IEnumerableNull.GetEnumerator();
        while (enumerator.MoveNext()) ++ct1;
        if (enumerator is IDisposable d) d.Dispose();
        int ct2 = 0;
        enumerator = ui.Pattern.IEnumerableNotNull.GetEnumerator();
        while (enumerator.MoveNext()) ++ct2;
        if (enumerator is IDisposable d2) d2.Dispose();

        Assert.That(ct1, Is.EqualTo(5));
        Assert.That(ct2, Is.EqualTo(6));

        Assert.That(ui.Pattern.ICollectionNull.Count, Is.EqualTo(5));
        Assert.That(ui.Pattern.ICollectionNotNull.Count, Is.EqualTo(6));
        Assert.That(ui.Pattern.TestICollectionNotNull.Count, Is.EqualTo(6));

        Assert.That(ui.Pattern.IListNull.Count, Is.EqualTo(5));
        Assert.That(ui.Pattern.IListNotNull.Count, Is.EqualTo(6));

        Assert.That(ui.Pattern.IGenericListNull.Count, Is.EqualTo(5));
        Assert.That(ui.Pattern.IGenericListNotNull.Count, Is.EqualTo(6));

        Assert.That(ui.Pattern.IReadOnlyCollectionNull.Count, Is.EqualTo(5));
        Assert.That(ui.Pattern.IReadOnlyCollectionNotNull.Count, Is.EqualTo(6));

        Assert.That(ui.Pattern.IReadOnlyListNull.Count, Is.EqualTo(5));
        Assert.That(ui.Pattern.IReadOnlyListNotNull.Count, Is.EqualTo(6));

        Assert.Pass();
    }

    [UnturnedUI(BasePath = "Base/Path")]
    public class TestUI : UnturnedUI
    {
        public readonly Pattern1 Pattern = ElementPatterns.Create<Pattern1>("P1s/Pattern");

        public TestUI() : base(TestGuid) { }

#nullable disable
        public class Pattern1 : PatternRoot
        {
            [Pattern("P2s/Pattern2_Single")]
            public Pattern2 Pattern = new Pattern2();

            public Pattern1()
            {
                ArrayNotNull[^1] = new Pattern2();
            }
            
            [ArrayPattern(1, To = 5)]
            [Pattern("P2s/Array_Null{0}")]
            public Pattern2[] ArrayNull { get; set; }
            
            [ArrayPattern(1, To = 5)]
            [Pattern("P2s/Array_Not_Null{0}")]
            public Pattern2[] ArrayNotNull { get; set; } = new Pattern2[6];
            
            [ArrayPattern(1, To = 5)]
            [Pattern("P2s/List_Null{0}")]
            public List<Pattern2> ListNull { get; set; }
            
            [ArrayPattern(1, To = 5)]
            [Pattern("P2s/List_Not_Null{0}")]
            public List<Pattern2> ListNotNull { get; set; } = new List<Pattern2> { new Pattern2() };
            
            [ArrayPattern(1, To = 5)]
            [Pattern("P2s/ArrayList_Null{0}")]
            public ArrayList ArrayListNull { get; set; }
            
            [ArrayPattern(1, To = 5)]
            [Pattern("P2s/ArrayList_Not_Null{0}")]
            public ArrayList ArrayListNotNull { get; set; } = new ArrayList { new UnturnedUIElement("test") };
            
            [ArrayPattern(1, To = 5)]
            [Pattern("P2s/IEnumerable_Null{0}")]
            public IEnumerable IEnumerableNull { get; set; }
            
            [ArrayPattern(1, To = 5)]
            [Pattern("P2s/IEnumerable_Not_Null{0}")]
            public IEnumerable IEnumerableNotNull { get; set; } = new List<UnturnedUIElement> { new UnturnedUIElement("test") };
            
            [ArrayPattern(1, To = 5)]
            [Pattern("P2s/ICollection_Null{0}")]
            public ICollection ICollectionNull { get; set; }
            
            [ArrayPattern(1, To = 5)]
            [Pattern("P2s/ICollection_Not_Null{0}")]
            public ICollection ICollectionNotNull { get; set; } = new List<UnturnedUIElement> { new UnturnedUIElement("test") };
            
            [ArrayPattern(1, To = 5)]
            [Pattern("P2s/TestICollection_Not_Null{0}")]
            public ICollection TestICollectionNotNull { get; set; } = new TestCollection { new UnturnedUIElement("test") };
            
            [ArrayPattern(1, To = 5)]
            [Pattern("P2s/IList_Null{0}")]
            public IList IListNull { get; set; }
            
            [ArrayPattern(1, To = 5)]
            [Pattern("P2s/IList_Not_Null{0}")]
            public IList IListNotNull { get; set; } = new List<UnturnedUIElement> { new UnturnedUIElement("test") };
            
            [ArrayPattern(1, To = 5)]
            [Pattern("P2s/IGenericList_Null{0}")]
            public IList<UnturnedUIElement> IGenericListNull { get; set; }
            
            [ArrayPattern(1, To = 5)]
            [Pattern("P2s/IGenericList_Not_Null{0}")]
            public IList<UnturnedUIElement> IGenericListNotNull { get; set; } = new List<UnturnedUIElement> { new UnturnedUIElement("test") };
            
            [ArrayPattern(1, To = 5)]
            [Pattern("P2s/IReadOnlyCollection_Null{0}")]
            public IReadOnlyCollection<UnturnedUIElement> IReadOnlyCollectionNull { get; set; }
            
            [ArrayPattern(1, To = 5)]
            [Pattern("P2s/IReadOnlyCollection_Not_Null{0}")]
            public IReadOnlyCollection<UnturnedUIElement> IReadOnlyCollectionNotNull { get; set; } = new List<UnturnedUIElement> { new UnturnedUIElement("test") };
            
            [ArrayPattern(1, To = 5)]
            [Pattern("P2s/IReadOnlyList_Null{0}")]
            public IReadOnlyList<UnturnedUIElement> IReadOnlyListNull { get; set; }
            
            [ArrayPattern(1, To = 5)]
            [Pattern("P2s/IReadOnlyList_Not_Null{0}")]
            public IReadOnlyList<UnturnedUIElement> IReadOnlyListNotNull { get; set; } = new List<UnturnedUIElement> { new UnturnedUIElement("test") };
        }

        public class TestCollection : ICollection<UnturnedUIElement>, ICollection
        {
            public List<UnturnedUIElement> List = new List<UnturnedUIElement>();
            public IEnumerator<UnturnedUIElement> GetEnumerator() => List.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)List).GetEnumerator();
            public void Add(UnturnedUIElement item) => List.Add(item);
            public void Clear() => List.Clear();
            public bool Contains(UnturnedUIElement item) => List.Contains(item);
            public void CopyTo(UnturnedUIElement[] array, int arrayIndex) => List.CopyTo(array, arrayIndex);
            public bool Remove(UnturnedUIElement item) => List.Remove(item);
            public void CopyTo(Array array, int index) => ((ICollection)List).CopyTo(array, index);
            public int Count => List.Count;
            public bool IsSynchronized => ((ICollection)List).IsSynchronized;
            public object SyncRoot => ((ICollection)List).SyncRoot;
            public bool IsReadOnly => ((ICollection<UnturnedUIElement>)List).IsReadOnly;
        }

        public class Pattern2 : PatternRoot
        {
            [ArrayPattern(1, To = 6)]
            [Pattern("Pattern3_{0}/Value")]
            public UnturnedLabel[] Labels { get; set; }
        }
#nullable restore
    }
}