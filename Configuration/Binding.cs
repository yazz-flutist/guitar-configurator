using System.Collections.Generic;
using Avalonia.Media;

namespace GuitarConfiguratorSharp.NetCore.Configuration
{
    public abstract class Axis : Binding
    {
        protected Axis(Microcontroller.Microcontroller controller, InputControllerType inputType, IOutputAxis type, Color ledOn, Color ledOff, float multiplier, int offset, int deadzone, bool trigger) : base(controller, inputType, ledOn, ledOff)
        {
            Multiplier = multiplier;
            Offset = offset;
            Deadzone = deadzone;
            Trigger = trigger;
            Type = type;
        }

        public IOutputAxis Type { get; }
        public float Multiplier { get; }
        public int Offset { get; }
        public int Deadzone { get; }

        public bool Trigger { get; }

        internal abstract string GenerateRaw(IEnumerable<Binding> bindings, bool xbox);
    }
    public abstract class Button : Binding
    {
        protected Button(Microcontroller.Microcontroller controller, InputControllerType inputType, int debounce, IOutputButton type, Color ledOn, Color ledOff) : base(controller, inputType, ledOn, ledOff)
        {
            this.Debounce = debounce;
            this.Type = type;
        }

        public IOutputButton Type { get; }

        public int Debounce { get; }

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

        public Microcontroller.Microcontroller Controller { get; }

        protected Binding(Microcontroller.Microcontroller controller, InputControllerType inputType, Color ledOn, Color ledOff)
        {
            InputType = inputType;
            LedOn = ledOn;
            LedOff = ledOff;
            Controller = controller;
        }
        // provide C code that generates a boolean value
        public abstract string Generate(IEnumerable<Binding> bindings, bool xbox);
    }
}