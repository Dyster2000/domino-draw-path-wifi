/*
This file is part of DominoDrawWifi.

DominoDrawWifi is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation version 3 or later.

DominoDrawWifi is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with DominoDrawWifi. If not, see <https://www.gnu.org/licenses/>.
*/

namespace DominoPathDrawWifiApp;

public partial class ManualDrive : ContentPage
{
    private WifiHandler Wifi { get; set; }

    public ManualDrive()
    {
        InitializeComponent();
    }

    public void Init(WifiHandler wifi)
    {
        Wifi = wifi;

        WifiConnect.Init(this, Wifi);
        DriveControls.Init(Wifi, true);
    }

    private async void RangePointer_ValueChanged(object sender, Syncfusion.Maui.Gauges.ValueChangedEventArgs e)
    {
        var dir = ((int)e.Value - 50) * 90 / 40; // Convert 10 to 90 range -> -90 to  +90

        Wifi.ManualCommandData.UpdateFromStatus(Wifi.StatusData);
        Wifi.ManualCommandData.Direction = (byte)dir;

        Wifi.SendManualCommand(true);
    }
}