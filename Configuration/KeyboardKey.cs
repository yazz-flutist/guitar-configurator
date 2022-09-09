using System;
using Avalonia.Input;

namespace GuitarConfiguratorSharp.NetCore.Configuration;

public class KeyboardKey : IOutputButton
{
    public Key Key { get; }
    public int Index(bool xbox)
    {
        throw new NotImplementedException();
    }

    public OutputType OutputType => OutputType.Keyboard;

    public string Generate(bool xbox)
    {
        throw new NotImplementedException();
    }

    // This gives us a function to go from key code to bit
    // https://github.com/qmk/qmk_firmware/blob/master/tmk_core/protocol/report.h
    // https://github.com/qmk/qmk_firmware/blob/master/tmk_core/protocol/report.c
    // https://github.com/qmk/qmk_firmware/blob/master/quantum/keycode.h
}