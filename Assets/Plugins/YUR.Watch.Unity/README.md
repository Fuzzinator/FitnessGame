# YUR.Watch.Unity

A quick to set up Unity integration of [YUR's VR fitness tracking service](https://yur.fit). With the YUR.watch developers have access to several features, including the ability to read player workout data in realtime and set workout metadata for capturing in-game analytics.

## Key concepts

- ⏱ **Watch**: An asset attached to a controller, providing
  the user with live information about their workout.
- 💪 **Workout**: Fitness data calculated from
  the movements of the user during gameplay. A workout begins
  right as the game starts, and ends when it finishes.
- 🔖 **Tags**: A way for developers to associate a subset of the workout with a string "tag", e.g. `"level_boss_2"`, or `"level_main_menu"`.

## Getting started

### Download & install

You can get the package either via the [Unity Asset Store](https://assetstore.unity.com), or directly from [GitHub](https://github.com/YURInc/YUR.Watch.Unity/).

#### Unity Asset Store:

Use the 'Add to my assets' button, then 'Open in Unity', then 'Download', and finally 'Import' to install the package to your project.

#### GitHub:

Versions of the watch are available here from the [Releases](https://github.com/YURInc/YUR.Watch.Unity/releases/) page on GitHub.

> _Note:_ For stability and compatibility it is **highly** recommended that you use the releases page instead of cloning the repo into your project.

Once you have the watch downloaded, unzip it and place it in your project's `Packages/` directory (creating it if it doesn't exist). Afterwards, the folder structure should look something like this: `YourProject/Packages/YUR.Watch.Unity/`.

---

### Setup

> (There is an example scene in `Packages/YUR.watch/Samples/`)

Once you have the package, you can open the editor. At the
time of writing, you'll likely get a notification about
updating the `XR InteractionLayerMask`. Select 'No Thanks'.

Going forward we're going to assume you have three game
objects:

- HMD
- Left Controller
- Right Controller

The specifics of these aren't relevant - as long as they
move and rotate with the user's movements, they should be
compatible with the watch.

There are four scripts located in
`Packages/YUR.watch/Core/`:

- `YUR_HMD`
- `YUR_LeftHand`
- `YUR_RightHand`

and

- `YUR_Watch`

Attach the first three scripts to your respective game objects, and place the final one on an empty game object in the root of your scene.

Next, find the scriptable object file in
`Packages/YUR.watch/Core/Scriptable/` called
`SO_YURWatchSettings`.

Drag this scriptable object onto the `Settings` field of the `YURWatch` script.

In the inspector for the settings, there should be some configuration options. Fill out the Game Name field using the format `com.YourCompany.YourGameName`.

Next are the left and right hand setup fields. Use these to offset the watch in a way that works for your specific controller model / setup.

> This process can be fiddly: a way to speed it up is to > play your scene in editor, navigate out of the Game window > back to Scene, and update the settings while the game is > running. These settings will persist after gameplay stops!

Finally is the hand in use: this is the default preferred hand that the watch is on.

Now you're ready to go!

## Interface

We expose a small number of functions via the `YURWatch.cs` script:

```cs
void SetPreferredWatchHand(HandSide hand);  // update the preferred hand
bool IsConnected();                         // get connection status
YUR_SDK.CResults GetResults();              // get current results from calculation
void SetTag(string tag);                    // set the current tag (pass empty string to end tag)
```

## Tags

We mentioned this in the **Key concepts** section at the
beginning of the document, but to reiterate, tags are:

> A way for developers to associate subset of the workout with a string "tag", e.g. `"level_boss_2"`, or `"level_main_menu"`.

You can access this functionality through the `SetTag()`
function in `YURWatch.cs`. Common uses are levels (as in the
above example), but you can put anything you like in here!

You can call this function once to begin tagging a period,
and again with an empty string to stop tagging, or with a
different string to change the tag.

## Known Issues

- Offline workouts are not supported (we are actively
  working on this!)
