BARIS: Building A Rocket Isn't Simple

Building rockets can take time!
Staging events can fail!
Parts can wear out!
Event cards!

---INSTALLATION---

Copy the contents of the mod's GameData directory into your KSP's GameData folder. Your folders should look like this:
GameData/WildBlueIndustries/000ABARISBridgeDoNotDelete
GameData/WildBlueIndustries/000BARIS
GameData/ModuleManager.dll

NOTE: ModuleManager is REQUIRED.

---REVISION HISTORY---

1.3.0
MTBF
- Deactivated parts will lose MTBF at a slower rate (default: 1/10th) than activated parts. Think hibernation mode... 
NOTE: the hibertating MTBF decay rate is varied per part. Deactivated engines lose MTBF slower than deactivated fuel tanks, for instance. Look at MM_BARIS.cfg for examples.

Launch Escapes
- Base chance to escape an exploding vehicle is now based on the kerbal with the highest Stupidity in the crew.

Fuel Tanks
- Additional MM patches for fuel tanks- thanks Hotaru! :)
- Fuel tanks will make quality checks when the player locks or unlocks its resources.
- Fuel tanks will make quality checks when a resource is emptied or filled to capacity.
- Fuel tanks will be considered deactivated if all their resources are locked.
- You can toggle the lock/unlock state of all resources in a tank using the new "Toggle Resource Locks" context menu button and Action Group button.

Bug Fixes
- Fixed bug where parts were losing MTBF faster than intended.
- Fixed bug where some parts were allowed to break even when prevented from breaking.

1.2.7
Event Cards
- Vehicle integration completed event result won't be available when KCT is installed.

MTBF
- Part condition is now based on MTBF instead of part quality.
- New Condition summary: Maintenance Required - When MTBF runs out, this summary will indicate that the part needs periodic maintenance in order to maintain its quality.
- As parts gain flight experience, they'll gain MTBF in addition to their flight experience bonus.
- MTBF capped at 5,112 hours (2 kerbal-years).
- ModuleQualityControl now allows part-based MTBF caps just like it does with quality rating caps. These caps override the global settings.
- Fixed issue where MTBF improvements gained through part upgrades wasn't being applied.
- Fixed issue where MTBF improvements gained through facility upgrades wasn't being applied.

Configuration
- Added new Constants.cfg file; many of BARIS's constant values can be configured with this file, including the MTBF cap...

Other Fixes
- Fixed issue where the odds of a part exploding during launch and/or post-launch weren't honoring a 0% chance.

1.2.5
Test Bench
- Test Bench now calculates the simulation costs based upon breakable parts, not all the parts in the vessel.
- Test Bench lists both initial and integrated Reliability, both before and after purchasing a Reliability upgrade.
- Test Bench lets you add 1, 5, and 10 points of Reliability at a time, with appropriate cost increases.
- For KCT users, the VAB/SPH BARIS button will show what the vessel's reliability will be after construction.

Escape Pods
- Removed escape pod flag from ModuleQualityControl; it was causing confusion.

Part Failures
- Messages are now displayed in the correct sequence during a staging event's critical failure.
- Added new Settings option to give parts the option to potentially explode during failures. It is off by default. They can explode during failed staging events and during post-launch critical failures when the part has run out of MTBF.
- Added new Settings option to control how likely a part will explode during staging events.
- Added new Settings option to control how likely a part will explode during a post-launch activity when the part critically fails and is out of MTBF.
- Critical failures are less likely to happen during post-launch activities.
- Revised engine failure modes: shutdown (75% chance unless engine can't shut down, in which case it explodes); stuck on (20% chance unless engine can't shut down, in which case it explodes); explode (5% chance).
- Increased the maximum number of seconds in which to make a staging check.
- Fixed persistence issues with broken parts.
- Crew will no longer try to escape an unkermanned vessel.

Event Cards
- The available event cards now depend upon the current game mode.

1.2.0
- Fixed issues with event card tips repeatedly showing up.
- Tool tip cards won't show up if BARIS is disabled.
- Parts on new vessels will receive default flight experience points when launches can't fail.
- Parts created in the field will receive default flight experience if none existed before. This will help with incorporating BARIS into existing games.
- Improvements to flight experience will now affect vessels currently undergoing vehicle integration.
- Fixed issue with flight experience not being added to parts after vehicle integration was completed.
- New VAB/SPH button: Test Bench - If you assemble a collection of parts in the VAB/SPH, you can spend Science in Science Sandbox and Career games and/or Funds in Career games to simulate launch conditions and gain flight experience. That flight experience will improve your vessel Reliability ratings on future craft. You can adjust the per-part Funds cost in the Difficulties screen; Science cost is based on how many flights it takes to gain a flight experience bonus point.
- Vehicle integration rush jobs are now only available in Career games.

1.1.0
- Created a bridging dll for those who want to optionally incorporate BARIS into their mods.
- Event cards won't spawn during high timewarp, but if the timer expires then you'll receive an event card once you exit timewarp.
- New debug option: In the editor, you can add flight experience to parts in the currently loaded vessel. Similarly, you can do the same for the active vessel in flight. This should help players integrate BARIS into existing saves.
- Vessels will make a Reliability check a few minutes after launch if you haven't already achieved orbit. This will cover SSTOs and add extra pucker factor.
- Added new "Event Card" hints for first-time players.
- Clarified KSPedia entry for KCT support.
- Moved stock launch clamp to Engineering 101 to help with early-game static fire tests.
- When staging events fail, parts might still gain flight experience. The odds of that happening is configurable from the BARIS settings screen.
- Fixed issue with Astronaut payroll not showing up when KCT is installed.
- Dropped support for KSP 1.2.2; you can thank the bridging dll for that...

1.0.5
- Fixed issue where Event Cards would happen even when BARIS is disabled.
- Updated "Reliability & MTBF" KSPedia page to show where to find a part's MTBF.
- Added new KSPedia pages describing how to enable BARIS.

1.0
- Eliminated duplicate warnings that appear when you revert a non-integrated vessel back to the editor.
- Quality Check event results can be performed as soon as the event card is played instead of waiting for the next quality check.
- Added Micrometeor Damage event card.

0.9
- Added event card images.
- Added KSPedia.
- Added new vehicle integration status screen to the Space Center screen.
- Updated Wiki with new pages and updated existing ones.
- Bug fixes.

0.8
- Fixed an issue where High Bays/Hangar Bays and construction worker GUI would appear even when vehicle integration is turned off and/or when launches can fail.
- Fixed an issue where the BARIS GUI was available when BARIS itself was disabled ("Parts Can Break" option is off).
- Added the Mk1 Launch Escape System. It is designed to fit over the stock Mk16 Parachute but can also be fitted to size 0 parts.

0.7.0
- Added support for KerbalConstructionTime. If installed, then the High Bay/Hangar Bay screen won't be available, and the Space Center screen won't have construction workers.

0.6.0
- Added Event Cards into BARIS. Event Cards will affect your game in various ways; you can enable/disable cards as well as control their frequency of appearing via the Settings->Difficulty dialog. With Debug mode turned on, you can play individual cards as desired from the Space Center BARIS screen.

WANTED: If you're willing to donate screen shots for the cards, I need 256 x 256 images that are appropriate for the event card's subject.

- Fixed an issue with adding/removing workers.
- Added Race Into Space music by Brian Langsbard. License: GPL V2

0.5.3
- Added KSP 1.2.2 version of the BARIS.dll. It is located in the Extras/KSP122DLL folder.

0.5.2
- Fixed another case where you'd get integration warning spam in the editor.
- Fixed an issue where, if you cancel a vehicle integration, the allocated workers were lost.
- Difficulty Modifier is now done by category instead of a raw value.

0.5.1

- Fixed integration warning spam in the editor.
- Fixed duplicate editor bays appearing after upgrading the VAB/SPH.
- Fixed NRE that happens when trying to rebuild the vessel summary cache and there are no vessels in the game.

0.5.0 Pre-release alpha

---ACKNOWLEDGEMENTS

Module Manager by Sarbian

Event Card Images By Squad: BrainDrain, CorporateSponsorship, FluShots, HazzardPay, ManufacturingDefects, Protests, SafetyInspections, SpeedChallenge, WorkerStrike

AstronautOnVacation image by Malaclypse

---LICENSE---
Race Into Space music by Brian Langsbard. License: GPL V2

Art Assets, including .mu, .mbm, and .dds files but excluding Event Card Images By Squad and AstronautOnVacatin image are copyright 2014-2016 by Michael Billard, All Rights Reserved.

Wild Blue Industries is trademarked by Michael Billard. All Rights Reserved.
Note that Wild Blue Industries is a ficticious entity 
created for entertainment purposes. It is in no way meant to represent a real entity.
Any similarity to a real entity is purely coincidental.

Source code copyright 2014-2017 by Michael Billard (Angel-125)

    This source code is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.