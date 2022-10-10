I2C ID: Left Turntable: 0x0E
I2C ID: Right Turntable: 0x0D
TWI Freq: 250000hz

There is a switch hidden in the turntable that is pressed depending on the side the turntable is inserted, and this changes the I2C id of the turntable.

Do a 3 read from 0x12
Byte 1: frets, stored in the same way as an xbox 360 controller
Byte 2: buttons, bit reversed
Byte 3-4: turntable spin (int16, little endian)

Note that the turntable spin only ever goes between -2 and +2, so you dont really need to read both bytes, a read of just byte 3 is enough