# Building a RF (Wireless) Guitar and Receiver
## You will need
* Two NRF24l01s
* The parts to build either a wii adaptor or directly wired guitar from their respective guides.
  * For the Arduino, an Arduino Pro Mini is recommended for the arduino that is used as a transmitter, as these use very small amounts of power compared to other arduinos
  * If using an Arduino Pro Mini, you will also need an FTDI breakout for it
  * You will also need batteries for this arduino
* Another arduino to use as a receiver, this can be one of the following:
  * Arduino Pro Micro (3.3v)
  * Arduino Pro Micro (5v)
  * Arduino Leonardo
  * Arduino Micro
  * Arduino Uno
  * Arduino Mega
* Some Wire
* A Soldering Iron
* A multimeter
* Wire Strippers
* Wire Cutters
* Heat shrink

```note
RF transmitting Arduinos (the side that connects to the controller) are programmed to go to sleep after 10 minutes, and then will need to be woken up by their respective wakeup pins, which will be explained later in the guide.
```

```danger
PS2 Guitars and RGB leds are incompatible with RF, as they all use the same pins.
```

## The finished product
![Finished adaptor](../assets/images/rf.jpg)

## Steps
1. Connect the VCC and GND pins on each RF module to its respective Arduino.
2. Connect the RF module pins to the following Arduino pin numbers:
   |Pin numbers|SCK|MOSI|MISO|CE|IRQ|CSN|
   |---|---|---|---|---|---|---|
   |Pro Micro, Leonardo, Micro|15|16|14|0|1|10|
   |Uno, Pro Mini|13|11|12|8|2|10|
   |Mega|52|51|50|53|2|10|
3. Hook a button up to the following pin on your transmitting arduino, this is used for waking up the device from sleep, and can be an existing button. For example, you could wire your start button here and use it to wake the arduino up from sleep.
   | |Wakeup Pin|
   |---|---|
   |Pro Micro, Leonardo, Micro|7|
   |Uno, Mega, Pro Mini|3|
4. Follow the guide of your choice to hook up the controller inputs to the transmitter arduino
   * [Direct](direct.md)
   * [Wii](wii.md)
5. Plug in the receiver arduino, open the config tool and program it.
6. Hit the Configure RF button, and then enable RF
7. Click on Program RF Transmitter
8. Plug in your transmitting arduino
9.  Program the transmitting arduino
10. Configure the guitar, this is detailed better in the guide specific to the type of guitar your creating
   * [Direct](direct.md)
   * [Wii](wii.md)
