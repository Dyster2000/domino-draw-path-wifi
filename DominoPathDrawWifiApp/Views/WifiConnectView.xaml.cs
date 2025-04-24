/*
This file is part of DominoDrawWifi.

DominoDrawWifi is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation version 3 or later.

DominoDrawWifi is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with DominoDrawWifi. If not, see <https://www.gnu.org/licenses/>.
*/

using System.Net;
using CommunityToolkit.Maui.Views;

namespace DominoPathDrawWifiApp;

public partial class WifiConnectView : ContentView
{
    private Page Owner { get; set; }
    private WifiHandler Wifi { get; set; }

    public WifiConnectView()
    {
        InitializeComponent();
    }

    public void Init(Page owner, WifiHandler wifi)
    {
        Owner = owner;
        Wifi = wifi;

        Wifi.OnConnected += Wifi_OnConnected;
        Wifi.OnDisconnected += Wifi_OnDisconnected;
    }

    private async void ConnectButton_Clicked(object sender, EventArgs e)
    {
        if (!Wifi.IsConnected)
        {
            /*if (!await Wifi.CheckWifiStatus())
            {
                if (!await Wifi.RequestWifiAccess())
                    return;
            }*/

            var popup = new ScanPopup(Wifi);

            Owner.ShowPopup(popup);
            await Wifi.Connect(popup);
            popup.Close();
        }
        else
        {
            Wifi.Disconnect();
        }
    }

    private void Wifi_OnConnected()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ConnectLabel.Text = "Connected";
            ConnectBar.BackgroundColor = Colors.Green;
            ConnectButton.Text = "Disconnect";
        });
    }

    private void Wifi_OnDisconnected()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ConnectLabel.Text = "Not Connected";
            ConnectBar.BackgroundColor = Colors.Red;
            ConnectButton.Text = "Connect";
        });
    }
}
