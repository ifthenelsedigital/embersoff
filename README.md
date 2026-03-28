BEGINNING OF THE GAME: The game is started from the ITELogo Splash Scene. 
SCRIPTING LANGUAGE: C# 
 
SCENES STRUCTURE: The game has scenes split as: 
1.	2 Logo Splash Screens (ITE Logo and Embers Off Logo) 
2.	1 Main menu 
3.	1 Training Scene 
4.	1 Loading Scene 
5.	1 Story Scene  
6.	6 ACT Scenes  
7.	1 Game Completion Scene (End) 
8.	1 Exit Splash Screen 
 
1.0.0 	MAIN MENU 
The Main menu is simply designed with simple buttons. 
Right-Top – Exit 
Right-Bottom – About 
Left-Bottom – Local Data 
All UI Callbacks and system operations in the Menu scene are controller by MenuController.cs

 
1.0.1 	SETTINGS  
 
  
All these options have their usual meaning. 
More settings with simple meanings can be found under the Graphics, Image Effects, Audio tabs. 
All these settings are stored as PlayerPrefs.Set(). Refer to the script: Settings.cs in the function ResetDefaultSettings() to check what item is set in which type (integer, float, string etc). In every act, these settings are automatically applied in the Awake() void. 
 
1.0.2 	DIFFICULTY AND ACT SELECTION 
When a player clicks play, the game asks if he wants to continue or select an Act.  
It’s important to know that the difficulty level selected is stored in the PlayerPrefs right from the Main Menu as PlayerPrefs.SetInt(“Difficulty”, int). 
DIFFICULTY int:  
•	0 – Normal
• 1 – Insane 
•	2 – Reality 
This difficulty is automatically set whenever a player starts an act and the enemy spawn rate and damage are affected. 
 
1.1.0 	STORY SCENE 
After the player has chosen the difficulty, the script (Menu.cs) automatically checks if the player has watched the story of that particular act before. If not, it opens the scene – Story. 
This scene plays the story of the chosen act using a pre-rendered video.  
Also, the scene provides options to turn on/off subtitles [En-US] and to skip the story.  
 
The story can be paused if Space Bar is pressed – then these options appear. 
 
1.1.1 	ACT 
After the player completes/skips the story, the particular Act scene is opened. Each act has a different type of combat style. The whole scene is controlled by ActController class (example – Act1Controller.cs) 
 
1.1.2 	PLAYER MOVEMENT & OTHER CONTROLS 
The player movement is a result of 2 scripts – Player.cs and FirstPersonController.cs 
[The player prefab has already setup with these scripts and all dependencies] 
 
The FirstPersonController.cs is referenced as ‘MainController’ in the Player.cs 
The Player.cs provides values of inputs to the MainController in the Update() void. 
 
This results in the running of the player in particular direction of input according to the gun.                 Also, Player.cs operates on the viewing of the world by calling a void – LookRotation() [Of   MainController].  

Other controls: 
1.	Right Click – Open Scope 
2.	Left click – Fire 
3.	W, A, S, D - Move 
4.	Middle Mouse and 3 – Grenade 
5.	1 – Primary Weapon [All rifles are Primary Weapons] 
6.	2 – Secondary Weapon [All Pistols/Revolvers are Secondary Weapons] 7. 4 – Melee Weapon [The game currently has only 1 melee weapon – Knife] 
8.	E – Pick up the first item available nearby. 
9.	F – Pick up the second item available nearby. 
10.	R – Reload 
11.	G – Settings 
12.	Esc – Open an exit confirmation 
13.	T – Show Tasks 
14.	Space – Jump 
15.	C – Crouch 
16.	Z – Prone 
17.	Scroll – Change Primary to Secondary, Secondary to Melee, and Melee to Grenade. 
18.	*Some others such as X or P might be needed for particular tasks in particular acts for which the player will be requested to Press. This is checked by a script called PressChecker.cs whenever required. 
 
All these controls are confirmed in the Update() void of Player.cs itself. 
  
1.1.3 	ENEMY AI 
The game spawns enemy soldiers through the ActControllers. The soldier controls itself by script: 
1.	Act1Soldier.cs [For Act-1] 
2.	Soldier.cs [For all other acts]  
 
Why different for Act-1? 
In Act-1,  player’s mates are British and enemies are Japanese. But in all other acts, mates are Indian and enemies are British. 
All acts have set their specific soldier prefabs such as Act2 has BritainSoldier(Winchester). These prefabs can be found under Characters&Weapons/Characters/Britain/Soldiers. 
 
The AI works in 2 ways, Attacking and Defensive, controlled by ActControllers.  
In Defensive mode, the soldier reaches his cover point (when requested by the ActController) and fight from there without moving. 
In Attacking mode, the soldier searches for player using RayCast method to check if the player is in sight. While the player is not in sight, it chases the player by a coroutine – ChaseTarget(). 
 
 
Here, ‘nav’ is a variable reference for the NavMeshAgent and the target is the Player transform. 
When the player is in sight, the soldier uses Attack() in loop until a specific value of damage according to the difficulty is reached.  
 
The number of accurate shots a soldier can shoot to the player is called hitTime in this script. 
The soldier, after completing one round of fire on the player, automatically goes to a cover point nearby (if not occupied by some other soldier). 
 
1.1.4 	NEARBY ITEMS 
Whenever the player is near an enemy loot bag or an AmmoBox, a panel like this would open: 
  
Keys E and F can be pressed to pick an item numbered 1 and 2 in this panel respectively.  X or Escape can be pressed to close this panel. 
 
This nearby panel is opened by a script called CollectablesManager.cs whenever a player comes in a specific range of the loot bag/ammobox. 
This script also holds the data of items stored in that particular loot bag or an AmmoBox. 
  
 
1.1.5 	ITEMS AND ICONS 
All the data of the game’s weapons and their respective icons which appear on the UI are stored inside the script ItemsAndIcons.cs 
This script is usually referenced as ‘iai’ in all other scripts & is added to the Controller game object in every act. 

 
1.1.6 	UI ANIMATIONS 
Almost all buttons of the game have these animations. These buttons have 3 animation states: 1. Idle 
2.	Hover 
3.	Pressed 
These states automatically get triggered by a script attached on them which uses the voids OnPointerEnter, OnPointerExit, OnPointerDown, and OnPointerUp (basic Unity Event Handlers). 
 

 
                                     
 
 
