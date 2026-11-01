# RPGFramework.Core

The centre of an RPG Framework game. Core switches between the game's modules, such as the title screen, the field and
battle, each in a scene of its own. It also owns what every module shares:

- the game's variables and the memory they live in;
- saves and settings;
- input routing and dialogue windows;
- transitions, audio intents and the update loop.

Requires Unity 6000.6 or newer, the Input System, Unity.Mathematics, RPGFramework.Audio (the music a battle pauses is an
Audio snapshot), RPGFramework.DI, RPGFramework.Hashing and RPGFramework.Core.SharedTypes. It works with any render
pipeline: its transitions are drawn with UI Toolkit, though the Subtractive Transition needs URP (below).

---

## Modules

A module is a scene and its `IModule`, which a component in that scene binds. Each has a byte id. Your game supplies two databases:

- an **`IModuleDatabase`**, mapping each id to its module's interface;
- an **`ISceneDatabase`**, mapping each module to its scene.

Start the game from a component in your first scene:

```csharp
ICoreModule core = await CoreModuleFactory.Create(m_GlobalInstaller, SplashScreenModuleId);

await core.StartAsync();
```

`Create` builds the global container, binding Core's services and then your global installer's, and runs its
`Bootstrap`. `StartAsync` enters the first module.

**Modules are a stack.** The bottom one is where the game is — the title, a field — and a module can open over it, as
the menu does over the field: the field stays loaded, suspended, and carries on exactly as it was when the menu closes.
A module finishing tells Core what happened, as a byte its shared types define, and **your module router** decides
what follows:

```csharp
await core.RequestModuleChangeAsync(FieldOutcomes.OPEN_MENU);
```

The router answers with a `ModuleChange`, which Core carries out:

| Change | What happens |
| --- | --- |
| `Over` | the module on top is suspended (`OnSuspendAsync`), and the next one's scene loads beside it |
| `Close` | the module on top exits and its scene unloads; the one under it resumes (`OnResumeAsync`) |
| `Replace` | the module on top exits and its scene unloads; the next one's loads in its place |
| `Clear` | every module exits, top first, and the next one's scene loads alone |

Each loaded module gets a scene container built from its scene's `SceneInstallerMonoBehaviour`, falling back to the
global one, and is resolved from it; `IDIResolver` in the global container is the top module's. A module finds its own
objects through **`IModuleScene`**, bound in its container, rather than searching every scene, since others can be
loaded beside it. A change says how the next module is entered — `ModuleEntries.NEW`, or `RETURN` to one a battle
replaced, which carries on from what it kept — and the module gets it in `OnEnterAsync`.

**Every change happens under a covered screen.** Core covers the screen before it touches a module, carries the
change out, and reveals the module on top, then tells it so with **`OnRevealed`**: that is where a module starts anything
the player should see from its beginning. Routed input reaches modules again as the reveal starts, so a module that
should take none while it plays — a menu — turns its own input on in `OnRevealed`, and one the player may act in as it
appears — a field, walked into as it fades in — turns it on as it enters. `OnRevealed` is not called when the module's
own cover took over the reveal. A module does not fade itself for a change. Changes
run one at a time: one asked for during another waits for it, and is routed from whichever module is then on top.

**A module binds its own types**, from a component in its scene that implements **`IModuleBindings`**, so it is built
fresh on every entry and goes with its scene. Core runs your scene installer first, then the module's bindings, each
only where nothing is bound yet: your scene installer binds what is your game's — a generated database, a config — and
binding one of a module's interfaces there replaces the module's own. **Every module scene needs both**, the
`SceneInstallerMonoBehaviour` with your installer, even one that binds nothing, and the module's component; a scene
missing either throws as the module loads. A module of your own implements `IModuleBindings` on a component in its
scene, as the framework's do.

**Bind an `IModuleRouter` in your global installer.** RPGFramework.ModuleRouter's `DefaultModuleRouter` has the
framework's routes; derive from it for your own modules' (see its README).

Modules talk through **stores**, small services that hold one thing for another module to read:

| Store | Holds |
| --- | --- |
| `ICurrentModuleStore` | the module the playthrough is in; saved, so a loaded game resumes there |
| `ISaveEnabledStore` | whether the game may be saved here |
| `ILocationNameStore` | where the player is, as a localisation key's hash |
| `IBattleMusicStore` | the track every battle plays, as its music asset's name hash; saved |
| `ISongVolumeStore` | the volume the songs play at, as the scripts last set it, under the player's music volume; saved |
| `IPausedMusicStore` | the music a battle paused, for the module it returns to to resume |
| `IPlayTimeStore` | how long the playthrough has been played, in seconds, counted from leaving the title until back at it |
| `ICountdownStore` | the countdown's seconds left, counted down beside the play time whenever above zero, and whether it is shown; saved |
| `ISettingsStore` | the player's language, music and sound effect volumes and message speeds; in the settings file |

**Core binds** the input router, transitions, the save, settings and memory services, dialogue windows and the stores
above, after your global installer and each only where it bound nothing, so you can replace one, such as the
`ISaveStorage` saves are written through. **Your global installer binds**:

- the two databases, and a module router;
- your **variable map**, as `IVariableMap`, `IMemoryServiceArgs` and `ITempMemoryArgs`;
- a **Transition Config** (Create > RPG Framework > Transitions), as `ITransitionConfig`: the panel the transitions
  draw on — Core's **`TransitionPanelSettings`**, which sorts above its own — and the transitions used when none is
  named, such as Core's **`SubtractiveFade`**, and the two shaders Core's transitions draw with, set to Core's when the
  config is made; and a **Transition Database**, as `ITransitionDatabase`, holding every
  transition a router, a script or a module names;
- a **Dialogue Window UI Provider** (Create > RPG Framework > Dialogue);
- an **`IAudioIntentPlayer`**;
- an **`ISfxPlayer`** (RPGFramework.Audio), which transitions play their sounds through;
- whatever your modules need globally.

---

## Variables

Everything a game remembers lives in **variables**, declared in a **Variable Map** asset (Create > RPG Framework > Core >
Variable Map) rather than in code:

- **Each variable has a bank** — Persistent, saved; Session, kept until the game closes; Temp, a script's scratch; or
  Settings, the player's own — **and a width**: `Bool`, `SByte`, `Byte`, `Short`, `UShort`, `Int`, `UInt`, `Long`,
  `ULong` or `Float`.
- **A variable can be an array** of a width, or a **record** of named fields, each with its own width and count. Records
  can be arrays too.
- **Every variable is a fixed size at a fixed place**, so a save is one block of memory, and a value can be read from a
  save without loading it. There are no strings: text the game shows is stored as a localisation key's hash.
- **The map places each variable itself**, filling gaps left by deleted ones, and never lets two overlap.
- **Every variable keeps a permanent id**, so renaming or moving one keeps its saved value.
- **Each variable has a default**, and a new game starts from them, as a first launch does for the settings.
- **Packages declare the variables their own code reads**, and the map adds any that are missing and refuses to build
  without them.

Core requires these:

| Variable | Bank | What it is |
| --- | --- | --- |
| `CurrentModule` | Persistent | the module the playthrough is in; its default is where a new game begins |
| `LoadedFromSave` | Session | whether the playthrough was loaded from a save |
| `SaveEnabled` | Session | whether saving is allowed; its default is your game's policy, saving anywhere or at save points |
| `PlayTime` | Persistent | seconds played |
| `Countdown` | Persistent | the countdown's seconds left |
| `CountdownShown` | Persistent | whether the countdown is on screen |
| `CountdownX`, `CountdownY` | Persistent | where the countdown is shown, in the field and in battle alike |
| `LocationName` | Persistent | where the player is |
| `BattleMusic` | Persistent | the track every battle plays until a script sets another; its default is a new game's, zero none |
| `SongVolume` | Persistent | the volume the scripts last set the songs to, 0 to 1, under the player's; its default is a new game's |
| `Money` | Persistent | the party's money; its default is what a new game starts with, and what it is called is your game's text |
| `Language` | Settings | the language the player chose, as its name's hash; leave its default at zero, which asks on a first launch |
| `MusicVolume` | Settings | the player's music volume, 0 to 1, under which every song plays at its `SongVolume` |
| `SfxVolume` | Settings | the player's sound effects' volume, 0 to 1 |
| `BattleMessageSpeed` | Settings | how fast battle messages appear, 0 to 1; 0.5 is as authored |
| `FieldMessageSpeed` | Settings | how fast the field's dialogue appears, 0 to 1; 0.5 is as authored |

Read and write Persistent, Session and Settings variables through **`IMemoryService`**, by bank and address. Temp
memory belongs to whatever runs a script, so each script has its own.

---

## Saving

**`ISaveDataService`** saves the Persistent bank:

- **Starting:** `BeginSave` begins a playthrough, from a save file if one exists or as a new game if not. A new game
  writes nothing until the player saves.
- **Writing and managing saves:** `CommitSave` writes it, `DeleteSave` deletes one, and `GetListOfSaveFiles` and
  `GetUnusedSaveFileName` list the slots, `save000.sav` to `save255.sav`.
- **Previews:** `ReadPreview` reads a save without loading it, so a save menu can show each slot's location, play time
  or any other saved value with the game in progress untouched. `SavePreview.Read` and `ReadNumber` take a value's
  name as scripts write it: `Money`, `Items[3]`, `Party[0].Level`.
- **Files are checksummed and written safely**: a write that is cut short leaves the previous file whole.
- **A save records its layout**, so it still loads after the variable map has changed: a variable added since takes its
  default, a renamed or moved one keeps its value, and a deleted one is dropped.
- **A save that can't be loaded says why**: it is damaged, or was made by a newer version of the game.

## Settings

The player's settings are the **Settings** bank's variables, kept in `settings.dat` beside the saves rather than in
them, since they belong to whoever is playing, not to a playthrough:

- **Core reads the file as the game starts**, before your global installer's `Bootstrap`. With no file, every setting
  starts at its default, which is a first launch; a new game or a load leaves them alone.
- **`ISettingsStore`** reads and writes Core's own, and **`ISettingsService.Commit`** writes the file.
- **Your own settings are variables too**: declare them in the Settings bank, read and write them through
  `IMemoryService`, and commit. Scripts cannot use settings.
- **The file records its layout**, as a save does, so settings survive a changed map: one moved or renamed keeps its
  value, and one added starts at its default. A damaged file starts every setting at its default, and one from a newer
  build keeps what this build knows.

---

## Input

- **`InputAdapter`**, one per module scene, turns Input System actions into Core's controls (Primary, Secondary,
  Tertiary, Quaternary, the shoulders, triggers, Start and Select), and holds the direction held, its `Movement`, for
  what moves to read each tick. A direction held through a transition or a conversation moves the player as soon as
  movement reaches it again; a button held through one does not act again.
- **`IInputRouter`** passes presses down a stack of **input contexts** until one uses them, and says whether movement
  reaches a context (`MovementReaches`): only when no context above it takes movement (`TakesMovement`). A conversation
  the player can walk away from takes only confirm, and movement reaches the field beneath. Nothing is passed on while
  the screen fades, so a press during a change of module does nothing and nothing moves.
- **`ClockText`** writes the play time as hours and minutes, uncapped, and the countdown as minutes and seconds, two
  digits each, and **`ClockBlink`** blinks a clock's colon once a second. **`CountdownLabel`** is the countdown on
  screen, shown while it is shown, which the field and a battle place; restyle it through `rpg-countdown`.
- **`SelectionScope`** is for UI Toolkit screens you build yourself, such as a battle's commands. Over a root element,
  it moves the cursor only between **`RPGUIButton`s' neighbours** (link them with `RPGUIButton.LinkColumn` and
  `LinkRow`), by key, pad or pointer alike, and while its input is off lets nothing in it react at all. It plays no
  sound: it raises `Moved` when the player moves the cursor, for your logic to play the cursor sound — focus your code
  sets is not a move. A button's `Chosen` is a click or a submit.

## Dialogue

Dialogue windows type text with markup in curly braces:

```
You found a {KeyItem}sword{/KeyItem}!{Wait 1} You have {Var 0} gold.
```

| Markup | Does |
| --- | --- |
| `{Name}…{/Name}` | a style from your **Dialogue Text Styles** asset: a colour, and whether it flashes |
| `{Colour c}…{/Colour}` | a colour directly, `#RRGGBB` or a name such as `yellow` |
| `{Blink}…{/Blink}` | flashing text |
| `{Wait s}` | a pause of s seconds |
| `{Var n}` | message variable n, 0 to 7, set by the module showing the window |
| `{NewPage}` | a new page; `[Name]` at the start of a page names the speaker |
| `{{` | a literal brace |

- **Name a style for what a thing is**, such as a location or a key item, rather than giving it a colour. Recolouring
  every location is then one edit.
- **Windows have styles**, spoken, thought or transparent, and type at the player's message speed.
- **A question's answers are chosen like a menu's**, by key, pad or pointer: hovering one selects it, and a confirm
  press or a click answers. They take input only once the last answer has typed.
- **RPG Framework > Dialogue > Dialogue Preview** types a line as the game will and lists any mistakes in its markup.

## Transitions, audio intents and the update loop

- **`ITransitionService`** covers and reveals the screen, drawing on a panel above every module's UI, so menus and
  dialogue are covered too. Core uses it around every change; a module uses it for its own steps, such as a field jump
  or a menu change. A **transition** is an asset that covers or reveals over its own seconds and curve, and may play a
  **sound** as it starts — chosen in its inspector by name from your sound effect providers and stored as the name's
  hash, so it loads from wherever your `ISfxPlayer` gets its sounds, bundles included. Core has five kinds (Create >
  RPG Framework > Transitions):
  - **Colour** — a colour laid over the screen by opacity.
  - **Subtractive** — an amount taken from each channel of the live screen and clamped at zero, so colours sink into
    black, dark ones first: the look of PS1-era fades. Its colour is what is taken once covered; white takes
    everything. It is a UI Toolkit backdrop filter, which only URP draws, so on another pipeline fade with a colour.
    Core ships **`SubtractiveFade`**, half a second to black.
  - **Mask Wipe** — a greyscale texture stretched over the screen, with a softness for its edge: a cover reaches the
    texture's black first and its white last, and a reveal through the same texture uncovers its white first.
  - **Material** — any UI material, such as a UI Shader Graph's, given how covered the screen is, 0 to 1, in its
    `_Covered` float; one that captures the frame draws over a picture of the screen as its element's texture.
  - **Sequence** — stages played one after another, each through its own curve in a share of the sequence's seconds
    in proportion to its own, and staying as it finished beneath those after it, as a flash before a wipe. It plays
    its own sound, not its stages'.

  Your own derives from `TransitionAsset`: it builds what it draws in `Begin`, in the context's `Host`, and shows it as
  covered as the screen in its playback's `Show`; `Seconds` tells it how long it lasts, for motion it times itself. One that returns true from `CapturesFrame` is handed a picture of the screen as it starts, and
  owns the whole screen, so the change it covers is carried out beneath it at once. Black lies beneath a transition
  once the screen is fully covered, and beneath a captured frame, so a frame in which no camera renders never shows,
  as between one module's scene going and the next's loading. Transitions
  are named by the hash of their names (`NameHash`), `0` for the defaults; a `ModuleChange` names its cover and reveal
  (`WithTransitions`), and `CoverSeconds` says how long a cover takes, for a music fade timed to it. A cover while the
  screen is covered, or a reveal while it is clear, does nothing, and nothing routed reaches a module from a cover
  starting until a reveal starts. `RevealAsync` completes `true` once the screen is clear, or `false` when a cover
  asked for meanwhile took over.
- **A module's own transitions**: a **`TransitionLayer`** plays the same transitions into an element of the module's
  own, beneath its UI — Field fades the screen under its message windows with one — each from wherever the last left
  it, with `Cut` to cover or clear at once. It plays no sound: play the transition's `Sound`, found with `FindCover` or
  `FindReveal`, as you start one. A **`ScreenColour`** takes a colour from everything beneath an element, or
  adds one, live, drawn with the config's `ScreenColourShader`; it is what the Subtractive kind draws with, and needs
  URP.
- **`IAudioIntentPlayer`**: modules say what happened, such as `PlayAsync(AudioIntent.Confirm, AudioContext.Menu)`, and
  your game's implementation decides which sound that is. Its task completes once the sound has played out, at once
  for an intent with no sound or a sound that loops, so a caller can wait for it; most don't.
- **`UpdateManager`** ticks `IUpdatable` every frame and `IFixedUpdatable` every physics step, so a module owns its own
  update and stops it by unregistering. A change asked for during a tick takes effect before the next, the last asked
  for each standing, and registering twice ticks once.

---

## Tests

Core's tests are in `Tests/Editor`, on Unity's Test Framework. They compile only in a project that lists Core as
testable, so a game that doesn't builds none of them. To run them, add Core to `testables` in `Packages/manifest.json`:

```json
"testables": [
  "com.rpgframework.core"
]
```

then open **Window › General › Test Runner** and run the **EditMode** tests.

## Not in this version

- **Controls can't be rebound**, and there are no named actions above the fixed controls.
- **Some vocabularies are fixed enums a game can't extend**: the audio intents and contexts, and the controls.
- **Saves use the file system directly**, so consoles, which save through their own platform services, aren't
  supported yet.
- **The Universal Render Pipeline is required.**
