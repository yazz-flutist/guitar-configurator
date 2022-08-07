using System.Collections.Generic;
namespace GuitarConfiguratorSharp.Configuration
{
    public enum StandardAxisType
    {
        LeftTrigger,
        RightTrigger,
        LeftStickX,
        LeftStickY,
        RightStickX,
        RightStickY,
        AccelerationX,
        AccelerationY,
        AccelerationZ,
        MouseX,
        MouseY,
        ScrollX,
        ScrollY
    }

    public enum AnalogToDigitalType
    {
        JoyLow,
        JoyHigh,
        Trigger
    }

    public enum MouseAxisType
    {
        X,
        Y,
        ScrollX,
        ScrollY
    }

    public class StandardAxisMap
    {

        public static readonly Dictionary<WiiAxis, StandardAxisType> wiiAxisMap = new Dictionary<WiiAxis, StandardAxisType>() {
            {WiiAxis.ClassicLeftStickX, StandardAxisType.LeftStickX}
        };
        public static readonly Dictionary<PS2Axis, StandardAxisType> ps2AxisMap = new Dictionary<PS2Axis, StandardAxisType>() {
            {PS2Axis.DualshockLeftX, StandardAxisType.LeftStickX}
        };
    }
    public enum PS2Controller
    {
        Digital,
        Dualshock,
        Dualshock2,
        FlightStick,
        NegCon,
        JogCon,
        Mouse,
        Guitar
    }

    public enum WiiController
    {
        Nunchuk,
        ClassicController,
        ClassicControllerHighRes,
        UDraw,
        Drawsome,
        Guitar,
        Drum,
        DJ,
        Taiko,
        MotionPlus
    }
    public enum WiiAxis
    {
        ClassicLeftStickX,
        ClassicLeftStickY,
        ClassicRightStickX,
        ClassicRightStickY,
        ClassicLeftTrigger,
        ClassicRightTrigger,
        ClassicHiResLeftStickX,
        ClassicHiResLeftStickY,
        ClassicHiResRightStickX,
        ClassicHiResRightStickY,
        ClassicHiResLeftTrigger,
        ClassicHiResRightTrigger,
        DrumGreen,
        DrumRed,
        DrumYellow,
        DrumBlue,
        DrumOrange,
        DrumKickPedal,
        DrumHiHatPedal,
        GuitarJoystickX,
        GuitarJoystickY,
        GuitarWhammy,
        GuitarTapBar,
        NunchukStickX,
        NunchukStickY,
        NunchukAccelerationX,
        NunchukAccelerationY,
        NunchukAccelerationZ,
        NunchukRotationPitch,
        NunchukRotationRoll,
        DJTurntableLeft,
        DJTurntableRight,
        DJCrossfadeSlider,
        DJEffectDial,
        DJStickX,
        DJStickY,
        UDrawPenX,
        UDrawPenY,
        UDrawPenPressure,
        DrawsomePenX,
        DrawsomePenY,
        DrawsomePenPressure,
    }

    // Have two modes, one where people just pick a standard axis when mapping ps2 stuff
    // And an advanced mode which exposes all of the below for mapping
    // Note that if someone is using one of these, then we also need to enable the pressures for it.
    public enum PS2Axis
    {
        GunconHSync,
        GunconVSync,
        MouseX,
        MouseY,
        NegConTwist,
        NegConTwistI,
        NegConTwistII,
        NegConTwistL,
        JogConWheel,
        GuitarWhammy,
        DualshockLeftX,
        DualshockLeftY,
        DualshockRightX,
        DualshockRightY,
        Dualshock2LeftX,
        Dualshock2LeftY,
        Dualshock2RightX,
        Dualshock2RightY,
        Dualshock2L1,
        Dualshock2R1,
        Dualshock2RightButton,
        Dualshock2LeftButton,
        Dualshock2UpButton,
        Dualshock2DownButton,
        Dualshock2Triangle,
        Dualshock2Circle,
        Dualshock2Cross,
        Dualshock2Square,
        Dualshock2L2,
        Dualshock2R2
    }
}