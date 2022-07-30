using System.Collections.Generic;

namespace GuitarConfiguratorSharp.Configuration
{
    public enum StandardButtonType
    {
        A,
        B,
        X,
        Y,
        LB,
        RB,
        LT,
        RT,
        Up,
        Down,
        LeftStick,
        RightStick,
        Left,
        Right,
        Start,
        Select,
        Home,
        Capture
    }

    public enum PS2ButtonType
    {
        GuitarGreen,
        GuitarRed,
        GuitarYellow,
        GuitarBlue,
        GuitarOrange,
        Select,
        L3,
        R3,
        Start,
        Up,
        Right,
        Down,
        Left,
        L2,
        R2,
        L1,
        R1,
        Triangle,
        Circle,
        Cross,
        Square

    }

    public enum WiiButtonType
    {
        GuitarGreen,
        GuitarRed,
        GuitarYellow,
        GuitarBlue,
        GuitarOrange,
        GuitarMinus,
        GuitarPlus,
        GuitarStrumUp,
        GuitarStrumDown,
        DrumGreen,
        DrumRed,
        DrumYellow,
        DrumBlue,
        DrumOrange,
        DrumMinus,
        DrumPlus,
        DrumKickPedal,
        DrumHiHatPedal,
        NunchukC,
        NunchukZ,
        UDrawPenClick,
        UDrawPenButton1,
        UDrawPenButton2,
        TaTaConLeftDrumRim,
        TaTaConLeftDrumCenter,
        TaTaConRightDrumRim,
        TaTaConRightDrumCenter,
        DJHeroEuphoria,
        DJHeroLeftGreen,
        DJHeroLeftRed,
        DJHeroLeftBlue,
        DJHeroRightGreen,
        DJHeroRightRed,
        DJHeroRightBlue,
        DJHeroMinus,
        DJHeroPlus,
        ClassicA,
        ClassicB,
        ClassicX,
        ClassicY,
        ClassicDPadUp,
        ClassicDPadDown,
        ClassicDPadLeft,
        ClassicDPadRight,
        ClassicZL,
        ClassicZR,
        ClassicLT,
        ClassicRT,
        ClassicPlus,
        ClassicMinus,
        ClassicHome


    }

    public class StandardButtonMap
    {

        public static readonly Dictionary<PS2ButtonType, StandardButtonType> ps2ButtonMap = new Dictionary<PS2ButtonType, StandardButtonType>() {
            {PS2ButtonType.GuitarGreen, StandardButtonType.A}
        };
        public static readonly Dictionary<WiiButtonType, StandardButtonType> wiiButtonMap = new Dictionary<WiiButtonType, StandardButtonType>() {
            {WiiButtonType.GuitarGreen, StandardButtonType.A}
        };
    }

    public enum GHWTTarButton 
    {
        TapGreen,
        TapRed,
        TapYellow,
        TapBlue,
        TapOrange
    }

    public enum GHFiveTarButton 
    {
        None,
        Green,
        Red,
        Yellow,
        Blue,
        Orange,
        TapGreen,
        TapRed,
        TapYellow,
        TapBlue,
        TapOrange
    }
}