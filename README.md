# Simulacrum
 KukaVarProxy plugin for grasshopper
 
 Made by: [Nicolas Kubail Kalousdian](https://github.com/t3ch-support) and [Simon Lut](https://github.com/simonlut)
 
 Read and write global variables to a KUKA Robot arm using [KukaVarProxy](https://github.com/ImtsSrl/KUKAVARPROXY), directly from Grasshopper.

## Setup 
Kuka:
1. Start the Kuka and run your .src files. Make sure to put all your global variables in a separate .dat file!
2. Switch the KUKA to expert mode. 
3. Use alt-tab to go to a different program until the start button appears in the left-button corner.
4. Go to 'mein bilder' from the start menu, and then navigate to the desktop.
5. Start KukaVarProxy.exe
6. Go back to Kuka by using alt-tab, you are always able to switch back-and-forth this way.
7. Make sure to initialize the global variables you defined in your .dat file in your .src file.
8. Run the .src file to the point that the global variables are initialized. They are now accessible by KukaVarProxy.

Laptop:
1. Drag the .gha file in Simulacrum\bin in your libraries folder of Grasshopper.
2. Plug-in the Robot Ethernet Cable in your laptop.
2. In windows: search for 'view network connections'.
3. Right-click Ethernet and click on properties.
4. Click on 'Internet Protocol Version 4' and go to properties.
5. Click on 'Use the following IP Address' and type in the Robot IP Address
(make sure the last number you type in for 'IP Address' is different to the actual robot IP address.
6. Open the example file in grasshopper and make sure that the IP address is the robot IP address and the port is 7000.
7. Click connect, and use the Netstat monitor to see if the connection is established.
8. ??? profit

