/*
	Intergrid Torpedo Lockon and Fire
	Written by uglydisease
	
	Special thanks to Alyius for the ray casting logic.
	NOTE : This requires my torpedo launch script on the grid with the torpedos to be able to actually launch torpedos.
	REEIEVER SCRIPT INFO :
	Guidance Block Launch Script Arguments https://steamcommunity.com/sharedfiles/filedetails/?id=1793452866
	
	- Setup
		You must have a program block with the word RAY in the name, I would just name this pb with Raycast
		You must have a camera with the word RAY in it, this will be the aimer
		Optionally you may have text panels to keep track of target and status, they must have names with both status / target and RAY
	- Usage
		Argument SCAN - Finds the nearest target in range in front of the camera
		Argument DISTANCE x - Adds x to the max range of the lidar
		Argument SCAN:FIRE - First does a scan then reports the gps back to any torpedo capable grid with my torpedo launch script in range
		Argument SEND - Sends the gps coordinates to all torpedos in range
		Argument SEND@{commands} - Sends the gps coordinates along with the commands to any torpedo capable grid in range can be any list of commands accepted by Guidance Block Launch Script Arguments
		
	Purpose :
		This script is designed to allow smaller craft and drones to call in torpedo strikes against targets, even targets that are hundreds of km away
		For example you could set up a base on the moon with a battery of torpedos send a tiny scout to earth, and fire a blizzard of torpedos at a target base.
*/

IMyCameraBlock lidar = null;
IMyTextSurface targetPanel = null;
IMyTextSurface statusPanel = null;
IMyProgrammableBlock thisPB = null;

const string TAG = "RAY";
const string STATUS = "STATUS";
const string TARGET = "TARGET";
const string LIDAR = "LIDAR";

double distance = 10000;

bool init = false;

Program() {
 Runtime.UpdateFrequency = UpdateFrequency.Update100;
}

void Main(string arguments, UpdateType updateSource) {
 if (!init) {
  var blocks = new List < IMyTerminalBlock > ();
  GridTerminalSystem.GetBlocksOfType < IMyTextSurface > (blocks, block => (block is IMyTextSurface));

  for (int i = blocks.Count - 1; i >= 0; i--) {
   if (blocks[i] is IMyTextSurface && blocks[i].CustomName.IndexOf(TAG) > -1) {
    if (blocks[i].CustomName.ToUpper().IndexOf(STATUS) > -1) {
     statusPanel = ((IMyTextSurface) blocks[i]);
    } else if (blocks[i].CustomName.ToUpper().IndexOf(TARGET) > -1) {
     targetPanel = ((IMyTextSurface) blocks[i]);
    }
   }
  }
  if (targetPanel == null) {
   Echo("WARNING : No targetPanel found, NOT REQUIRED");
  } else {
   targetPanel.ContentType = ContentType.TEXT_AND_IMAGE;
  }
  if (statusPanel == null) {
   Echo("WARNING : No statusPanel found, NOT REQUIRED");
  } else {
   statusPanel.ContentType = ContentType.TEXT_AND_IMAGE;
  }

  GridTerminalSystem.GetBlocksOfType < IMyCameraBlock > (blocks, block => (block is IMyCameraBlock));
  for (int i = blocks.Count - 1; i >= 0; i--) {
   if (blocks[i] is IMyCameraBlock && blocks[i].CustomName.IndexOf(TAG) > -1) {
    lidar = ((IMyCameraBlock) blocks[i]);
    break;
   }
  }
  if (lidar == null) {
   Echo("ERROR : No lidar found");
  }

  GridTerminalSystem.GetBlocksOfType < IMyProgrammableBlock > (blocks, block => (block is IMyProgrammableBlock));
  for (int i = blocks.Count - 1; i >= 0; i--) {
   if (blocks[i] is IMyProgrammableBlock && blocks[i].CustomName.IndexOf(TAG) > -1) {
    thisPB = ((IMyProgrammableBlock) blocks[i]);
    break;
   }
  }
  if (thisPB == null) {
   Echo("ERROR : unable to find a program block with the word RAY in it.");
  }


  if (lidar != null) {
   lidar.EnableRaycast = true;
   lidar.ApplyAction("OnOff_On");
  }

  init = (lidar != null && targetPanel != null && statusPanel != null);
  if (!init) return;
 }

 if (arguments.Length > 0) {
  string[] tokens = arguments.Trim().Split(':');


  string cmdToken = (tokens.Length > 0 ? tokens[0].Trim().ToUpper() : "");

  float fval;
  if (arguments.ToUpper().StartsWith("SEND")) {
   string[] commands = arguments.Split('@');
   string message = "ActionList;";
   message += thisPB.CustomData + ";";
   if (commands.Length > 1) {
    message += commands[1] + ";";
   }

   IGC.SendBroadcastMessage("TorpedoGuidancePB",
    message
   );
   Echo("Your target was broadcasted " + message);
  } else {
   switch (cmdToken) {
    case "DISTANCE":
     if (tokens.Length > 1 && tokens[1].Length > 0) {
      char sign = tokens[1][0];
      if (sign == '+' || sign == '-') {
       tokens[1] = tokens[1].Substring(1);
      }
      if (float.TryParse(tokens[1], out fval)) {
       distance = Math.Max((sign == '+' ? distance + fval : (sign == '-' ? distance - fval : fval)), 0);
      }
     }
     break;
    case "SCAN":
     MyDetectedEntityInfo entityInfo = lidar.Raycast(Math.Min(distance, lidar.AvailableScanRange));

     StringBuilder sbTarget = new StringBuilder();
     StringBuilder sbGPS = new StringBuilder();

     if (entityInfo.IsEmpty()) {
      sbTarget.Append("-- No Targets Found --");
     } else {
      sbTarget.Append("EnityId: ").Append(entityInfo.EntityId).Append('\n');
      sbTarget.Append("Distance: ").Append(Math.Round(Vector3D.Distance(lidar.GetPosition(), (entityInfo.HitPosition == null ? entityInfo.Position : entityInfo.HitPosition.Value)), 2)).Append("m\n");
      sbTarget.Append("Radius: ").Append(Math.Round(Vector3D.Distance(entityInfo.BoundingBox.Min, entityInfo.BoundingBox.Max), 2)).Append("m\n");
      sbTarget.Append("\nHit Position X: ").Append(Math.Round((entityInfo.HitPosition == null ? entityInfo.Position : entityInfo.HitPosition.Value).X, 2)).Append('\n');
      sbTarget.Append("Hit Position Y: ").Append(Math.Round((entityInfo.HitPosition == null ? entityInfo.Position : entityInfo.HitPosition.Value).Y, 2)).Append('\n');
      sbTarget.Append("Hit Position Z: ").Append(Math.Round((entityInfo.HitPosition == null ? entityInfo.Position : entityInfo.HitPosition.Value).Z, 2)).Append('\n');
      sbTarget.Append("\nCenter X: ").Append(Math.Round(entityInfo.Position.X, 2)).Append('\n');
      sbTarget.Append("Center Y: ").Append(Math.Round(entityInfo.Position.Y, 2)).Append('\n');
      sbTarget.Append("Center Z: ").Append(Math.Round(entityInfo.Position.Z, 2)).Append('\n');

      //sbGPS.Append("GPS:Hit Position:").Append(VectorToString((entityInfo.HitPosition == null ? entityInfo.Position : entityInfo.HitPosition.Value), 2)).Append(":\n");
      sbGPS.Append("GPS:Center:").Append(VectorToString(entityInfo.Position, 2)) /*.Append(":\n")*/ ;
     }

     targetPanel.WriteText(sbTarget);
     thisPB.CustomData = sbGPS.ToString();
     //SEND@TYPE 2;LAUNCH
     if (tokens.Length > 1 && tokens[1] == "FIRE") {
      string message = "ActionList;TYPE 2;" + sbGPS.ToString() + ";" + "LAUNCH";
      IGC.SendBroadcastMessage("TorpedoGuidancePB",
       message
      );
      Echo("Your target was broadcasted " + message);
     }

     break;
   }
  }
 }

 StringBuilder sb = new StringBuilder();

 sb.Append("Total Charges: ").Append(Math.Round(lidar.AvailableScanRange, 0)).Append("m\n");
 sb.Append("Scan Distance: ").Append(Math.Round(distance, 0)).Append("m\n");

 float coveragePercent = (float) Math.Min(lidar.AvailableScanRange / distance * 100, 100);
 sb.Append("\nCoverage: ").Append(Math.Round(coveragePercent, 0)).Append("%\n");
 DrawProgressBar(sb, statusPanel.FontSize, coveragePercent).Append('\n');
 sb.Append("Available Scans Count: ").Append(Math.Floor(lidar.AvailableScanRange / distance)).Append('\n');

 statusPanel.WriteText(sb);
}

StringBuilder DrawProgressBar(StringBuilder sb, float fontSize, float percent) {
 int total = (int)((1f / fontSize) * 72) - 2;
 int filled = (int) Math.Round(percent / 100 * total);
 sb.Append('[').Append('I', filled).Append('`', total - filled).Append(']');
 return sb;
}

string VectorToString(Vector3D vector, int decimals) {
 return Math.Round(vector.GetDim(0), decimals) + ":" + Math.Round(vector.GetDim(1), decimals) + ":" + Math.Round(vector.GetDim(2), decimals);
}