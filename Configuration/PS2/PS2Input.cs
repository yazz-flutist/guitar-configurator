using System.Collections.Generic;

namespace GuitarConfiguratorSharp.NetCore.Configuration.PS2;

public interface IPs2Input {
    public Ps2Controller Ps2Controller {get;}
    static public Dictionary<Ps2Controller, string> caseStatements = new Dictionary<Ps2Controller, string>() {
        {Ps2Controller.Digital, "PSPROTO_DIGITAL"}
    };
}