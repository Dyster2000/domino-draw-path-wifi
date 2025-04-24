/*
This file is part of DominoDrawWifi.

DominoDrawWifi is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation version 3 or later.

DominoDrawWifi is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with DominoDrawWifi. If not, see <https://www.gnu.org/licenses/>.
*/

namespace DominoPathDrawWifiApp;

public partial class MainTabPage : TabbedPage
{
    private WifiHandler _Wifi;

    public MainTabPage()
    {
        InitializeComponent();

        _Wifi = new WifiHandler(this);
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        Manual.Init(_Wifi);
        Draw.Init(_Wifi);
    }
}
