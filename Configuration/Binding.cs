using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using Avalonia.Input;
using System.Text.Json.Serialization;
using Dahomey.Json.Attributes;

namespace GuitarConfiguratorSharp.Configuration
{
    public enum DevicePinMode
    {
        VCC = 0,
        Floating = 1,
        Ground = 2,
        BusKeep = 3,
    }

    public abstract class GroupableAxis : Axis
    {
        protected GroupableAxis(Microcontroller controller, InputControllerType inputType, OutputAxis type, Color ledOn, Color ledOff, float multiplier, int offset, int deadzone, bool trigger) : base(controller, inputType, type, ledOn, ledOff, multiplier, offset, deadzone, trigger)
        {
        }

        public abstract StandardAxisType standardAxis { get; }
    }

    public abstract class GroupableButton : Button
    {
        protected GroupableButton(Microcontroller controller, InputControllerType inputType, int debounce, OutputButton type, Color ledOn, Color ledOff) : base(controller, inputType, debounce, type, ledOn, ledOff)
        {
        }

        public abstract StandardButtonType standardButton { get; }
    }


    [JsonDiscriminator(nameof(AnalogToDigital))]
    public class AnalogToDigital : Button, WiiInput, PS2Input
    {
        public AnalogToDigital(Microcontroller controller, int threshold, Axis analog, AnalogToDigitalType analogToDigitalType, int debounce, OutputButton type, Color ledOn, Color ledOff) : base(controller, analog.InputType, debounce, type, ledOn, ledOff)
        {
            this.Analog = analog;
            this.AnalogToDigitalType = analogToDigitalType;
            this.Threshold = threshold;
        }
        public AnalogToDigitalType AnalogToDigitalType { get; set; }
        public Axis Analog { get; set; }
        public override string Input => Analog.Input;

        public int Threshold { get; }

        [JsonIgnore]
        public int Pin => Analog is DirectAnalog ? (Analog as DirectAnalog)!.Pin : throw new InvalidOperationException();
        [JsonIgnore]
        public PS2Controller ps2Controller => Analog is PS2Analog ? (Analog as PS2Analog)!.ps2Controller : throw new InvalidOperationException();
        [JsonIgnore]
        public WiiController WiiController => Analog is WiiAnalog ? (Analog as WiiAnalog)!.WiiController : throw new InvalidOperationException();

        public override string generate(IEnumerable<Binding> bindings)
        {
            switch (AnalogToDigitalType)
            {
                case AnalogToDigitalType.Trigger:
                    return $"{Analog.generateRaw(bindings)} > {(int)((Threshold + (Analog.Offset * Analog.Multiplier)) * 2)}";
                case AnalogToDigitalType.JoyHigh:
                    return $"{Analog.generateRaw(bindings)} > {(int)(Threshold + 128 + (Analog.Offset * Analog.Multiplier))}";
                case AnalogToDigitalType.JoyLow:
                    return $"{Analog.generateRaw(bindings)} < {(int)((-Threshold) + 128 + (Analog.Offset * Analog.Multiplier))}";
            }
            return "";
        }
    }
    [JsonDiscriminator(nameof(DigitalToAnalog))]
    public class DigitalToAnalog : Axis, WiiInput, PS2Input
    {
        public DigitalToAnalog(Microcontroller controller, int value, Button button, AnalogToDigitalType analogToDigitalType, OutputAxis type, Color ledOn, Color ledOff, float multiplier, int offset, int deadzone, bool trigger) : base(controller, button.InputType, type, ledOn, ledOff, multiplier, offset, deadzone, trigger)
        {
            this.Button = button;
            this.AnalogToDigitalType = analogToDigitalType;
            this.Value = value;
        }
        public AnalogToDigitalType AnalogToDigitalType { get; set; }

        public Button Button { get; set; }
        public int Value { get; set; }
        [JsonIgnore]
        public int Pin => Button is DirectDigital ? (Button as DirectDigital)!.Pin : throw new InvalidOperationException();
        [JsonIgnore]
        public PS2Controller ps2Controller => Button is PS2Button ? (Button as PS2Button)!.ps2Controller : throw new InvalidOperationException();
        [JsonIgnore]
        public WiiController WiiController => Button is WiiButton ? (Button as WiiButton)!.WiiController : throw new InvalidOperationException();

        public override string Input => Button.Input;

        public override string generate(IEnumerable<Binding> bindings)
        {
            switch (AnalogToDigitalType)
            {
                case AnalogToDigitalType.Trigger:
                case AnalogToDigitalType.JoyHigh:
                    return $"({Button.generate(bindings)}) * {Value}";
                case AnalogToDigitalType.JoyLow:
                    return $"({Button.generate(bindings)}) * {-Value}";
            }
            return "";
        }

        internal override string generateRaw(IEnumerable<Binding> bindings)
        {
            return generate(bindings);
        }
    }



    public abstract class Axis : Binding
    {
        protected Axis(Microcontroller controller, InputControllerType inputType, OutputAxis type, Color ledOn, Color ledOff, float multiplier, int offset, int deadzone, bool trigger) : base(controller, inputType, ledOn, ledOff)
        {
            Multiplier = multiplier;
            Offset = offset;
            Deadzone = deadzone;
            Trigger = trigger;
            Type = type;
        }

        public OutputAxis Type { get; }
        public float Multiplier { get; }
        public int Offset { get; }
        public int Deadzone { get; }

        public bool Trigger { get; }

        internal abstract string generateRaw(IEnumerable<Binding> bindings);
    }
    public abstract class Button : Binding
    {
        protected Button(Microcontroller controller, InputControllerType inputType, int debounce, OutputButton type, Color ledOn, Color ledOff) : base(controller, inputType, ledOn, ledOff)
        {
            this.Debounce = debounce;
            this.Type = type;
        }

        public OutputButton Type { get; }

        public int Debounce { get; }

    }

    public enum OutputType
    {
        Generic,
        Xbox,
        Midi,
        Keyboard
    }

    public interface OutputAxis
    {
        public StandardAxisType Type { get; }
        public abstract string generate();
        public OutputType OutputType { get; }
    }
    public interface OutputButton
    {
        public abstract int index();
        public OutputType OutputType { get; }
    }
    [JsonDiscriminator(nameof(XboxControllerButton))]
    public class XboxControllerButton : OutputButton
    {
        public static List<StandardButtonType> order = new List<StandardButtonType>(){
            StandardButtonType.Up,
            StandardButtonType.Down,
            StandardButtonType.Left,
            StandardButtonType.Right,
            StandardButtonType.Start,
            StandardButtonType.Select,
            StandardButtonType.LeftStick,
            StandardButtonType.RightStick,
            StandardButtonType.LB,
            StandardButtonType.RB,
            StandardButtonType.Home,
            StandardButtonType.Capture,
            StandardButtonType.A,
            StandardButtonType.B,
            StandardButtonType.X,
            StandardButtonType.Y
        };
        public StandardButtonType Type { get; }

        public OutputType OutputType => OutputType.Xbox;

        public int index()
        {
            return order.IndexOf(Type);
        }
    }
    [JsonDiscriminator(nameof(GenericControllerButton))]
    public class GenericControllerButton : OutputButton
    {
        public static List<StandardButtonType> order = new List<StandardButtonType>(){
            StandardButtonType.Y,
            StandardButtonType.B,
            StandardButtonType.A,
            StandardButtonType.X,
            StandardButtonType.LB,
            StandardButtonType.RB,
            StandardButtonType.LT,
            StandardButtonType.RT,
            StandardButtonType.Start,
            StandardButtonType.Select,
            StandardButtonType.LeftStick,
            StandardButtonType.RightStick,
            StandardButtonType.Home,
            StandardButtonType.Capture
        };
        public StandardButtonType Type { get; }
        public OutputType OutputType => OutputType.Generic;
        public int index()
        {
            return order.IndexOf(Type);
        }

        public GenericControllerButton(StandardButtonType type)
        {
            Type = type;
        }
    }

    [JsonDiscriminator(nameof(GenericControllerHat))]
    public class GenericControllerHat : OutputButton
    {
        public static List<StandardButtonType> order = new List<StandardButtonType>(){

            StandardButtonType.Up,
            StandardButtonType.Down,
            StandardButtonType.Left,
            StandardButtonType.Right,
        };
        public StandardButtonType Type { get; }
        public OutputType OutputType => OutputType.Generic;
        public int index()
        {
            return order.IndexOf(Type);
        }

        public GenericControllerHat(StandardButtonType type)
        {
            Type = type;
        }
    }


    [JsonDiscriminator(nameof(GenericAxis))]
    public class GenericAxis : OutputAxis
    {
        public StandardAxisType Type { get; }
        public OutputType OutputType => OutputType.Generic;

        public static Dictionary<StandardAxisType, string> mappings = new Dictionary<StandardAxisType, string>() {
            {StandardAxisType.LeftStickX, "l_x"},
            {StandardAxisType.LeftStickY, "l_y"},
            {StandardAxisType.RightStickX, "r_x"},
            {StandardAxisType.RightStickY, "r_y"},
            {StandardAxisType.LeftTrigger, "axis[4]"},
            {StandardAxisType.RightTrigger, "axis[5]"},
            {StandardAxisType.AccelerationX, "accel[0]"},
            {StandardAxisType.AccelerationY, "accel[1]"},
            {StandardAxisType.AccelerationZ, "accel[2]"},
        };

        public string generate()
        {
            return "report->" + mappings[Type];
        }

        public GenericAxis(StandardAxisType type)
        {
            Type = type;
        }
    }

    [JsonDiscriminator(nameof(XboxAxis))]
    public class XboxAxis : OutputAxis
    {
        public StandardAxisType Type { get; }
        public OutputType OutputType => OutputType.Xbox;

        public static Dictionary<StandardAxisType, string> mappings = new Dictionary<StandardAxisType, string>() {
            {StandardAxisType.LeftStickX, "l_x"},
            {StandardAxisType.LeftStickY, "l_y"},
            {StandardAxisType.RightStickX, "r_x"},
            {StandardAxisType.RightStickY, "r_y"},
            {StandardAxisType.LeftTrigger, "lt"},
            {StandardAxisType.RightTrigger, "rt"},
        };

        public string generate()
        {
            return "report->" + mappings[Type];
        }
    }
    [JsonDiscriminator(nameof(MouseAxis))]
    public class MouseAxis : OutputAxis
    {
        public StandardAxisType Type { get; }

        public static Dictionary<StandardAxisType, string> mappings = new Dictionary<StandardAxisType, string>() {
            {StandardAxisType.MouseX, "X"},
            {StandardAxisType.MouseY, "Y"},
            {StandardAxisType.ScrollX, "ScrollX"},
            {StandardAxisType.ScrollY, "ScrollY"},
        };
        public OutputType OutputType => OutputType.Keyboard;

        public string generate()
        {
            return "report->" + mappings[Type];
        }

    }
    public class KeyboardKey : OutputButton
    {
        public Key key { get; }
        public OutputType OutputType => OutputType.Keyboard;
        // This gives us a function to go from key code to bit
        // https://github.com/qmk/qmk_firmware/blob/master/tmk_core/protocol/report.h
        // https://github.com/qmk/qmk_firmware/blob/master/tmk_core/protocol/report.c
        // https://github.com/qmk/qmk_firmware/blob/master/quantum/keycode.h
        // We would have to make a map that maps from Avalonia Keys to the keycodes above, and then we can use the code in report.c to convert that to a bit.
        public int index()
        {
            throw new NotImplementedException();
        }
    }
    // public class MIDINote : OutputAxis
    // {
    //     public int note { get; }
    //     public OutputType OutputType => OutputType.Midi;

    //     public string generate()
    //     {
    //         // TODO: this
    //         throw new NotImplementedException();
    //     }
    // }
    // public class MIDICommand : OutputAxis
    // {
    //     public int command { get; }
    //     public OutputType OutputType => OutputType.Midi;

    //     public string generate()
    //     {
    //         // TODO: this
    //         throw new NotImplementedException();
    //     }

    // }
    public abstract class Binding
    {

        public InputControllerType InputType { get; }

        public Color LedOn
        {
            get;
            set;
        }
        public Color LedOff
        {
            get;
            set;
        }

        public abstract string Input
        {
            get;
        }

        public Microcontroller Controller { get; }

        protected Binding(Microcontroller controller, InputControllerType inputType, Color ledOn, Color ledOff)
        {
            InputType = inputType;
            LedOn = ledOn;
            LedOff = ledOff;
            Controller = controller;
        }
        // provide C code that generates a boolean value
        public abstract string generate(IEnumerable<Binding> bindings);
    }
}