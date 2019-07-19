string LCD_name = "LCD Panel";
public
const string TAG = "[IGC]";

//===================================================================

IMyBroadcastListener listener;
IMyTextSurface panel;

// unique name, see bellow
const string callbackMagic = "<incoming_broadcast_action>";
string message = "ActionList;type 3;Det_DIST 20;GPS:uglydisease #1:9122.15:-7181.37:14153.59:;LAUNCH 1";

public Program() {
 var blocks = new List < IMyTerminalBlock > ();
 GridTerminalSystem.GetBlocksOfType < IMyTextSurface > (blocks, block => (block is IMyTextSurface));

 for (int i = blocks.Count - 1; i >= 0; i--) {
  if (blocks[i] is IMyTextSurface && blocks[i].CustomName.IndexOf(TAG) > -1) {
   panel = ((IMyTextSurface) blocks[i]);
   break;
  }
 }
 if (panel == null) {
  Echo("No panel found with tag " + TAG);
 } else {
  panel.ContentType = ContentType.TEXT_AND_IMAGE;
 }

 // Register this listener for BROADCAST_LOCATION tagged messages
 listener = IGC.RegisterBroadcastListener("Torpedo Payload Guidance");
 // What argument will the PB be run when received one (or more) messages
 listener.SetMessageCallback(callbackMagic);
}

public void Main(string argument, UpdateType updateSource) {
 if (panel == null) {
  Echo("No panel found with tag " + TAG);
 }
 // If we received message, PB is run with argument callbackMagic
 // If you don't set message callback, you have to check for pending messages
 // by yourself every few moments.
 if (argument == callbackMagic) {
  if (panel == null) {
   Echo("LCD not found! Cannot show message.");
   //return;
  }
  // Clear screen
  panel.WriteText("");

  // There can be multiple messages pending. We will show them all.
  while (listener.HasPendingMessage) {
   MyTuple < string, Vector3D, string > data;

   // Accept next message pending.
   MyIGCMessage msg = listener.AcceptMessage();

   try {
    // msg.Data is 'object'. We try to cast it to a known type.
    data = (MyTuple < string, Vector3D, string > ) msg.Data;
   } catch (InvalidCastException) {
    // When we fail, there is not much to do, show error message, and continue.
    panel.WriteText("Incoming broadcast:\n  #Unknown received data!\n");
    continue;
   }

   // Parse the data to human-readable text ad print it on screen
   panel.WriteText(
    "Incoming broadcast:\n" +
    $ "  Location: {Vector3D.Round(data.Item2)}\n" +
    $ "  Ship Name: {data.Item1}\n" +
    $ "  Sender address: {msg.Source}" +
    (data.Item3.Length > 0 ? $ "\n  Message: {data.Item3}" : "") + "\n",
    append: true);
  }
  // Do not continue (send message), otherwise endless loop of messages.
  return;
 }

 // We can broadcast anything immutable.
 // (You can't receive reference to that object in the other PB, change it there,
 // and watch the changes in this PB)
 // Examples: MyTuple, Vector3D, Quaternions, ImmutableArray/..List/..Queue etc., string
 // You can't send: your own structs/classes, references (blocks), List, MyDetectedEntityInfo, MyWaypointInfo

 // We probably run this PB to send message & location.
 // For this example, we will send name of ship, location (of the PB) and custom message.
 IGC.SendBroadcastMessage("TorpedoGuidancePB",
  message
 );
 Echo("Your position was broadcasted" + (argument.Length > 0 ? $ " with message: '{argument}'." : "."));
}