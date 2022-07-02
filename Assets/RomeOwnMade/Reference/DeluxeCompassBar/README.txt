//
// Deluxe Compass Bar
// A GUI and script package to include a compass and object tracking GUI to your game
// Copyright 2016 While Fun Games
// http://whilefun.com
//

===================
>Setup Instructions:
===================

1) Make a new scene and Add a player control object of your choice (e.g. the included Standard Assets -> Character Controllers -> First Person Controller)

Note: If using an off the shelf character controller, also add the CompassCamera prefab as a child of your character controller and sibling
of MainCamera, with position (0,0,0). See included character controller for example of how to set this up.

2) Add CompassWorldPoles prefab into the scene, ensure it is at top scene level (has no parent), set XYZ coords to (0,0,0)
3) Add TheCompass prefab into your scene, ensure it is at top scene level (has no parent), set XYZ coords to (0,0,0)
4) Run scene, base compass should now work.

Refer to demo scene included in the package for examples on how to add Compass Markers to your game objects (e.g. Monsters, Quests, Objectives, Locations, etc.)

Notes: 
-----
CompassWorldPoles prefab contain a visual dev/debug child object called "demoMeshes". These are provided for visual reference only and can be deleted.

If desired, set the Y-Rotation of the CompassWorldPoles prefab to suit your game world's "Magnetic North".


===============================================
>Add a Compass Markers to track any Game Object:
===============================================
(See demo scene and DemoMarkerSpawnerScript for example code.)

1) Add CompassMarkerScript to your Game Object

e.g. gameObject.AddComponent<CompassMarkerScript>();

2) (Optional) Set specific decoration flags you want for this compass marker

e.g.

gameObject.GetComponent<CompassMarkerScript>().setMarkerIconType(TheCompassScript.IconType.eImportant);
gameObject.GetComponent<CompassMarkerScript>().showUpDownArrow = theCompass.GetComponent<TheCompassScript>().drawUpDownArrows;
gameObject.GetComponent<CompassMarkerScript>().showDistanceLabel = theCompass.GetComponent<TheCompassScript>().drawDistanceText;

3) Initialize the Compass Marker

e.g. gameObject.GetComponent<CompassMarkerScript>().initTrackingOfMarker();


=============================================
>Remove a Compass Marker from any Game Object:
=============================================
(See demo scene and MarkerSpawnerScript for example code.)

1) Either remove CompassMarkerScript (or just Destroy the game object entirely)

e.g. Destroy(gameObject.GetComponent<CompassMarkerScript>());

=======================================================================================
>Add a Compass Marker to game objects placed statically in scene (i.e. Direct Tracking):
=======================================================================================
If you don't want to change your Game Object's scripts as noted in the above 2
cases, you can simply track these objects for their lifetime by:

1) Create any object (e.g. a Cube)
2) Attach CompassMarkerScript
3) Assign a Sprite to "Marker Icon Custom" in the Inspector
3) Check any of the decoration flags you want for the marker
4) Check the "Direct Tracking" check box in the Inspector
5) Run the scene. The object will be tracked on the compass until the object is destroyed.

This is a good option for always-marked objects like unique enemies, landmarks, towns, etc.

==================================================================
>Add compass "Hints" when Object with Compass Marker is off screen:
==================================================================
Often Compass Markers fall outside the compass, and are not shown at all (e.g. When they are behind
you). For some super special objects, you may want to show a hint as to the direction even when the
object's Compass Marker is not on the compass. To do this, simply:

A) Set showOffScreenHints to true when adding the marker (3rd argument is true here)

e.g. gameObject.GetComponent<CompassMarkerScript>().showOffScreenHints = true;

or, B) Check the Show Off Screen Hints check box in the Inspector.

e.g. When using CompassMarkerScript for Direct Tracking (see above)

Note:
----
It is a good idea to use Marker Hints very sparingly. It is recommended that you really only 
show hints on the single most imporant object that is being tracked at any given time.

=============================================
>Creating Quest Markers for Multi-Part Quests:
=============================================
Please refer to "demoQuestMarker" Prefab, "DemoQuestMarkerScript" Script, and
the "compassQuestsAndTownsScene" Scene for details. These provide an example
of how you can create a trail of quest markers that are activated in order, as
the markers are reached by the player. This is great for multi-part quests in
your game.

===============
>Graphics Guide:
===============
Note:
----
It is strongly advised to ensure GUI textures are imported as Texture Type 
"Sprite(2D and GUI)", otherwise they will likely display incorrectly.

To switch or insert your own custom graphics:
1) If desired, create the following graphics using the provided templates.
2) Replace the following Sprite variables in the Inspector for TheCompass prefab:

>TheCompass Prefab:
------------------
-GUI Background
-Bookend Left and Right (if desired)
-GUI Ticks (if desired)
-Direction Labels (North, South, East, West)
-Up and Down arrows (if desired)
-Left and Right Hint arrows (if desired)

============================
>Graphical Element Breakdown:
============================
Note:
----
The resolutions are a guideline only. You can make these different sizes if you
wish. For example, the Hero skin uses larger Up/Down Arrow graphics because
they looked better than the small ones.

Background(512x66): For static compass elements.

Ticks(512x66): This image slides around as you rotate the compass.

Bookends(66x66): Provide visual flair, gives compass "sides". Refer to RPG skin
for best example of how this can add to the look of the skin.

Direction Labels(32x66): The North/South/East/West labels.

Marker Icons(32x66): The icon that represents a marker type (e.g. Quest Marker,
Monster, Location, etc.). See CompassMarker prefab and script to add your own 
types of markers and custom icon artwork.

Hint Arrows(32x32): The arrow icon that is drawn when "Show Hints" is enabled
on a Compass Marker.

Up/Down Arrows(14x8): Drawn above/below Marker Icons to indicate if game object
is above/below compass location.


============================================
>Suggested Values For Provided Compass Skins:
============================================

+---------------------------------+----------------------+----------------------+-----------------------+-------------------------+-------------------------+----------------+
| Skin     | Up/Down Arrow Offset | Distance Text Offset | Draw Up/Down Arrows? |  Pad Up/Down Arrows?  |     Draw Distance?      |  Draw Distance Outline? | Base Font Size |
+---------------------------------+----------------------+----------------------+-----------------------+-------------------------+-------------------------+----------------+
| Demo     |         36           |          63          |         Yes          |          Yes          |       Yes (White)       |        Yes (Black)      |       12       |
| Tactical |         26           |          56          |         Yes          |          Yes          |       Yes (Yellow)      |        Yes (Black)      |       12       |
| RPG      |         N/A          |          N/A         |         No           |          N/A          |           No            |            N/A          |       12       |
| Modern   |         36           |          64          |         Yes          |          Yes          |       Yes (White)       |     Yes (Dark Gray)     |       12       |
| Hero     |         -16          |          58          |         Yes          |          No           |    Yes (Light Blue)     |     Yes (Dark Gray)     |       12       |
| Flight   |         20           |          50          |         Yes          |          Yes          |       Yes (Green)       |            No           |       8        |
| Blocky   |         N/A          |          N/A         |         No           |          N/A          |           No            |            N/A          |       12       |
+---------------------------------+----------------------+----------------------+-----------------------+-------------------------+-------------------------+----------------+


===================
Other Package Notes:
===================

-All Prefabs in the Prefabs > CompassDemoPrefabs folder (e.g. "demoLevelStuff") are not critical for this package. 
-All assets in the Materials > CompassDemoMaterials folder are for dev/debug only, and are not critical for this package.
-DemoMonsterScript, DemoMarkerSpawnerScript, and DemoQuestMarkerScript and DemoHelper are for demo and code reference only, and are not critical for this package.

Feel free to delete the above items as you see fit.




If you have any problems using this package, or have tweaks or new features you'd like to see included, please let me know.

Thanks,
Richard

@whilefun
support@whilefun.com



THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

