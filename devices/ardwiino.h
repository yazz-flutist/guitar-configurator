#pragma once
#include <math.h>

#include <QDebug>
#include <QJSValue>
#include <QObject>
#include <QQueue>
#include <QSerialPort>
#include <QSerialPortInfo>
#include "submodules/Ardwiino/src/shared/output/serial_commands.h"

#include "usb/usbdevice.h"
#include "device.h"
#include "ardwiinolookup.h"
#include "deviceconfiguration.h"
class Ardwiino : public Device {
    Q_OBJECT
    Q_PROPERTY(bool ready READ isReady NOTIFY readyChanged)
    Q_PROPERTY(DeviceConfiguration* config MEMBER m_configuration NOTIFY configurationChanged)
   public:
    explicit Ardwiino(UsbDevice_t devt, QObject* parent = nullptr);
    QString getDescription();
    bool isReady();
    virtual void close();
    virtual bool open();
    virtual void bootloader();
    virtual bool isConfigurable() {
        return m_configurable;
    }
    board_t board() const {
        return m_board;
    }
    QString boardShortName() const {
        return m_board.shortName;
    }
    QString boardName() const {
        return m_board.name;
    }
    bool isOutdated() const {
        return m_isOutdated;
    }
    DeviceConfiguration* getConfig() const {
        return m_configuration;
    }
   public slots:
    void writeConfig();
    void findDigital(QJSValue callback);
    void findAnalog(QJSValue callback);
    void writeData(int cmd, QByteArray data = QByteArray());
   signals:
    void readyChanged();
    void ledsChanged();
    void midiChanged();
    void configurationChanged();

   protected:
    QString m_processor;

   private:
    // data_t readData();
    QByteArray readConfig();
    UsbDevice m_usbDevice;
    DeviceConfiguration* m_configuration;
    bool m_isOpen;
    bool m_isOutdated;
    bool m_hasPinDetectionCallback;
    bool m_configurable;
    QJSValue m_pinDetectionCallback;
    uint16_t m_extension;
    inline virtual bool isEqual(const Device& other) const {
         return m_deviceID.bus == other.getUSBDevice().bus && m_deviceID.port == other.getUSBDevice().port;
    }
};
