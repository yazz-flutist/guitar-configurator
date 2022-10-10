
## Controller Info

- Type: Gamepad (1)
- Subtype: 23, not part of XInput standards
- Hardware IDs:
  - VID: 
  - PID: 

## Input Info

Face buttons work like a standard Xbox 360 controller.

Tables:

- Left table:

  | Action  | Input                 |
  | :-----  | :---:                 |
  | Scratch | Left stick X          |
  | Green   | A + LT `0b_0000_0001` |
  | Red     | B + LT `0b_0000_0010` |
  | Blue    | X + LT `0b_0000_0100` |

- Right table:

  | Action  | Input                 |
  | :-----  | :---:                 |
  | Scratch | Left stick Y          |
  | Green   | A + RT `0b_0000_0001` |
  | Red     | B + RT `0b_0000_0010` |
  | Blue    | X + RT `0b_0000_0100` |

- Scratching:
  - Positive is clockwise, negative is counter-clockwise.
  - Only uses a tiny range (around -64 to +64 in my testing), presumably so as to not register on the Xbox 360 menus.

Crossfader: Right stick Y

- Range is inverted: left is positive, right is negative.

Effects knob: Right stick X

- Clockwise increments, counter-clockwise decrements.
- Wraps around when reaching maximum or minimum.

Euphoria button: Y button

- Light control: Right vibration
  - In my testing, first turns on at around 7936 (`0b_0001_1111_0000_0000`), and maxes out at 65535 (`0b_1111_1111_1111_1111`).