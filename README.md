# BveSpeedInterface

An InputDevicePlugin for BveTs 5/6, enabling the direct changing of the train speed and position via UDP.

This is achieved by acquiring references of the ToolStripTextBox inside BveTs's "Time and position" debug window with publicly available Windows Forms API calls, and then changing their values and firing WinForm events on them.

## UDP Interface

This plugin listens on UDP port 10492. The port can be changed by modifying InputDeviceImpl.cs recompiling.

You can control the plugin by sending UDP messages (all lowercase).

- `setspeed <NUMBER>`  
Sets the speed. Integer only, due to BVE limitations.

- `setspeedauto <0/1>`  
If 0, the speed is only set once when `setspeed`. If 1, the speed is set every frame, making the train keep the speed specified by `setspeed`. Default is 0. Send this message once and its value will be remembered.

- `setposition <NUMBER>`
- `setpositionauto <0/1>`  
Same as above, but for position.

- `zeroaxis`  
Returns reverser, power and brake handles to neutral, in order to reduce the effect of the brake of the in-game train on the speed.
