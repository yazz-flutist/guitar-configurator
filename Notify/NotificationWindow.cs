// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using Avalonia.Win32;
using LibUsbDotNet.DeviceNotify;
using LibUsbDotNet.Main;

namespace GuitarConfigurator.NetCore.Notify
{
    public class NotificationWindow : WindowImpl
    {
        private const int WM_DEVICECHANGE = 0x219;
        private readonly OnDeviceChangeDelegate mDelDeviceChange;

        public NotificationWindow(OnDeviceChangeDelegate mDelDeviceChange)
        {
            this.mDelDeviceChange = mDelDeviceChange;
        }

        protected override IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == WM_DEVICECHANGE)
            {
                mDelDeviceChange(msg, wParam, lParam);
            }

            return base.WndProc(hWnd, msg, wParam, lParam);
        }
        #region Nested Types

        #region Nested type: OnDeviceChangeDelegate

        public delegate void OnDeviceChangeDelegate(uint msg, IntPtr wParam, IntPtr lParam);

        #endregion

        #endregion
    }
}