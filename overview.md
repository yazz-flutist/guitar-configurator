# Start
When the tool is opened, the user is presented with a list of microcontrollers and already configured controllers.

# Non Programmed microcontroller
The user can then pick a non programmed microcontroller

# Controller type choice
The user is presented with a form where they can set the type of controller they are configuring, with the following options

## Input type
- Direct (the user will be wiring inputs direcly to the pins on the microconroller)
- Wii (the user will be plugging a wii extension into the microcontroller)
- PS2 (the user will be plugging a PS2 controller into the microcontroller)

## Output type
- Generic Game controller (A controller compatible with PC, Wii, Xbox360, PS3)
  - They can then pick if they are emulating a guitar, drums or a standard controller
- XInput Game controller (A controller that fits the XInput spec, some games require it)
  - They can then pick if they are emulating a guitar, drums or a standard controller
- Keyboard / Mouse (Emulates a keyboard and mouse)
- MIDI (Emulates a musical device that can be piped into a music DAW)

## Advanded or simple
The user should also be able to pick if they want to use advanced mode or simple mode, this is used later for binding
Then they can hit a continue button

# Binding interface (The user is brought straight here if they pick an already configured controller)
The user is then dropped into an interface where they can pick different inputs based on the type of controller input

## Inputs
- Direct
  - Analog input (The user picks an analog microcontroller pin, and then its analogue value can then be mapped)
  - Digital input (The user picks a digital microcontroller pin, and then its digital signal can be mapped)
- Wii
  - Advanced mode
    - Guitar Analog input (Joystick x, Joystick y, Whammy)
    - Guitar Button input (Green, Red, Yellow, Blue, Orange, start, select, strum up, strum down)
    - Classic controller button (A, B, X, Y, Up, Down, Left, Right, zl, zr, Plus, Minus, Home)
    - Classic controller axis (left trigger, right trigger, left joystick x, left joystick y, right joystick x, right joystick y)
    - Nunchuk button (C, Z)
    - Nunchuk axis (Joystick, Acceleration (X,Y,Z), Rotation (X,Y)) (There is no Z for rotation as it is pieced together based on the acceleration, and that can only be done on two axis)
    - Wii UDraw Tablet axis (Pen X, Pen Y, Pen Pressure)
    - Wii UDraw Tablet buttons (Pen click, Pen button 1, Pen button 2)
    - Drawsome tablet axis (Pen x, Pen Y, Pen Pressure)
    - Wii TaTaCon Controller buttons (Left Drum Rim, Left Drum Center, Right Drum Rim, Right Drum Center)
    - DJ Hero controller button (Euphoria button, Right Green, Right Red, Right Blue, Left Green, Left Red, Left Blue, Plus, Minus)
    - DJ Hero controller axis (Crossfade slider, Joystick x, Joystick y, Left Turntable position, Right turntable position)
    - Drum Button (Green, Red, Yellow, Blue, Orange, Bass kick pedal, hi-hat pedal, minus, plus)
    - Drum Axis (Green, Red, Yellow, Blue, Orange, Bass kick pedal, hi-hat pedal, joystick x, joystick y)
  - Simple Mode
    - Expose all the above inputs mapped to a standard controller, so
    - Buttons (A, B, X, Y, Up, Down, Left, Right, Lt, Rt, Start, Select, Home)
    - Axis (left trigger, right trigger, left joystick x, left joystick y, right joystick x, right joystick y)
- PS2
  - Advanced mode
    - Digital / Dualshock / Dualshock 2 buttons (Cross, Square, Triangle, Circle, Up, Down, Left, Right, Lt, Rt, Start, Select, Home)
    - Dualshock / Dualshock 2 Axis (left trigger, right trigger, left joystick x, left joystick y, right joystick x, right joystick y)
    - Dualshock 2 Axis, above plus pressure for (ACross, Square, Triangle, Circle, Up, Down, Left, Right, Lt, Rt, Start, Select, Home)
    - GunCon buttons (Left button, right button, Trigger)
    - GunCon axis (HSync, VSync)
    - Mouse button (Left, right)
    - Mouse Axis (X, Y)
    - NegCon Button (Left, Right, Up, Down, Start, A, B, I, II)
    - NegCon Axis (Twist) plus pressure for (Left, Right, Up, Down, Start, A, B, I, II)
    - JogCon Button (Left, Right, Up, Down, Start, Select, Cross, Square, Triangle, Circle)
    - JogCon Axis (Wheel rotation) plus pressure for (Left, Right, Up, Down, Start, Select, Cross, Square, Triangle, Circle)
    - Guitar Axis (Whammy)
    - Guitar Button (Green, Red, Yellow, Blue, Orange, Strum up, Strum Down, Plus, Minus, Star power tilt)
  - Simple Mode
    - Expose all the above inputs mapped to a standard controller, so
    - Buttons (A, B, X, Y, Up, Down, Left, Right, Lt, Rt, Start, Select, Home)
    - Axis (left trigger, right trigger, left joystick x, left joystick y, right joystick x, right joystick y)
- Analog to digital (allow a user to map an axis to a digital action once it goes past a certian threshold)
  - This gets used for mapping the joystick on guitars to a dpad, as clone hero only works with dpads, but it could also be useful for other things
- Digital to analog (allow a user to map a digital signal to an analog signal aka on maps to one value and off to another)
  -  This will be useful for MIDI as all midi actions are analog

## Outputs
When an input is selected, a user can then bind it to any output, depending on the type of controller they have chosen
- For a generic controller or xinput controller, they can bind to
  - buttons: (a, b, x, y, up, down, left, right, l2, r2, left stick click, right stick click, start, select, home, capture)
  - axis: (l1, r1, left joystick x, left joystick y, right joystick x, right joystick y)
- For a mouse or keyboard they can bind to
  - buttons
    - any key
    - mouse buttons
  - axis
    - mouse x
    - mouse y
    - scroll x
    - scroll y
- For MIDI they can bind to
  - A midi note (where the axis value is converted to the veolcity of the note)
  - A midi control signal (where the axis value is converted to the value sent via CC)

## Examples (Simple Mode)
So for example, a user could pick wii, simple mode, generic controller
and then map the left joystick x on a wii controller to the left joystick x of the generic controller

## Examples (Advanced mode)
If the user picked advanced mode, they could
map the whammy of a guitar hero controller to the right joystick x of the generic controller
and then map the right joystick x of the classic controller to the right joystick x of the generic controller

## Defaults
I think when someone picks one of the following combinations, we should provide some bound outputs by default as we are mapping one controller to another so it is obvious
- Input of Wii and output of Generic Controller
- Input of Wii and output of XInput Controller
- Input of PS2 and output of Generic Controller
- Input of PS2 and output of XInput Controller