/*
This file is part of DominoDrawWifi.

DominoDrawWifi is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation version 3 or later.

DominoDrawWifi is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with DominoDrawWifi. If not, see <https://www.gnu.org/licenses/>.
*/

using System.Linq;

namespace DominoPathDrawWifiApp;

internal struct IntPoint
{
    public int X;
    public int Y;

    public IntPoint()
    {
        X = 0;
        Y = 0;
    }

    public IntPoint(SizeF src)
    {
        X = (int)Math.Round(src.Width);
        Y = (int)Math.Round(src.Height);
    }

    public static bool operator ==(IntPoint p1, IntPoint p2)
    {
        return p1.X == p2.X && p1.Y == p2.Y;
    }

    public static bool operator !=(IntPoint p1, IntPoint p2)
    {
        return p1.X != p2.X || p1.Y != p2.Y;
    }

    public override string ToString()
    {
        return $"({X}, {Y})";
    }
}

public partial class DrawDrive : ContentPage
{
    private WifiHandler Wifi { get; set; }
    private List<PointF> _Points;

    public DrawDrive()
    {
        InitializeComponent();

        _Points = new List<PointF>();
    }

    public void Init(WifiHandler wifi)
    {
        Wifi = wifi;

        WifiConnect.Init(this, Wifi);
        DriveControls.Init(Wifi, false);
        DriveControls.EnableMoving(false);

        Wifi.OnConnected += Wifi_OnConnected;
        Wifi.OnDisconnected += Wifi_OnDisconnected;
    }

    private void DrawInput_DrawingLineCompleted(object sender, CommunityToolkit.Maui.Core.DrawingLineCompletedEventArgs e)
    {
        _Points.Clear();
        foreach (var entry in e.LastDrawingLine.Points)
            _Points.Add(entry);

        if (Wifi.IsConnected)
            SendPathButton.IsEnabled = true;
    }

    private PathStep CalcNextStep(IntPoint p1, IntPoint p2)
    {
        double distance = Math.Sqrt(Math.Pow((p2.X - p1.X), 2) + Math.Pow((p2.Y - p1.Y), 2));
        UInt16 distanceRemaining = (UInt16)(distance);
        double angle = Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
        if (angle < 0)
            angle += Math.PI * 2;
        UInt16 angleDeg = (UInt16)(angle * 180 / Math.PI);

        return new PathStep(distanceRemaining, angleDeg);
    }

    private async void SendPathButton_Clicked(object sender, EventArgs e)
    {
        var start = _Points[0];
        var startRaw = _Points[0];
        List<PathStep> drivePath = new List<PathStep>();
        List<SizeF> pathSteps = new List<SizeF>();
        IntPoint prevPoint = new IntPoint();
        bool pastStart = false;

        List<IntPoint> debugPoints = new List<IntPoint>();
        debugPoints.Add(prevPoint);

        start.X = (float)(start.X * DrawScale.Value);
        start.Y = (float)((DrawInput.Height - start.Y) * DrawScale.Value);
        startRaw.Y = (float)(DrawInput.Height - startRaw.Y);
        foreach (var point in _Points)
        {
            PointF adjustedPoint = new Point(point.X, (DrawInput.Height - point.Y));

            if (!pastStart)
            {
                double distanceFromStart = Math.Sqrt(Math.Pow((adjustedPoint.X - startRaw.X), 2) + Math.Pow((adjustedPoint.Y - startRaw.Y), 2));

                if (distanceFromStart > 5)
                    pastStart = true;
                else
                    continue;
            }
            adjustedPoint.X = (float)(adjustedPoint.X * DrawScale.Value);
            adjustedPoint.Y = (float)(adjustedPoint.Y * DrawScale.Value);
            IntPoint thisPoint = new IntPoint(adjustedPoint - start);

            if (thisPoint != prevPoint)
            {
                debugPoints.Add(thisPoint);
                drivePath.Add(CalcNextStep((IntPoint)prevPoint, thisPoint));
            }
            prevPoint = thisPoint;
        }

        Wifi.DrawCommandData.DrivePath = drivePath;

        DriveControls.EnableMoving(false);

        await Wifi.SendDrawCommand();

        DriveControls.EnableMoving(true);
    }

    private void Wifi_OnConnected()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            SendPathButton.IsEnabled = _Points.Count > 0;
        });
    }

    private void Wifi_OnDisconnected()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            SendPathButton.IsEnabled = false;
        });
    }
}