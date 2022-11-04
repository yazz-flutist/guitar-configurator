# Rock Band Auto-Calibration Sensors

## Wii
Activating the sensors is as simple as sending the following as HID FEATURE Reports:

```csharp
using HidSharp;
using System.IO;

byte[] activate_mic = {0x00, 0xE9, 0x00, 0x83, 0x1B, 0x00, 0x00, 0x00, 0x02};
byte[] activate_light = {0x00, 0xE9, 0x00, 0x83, 0x1B, 0x00, 0x00, 0x00, 0x01};
byte[] disable_mic_light = {0x00, 0xE9, 0x00, 0x83, 0x1B, 0x00, 0x00, 0x00, 0x00};
byte[][] other_commands = new byte[4][] {
   new byte[9] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00},
   new byte[9] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00},
   new byte[9] {0x00, 0x00, 0x00, 0x83, 0x00, 0x00, 0x00, 0x00, 0x00},
   new byte[9] {0x00, 0xE9, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}
};

private IEnumerable<HidDevice> findHidSharpDevices(Joystick joystick) {
    string id = joystick.hardwareIdentifier;
    id = id.Substring(id.Length - 36);
    int vid = Convert.ToInt32(id.Substring(4, 4), 16);
    int pid = Convert.ToInt32(id.Substring(0, 4), 16);
    var list = DeviceList.Local;
    return list.GetHidDevices(vid, pid, null, null);
}

private IEnumerator<WaitForSeconds> sendPacket(IEnumerable<HidDevice> devices, byte[] packet) {
    foreach (var device in devices) {
        var stream = device.Open();
        stream.SetFeature(packet);
        yield return new WaitForSeconds(1.473f);

        foreach (byte[] cmd in other_commands) {
            stream.SetFeature(cmd);
            yield return new WaitForSeconds(1.473f);
        }

        byte[] data = new byte[0x001C];
        stream.GetFeature(data);
        yield return new WaitForSeconds(1.473f);

        stream.Close();
    }
}

private void enableMic(IEnumerable<HidDevice> devices) {
    StartCoroutine(sendPacket(devices, activate_mic));
}
private void enableLight(IEnumerable<HidDevice> devices) {
    StartCoroutine(sendPacket(devices, activate_light));
}
private void disableSensors(IEnumerable<HidDevice> devices) {
    StartCoroutine(sendPacket(devices, disable_mic_light));
}
```

Once an initialisation packet is sent, the audio data can be read from byte 15 of the standard input report. This would be one of the button axis bytes. The value will be 0x3F or 0x5F when a chirp is detected.

Once an initialisation packet is sent, the light sensor data can be read from byte 16 of the standard input report. This would be one of the button axis bytes. The value will be 0x32 or 0x7F when there is a white screen.

It would be interesting to know if we actually need to read after we set things.

It would be nice to dump this data and see what different screens do, maybe we dont have to hardcode a value.

# Ps3
Activating the sensors is as simple as sending the following as HID OUTPUT Reports, and then reading the left joystick x axis (first axis):

```csharp
byte[] activate_mic_ps3 = {0x00, 0x01, 0x08, 0x01, 0x40, 0x00, 0x00, 0x00, 0x00};
byte[] activate_light_ps3 = {0x00, 0x01, 0x08, 0x01, 0xFF, 0x00, 0x00, 0x00, 0x00};
byte[] disable_mic_light_ps3 = {0x00, 0x01, 0x08, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00};
private void enableMicPs3(IEnumerable<HidDevice> devices) {
    foreach (var device in devices) {
        var stream = device.Open();
        stream.Write(activate_mic_ps3);
        stream.Close();
    }
}

private void enableLightPs3(IEnumerable<HidDevice> devices) {
    foreach (var device in devices) {
        var stream = device.Open();
        stream.Write(activate_light_ps3);
        stream.Close();
    }
}

private void disableSensorsPs3(IEnumerable<HidDevice> devices) {
    foreach (var device in devices) {
        var stream = device.Open();
        stream.Write(disable_mic_light_ps3);
        stream.Close();
    }
}
```