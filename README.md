# RPGFramework.Core

The centre of an RPG Framework game. Core switches between the game's modules, such as the title screen, the field and
battle, each in a scene of its own. It also owns what every module shares:

- the game's variables and the memory they live in;
- saves and settings;
- input routing and dialogue windows;
- transitions, audio intents and the update loop.

Requires Unity 6000.6 or newer, the Input System, Unity.Mathematics, RPGFramework.Audio (the music a battle pauses is an
Audio snapshot), RPGFramework.DI, RPGFramework.Hashing and RPGFramework.Core.SharedTypes. It works with any render
pipeline: its transitions are drawn with UI Toolkit.

---

## Modules

A module is a scene and the `IModule` its scene installer binds. Each has a byte id. Your game supplies two databases:

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
change out, and reveals the module on top, then tells it so with **`OnRevealed`**: that is where a module starts taking
input, or starts anything the player should see from its beginning. A module does not fade itself for a change. Changes
run one at a time: one asked for during another waits for it, and is routed from whichever module is then on top.

**Bind each module in its own scene installer.** It is then built fresh on every entry and goes with its scene. A
module missing from its scene installer fails with *No binding exists*, which is how you find out.

**Bind an `IModuleRouter` in your global installer.** RPGFramework.ModuleRouter's `DefaultModuleRouter` has the
framework's routes; derive from it for your own modules' (see its README).

Modules talk through **stores**, small services that hold one thing for another module to read:

| Store | Holds |
| --- | --- |
| `ICurrentModuleStore` | the module the playthrough is in; saved, so a loaded game resumes there |
| `ISaveEnabledStore` | whether the game may be saved here |
| `ILocationNameStore` | where the player is, as a localisation key's hash |
| `IBattleMusicStore` | the track every battle plays, as its music asset's name hash; saved |
| `IMusicVolumeStore` | the volume the music plays at, as the scripts last set it; saved |
| `IPausedMusicStore` | the music a battle paused, for the module it returns to to resume |
| `IPlayTimeStore` | how long the playthrough has been played, in seconds, counted from leaving the title until back at it |

**Core binds** the input router, transitions, the save, settings and memory services, dialogue windows and the stores
above. **Your global installer binds**:

- the two databases, and a module router;
- your **variable map**, as `IVariableMap`, `IMemoryServiceArgs` and `ITempMemoryArgs`;
- an **`IDefaultSettings`**;
- a **Transition Config** (Create > RPG Framework > Transitions), as `ITransitionConfig`: the panel the transitions
  draw on — Core's **`TransitionPanelSettings`**, which sorts above its own — and the transitions used when none is
  named, such as a **Colour Transition**; and a **Transition Database**, as `ITransitionDatabase`, holding every
  transition a router, a script or a module names;
- a **Dialogue Window UI Provider** (Create > RPG Framework > Dialogue);
- an **`IAudioIntentPlayer`**;
- whatever your modules need globally.

---

## Variables

Everything a game remembers lives in **variables**, declared in a **Variable Map** asset (Create > RPG Framework > Core >
Variable Map) rather than in code:

- **Each variable has a bank** (Persistent, Session or Temp) **and a width**: `Bool`, `SByte`, `Byte`, `Short`,
  `UShort`, `Int`, `UInt`, `Long`, `ULong` or `Float`.
- **A variable can be an array** of a width, or a **record** of named fields, each with its own width and count. Records
  can be arrays too.
- **Every variable is a fixed size at a fixed place**, so a save is one block of memory, and a value can be read from a
  save without loading it. There are no strings: text the game shows is stored as a localisation key's hash.
- **The map places each variable itself**, filling gaps left by deleted ones, and never lets two overlap.
- **Every variable keeps a permanent id**, so renaming or moving one keeps its saved value.
- **Each variable has a default**, and a new game starts from them.
- **Packages declare the variables their own code reads**, and the map adds any that are missing and refuses to build
  without them.

Core requires these:

| Variable | Bank | What it is |
| --- | --- | --- |
| `CurrentModule` | Persistent | the module the playthrough is in; its default is where a new game begins |
| `LoadedFromSave` | Session | whether the playthrough was loaded from a save |
| `SaveEnabled` | Session | whether saving is allowed; its default is your game's policy, saving anywhere or at save points |
| `PlayTime` | Persistent | seconds played |
| `LocationName` | Persistent | where the player is |
| `BattleMusic` | Persistent | the track every battle plays until a script sets another; its default is a new game's, zero none |
| `MusicVolume` | Persistent | the volume the music plays at, 0 to 1; its default is a new game's |

Read and write Persistent and Session variables through **`IMemoryService`**, by bank and address. Temp memory belongs
to whatever runs a script, so each script has its own.

---

## Saving

**`ISaveDataService`** saves the Persistent bank:

- **Starting:** `BeginSave` begins a playthrough, from a save file if one exists or as a new game if not. A new game
  writes nothing until the player saves.
- **Writing and managing saves:** `CommitSave` writes it, `DeleteSave` deletes one, and `GetListOfSaveFiles` and
  `GetUnusedSaveFileName` list the slots, `save000.sav` to `save255.sav`.
- **Previews:** `ReadPreview` reads a save without loading it, so a save menu can show each slot's location and play
  time with the game in progress untouched.
- **Files are checksummed and written safely**: a write that is cut short leaves the previous file whole.
- **A save records its layout**, so it still loads after the variable map has changed: a variable added since takes its
  default, a renamed or moved one keeps its value, and a deleted one is dropped.
- **A save that can't be loaded says why**: it is damaged, or was made by a newer version of the game.

## Settings

**`ISettingsService`** keeps the player's settings, such as language, volumes and message speeds, in `settings.dat`
beside the saves rather than in them, since they belong to whoever is playing, not to a playthrough. Your
**`IDefaultSettings`** fills in the defaults every launch, and the file's values are laid over them. With no file, it is a
first launch. A game can add sections of its own.

---

## Input

- **`InputAdapter`**, one per module scene, turns Input System actions into Core's controls (Primary, Secondary,
  Tertiary, Quaternary, the shoulders, triggers, Start and Select) and movement.
- **`IInputRouter`** passes them down a stack of **input contexts** until one uses them. A conversation the player can
  walk away from takes only confirm, and movement goes on to the field beneath. Nothing is passed on while the screen
  fades, so a press during a change of module does nothing.
- **`SelectionScope`** is for UI Toolkit screens you build yourself, such as a battle's commands. Over a root element,
  it moves the cursor only between **`RPGUIButton`s' neighbours** (link them with `RPGUIButton.LinkColumn` and
  `LinkRow`), by key, pad or pointer alike, plays the cursor sound as focus moves, and while its input is off lets
  nothing in it react at all. A button's `Chosen` is a click or a submit.

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
  or a menu change. A **transition** is an asset that covers or reveals over its own seconds and curve: Core has the
  **Colour Transition**, and your own derives from `TransitionAsset`: it builds what it draws in `Begin` and shows it as
  covered as the screen in its playback's `Show`. One that returns true from `CapturesFrame` is handed a picture of the
  screen as it starts, and owns the whole screen, so the change it covers is carried out beneath it at once. Transitions
  are named by the hash of their names (`NameHash`), `0` for the defaults; a `ModuleChange` names its cover and reveal
  (`WithTransitions`), and `CoverSeconds` says how long a cover takes, for a music fade timed to it. A cover while the
  screen is covered, or a reveal while it is clear, does nothing, and nothing routed reaches a module from a cover
  starting until the screen is clear.
- **`IAudioIntentPlayer`**: modules say what happened, such as `PlayAsync(AudioIntent.Confirm, AudioContext.Menu)`, and
  your game's implementation decides which sound that is. Its task completes once the sound has played out, at once
  for an intent with no sound or a sound that loops, so a caller can wait for it; most don't.
- **`UpdateManager`** ticks `IUpdatable` every frame and `IFixedUpdatable` every physics step, so a module owns its own
  update and stops it by unregistering.

---

## Not in this version

- **Controls can't be rebound**, and there are no named actions above the fixed controls.
- **Some vocabularies are fixed enums a game can't extend**: the audio intents and contexts, and the controls.
- **Saves use the file system directly**, so consoles, which save through their own platform services, aren't
  supported yet.
- **The Universal Render Pipeline is required.**
- **A module's own types must be public** to be bound in its scene installer.
