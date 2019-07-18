# Guidance-Block-Script-Arguments
An in game script for space engineers, enhances torpedo mod to allow more complex series of actions.
See Steam page https://steamcommunity.com/sharedfiles/filedetails/?id=1793452866

Original Code written by : R.U.Sirius
https://steamcommunity.com/workshop/filedetails/?id=1408954946
Based off code by : Alyius
https://steamcommunity.com/workshop/filedetails/?id=1408954946
Multi argument code by : uglydisease

Torpedo Guidance Control Script

Editable Fields :
string GuidanceTag =
int launchSelectionType = 


Single Argument Commands : 
TRDM_On or TRDM_OFF 
Turns on the "Target Random Block" option for all Guidance Blocks with the associated GuidanceTag, good for targeting large ships/stations, not so good for small ones. Must be set before lock on for it to work.

TRDM ON or TRDM OFF
Turns off the "Target Random Block", same as above.

Lock
This will turn On and set a Lock on target for all Guidance Blocks with the associated GuidanceTag, based on whatever settings you selected for them.

Launch
Launches one missile/torpedo each time it's run. Which Guidance Block is launched is based upon launchSelectionType.

Off
Turns off all Guidance Blocks with associated GuidanceTag

Properties
Lists the properties that the guidance block has

MultArgument Commands : 

LAUNCH x
Launches x torpedos

DET_DIST x
Torpedo distance from target that causes detonation

CLEAR x
Clearance in seconds from launch that the torpedo becomes active

NAV_CONST x
Sets the navigation constant to x

MAX x
Maximum lockon distance

MIN x
Minimum block size of target for lockon

DMS x
Dead man switch distance

DETACH / THRUST / GYRO x
Sets the tag that the torpedo uses blank by default

{GPS coordinate}
Sets the gps for all torpedos to the coordinate. Paste coord into argument. 

TYPE x
Sets the lock on type x is a number as follows :
0 : LOCK_ON_FORWARD,
1 : LOCK_ON_AIM_BLOCK,
2 : LOCK_ON_GPS,
3 : GPS_MODE,
4 : GLIDE_MODE

Multiple Commands in an Argument :
Use ActionList followed by your commands seperated by semicolons. 
Note : Don't add extra spaces as these will get picked up as splitters between commands and argument.	

Example:
ActionList;type 0;Det_DIST 20;LAUNCH 2 
- Sets the fire type to Lock On Seek Forward
- Sets the detonation distance to 20m
- Fires two torpedos

ActionList;type 2;Det_DIST 20;GPS:uglydisease #2:-11091.43:2154.46:15634.07;LAUNCH 6
-Sets the fire type to Lock On In Gps Area
-Sets the detonation distance to 20m
-Sets the gps point in the guidances
-Fires six torpedos
