# CollectItAll

## Game Concept
**CollectItAll** is a resource collection and progression game. Players explore a lowpoly
open world, gathering scattered resources (Apples, Ore, and Poppies) and hauling them back
to type-matched dropoff points to bank them. Banked resources earn points, which spend on
permanent upgrades (movement speed, gather range/speed, and two unlockable abilities:
Auto-Collect and a resource Highlight). A day/night cycle drives both atmosphere and
difficulty — nights bring wandering hazard "wisps" that chase the player and steal points,
and days occasionally roll foggy weather that halves movement and gather range until it
clears. The player's overarching goal is to collect 100% of the world's resources.

### Controls
| Action | Key |
| --- | --- |
| Move / Look / Jump / Sprint | StarterAssets defaults (WASD, mouse, Space, Shift) |
| Interact (gather) | E |
| Toggle upgrade menu | M |
| Ability 1 — Auto-Collect | 1 |
| Ability 2 — Highlight resources | 2 |
| Cycle camera zoom | Z |

### Where the project is headed
Short term, the focus is closing the remaining checkpoint 2 gap below — a fresh in-editor
playtest of the full loop now that the win screen is in. After that: more abilities, better
dropoff/resource art (current placeholders are called out in `TODO_README.md`), and
selectable game-length modes (quick/medium/long) that scale the number of resources to
collect.

---

## Checkpoint 2 Feature Overview

**Interactive/collectible objects:** Apple trees, ore rocks, and poppies
(`InteractableResource` instances), dropoff points (`DropoffLocation.cs`), and night wisp
hazards (`NightWisp.cs`).

**Interaction systems:**
- Gather/pickup: `PlayerInteraction.cs` + `InteractableResource.cs` (press E, animation
  locked to the real clip length).
- Dropoff/banking: `DropoffLocation.cs` moves carried resources into the banked tally and
  awards points.
- Upgrade spending: `UpgradeMenuUI.cs` / `PlayerUpgrades.cs` (press M), spends points on
  speed, gather range/speed, and unlocking abilities.
- Abilities: Auto-Collect (1) and Highlight (2) via `PlayerAbilities.cs`.

**Progress tracking:** `ResourceCounter.cs` tallies collected-vs-total per resource type
and overall percentage; `PlayerInventory.cs` tracks carried-but-not-yet-banked amounts
(capped); `PlayerPoints.cs` tracks spendable currency earned from dropoffs.

**UI feedback:** Per-type resource counts + an overall progress bar (top-left), a
carried-inventory readout (`CarriedInventoryUI.cs`), a "Gathered X" popup
(`GatherPopupUI.cs`), ability cooldown radials + locked overlays, the upgrade menu's
costs/levels, a compass bar (`CompassMarkerBar.cs`), and a minimap (`MinimapFollow.cs`).

**Hazards/challenge systems:**
- Night wisps: nocturnal hazard that wanders, then chases and steals points on contact;
  blocked near dropoff safe zones (`NightWisp.cs`).
- Fog: randomly-rolled daytime weather hazard that halves move speed and gather range
  until it clears (`DayNightCycle.cs` / `PlayerUpgrades.cs`).
- Carry-capacity limit: `PlayerInventory` caps total carried resources, forcing return
  trips to a dropoff point instead of one long gathering run.

**Player/camera improvements:** Z cycles through several camera zoom distances
(`CameraZoom.cs`).

**Win condition:** `ResourceCounter` detects when every resource has been both collected
and deposited and shows `WinScreenUI` — a "You Win!" panel that freezes player control
(same pattern as the intro/upgrade screens) and offers a Restart button that reloads the
active scene, resetting all game state.

**Audio:**

**Visual polish:**

See `TODO_README.md` for the fuller running list of known placeholders and planned work
beyond checkpoint 2.

---

## External Assets & Resources Used
Below is a list of external packages, assets, and resources utilized in this project so far:

- **Starter Assets - Third Person Character Controller (URP)**: [Unity Asset Store](https://assetstore.unity.com/packages/essentials/starter-assets-thirdperson-urp-196526) - Base player controller, character setup, and input actions.
- **Polytope Studio - Lowpoly Environment Nature Free**: [Unity Asset Store](https://assetstore.unity.com/packages/3d/environments/low-poly-environment-nature-free-lowpoly-medieval-fantasy-series-187052) - Environment assets, trees, terrain materials, and rocks.
- **Mixamo Animations**:
  - [Pick Fruit Animation](https://www.mixamo.com/#/?page=1&query=pick+fru) - Interaction animation for picking fruit.
  - [Gather Animation](https://www.mixamo.com/#/?page=1&query=gather) - Interaction animation for gathering/picking up items.
- **Pixabay Audio Assets**:
  - [Nature Night Forest with Frogs and Crickets](https://pixabay.com/sound-effects/nature-night-forest-with-frogs-and-crickets-for-sleep-451153/) - Night ambient sound effect.
  - [Nature Forest Daytime](https://pixabay.com/sound-effects/nature-forest-daytime-446356/) - Day ambient sound effect.
  - [Meditative/Spiritual Atmospheric Documentary Music](https://pixabay.com/music/meditationspiritual-atmospheric-documentary-509386/) - Game background music track.
