/*
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
	TRDM_On or TRDM ON 
	Turns on the "Target Random Block" option for all Guidance Blocks with the associated GuidanceTag, good for targeting large ships/stations, not so good for small ones. Must be set before lock on for it to work.

	TRDM_Off or TRDM OFF
	Turns off the "Target Random Block".

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
	Sets the gps for all torpedos to the coordinate, 
	
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
	
	Possible Additions :
	- Implement timing mechanic between commands
	- Open to suggestions if any body is interested in adding functionality
*/

string GuidanceTag = "Torpedo Payload Guidance"; //Name of the guidance block(s) you wish to control
int launchSelectionType = 2; //0 = Any, 1 = Closest, 2 = Furtherest

public void Main(string argument) {
 //Block Declarations
 List < IMyRadioAntenna > ant = new List < IMyRadioAntenna > ();
 List < IMyRadioAntenna > gdblocks = new List < IMyRadioAntenna > ();
 GridTerminalSystem.GetBlocksOfType < IMyRadioAntenna > (ant);
 for (int i = 0; i < ant.Count; i++) {
  if (ant[i].CustomName.IndexOf(GuidanceTag) > -1) {
   gdblocks.Add(ant[i]);
  }
 }
 //Logic
 IMyRadioAntenna torpGuidance = null;

 switch (launchSelectionType) {
  case 1:
   torpGuidance = ReturnClosest(gdblocks) as IMyRadioAntenna;
   break;
  case 2:
   torpGuidance = ReturnFurthest(gdblocks) as IMyRadioAntenna;
   break;
  default:
   torpGuidance = ReturnAny(gdblocks) as IMyRadioAntenna;
   break;
 }
 if (torpGuidance == null) {
  Echo("No Guidance block named \n\n\r\"" + GuidanceTag + "\"\n\n\rEither the Guidance block is not \nproperly labelled or none exists. \n\nCheck the GuidanceTag in the script \nto see what tag to use.");
  return;
 } else {
  Echo("");
 }
 /* Multi argument operations 
  *	Written by uglydisease
  */
 string[] actionList = null;
 if (argument.ToUpper().StartsWith("ACTIONLIST")) {
  actionList = argument.ToUpper().Split(';');
 }
 bool multiAction = (actionList != null);
 int actions = multiAction ? actionList.Length : 1;

 for (int j = 0; j < actions; j++) {
  string[] parts = null;
  if (!multiAction) {
   parts = argument.ToUpper().Split(' ');
   if (parts.Length > 1) Echo("ACTION : " + parts[0] + ", " + parts[1]);
  } else {
   argument = actionList[j];
   parts = argument.ToUpper().Split(' ');
   if (parts.Length > 1) Echo("ACTION : " + parts[0] + ", " + parts[1]);
  }
  // Thanks to Lexx Lord for help with the vector3D formatting
  if (argument.StartsWith("GPS")) {
   Vector3D target;
   String targetName;
   bool success = ParseGPSCoordinates(argument, out target, out targetName);
   if (success) {
    for (int i = 0; i < gdblocks.Count; i++) {
     gdblocks[i].ApplyAction("OnOff_On");
     gdblocks[i].SetValue < Vector3D > ("Adn.PropertyGPSCoordinates", target);
     gdblocks[i].ApplyAction("OnOff_Off");
    }
   }
  } else if (parts.Length > 1) {
   string command = parts[0];
   string unit = parts[1];

   // Launch number of torpedos
   if (command.ToUpper() == "LAUNCH") {
    for (int i = 0; i < Min(gdblocks.Count, Int32.Parse(unit)); i++) {
     gdblocks[i].ApplyAction("OnOff_On");
     gdblocks[i].ApplyAction("Adn.ActionLaunchMissile");
    }
   }
   // Det Distance
   else if (command.ToUpper() == "DET_DIST") {
    for (int i = 0; i < gdblocks.Count; i++) {
     gdblocks[i].ApplyAction("OnOff_On");
     gdblocks[i].SetValueFloat("Adn.PropertyProximityDistance", Convert.ToSingle(unit));
     gdblocks[i].ApplyAction("OnOff_Off");
    }
   }
   // Clearance in seconds 
   else if (command.ToUpper() == "CLEAR") {
    for (int i = 0; i < gdblocks.Count; i++) {
     gdblocks[i].ApplyAction("OnOff_On");
     gdblocks[i].SetValueFloat("Adn.PropertyLaunchSeconds", Convert.ToSingle(unit));
     gdblocks[i].ApplyAction("OnOff_Off");
    }
   }
   // Navigation Constant
   else if (command.ToUpper() == "NAV_CONST") {
    for (int i = 0; i < gdblocks.Count; i++) {
     gdblocks[i].ApplyAction("OnOff_On");
     gdblocks[i].SetValueFloat("Adn.PropertyPropNavConstant", Convert.ToSingle(unit));
     gdblocks[i].ApplyAction("OnOff_Off");
    }
   }
   // Max lock on distance
   else if (command.ToUpper() == "MAX") {
    for (int i = 0; i < gdblocks.Count; i++) {
     gdblocks[i].ApplyAction("OnOff_On");
     gdblocks[i].SetValueFloat("Adn.PropertyLockOnMaximumDistance", Convert.ToSingle(unit));
     gdblocks[i].ApplyAction("OnOff_Off");
    }
   }
   // Min grid size to lock on to
   else if (command.ToUpper() == "MIN") {
    for (int i = 0; i < gdblocks.Count; i++) {
     gdblocks[i].ApplyAction("OnOff_On");
     gdblocks[i].SetValueFloat("Adn.PropertyLockOnMinimumGridSize", Convert.ToSingle(unit));
     gdblocks[i].ApplyAction("OnOff_Off");
    }
   }
   // Dead Man Switch Distance
   else if (command.ToUpper() == "DMS") {
    for (int i = 0; i < gdblocks.Count; i++) {
     gdblocks[i].ApplyAction("OnOff_On");
     gdblocks[i].SetValueFloat("Adn.PropertyDMSDistance", Convert.ToSingle(unit));
     gdblocks[i].ApplyAction("OnOff_Off");
    }
   }
   // Detach merge block tag
   else if (command.ToUpper() == "DETACH") {
    for (int i = 0; i < gdblocks.Count; i++) {
     gdblocks[i].ApplyAction("OnOff_On");
     gdblocks[i].SetValue("Adn.PropertyDetachPortTag", unit);
     gdblocks[i].ApplyAction("OnOff_Off");
    }
   }
   // Thrusters tag
   else if (command.ToUpper() == "THRUST") {
    for (int i = 0; i < gdblocks.Count; i++) {
     gdblocks[i].ApplyAction("OnOff_On");
     gdblocks[i].SetValue("Adn.PropertyThrustersTag", unit);
     gdblocks[i].ApplyAction("OnOff_Off");
    }
   }
   // Gyros tag
   else if (command.ToUpper() == "GYRO") {
    for (int i = 0; i < gdblocks.Count; i++) {
     gdblocks[i].ApplyAction("OnOff_On");
     gdblocks[i].SetValue("Adn.PropertyGyroscopesTag", unit);
     gdblocks[i].ApplyAction("OnOff_Off");
    }
   }
   // Core GPS setter
   // Format GPS {X:-10633.45}{Y:1317.51}{Z:12883.83}
   else if (command.ToUpper() == "GPS") {
    Vector3D target;
    Vector3D.TryParse(unit, out target);

    for (int i = 0; i < gdblocks.Count; i++) {
     gdblocks[i].ApplyAction("OnOff_On");
     gdblocks[i].SetValue < Vector3D > ("Adn.PropertyGPSCoordinates", target);
     gdblocks[i].ApplyAction("OnOff_Off");
    }
   } else if (command.ToUpper() == "TYPE") {
    for (int i = 0; i < gdblocks.Count; i++) {
     gdblocks[i].ApplyAction("OnOff_On");
     gdblocks[i].SetValue("Adn.PropertyLockOnType", Int32.Parse(unit));
     gdblocks[i].ApplyAction("OnOff_Off");
    }
   }


   // Backwards comptability 
   else if (command.ToUpper() == "TRDM") {
    if (unit.ToUpper() == "ON") {
     for (int i = 0; i < gdblocks.Count; i++) {
      gdblocks[i].ApplyAction("OnOff_On");
      gdblocks[i].SetValueBool("Adn.PropertyTargetRandomGridBlock", true);
      gdblocks[i].ApplyAction("OnOff_Off");
     }
    } else if (unit.ToUpper() == "OFF") {
     for (int i = 0; i < gdblocks.Count; i++) {
      gdblocks[i].ApplyAction("OnOff_On");
      gdblocks[i].SetValueBool("Adn.PropertyTargetRandomGridBlock", false);
      gdblocks[i].ApplyAction("OnOff_Off");
     }
    }
   }
  }

  // Original operations
  else {
   // Lists Properties
   if (argument.ToUpper() == "PROPERTIES") {
    List < ITerminalAction > termActions = new List < ITerminalAction > ();
    gdblocks[0].GetActions(termActions);
    foreach(var action in termActions) {
     Echo(action.Id + " " + action.Name);
    }
    List < ITerminalProperty > properties = new List < ITerminalProperty > ();
    gdblocks[0].GetProperties(properties);
    foreach(var property in properties) {
     Echo(property.Id);
    }
   }
   if (argument.ToUpper() == "LOCK") {
    for (int i = 0; i < gdblocks.Count; i++) {
     gdblocks[i].ApplyAction("OnOff_On");
     gdblocks[i].ApplyAction("Adn.ActionLockOnTarget");
    }
   }
   if (argument.ToUpper() == "OFF") {
    for (int i = 0; i < gdblocks.Count; i++) {
     gdblocks[i].ApplyAction("OnOff_Off");
    }
   }
   if (argument.ToUpper() == "LAUNCH") {
    torpGuidance.ApplyAction("OnOff_On");
    torpGuidance.ApplyAction("Adn.ActionLaunchMissile");
   }
   if (argument.ToUpper() == "TRDM_ON") {
    for (int i = 0; i < gdblocks.Count; i++) {
     gdblocks[i].ApplyAction("OnOff_On");
     gdblocks[i].SetValueBool("Adn.PropertyTargetRandomGridBlock", true);
     gdblocks[i].ApplyAction("OnOff_Off");
    }
   }
   if (argument.ToUpper() == "TRDM_OFF") {
    for (int i = 0; i < gdblocks.Count; i++) {
     gdblocks[i].ApplyAction("OnOff_On");
     gdblocks[i].SetValueBool("Adn.PropertyTargetRandomGridBlock", false);
     gdblocks[i].ApplyAction("OnOff_Off");
    }
   }
  }
 }
}
//Control block selection based of Launch Selection Type//
IMyRadioAntenna ReturnAny(List < IMyRadioAntenna > gdblocks) {
 if (gdblocks.Count > 0) {
  Random rnd = new Random();
  return gdblocks[rnd.Next(0, gdblocks.Count)];
 }
 return null;
}

IMyRadioAntenna ReturnClosest(List < IMyRadioAntenna > gdblocks) {
 double currDist = 0;
 double closestDist = Double.MaxValue;
 IMyRadioAntenna closestBlock = null;

 for (int i = 0; i < gdblocks.Count; i++) {
  currDist = (gdblocks[i].GetPosition() - Me.GetPosition()).Length();
  if (currDist < closestDist) {
   closestDist = currDist;
   closestBlock = gdblocks[i];
  }
 }

 return closestBlock;
}

IMyRadioAntenna ReturnFurthest(List < IMyRadioAntenna > gdblocks) {
 double currDist = 0;
 double furthestDist = 0;
 IMyRadioAntenna furthestBlock = null;

 for (int i = 0; i < gdblocks.Count; i++) {
  currDist = (gdblocks[i].GetPosition() - Me.GetPosition()).Length();
  if (currDist > furthestDist) {
   furthestDist = currDist;
   furthestBlock = gdblocks[i];
  }
 }

 return furthestBlock;
}
IMyTerminalBlock GetClosestBlockFromReference(List < IMyTerminalBlock > checkBlocks, IMyTerminalBlock referenceBlock) {
 IMyTerminalBlock checkBlock = null;
 double prevCheckDistance = Double.MaxValue;

 for (int i = 0; i < checkBlocks.Count; i++) {
  double currCheckDistance = (checkBlocks[i].GetPosition() - referenceBlock.GetPosition()).Length();
  if (currCheckDistance < prevCheckDistance) {
   prevCheckDistance = currCheckDistance;
   checkBlock = checkBlocks[i];
  }
 }

 return checkBlock;
}

public bool ParseGPSCoordinates(string vectorStr, out Vector3D parsedVector, out string parsedName, int start = 0) {
 string[] tokens = vectorStr.Split(':');
 parsedVector = new Vector3D();
 parsedName = "";

 if (tokens.Length < start + 5) {
  return false;
 }

 parsedName = tokens[start + 1];

 double result;

 if (double.TryParse(tokens[start + 2], out result)) {
  parsedVector.SetDim(0, result);
 } else {
  return false;
 }

 if (double.TryParse(tokens[start + 3], out result)) {
  parsedVector.SetDim(1, result);
 } else {
  return false;
 }

 if (double.TryParse(tokens[start + 4], out result)) {
  parsedVector.SetDim(2, result);
 } else {
  return false;
 }

 return true;
}

public int Min(int num1, int num2) {
 return (num1 < num2 ? num1 : num2);
}