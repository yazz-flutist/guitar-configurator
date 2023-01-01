# Types of classes
* Input
    * Mega 
    * Micro
    * Uno / Pro Mini
    * Wii
    * GH5
    * WT
    * USB Passthrough
* Output
  * Generic
  * MIDI
  * Keyboard/Mouse/Joystick
* Binding
  * Input Type
    * Direct Digital
    * Direct Analog
    * Direct Analog to Digital
    * Direct Digital to Analog
    * Wii Button
    * Wii Axis
    * Wii Analog to Digital
    * Wii Digital to Analog
    * PS2 Button
    * PS2 Axis
    * PS2 Analog to Digital
    * PS2 Digital to Analog
  * If Direct
    * Pin Mode
      * VCC
      * Floating
      * Ground
      * BusKeep
    * Pin
  * If wii / ps2
    * axis / button enum
  * If analog to digital
    * deadzone
      * tbh, this isn't so necessary, as we have the deadzone for calibration, and  should be able to rely on that.
  * If digital to analog
    * active value
    * inactive value
  * Output type
    * Generic Controller
      * Generic Axis
      * Generic button
    * Midi
      * Midi control command
      * Midi note
    * Keyboard/Mouse
      * Keyboard button
      * Keyboard modifiers
      * Mouse button
      * Mouse axis
  * If Midi
    * Midi channel
    * Midi velocity or Midi cc command
  * If Generic
    * Axis (number)
    * Button (Enum)
  * If Keyboard/Mouse
    * Axis (number)
    * Mouse Button (Enum)
    * Keyboard Modifiers (Enum flags)
    * Keyboard Button (char)
  * Calibration (if analog)
    * multiplier
    * offset
    * deadzone
  * Image
    * Based on name
  * debounce
  * led
    * colour for active
    * colour for not active

* Init
  * Direct
    * Buttons
      * AVR
        * Loop over all bindings, and for each port, construct a single DDRx / PORTx based on the state of the port, combining all 8 into 1
      * Pico
        * unfortunately, there is nothing more optimial than just doing the following for each binding,
        * as there is a block of data per pin, not a single bit mask
        ```c
            gpio_init(pin);
            gpio_set_dir(pin, mode == OUTPUT);
            gpio_set_pulls(pin, mode == INPUT_PULLUP || mode == INPUT_PULLUP_ANALOG,
                        false);
        ```
    * Analog
      * AVR
        * We can code an init function into the code itself, and then we just call it if we have analog pins
        * hardcode in a list of enabled analog pins based on the bindings
        * then we just have a tick function that kicks off an analog read
      * Pico
        * hardcode in a list of enabled analog pins based on the bindings
        * We would just code in the pin modes, this would be handled with the same loop as digital
  * 

* IO Mappings:
  * buttons
      ```c
      // Essentially, sort the buttons in reverse order, and then write each button bit one at a time
      // If a button is skipped, you can just shift over it
      // each input would provide an implementation for turning a pin number into a assignment string
      // If the first bit isnt used then you can skip the first shift and start low at 0
      // and we can support multiple inputs going to a single button by just oring both.
      uint8_t low = (PORTB >> 1) & 1;
      low <<= 1;
      low |= (PORTB >> 2) & 1;
      low |= (PORTB >> 4) & 1;
      low <<= 2;
      low |= (PORTB >> 4) & 1;
      low <<= 1;
      low |= (PORTB >> 5) & 1;
      low <<= 1;
      low |= (PORTB >> 6) & 1;
      low <<= 1;
      low |= (PORTB >> 7) & 1;
      low <<= 1;
      low |= (PORTB >> 8) & 1;
      controller->buttons_low = low;
      ```
  * Button -> Axis
      ```c
      // If we need both triggered and non triggered values
      controller->axis = (~(PORTB >> 1) & 1) ? triggered : not_triggered;
      // If we only need a triggered value
      controller->axis = (~(PORTB >> 1) & 1) * triggered;
      ```
  * Axis -> Button
      ```c
      // Thanks to deadzones, we can expect analog to stay at 0 when not triggered.
      uint8_t low = adc_read(analogIndex, offset, multiplier, deadzone) != 0;
      controller->buttons_low = low;
      ```
  * Wii / PS2 Axis -> Button
      ```c
      // we would need to code in our own deadzones for this
      // or, we allow calibration of joystick axis' for adc

      uint8_t low = ((data[0] - 0x80) << 8) > 3;
      controller->buttons_low = low;
      ```
  * AVR Direct button -> Generic Button / Keyboard Button
      ```c
      //For pull up, use ~ to invert the bits
      uint8_t low = ~(PORTB >> 1) & 1;
      controller->buttons_low = low;
      ```

  * Pico Direct button -> Generic Button / Keyboard Button
      ```c
      //For pull up, use ~ to invert the bits
      uint8_t low = ~(sio_hw->gpio_in >> 1) & 1;
      controller->buttons_low = low;
      ```

  * AVR / Pico Direct Axis -> Generic Axis / Mouse Axis
      ```c
      // adc_read will be defined within the codebase
      controller->axis = adc_read(analogIndex, offset, multiplier, deadzone);
      ```

  * Wii button -> Generic Button / Keyboard Button
    * Will have to do something different for each type of extension, but
      ```c
      uint8_t low = (data[5] >> 2) & 1;
      ```

  * Wii axis -> Generic Axis / Mouse Axis
    * Will have to do something different for each type of extension, but
      ```c
      controller->axis = (data[0] - 0x80) << 8;
      ```

  * PS2 button -> Generic Button / Keyboard Button
    * Will have to do something different for each type of controller, but
      ```c
      uint8_t low = (in[5] >> 2) & 1;
      ```
    * note that there is a special case for the negcon, as it by default will need some analog to digital conversions

  * PS2 axis -> Generic Axis / Mouse Axis
    * Will have to do something different for each type of controller, but
      ```c
      controller->axis = (in[0] - 0x80) << 8;
      ```
    * the jogcon also has a special case as its steering wheel has an odd way to map to a value
    * guitars also need a digital to analog conversion for star power (button: L2) ((in[5] & 1) << 15)
      * in a tidy twist, the L2 button is the first bit of in[5], so we can just isolate that bit and shift it to the end for star power
    * Now in theory, we can support analog pressure data as something that can be mapped. 
      * Obviously, we would need to be careful that we disable this extra data when it isnt being used.


* More mapping
  * Implement a mapping between WiiAxis/PS2Axis and StandardAxis
    * You would essentially store a binding for each type of WiiAxis/PS2Axis
    * By default, default to standardaxis
    * for midi, also expose the drums by default but nothing else   
  * When in advanced mode, show all the inputs, otherwise just show the StandardAxis
  * Implement a PS2Buttons and a WiiButtons
    * Note that there are a couple of controllers out there for the wii that use different buttons 
  * We could make it so that PS2Axis and WiiAxis store both a StandardAxis and a PS2/WiiAxis, and then in the gui we just show the correct one and hide duplicates
    * Might have to make it so that you cant go back to simple mode once your in advanced mode
    * tbh, it would also probably be reasonable to not let you swap modes on the fly, as there really wont be a easy way to map from MIDI to Gamepad
  * Im thinking that when you configure a device for the first time (or when a device is converted) we offer a gui where you pick the type of device you are making and if you want advance mode, and then we just don't let it be changed after.

  * for generic mode, we will need to implement all inputs for PS3, and then just have the code for converting them to XInput when plugged into a 360
  * 