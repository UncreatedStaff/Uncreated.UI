[![Latest](https://github.com/UncreatedStaff/Uncreated.UI/actions/workflows/push_validation.yml/badge.svg)](https://github.com/UncreatedStaff/Uncreated.UI/actions/workflows/push_validation.yml) [![NuGet](https://github.com/UncreatedStaff/Uncreated.UI/actions/workflows/dotnet.yml/badge.svg)](https://github.com/UncreatedStaff/Uncreated.UI/actions/workflows/dotnet.yml)

Releases are available from the [NuGet package](https://www.nuget.org/packages/Uncreated.UI/).

# Uncreated.UI
Object oriented abstraction for Unturned's server-side UI API (EffectManager).

This library was built for [Uncreated Warfare](https://github.com/UncreatedStaff/UncreatedWarfare/) but can be used for any projects in correspondence with the GPL-3.0 license.

You must be targeting .NET Standard 2.1 to use this library.
This means the [netstandard.dll](https://github.com/UncreatedStaff/Uncreated.UI/raw/refs/heads/master/Libraries/netstandard.dll) library must be included in your Libraries folder from the correct Unity installation.
This library references [Microsoft.Extensions.Logging.Abstractions](https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions/3.1.0) 3.1.0 (netstandard2.0) and [DanielWillett.ReflectionTools](https://www.nuget.org/packages/DanielWillett.ReflectionTools/3.1.0) 3.1.0 (netstandard2.1) which can be downloaded from NuGet.

# Creating a simple UI
Simple UIs that use only formatting arguments can be defined without creating a new class.
```cs

public static UnturnedUI ExampleUI { get; private set; }

// if you want to support reloading you should dispose it on unload.
protected override void Load()
{
    ExampleUI = new UnturnedUI(new Guid("a695abdc9b0947d9ad0cd45d1b7a9931"));
}
protected override void Unload()
{
    ExampleUI.Dispose();
}
```


# Creating a complex UI
In a dependency-injection environment, this object would be registered as a singleton, otherwise a static field or property would work fine.

All properties and fields (public and private) are registered as UI elements unless they're marked with [Ignore] or are compiler-generated.

`UnturnedUI` implements `IDisposable` which will clean up any event handlers and should be done on plugin unload or by the service container.

Int16 keys are generated automatically and simply increment for each UI created. `keyless` can be set to true to use a key of -1, which allows using `Lifetime` + `Lifetime_Spread` and prevents any further changes of the UI once it's sent. UIs will not respect their lifetime unless they are keyless.

Most 'fire-and-forget' methods like sending a UI or setting text can be invoked from another thread and will be queued to run on the main thread on the next frame. Check the XML docs to be sure.

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

### Late Registration
Sometimes you may have to define an element in the constructor. Since the base constructor does all the element discovery, any other elements have to be manually registered.

```cs
public class ExampleUI : UnturnedUI
{
    public UnturnedLabel Label { get; }

    public ExampleUI() : base(0)
    {
        Label = new UnturnedLabel("Path/To/Element");

        // this also works with enumerables, presets, and patterns.
        this.LateRegisterElement(Label);
    }
}
```


## UI Paths
You may have noticed that in the first example, UI elements are defined using paths. Most UI methods in Unturned allow you to pass a hierarchy path for performance reasons instead of just the name.

Button and text box names still must be unique across all active UIs in the server. This is an Unturned limitation.

UI's can add a base path to which all paths are appended to.

Paths in a UI with a base path can use `~/` to ignore the base path.

Relative path symbols like `../` and `./` can be used but the destination must fall within the UI for obvious reasons.

```cs
[UnturnedUI(BasePath = "Canvas/Image")]
public class ExampleUI : UnturnedUI
{

    // actual path: Canvas/Image/TestLabel
    public UnturnedLabel TestLabel { get; } = new UnturnedLabel("TestLabel");
    
    // actual path: TestElement
    public UnturnedUIElement TestElement { get; } = new UnturnedUIElement("~/TestElement");
    
    // implementation not shown
}
```


## UI Data
Specific data structures can be linked to UI elements and players. This is how the `UnturnedEnumButton`, `UnturnedToggle`, and `UnturnedTextBox` keep track of player data.

The default UI data tracker can be accessed or changed using `UnturnedUIDataSource.Instance { get; set; }`. Most of its functions are not thread-safe. Check XML comments to be sure.

The `Element` can also be `null` so the data is just linked to the UI object itself.

UI data does not persist between player sessions (it is removed on disconnect).

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
Data can be accessed, added, or removed using the static helper functions in `UnturnedUIDataSource`.
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


# Patterns
Create your own data types and fill lists of them automatically using patterns.

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

Instead of creating a `Root` element, you can also just inherit your pattern from one of the following classes:
* `PatternRoot`
* `PatternRoot<TUIElement>`
* `PatternLabelRoot`
* `PatternButtonRoot`
* `PatternTextBoxRoot`
* `PatternImageRoot`

These classes act as a root element but also implement the corresponding interfaces so you can call extension methods such as `IElement.Hide(...)` on them.

## Using Presets
Presets are types who's members are initialized from the constructor. See a list of built-in presets in the `Built in Types > Presets` section of this document.

For a constructor to be considered, it must have only string parameters, such as: `StatePlaceholderTextBox(string path, string? placeholderPath, string? statePath)`.

Any paths after the first parameter which start with `./` or `../` are expected to be taken as relative to the first parameter.
```cs
public class ExampleUI : UnturnedUI
{
    public PlayerListElement[] PlayerListElements { get; }
        = ElementPatterns.CreateArray<PlayerListElement>("PlayerList/Player_{0}", 1, to: 10);
        
    public class PlayerListElement
    {
        // actual path PlayerList/Player_#
        [Pattern(Root = true)]
        public UnturnedUIElement Root { get; set; }

        // actual button path PlayerList/Player_#/Name_#
        // actual label path  PlayerList/Player_#/Name_#/Label
        [Pattern("Name_{0}", PresetPaths = [ "./Label" ])]
        public LabeledButton Name { get; set; }
    }
}
```

## Format Modes

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

## Array Patterns
The ArrayPattern attribute can be used to create multiple nested pattern in a pattern class.

```cs
[ArrayPattern(1, To = 4)]
[Pattern("Item_{0}")]
public ElementType[] Elements { get; set; }
```

Patterns strings (including the AdditionalPath field) can reference the index of the current object by using {1} and the index of the parent object using {2}. Higher numbers will continue marching up the type hierarchy.
This is especially useful for buttons since they need to have unique names.
```cs

public class ServerListUI : UnturnedUI
{
    public readonly ServerList[] ServerLists = ElementPatterns.CreateArray<ServerList>("ServerList_{0}/", 1, to: 10);
        
    public ServerListUI() : base(12345) { }

    public class ServerList : PatternRoot
    {
        [ArrayPattern(1, To = 4)]
        [Pattern("Server_{0}")]
        public ServerInfo[] Servers { get; set; }
    }

    public class ServerInfo : PatternRoot
    {
        [Pattern("SL_{1}_Server_{0}_JoinButton")]
        public UnturnedButton JoinButton { get; set; }
    }
}

```

In the above example, buttons will have a path resembling: `ServerList_10/Server_3/SL_10_Server_3_JoinButton`

# Logging and Dependency Injection
`ILogger` or `ILoggerFactory` instances from `Microsoft.Extensions.Logging` can also be supplied to the first argument of the constructors of all built-in element and preset types.

If a logger isn't supplied, `GlobalLogger.Instance` will be used instead, which is a static property that can be set at any time (but won't be updated on already existing UIs). It defaults to `NullLogger.Instance`;

All UI classes should usually be created as singleton services that will last the lifetime of the plugin.

This is because they use keys that increment each time a UI is created and if they're not singletons will change their key each time.

Also in this way, services can be injected into the parent class (like a configuration that stores the effect ID, a logger for debug logging, etc.) and used in the base constructor.


**Registering UI classes as a transient service can cause a memory leak as elements' user-specific data is stored until either the player logs off or the UI is disposed.**

# Custom element providers
Types can implement `IUnturnedUIElementProvider` to customize how that type is searched for elements. By default they're just searched using the fields and properties in the type.

When the element discovery finds a type implementing this interface, it will call the `EnumerateElements` method instead of searching fields and properties.


# Built in Types
The library comes with some built-in primitive element types and common preset types.

## Primitives

These are concrete classes that represent individual GameObjects.

| Type              | Purpose                                                                            |
| ----------------- | ---------------------------------------------------------------------------------- |
| UnturnedUIElement | The base type for all **primitive** types, any object in a Unity UI.               |
| UnturnedLabel     | Represents a text component in a Unity UI.                                         |
| UnturnedButton    | Represents a clickable button in a Unity UI.                                       |
| UnturnedTextBox   | Represents an input component in a Unity UI. It also derives from UnturnedLabel.\* |
| UnturnedImage     | Represents a web image in a Unity UI.                                              |

\* `UnturnedTextBox` has a property, `UseData` which enables tracking the text players enter so it can be retrieved at any time.

## Presets

These are concrete classes that are made up of a few primitives.

Some classes are just combinations of their simpler counterparts and have no description.

If there are multiple elements in the constructor of the preset, all the paths of the remaining ones can be relative of the first one if `./` or `../` is added to the front of the path.

* A `new LabeledButton("Canvas/ActionButton", "./Label")` will create a Button at `Canvas/ActionButton` and a label at `Canvas/ActionButton/Label`.

* A `new LabeledButton("Canvas/ActionButton", "../Label")` will create a Button at `Canvas/ActionButton` and a label at `Canvas/Label`.

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


## Interfaces

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
