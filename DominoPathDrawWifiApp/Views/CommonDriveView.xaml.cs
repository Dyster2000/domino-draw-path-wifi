/*
This file is part of DominoDrawWifi.

DominoDrawWifi is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation version 3 or later.

DominoDrawWifi is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with DominoDrawWifi. If not, see <https://www.gnu.org/licenses/>.
*/

namespace DominoPathDrawWifiApp;

public partial class CommonDriveView : ContentView
{
    private WifiHandler Wifi { get; set; }
    private bool ManualMode { get; set; }
    private bool MoveNotAllowed { get; set; } = false;

    public CommonDriveView()
    {
        InitializeComponent();
    }

    public void Init(WifiHandler wifi, bool manualMode)
    {
        Wifi = wifi;
        ManualMode = manualMode;

        Wifi.OnConnected += Wifi_OnConnected;
        Wifi.OnDisconnected += Wifi_OnDisconnected;
        Wifi.StatusData.OnDataChanged += StatusData_OnMovingChanged;
        Wifi.StatusData.OnDataChanged += StatusData_OnDispensingChanged;
        Wifi.StatusData.OnDataChanged += StatusData_OnStopOnEmptyChanged;
    }

    public void EnableMoving(bool enable)
    {
        MoveNotAllowed = !enable;
        MainThread.BeginInvokeOnMainThread(() =>
        {
            MoveButton.IsEnabled = enable;
        });
    }

    private void Wifi_OnConnected()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            IsEnabled = true;
        });
    }

    private void Wifi_OnDisconnected()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            IsEnabled = false;
        });
    }

    private void StatusData_OnMovingChanged()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            // If manual mode enables moving, then draw mode can't and vice versa
            if (MoveNotAllowed)
                MoveButton.IsEnabled = false;
            else
                MoveButton.IsEnabled = !Wifi.StatusData.Moving || ManualMode == Wifi.StatusData.ManualMode;

            if (Wifi.StatusData.Moving)
            {
                MoveButton.Text = "Moving";
                MoveButton.BackgroundColor = Colors.Green;
            }
            else
            {
                MoveButton.Text = "Stopped";
                MoveButton.BackgroundColor = Colors.Red;
            }
        });
    }

    private void StatusData_OnDispensingChanged()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (Wifi.StatusData.Dispensing)
            {
                DispenseButton.Text = "Dispense Dominoes - On";
                DispenseButton.BackgroundColor = Colors.Green;
            }
            else
            {
                DispenseButton.Text = "Dispense Dominoes - Off";
                DispenseButton.BackgroundColor = Colors.Red;
            }
        });
    }

    private void StatusData_OnStopOnEmptyChanged()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            StopOnEmpty.IsChecked = Wifi.StatusData.StopOnEmpty;
        });
    }

    private async void MoveButton_Clicked(object sender, EventArgs e)
    {
        Wifi.ManualCommandData.UpdateFromStatus(Wifi.StatusData);
        Wifi.ManualCommandData.Moving = !Wifi.StatusData.Moving; // Invert from current

        Wifi.SendManualCommand(ManualMode);
    }

    private async void DispenseButton_Clicked(object sender, EventArgs e)
    {
        Wifi.ManualCommandData.UpdateFromStatus(Wifi.StatusData);
        Wifi.ManualCommandData.Dispensing = !Wifi.StatusData.Dispensing; // Invert from current

        Wifi.SendManualCommand(ManualMode);
    }

    private async void StopOnEmpty_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        Wifi.ManualCommandData.UpdateFromStatus(Wifi.StatusData);
        Wifi.ManualCommandData.StopOnEmpty = StopOnEmpty.IsChecked;

        Wifi.SendManualCommand(ManualMode);
    }
}