/*
This file is part of DominoDrawWifi.

DominoDrawWifi is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation version 3 or later.

DominoDrawWifi is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with DominoDrawWifi. If not, see <https://www.gnu.org/licenses/>.
*/

using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Layouts;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace DominoPathDrawWifiApp;

public class WifiHandler
{
    public bool IsConnected { get; private set; }

    public DominoManualCommandData ManualCommandData { get; private set; }
    public DominoDrawCommandData DrawCommandData { get; private set; }

    public DominoStatusData StatusData { get; private set; }

    public event Notify OnDisconnected;
    public event Notify OnConnected;

    private ScanPopup _Popup;
    private Page _Owner;
    private CancellationTokenSource CancelControl;
    private bool ConnectionValid;

    private IPAddress ClientAddress;
    private static SemaphoreSlim ClientSync = new SemaphoreSlim(1);


    private Thread ReadThread;
    private readonly int ROBOT_PORT = 11005;

    public WifiHandler(Page owner)
    {
        ManualCommandData = new DominoManualCommandData();
        DrawCommandData = new DominoDrawCommandData();
        StatusData = new DominoStatusData();

        _Owner = owner;

        ReadThread = new Thread(StatusMonitorThread);
        ReadThread.IsBackground = true;
        ReadThread.Start();
    }

    public async Task<bool> CheckWifiStatus()
    {
        try
        {
            var requestStatus = await new WifiPermissions().CheckStatusAsync();
            return requestStatus == PermissionStatus.Granted;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task SendManualCommand(bool manualMode)
    {
        if (IsConnected)
        {
            try
            {
                Debug.WriteLine($"[SendManualCommand] Send now");
                ClientSync.WaitAsync();
                await ManualCommandData.Write(ClientAddress, manualMode);
                ClientSync.Release();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SendManualCommand] Error sending: {ex}");
            }
        }
    }

    public async Task SendDrawCommand()
    {
        if (IsConnected)
        {
            try
            {
                Debug.WriteLine($"[SendDrawCommand] Send now");
                ClientSync.WaitAsync();
                await DrawCommandData.Write(ClientAddress);
                ClientSync.Release();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SendDrawCommand] Error sending: {ex}");
            }
        }
    }

    public async Task<bool> RequestWifiAccess()
    {
        try
        {
            var requestStatus = await new WifiPermissions().RequestAsync();
            return requestStatus == PermissionStatus.Granted;
        }
        catch (Exception ex)
        {
            // logger.LogError(ex);
            return false;
        }
    }

    public void CancelScan()
    {
        if (CancelControl != null)
        {
            Debug.WriteLine($"[CancelScan] Cancel scan");
            CancelControl.Cancel();
        }
        else
            Debug.WriteLine($"[CancelScan] CancelControl is null");
    }

    public async Task Disconnect()
    {
        if (!IsConnected)
        {
            Debug.WriteLine($"[Disconnect] Not connected");
            return;
        }

        Debug.WriteLine($"[Disconnect] Call DisconnectDeviceAsync()");
        ClientAddress = null;
        IsConnected = false;
        OnDisconnected?.Invoke();
    }

    public async Task Connect(ScanPopup popup)
    {
        if (IsConnected)
        {
            Debug.WriteLine("[Connect] Trivial exit, already connected");
            return;
        }

        Debug.WriteLine("[Connect] Enter");
        _Popup = popup;

#if ANDROID
        if (!await PermissionsGrantedAsync())
        {
            await _Owner.DisplayAlert("Permission required", "Application needs location permission", "OK");
            return;
        }
#endif

        CancelControl = new CancellationTokenSource();

        var ipEndPoint = await FindRobot(CancelControl.Token) as IPEndPoint;
        if (ipEndPoint == null)
        {
            // If we get here, it's because they canceled the search
            return;
        }
        ClientAddress = ipEndPoint.Address;

        _Popup = null;
        CancelControl = null;

        OnConnected?.Invoke();
    }

    private async Task<EndPoint> FindRobot(CancellationToken cancelToken)
    {
        var broadcastUdpServer = new UdpClient(ROBOT_PORT);

        var task = Task.Run(() =>
        {
            while (true)
            {
                var from = new IPEndPoint(0, 0);
                try
                {
                    var recvBuffer = broadcastUdpServer.Receive(ref from);

                    var retMsg = DominoHereIAmMessage.Parse(recvBuffer);

                    if (retMsg != null)
                    {
                        Debug.WriteLine($"[Broadcast listener] Got DominoHereIAmMessage from {from}");
                        return from;
                    }
                    else
                        Debug.WriteLine($"[Broadcast listener] Got unknown msg from {from}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Broadcast listener] Got exception: {ex}");
                    return null;
                }
            }
        });

        try
        {
            // Wait for a response to get their actual address
            EndPoint from = await task.WaitAsync(cancelToken);

            broadcastUdpServer.Close();
            return from;
        }
        catch (TaskCanceledException ex)
        {
            broadcastUdpServer.Close();
            return null;
        }
    }

    private async void StatusMonitorThread()
    {
        while (true)
        {
            if (ClientAddress != null)
            {
                ClientSync.WaitAsync();
                IsConnected = await StatusData.Read(ClientAddress);
                ClientSync.Release();

                if (!IsConnected)
                {
                    // Set IsConnected based on getting respose
                    Debug.WriteLine("Lost connection to robot");
                    ClientAddress = null;
                    OnDisconnected?.Invoke();
                }
            }
            Thread.Sleep(1000);
        }
    }

    private async Task<bool> PermissionsGrantedAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        return status == PermissionStatus.Granted;
    }
}
