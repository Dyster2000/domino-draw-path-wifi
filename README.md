# DominoDrawPathWifi

The DominoDrawBluetooth directory is the Arduino code for the bluetooth path following robot. This requires an Arduino ESP32 nano chip.

The DominoPathDrawApp is a .net Maui app that is functional on an Android. Since it is Maui, it could be made to work on an iPhone as well. It just needs the iPhone specific bluetooth code added.

Wifi overview:
  To make the initial connection, the client app needs to know the address assigned to the domino robot when it connected to the wifi. In odrer to do this, the domino robot will broadcast it's address once a second using a UDP broadcast packet.
  To connect to the robot, the client app will listen for this packet until it receives it, and then start requesting status.

  The client communicates with the robot after that using REST API calls. The client will send a /status request once a second. Manual and Draw commands will send POST requests with the needed data.
