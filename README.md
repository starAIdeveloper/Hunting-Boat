# Hunting Boat

Unity 6 C# ocean fishing adventure inspired by the supplied boat-navigation and fishing references. The project contains a starting scene, procedural world and boat assets, a fishing loop, map, touch/keyboard controls, upgrades, local progression saves and Unity EditMode tests.

**Validation status:** C# syntax parsing and asset/source integrity checks passed locally. Unity is not installed in the execution environment. Unity compilation, shader compilation, gameplay, EditMode tests and native player builds have **not** been run. This repository is an implemented source project awaiting Unity integration verification, not a verified executable or a photorealistic reproduction of the reference artwork.

## Open and play

1. Clone `https://github.com/starAIdeveloper/Hunting-Boat.git`.
2. In Unity Hub choose **Add project from disk**, then select this repository root.
3. Open with **Unity 6.0 / 6000.0.38f1** and let Package Manager restore packages. The project uses the **Built-in Render Pipeline**, not URP/HDRP.
4. Open `Assets/HuntingBoat/Scenes/Ocean.unity`, or choose **Hunting Boat → Open Ocean Scene**.
5. Press Play. The scene creates the world, boat, cameras, HUD and generated engine sound at runtime.
6. Navigate to the gold waypoint at Lighthouse Shoal. Within 40 metres, slow below 5 km/h and press **F** to cast.
7. On a bite, press **F** within 2.5 seconds to hook. Reel while watching tension; ease off during surges. Land fish for credits, then install upgrades in the menu.

Use **Input Manager (Old)** or **Both** under Player Settings → Active Input Handling. Legacy axes are included for the UI event system. If Unity asks to update or resave project settings, allow it and inspect the Console before continuing. Do not convert this project to URP without porting its shaders.

## Controls

| Input | Action |
| --- | --- |
| W/S or ↑/↓ | Accelerate / brake and reverse |
| A/D or ←/→ | Turn |
| C | Chase / cockpit camera |
| F | Cast or hook a bite |
| Space or left mouse away from UI | Reel |
| Left Shift or right mouse away from UI | Pull rod, increasing tension |
| R | Release line / fish |
| M | Open/close navigation chart |
| Escape | Pause / resume menu |
| F5 | Save voyage |

On mobile platforms or narrow windows, directional hold buttons appear. Fishing hold buttons are provided on all platforms. Touch input is implemented but not tested on physical devices; landscape orientation is recommended. Android/iOS player configurations, platform-specific permission/signing setup and device QA are not included in this desktop-first project.

## Game systems

- Original mesh boat hull with deck, console, windshield, canopy, rails, outboards and fishing rod.
- Rigidbody arcade handling, drag, lateral stabilization, wave-height spring response and cosmetic sway. These are approximations, not a marine simulator.
- Three island landmarks and three navigable fishing buoys in a bounded ocean area. The HUD reports waypoint distance, heading, speed, hull health and credits.
- Custom ocean, sky, lit-object and wake shaders under `Resources`, so dynamic shader lookups are included in builds. Runtime procedural models require no external meshes or paid assets.
- Chase, cockpit and fishing camera contexts.
- Cast → wait → bite → hook → fight → land/escape state machine. Fish apply randomized force surges. Sustained slack or overload loses a fish; balanced reeling advances the catch.
- Three catch species with randomized in-game weights and lengths. They share a stylized fish model. This is a game rule system, not machine-learning fish behavior.
- Catch log, credit rewards, five engine/rod upgrade levels and a three-catch objective. Discoveries, catches and upgrades persist.
- Sunset, clear, storm and night presets. Weather advances every three minutes of active game time and can also be changed in the menu. Storm changes fog, lighting and visual wave strength; there is no meteorological simulation.
- Map waypoint selection, pause menu, boat recovery after damage, and synthesized engine audio.

This is a single-player source project. Multiplayer, multi-screen installations, FMOD integration, licensed photographic environments and platform certification shown in some reference panels are not implemented. No generated promotional image is presented as a screenshot of a running Unity build.

## Save files

The game stores `hunting-boat-save.json` in `Application.persistentDataPath`. Manual save, landed catches, discoveries, upgrades and normal application exit trigger saves. A backup is retained before replacing an existing file. Unsupported file operations report a save error without deleting the previous save.

Version and bounds checks reject malformed or incompatible data. Position, heading, credits, upgrade levels, weather, discovered grounds and up to 500 catches are retained. An unfinished fishing session restarts after loading; hull health is restored. Browser/WebGL persistence and abrupt process termination are not validated.

## Tests and build

In Unity, open **Window → General → Test Runner**, select **EditMode**, and run `HuntingBoat.Tests`. Twelve test cases cover bite timing, hook gating, slack/overload failure, balanced reeling, invalid timestep input, upgrade charging/limits, save validation, JSON roundtripping and rewards. The test cases are supplied but have not run locally.

Batch command, using your licensed Unity executable:

```bash
Unity -batchmode -nographics -projectPath "$PWD" -runTests -testPlatform EditMode -testResults artifacts/editmode.xml -logFile artifacts/unity-tests.log
```

Choose **Hunting Boat → Build Windows Player** or **Build macOS Player** after installing the corresponding Unity Hub build module. Output goes to `Builds/`. Native builds are not supplied.

Before releasing, verify the scene opens without errors, all four shaders compile, the boat moves and collides, chase/cockpit cameras work, fishing can succeed and fail, touch controls release correctly, menus pause/resume, saves roundtrip and both desktop build targets run. Measure runtime frame rate and memory on the intended hardware; no performance benchmark has been established.

GitHub Actions automatically performs source integrity checks. A separate manual Unity test workflow is included. It requires a valid GameCI-compatible Unity license/account setup in repository secrets (`UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD`) and must be configured according to your Unity license. Credentials are not included or requested by the project.

## Source integrity check

```bash
python -m venv .checks
# Activate the environment for your operating system.
pip install -r scripts/requirements-checks.txt
python scripts/validate_source.py
```

[Local static check report](artifacts/source-validation.json) records the exact scope: C# grammar parsing, asset GUID uniqueness, metadata presence, scene/build references, JSON manifests, custom shader lookup resolution and shader block balance. Parsing does not check Unity API type compatibility, package resolution, shader compilation or runtime behavior.

## Layout and history

- `Assets/HuntingBoat/Scripts`: boat, world, fishing/progression, HUD, saves and touch input.
- `Assets/HuntingBoat/Resources`: build-included custom shaders.
- `Assets/HuntingBoat/Scenes/Ocean.unity`: startup scene.
- `Assets/HuntingBoat/Editor`: open-scene and desktop build menus.
- `Assets/HuntingBoat/Tests/EditMode`: Unity NUnit tests.
- `ProjectSettings` / `Packages`: Unity project configuration.

Built with AI assistance in successive commits. Imported GitHub commits include their original local SHA. `Hunting-Boat-history.bundle` retains original local commit IDs, authors and timestamps. History records actual implementation steps without backdating.
