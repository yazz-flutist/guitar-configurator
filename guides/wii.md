# Building an Adaptor for a Wii Controller
## You will need
* One of the following Arduinos
  * Pi Pico (I recommend this the most, as it is the easiest to get started with and for most people it will be easier to purchase, and it runs at 3.3v so it works for everything.)
  * Arduino Pro Micro (3.3v)
  * Arduino Pro Micro (5v)
  * Arduino Leonardo
  * Arduino Micro
  * Arduino Uno
  * Arduino Mega
* A basic tilt switch (if you want tilt) (sometimes called a Mercury switch or ball tilt switch). 
  * It is recommended to use two, as this can help avoid accidental activations (this is what is done in some offical guitars)  
  * I know some people use the SW-520D (which is a ball tilt switch), I myself just use some mercury tilt sensors that my local electronics shop sells.
  * Make sure that you get a basic tilt sensor and not one with additional electronics, for example, this type of sensor is correct 
    
    ![Basic](../assets/images/s-l500.jpg)

```danger
  You do not want to get this type of sensor, as it does not work.
    
  ![Basic](../assets/images/s-l500.png)
```
```danger
For anyone wanting to use tilt, the only supported method is using digital tilt switches. Support for the MPU-6050 and analog tilt sensors is now deprecated and will be removed from the firmware in a future update. They are causing a lot of issues for porting this code between various microcontrollers, and take up too much space in the firmware.
```
* A Wii extension breakout board or an extension cable, such as [this](https://www.adafruit.com/product/4836). You can also choose to cut the end of the extension and solder your own cables on as well if you perfer.
* If your wii extension breakout does not support 3.3v input, and you are using a 5v pro micro, you will need a 3.3v voltage regulator. The breakout listed above does however support either voltage so this is not required for that breakout.
* Some Wire
* A Soldering Iron

```note
If you are unsure what microcontroller you want to use, click [here](https://sanjay900.github.io/guitar-configurator/guides/micro-controller-comparison.html) for a list of pros and cons about each microcontroller.
```

```danger
Be careful that you don't ever provide 5v power to the power pin of a Wii Extension, as they are not designed for this. The data pins however are tolerant of 5v, so you can hook these up directly to pins on your Arduino.
```

```danger
Be careful when programming an Arduino that has multiple variants. If you pick the wrong voltage, your device won't show up and you will need to follow the [rescuing](https://sanjay900.github.io/guitar-configurator/tool/rescuing.html) instructions to make it show up again!
```

## The finished product
![Finished adaptor](../assets/images/adaptor.jpg)

## Steps
1. Connect wires between the SDA and SCL pins on your breakout board / wii extension cable. 
Refer to the following image for the pinout of a Wii Extension connector.

    ![Finished adaptor](../assets/images/wii-ext.jpg)
   * For a Pi Pico, the SDA pin is GP18 and the SCL pin is GP19.
   * For an Arduino Pro Micro, Leonardo or Micro, the SDA pin is pin 2 and the SCL pin is pin 3.
   * For an Arduino Uno, pin A4 is SDA and A5 is SCL. Note that on newer arduinos, these pins are also available at the top of the board and are labeled SDA and SCL, but note that these are the same pins, so you can use either.
   * For an Arduino Mega, pin 20 is SDA and pin 21 is SCL.
```danger
If you are using a wii extension cable do NOT rely on the colours, the manufacturers are all over the place with this and the only way to validate them is to test each wire according to the above image. I've come across connectors wired with green as ground and black as VCC before, you just can't rely on the colours at all unfortunately.
```
2. Connect the vcc on the microcontroller to the vcc on the breakout
   * If you are using a 5v pro micro, and your breakout does not support 3.3v input, then you will need to hook up VCC from the arduino to a 3.3v regulator, and then hook up the output of the regulator to the breakout
   * If you are using the breakout linked [above](https://www.adafruit.com/product/4836), the `vin` pin is used for both 3.3v input and 5v input. The 3v pin is actually an output and is not needed for this project.
4. Connect the gnd pin on the wii breakout / extension cable to the gnd on your Arduino.
5. If you want tilt, connect the two switches in series between ground and any digital pin that has not already been used.
     * If you want to add LEDs to your guitar, avoid the MISO, MOSI and SCK pins on your Arduino.
       * For an Arduino Pro Micro (3.3v), Leonardo or Micro, the SCK pin is pin 15, the MOSI pin is pin 16 and the MISO pin is pin 14.
       * For an Arduino Uno, pin 11 is MOSI, pin 12 is MISO and pin 13 is SCK.
       * For a Pi Pico, GP3 is MOSI, GP4 is MISO and GP6 is SCK.uno, you cannot use pins A4 or A5.
6. Start the Ardwiino Configuration Tool
7. Plug in your Arduino
8. Your Arduino should show up, like in the following image.

    ![Arduino uno in home screen](../assets/images/device-pick-uno.png)
    * Using a Pico and don't see your device? Unplug the pico, hold the bootsel button, and plug it back in.
    
    ![PI Pico bootsel](../assets/images/pico-bootsel.png)

8. Click Continue
9. You will be met with the following screen (Note that it will be slightly different for an Arduino Uno or Arduino Mega)

    ![Arduino programming](../assets/images/programming.png)

10. For the Micro, Leonardo and Pro Micro, please pick the type of device you have in the dropdown. Note that you need to get the voltage right, otherwise your device will not work and will need to be reset.
11. Click `Start programming` and wait for it to program
12. Once it finishes, Click `Start Configuring`
13. You will be taken to following the configuration screen.

    ![Graphical config](../assets/images/main-screen-graphical.png)

14. Click on `Change Device Settings`.
15. You will be shown the following dialog:

    ![Graphical config](../assets/images/change-dialog.png)

16. Change `Controller Connectivity Type` to Wii.
17. You can also change the `Controller Output Type` here too if you would like to emulate a different type of controller, such as a drum set, a standard controller or even a controller for a different console, like a PS3 or a Switch.
18. If you would like the Left Joystick (The only Joystick on a Wii Guitar) to act like the DPad, then enable `Map Left Joystick to DPad`
19. If you would like to emulate the Home button by hitting both Start and Select at the same time, then enable `Map Start + Select to Home`
20. Hit Close
21. If you want to enable Tilt, click on Configure Tilt
    1. The following dialog will appear
      
        ![Graphical config](../assets/images/tilt-dialog.png)

    2. Set the Tilt Sensor Type to digital
    4. Click on the button underneath Pin Binding.
    5. The Pin Configuration Dialog will appear:

       ![Graphical config](../assets/images/pin-dialog.png)

    6. You can either click on the pin that you have plugged your tilt into, or use the `Automatically Find Pin Binding` button. You can then just tilt the guitar to activate tilt. Note that for analogue tilt sensors, you just need to pick the pin for the axis of tilt you want to activate your guitar. This will depend on how you have mounted your tilt sensor.
    7. Click `Apply Changes`
    8. Click `Close`
 1. Click `Write`
 2. At this point, your controller should be working, test it using the Windows controller tester or a game controller tester of your choice.
