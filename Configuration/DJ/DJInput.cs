using System;
using System.Collections.Generic;
using System.Linq;
using GuitarConfiguratorSharp.NetCore.Configuration.Json;
using GuitarConfiguratorSharp.NetCore.Configuration.Microcontrollers;

namespace GuitarConfiguratorSharp.NetCore.Configuration.DJ;
public class DjInput : TwiInput
{
    public static readonly string DjTwiType = "dj";
    public static readonly int DjTwiFreq = 250000;
    public DjInput(DjInputType input, Microcontroller microcontroller, int? sda = null, int? scl = null): base(microcontroller, DjTwiType, DjTwiFreq, sda, scl)
    {
        Input = input;
    }

    public DjInputType Input { get; set; }
    public override string Generate()
    {
        switch (Input)
        {
            case DjInputType.LeftTurntable:
                return "((int8_t)dj_left[2]) << 5";
            case DjInputType.RightTurnable:
                return "((int8_t)dj_right[2]) << 5";
            case DjInputType.LeftAny:
                return "dj_left[0]";
            case DjInputType.RightAny:
                return "dj_right[0]";
            case DjInputType.LeftBlue:
            case DjInputType.LeftGreen:
            case DjInputType.LeftRed:
                return $"(dj_left[0] & {1 << ((byte) Input) - ((byte) DjInputType.LeftGreen)})";
            case DjInputType.RightGreen:
            case DjInputType.RightRed:
            case DjInputType.RightBlue:
                return $"(dj_right[0] & {1 << ((byte) Input) - ((byte) DjInputType.RightGreen)})";

        }

        throw new InvalidOperationException("Shouldn't get here!");
    }

    public override bool IsAnalog => Input <= DjInputType.RightTurnable;

    public override string GenerateAll(bool xbox, List<Tuple<Input, string>> bindings,
        Microcontroller controller)
    {
        Console.WriteLine("yeet");
        Console.WriteLine(String.Join("\n", bindings.Select(binding => binding.Item2)));
        return String.Join(";\n", bindings.Select(binding => binding.Item2));
    }

    public override IReadOnlyList<string> RequiredDefines()
    {
        return base.RequiredDefines().Concat(new[] {"INPUT_DJ_TURNTABLE"}).ToList();
    }

    public override JsonInput GetJson()
    {
        return new JsonDjInput(Sda, Scl, Input);
    }
}

/*
#ifdef INPUT_DJ_TURNTABLE
    uint8_t dj_left[3] = {0,0,0};
    uint8_t dj_right[3] = {0,0,0};
    twi_readFromPointer(DJLEFT_ADDR, DJ_BUTTONS_PTR, sizeof(dj_left), dj_left);
    twi_readFromPointer(DJRIGHT_ADDR, DJ_BUTTONS_PTR, sizeof(dj_right), dj_right);
#endif

*/