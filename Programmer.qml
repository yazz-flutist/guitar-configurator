import QtQuick 2.13
import QtQuick.Window 2.13
import QtQuick.Controls 2.13
import QtQuick.Controls.Universal 2.13
import QtQuick.Layouts 1.13

Page {
    id: page

    ColumnLayout {
        id: column
        anchors.fill: parent
        Layout.alignment: Qt.AlignHCenter | Qt.AlignVCenter

        Image {
            id: image
            Layout.alignment: Qt.AlignHCenter
            source: "images/controller.png"
            fillMode: Image.PreserveAspectFit
            Layout.maximumHeight: applicationWindow.height/3
            Layout.maximumWidth: applicationWindow.width/3
        }

        Label {
            id: title
            text: qsTr("Arduino Programmer")
            Layout.alignment: Qt.AlignHCenter | Qt.AlignVCenter
            wrapMode: Text.WordWrap
            font.bold: true
            verticalAlignment: Text.AlignVCenter
            horizontalAlignment: Text.AlignHCenter
            font.pointSize: 30
            fontSizeMode: Text.FixedSize
        }


        ColumnLayout {
            id: column1
            Layout.alignment: Qt.AlignHCenter | Qt.AlignVCenter

            ColumnLayout {
                visible: scanner.selected.boardName() === "micro"
                id: micro
                Layout.alignment: Qt.AlignHCenter | Qt.AlignVCenter

                Label {
                    id: label
                    text: qsTr("An Arduino Pro Micro was detected.")
                    Layout.alignment: Qt.AlignHCenter | Qt.AlignVCenter
                }

                Label {
                    id: label1
                    text: qsTr("Please select the voltage your Pro Micro is running at")
                }

                Rectangle {
                    id: rectangle
                    color: "#00000000"
                    border.color: "#00000000"
                }

                ComboBox {
                    textRole: "key"
                    id: comboBox
                    Layout.alignment: Qt.AlignHCenter | Qt.AlignVCenter
                    model: ListModel {
                        id: model
                        ListElement { key: "3.3V"; freq: "8000000" }
                        ListElement { key: "5V"; freq: "16000000" }
                    }
                    onActivated: {
                        scanner.selected.setBoardFreq(model.get(currentIndex).freq)
                    }
                }





            }

            Label {
                id: label2
                text: qsTr("Programming Progress")
                Layout.alignment: Qt.AlignHCenter | Qt.AlignVCenter
            }

            ProgressBar {
                id: progressBar
                Layout.fillWidth: true
                value: 0
            }

            Rectangle {
                id: rectangle1
                color: "#00000000"
                border.color: "#00000000"
            }

            Button {
                id: button
                text: qsTr("Start Programming")
                Layout.alignment: Qt.AlignHCenter | Qt.AlignVCenter
            }



        }

        ScrollView {
            id: scrollView
            Layout.fillWidth: true
            Layout.fillHeight: true
            contentHeight: -1
            contentWidth: -1

            TextArea {
                id: toolOutput
                text: programmer.process_out
                activeFocusOnPress: false
                readOnly: true
                wrapMode: Text.NoWrap
            }
        }

    }












}



















































































































































/*##^## Designer {
    D{i:0;autoSize:true;height:1080;width:1920}D{i:2;anchors_height:400;anchors_width:802;anchors_x:646;anchors_y:232}
D{i:3;anchors_height:400;anchors_y:582}D{i:11;anchors_height:400;anchors_y:232}D{i:5;anchors_y:581}
D{i:16;anchors_height:200;anchors_width:200}D{i:4;anchors_y:581}D{i:17;anchors_height:157;anchors_x:234;anchors_y:531}
D{i:1;anchors_height:60;anchors_width:802;anchors_x:646;anchors_y:658}
}
 ##^##*/