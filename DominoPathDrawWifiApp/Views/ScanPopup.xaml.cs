/*
This file is part of DominoDrawWifi.

DominoDrawWifi is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation version 3 or later.

DominoDrawWifi is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with DominoDrawWifi. If not, see <https://www.gnu.org/licenses/>.
*/

using CommunityToolkit.Maui.Views;

namespace DominoPathDrawWifiApp;

public partial class ScanPopup : Popup
{
    private WifiHandler Wifi { get; set; }

    public ScanPopup(WifiHandler wifi)
    {
        InitializeComponent();

        Wifi = wifi;
    }

    public void SetMessage(string message)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Message.Text = message;
        });
    }

    private void CancelButton_Clicked(object sender, EventArgs e)
    {
        Wifi.CancelScan();
    }
}