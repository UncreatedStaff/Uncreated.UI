[![Latest](https://github.com/UncreatedStaff/Uncreated.UI/actions/workflows/push_validation.yml/badge.svg)](https://github.com/UncreatedStaff/Uncreated.UI/actions/workflows/push_validation.yml) [![NuGet](https://github.com/UncreatedStaff/Uncreated.UI/actions/workflows/dotnet.yml/badge.svg)](https://github.com/UncreatedStaff/Uncreated.UI/actions/workflows/dotnet.yml)

Object oriented abstraction for Unturned's server-side UI API (EffectManager).

You must be building using .NET Standard 2.1 to use this library.


# Creating a complex UI
In a dependency-injection environment, this object would be registered as a singleton, otherwise a static readonly field or property would work fine.

All properties and fields (public and private) are registered as UI elements unless they're marked with [Ignore].

`UnturnedUI` implements `IDisposable` which will clean up any event handlers and should be done on plugin unload or by the service container.

Int16 keys are generated automatically and simply increment for each UI created. `keyless` can be set to true to use a key of -1, which allows using `Lifetime` + `Lifetime_Spread` and prevents any further changes of the UI once it's sent. UIs will not respect their lifetime unless they are keyless.

```cs
public class ExampleUI : UnturnedUI
{

    public UnturnedLabel QuestionLabel { get; } = new UnturnedLabel("Canvas/Question");

    public UnturnedButton AcceptButton { get; } = new UnturnedButton("Canvas/Accept");

    public UnturnedButton CancelButton { get; } = new UnturnedButton("Canvas/Cancel");
    
                                                                         // some extra optional configuration options
    public ExampleUI() : base(new Guid("64615dc226f940da8fc356972f45d5f8"), hasElements: true, keyless: false, reliable: true, debugLogging: false)
    {
        AcceptButton.OnClicked += OnAcceptButtonClicked;
        CancelButton.OnClicked += OnCancelButtonClicked;
    }

    public void SendQuestion(Player player, string question)
    {
        // if QuestionLabel's unity text is '{0}'
        this.SendToPlayer(player, question);

        // if it's not, or you want an example to use in a README, you can do it separately
        this.SendToPlayer(player);
        QuestionLabel.SetText(player, question);
    }

    public void OnAcceptButtonClicked(UnturnedButton button, Player player)
    {
        this.ClearFromPlayer(player);
        // implementation not shown
    }

    public void OnCancelButtonClicked(UnturnedButton button, Player player)
    {
        this.ClearFromPlayer(player);
        // implementation not shown
    }
}
```


## UI Paths
You may have noticed that in the last example, UI elements are defined using paths. Most methods in Unturned allow you to pass a hierarchy path for performance reasons instead of just the name.

Button names still must be unique across all active UIs in the server. This is an Unturned limitation.

UI's can add a base path to which all paths are appended to.

Paths in a UI with a base path can use `~` to ignore the base path.

Relative path symbols like `../` and `./` can be used but the destination must fall within the UI for obvious reasons.

```cs
[UnturnedUI(BasePath = "Canvas/Image")]
public class ExampleUI : UnturnedUI
{

    // actual path: "[root]/Canvas/Image/TestLabel"
    public UnturnedLabel TestLabel { get; } = new UnturnedLabel("TestLabel");
    
    // actual path: "[root]/TestElement"
    public UnturnedUIElement TestElement { get; } = new UnturnedUIElement("~/TestElement");
    
    // implementation not shown
}
```


## UI Data
Specific data structures can be linked to UI elements and players. This is how the `UnturnedEnumButton`, `UnturnedToggle`, and `UnturnedTextBox` keep track of player data.

The default UI data tracker can be accessed or changed using `UnturnedUIDataSource.Instance { get; set; }`.

`UnturnedUIDataSource.RemovePlayer(Player)` should be invoked on player disconnect to avoid memory leaks if using UI data.

### Creating Custom Data
```cs
public class CustomUIData : IUnturnedUIData
{
    // Required by the interface
    public CSteamID Player { get; }
    public UnturnedUIElement Element { get; }
    public UnturnedUI Owner => Element.Owner;

    // custom data, can be basically any type
    public string? Data { get; set; }

    public CustomUIData(CSteamID playerId, UnturnedUIElement element, string? data)
    {
        Player = playerId;
        Element = element;
        Data = data;
    }
}
```

### Using Custom Data
Data can be gotten, added, or removed using the static helper functions in `UnturnedUIDataSource`.
```cs
// in a UI class
public void OnSomethingHappened(Player player)
{
    CustomUIData? dataObject = UnturnedUIDataSource.GetData<CustomUIData>(player.channel.owner.playerID.steamID, this.SomeElement);
    if (dataObject == null)
    {
        dataObject = new CustomUIData(player.channel.owner.playerID.steamID, this.SomeElement, "new data");
        UnturnedUIDataSource.AddData(dataObject);
    }
    else
    {
        // since the data is a class theres no need to save the changes or anything
        dataObject.Data = "something happened again";
    }

    _logger.LogInformation("Player {0}'s data value is now {1}.", player.channel.owner.playerID.playerName, dataObject.Data);
    this.SomeElement.SetText(dataObject.Data);
}
```


## Patterns
Create your own data types and fill lists of them automatically using patterns.

Currently only primitive types and other patterns are supported, none of the built-in presets can be used.

```cs
public class ExampleUI : UnturnedUI
{
    // the {0} in this name will be replaced by the index, in this case 1-10 inclusive. You can also supply a 'length' instead of 'to'.
    public PlayerListElement[] PlayerListElements { get; }
        = ElementPatterns.CreateArray<PlayerListElement>("PlayerList/Player_{0}", 1, to: 10);
        
    public class PlayerListElement
    {
        // marking an element as Root will put all other objects below this one. Only one object can be a root.

        // actual path PlayerList/Player_#
        [Pattern(Root = true)]
        public UnturnedUIElement Root { get; set; }

        // actual path PlayerList/Player_#/Name_#
        [Pattern("Name_{0}")]
        public UnturnedLabel Name { get; set; }

        // actual path PlayerList/Player_#/SteamID_#
        [Pattern("SteamID_{0}")]
        public UnturnedLabel SteamId { get; set; }
    }
}
```

### PatternAttribute

Different format modes can be used for defining how the names are created.

Most examples will use Replace with a root element, which is why replace is default.

Sometimes, Format may be used instead, which will replace `{1}` with the pattern name in the base path.
```cs
public TestClass[] Elements { get; } = ElementPatterns.CreateArray<TestClass>("Test/{0}/Class_{1}", 1, to: 10);
        
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
```

The others available are `Suffix` and `Prefix`.

Suffix simply adds the value after the base path and prefix adds it before.

### ArrayPatternAttribute
The ArrayPattern attribute can be used to create a nested pattern in a pattern class.

```cs
[ArrayPattern(1, To = 4)]
[Pattern(Item_{0})]
public ElementType[] Elements { get; }
```


## Built in Types
The library comes with some built-in primitive element types and common preset types.

### Primitives

These are concrete classes that represent individual GameObjects.

| Type              | Purpose                                                                            |
| ----------------- | ---------------------------------------------------------------------------------- |
| UnturnedUIElement | The base type for all **primitive** types, any object in a Unity UI.               |
| UnturnedLabel     | Represents a text component in a Unity UI.                                         |
| UnturnedButton    | Represents a clickable button in a Unity UI.                                       |
| UnturnedTextBox   | Represents an input component in a Unity UI. It also derives from UnturnedLabel.\* |
| UnturnedImage     | Represents a web image in a Unity UI.                                              |

\* `UnturnedTextBox` has a property, `UseData` which enables tracking text players enter so it can be retrieved at any time.

### Presets

These are concrete classes that are made up of a few primitives.

Some classes are just combinations of their simpler counterparts and have no description.

If there are multiple elements in the constructor of the preset, all the paths of the remaining ones can be relative of the first one if `./` or `../` is added to the front of the path.

> A `new LabeledButton("Canvas/ActionButton", "./Label")` will create a Button at `Canvas/ActionButton` and a label at `Canvas/ActionButton/Label`.

`..` is used to go up one object.

> A `new LabeledButton("Canvas/ActionButton", "../Label")` will create a Button at `Canvas/ActionButton` and a label at `Canvas/Label`.

| Type                             | Purpose                                                                                                                                                                 |
| -------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| LabeledButton                    | A button with a text component.                                                                                                                                         |
| StateButton                      | A button who's interactability can be enabled or disabled.                                                                                                              |
| RightClickableButton             | A button that can be right clicked.\*                                                                                                                                   |
| LabeledStateButton               | -                                                                                                                                                                       |
| RightClickableStateButton        | -                                                                                                                                                                       |
| LabeledRightClickableButton      | -                                                                                                                                                                       |
| LabeledRightClickableStateButton | -                                                                                                                                                                       |
| PlaceholderTextBox               | A text box with a placeholder text label (the greyed out text that shows when the box is empty).                                                                        |
| StateTextBox                     | A text box who's interactability can be enabled or disabled.                                                                                                            |
| StatePlaceholderTextBox          | -                                                                                                                                                                       |
| UnturnedEnumButton               | A button used to toggle between values of an enum, like a lot of the vanilla toggles. These have a `TextFormatter` property to convert enum values into text if needed. |
| UnturnedToggle                   | A button, label, and image that act as a toggle box or check box with a checked and unchecked state.                                                                    |

\* See an example of a right-clickable button from [this package](https://github.com/DanielWillett/UnturnedUIAssets/blob/main/UI/uGUI/DarkTheme/uGUI%20Assets.unitypackage) (check the [README](https://github.com/DanielWillett/UnturnedUIAssets/blob/main/README.md)). Basically a second button acts as the right click event.


### Interfaces

All elements and most presets also implement interfaces so you can create your own presets and use abstracted extension methods (ex. to allow usage of your own player class).

| Type                         | Purpose                                                                                                           |
| ---------------------------- | ----------------------------------------------------------------------------------------------------------------- |
| IElement                     | Base interface for all elements. On presets it usually targets the root or main element (like Button on buttons). |
| IStateElement                | Includes an element for setting the element's interactability.                                                    |
| ILabel                       | Includes an element for setting the element's text.                                                               |
| IButton                      | Includes an element for listening for a click.                                                                    |
| ILabeledButton               | -                                                                                                                 |
| ILabeledRightClickableButton | -                                                                                                                 |
| IRightClickableButton        | -                                                                                                                 |
| IImage                       | Includes an element for sending a web image URL.                                                                  |
| ITextBox                     | Includes an element for listening for text input.                                                                 |
| IPlaceholderTextBox          | Includes an element for listening for text input with a placeholder text label.                                   |
