# RPGFramework.Core

The centre of an RPG Framework game. Core switches between the game's modules, such as the title screen, the field and
battle, each in a scene of its own. It also owns what every module shares:

- the game's variables and the memory they live in;
- saves and settings;
- input routing and dialogue windows;
- screen fades, audio intents and the update loop.

Requires Unity 6000.6 or newer, the Universal Render Pipeline (the screen fades are a full-screen renderer feature), the
Input System, Unity.Mathematics, RPGFramework.DI, RPGFramework.Hashing and RPGFramework.Core.SharedTypes.

---

## Modules

A module is a scene and the `IModule` its scene installer binds. Each has a byte id. Your game supplies two databases:

- an **`IModuleDatabase`**, mapping each id to its module's interface;
- an **`ISceneDatabase`**, mapping each module to its scene.

Start the game from a component in your first scene:

```csharp
ICoreModule core = await CoreModuleFactory.Create(m_GlobalInstaller, SplashScreenModuleId);

await core.RequestModuleChangeAsync();
```

`Create` builds the global container, binding Core's services and then your global installer's, and runs its
`Bootstrap`. A module change, `RequestModuleChangeAsync`, goes to the module whose id is in `IChangeModuleStore`:

1. It exits the current module and loads the next one's scene.
2. It builds a new scene container from the scene's `SceneInstallerMonoBehaviour`, falling back to the global one.
3. It resolves the module from that container and enters it.

**Bind each module in its own scene installer.** It is then built fresh on every entry and goes with its scene. A
module missing from its scene installer fails with *No binding exists*, which is how you find out.

Modules talk through **stores**, small services that hold one thing for another module to read:

| Store | Holds |
| --- | --- |
| `IChangeModuleStore` | the module the next change goes to |
| `IResumeModuleStore` | the module a menu or battle returns to |
| `ICurrentModuleStore` | the module the playthrough is in; saved, so a loaded game resumes there |
| `ISaveEnabledStore` | whether the game may be saved here |
| `ILocationNameStore` | where the player is, as a localisation key's hash |
| `IPlayTimeStore` | how long the playthrough has been played, in seconds |

**Core binds** the input router, screen fades, the save, settings and memory services, dialogue windows and the stores
above. **Your global installer binds**:

- the two databases;
- your **variable map**, as `IVariableMap`, `IMemoryServiceArgs` and `ITempMemoryArgs`;
- an **`IDefaultSettings`**;
- an **`IRendererDataProvider`**, with your URP renderer data, and a **Screen Fade Config** (Create > RPG Framework >
  Rendering);
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
  walk away from takes only confirm, and movement goes on to the field beneath.

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
- **RPG Framework > Dialogue > Dialogue Preview** types a line as the game will and lists any mistakes in its markup.

## Screen fades, audio intents and the update loop

- **`IScreenFadeService`** fades the screen in and out, simply or with a transition such as the start of a battle.
- **`IAudioIntentPlayer`**: modules say what happened, such as `Play(AudioIntent.Confirm, AudioContext.Menu)`, and your
  game's implementation decides which sound that is.
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
