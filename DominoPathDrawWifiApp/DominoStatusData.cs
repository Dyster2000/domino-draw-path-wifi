/*
This file is part of DominoDrawWifi.

DominoDrawWifi is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation version 3 or later.

DominoDrawWifi is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with DominoDrawWifi. If not, see <https://www.gnu.org/licenses/>.
*/

using System.Net;
using System.Net.Http.Headers;
using System.Diagnostics;

namespace DominoPathDrawWifiApp;

public delegate void Notify();

public class DominoStatusData
{
    static readonly UInt16 MsgSize = 10;

    public bool Moving { get; set; }
    public bool Dispensing { get; set; }
    public bool StopOnEmpty { get; set; }
    public bool IsEmpty { get; set; }
    public bool ManualMode { get; set; }
    public byte Direction { get; set; }
    public uint DistanceTraveled { get; set; }

    public bool JustConnected { get; set; }

    public event Notify OnDataChanged;

    public DominoStatusData()
    {
        JustConnected = false;
    }

    public async Task<bool> Read(IPAddress addr)
    {
        bool SendSuccess = false;
        StatusRestData status = null;

        for (int tries = 0; tries < 5 && !SendSuccess; tries++)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "status");

                    client.BaseAddress = new Uri($"http://{addr}/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = client.Send(request);
                    if (response.IsSuccessStatusCode)
                    {
                        status = await response.Content.ReadAsAsync<StatusRestData>();
                        Assign(status);
                        return true;
                    }
                    else
                        return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DominoManualCommandData::Write] Error sending: {ex}");
                Thread.Sleep(100);
            }
        }
        return false;
    }

    private void Assign(StatusRestData msg)
    {
        var dataChanged = JustConnected;
        dataChanged = dataChanged || Moving != msg.Moving;
        dataChanged = dataChanged || Dispensing != msg.Dispensing;
        dataChanged = dataChanged || StopOnEmpty != msg.StopOnEmpty;
        dataChanged = dataChanged || IsEmpty != msg.IsEmpty;
        dataChanged = dataChanged || ManualMode != msg.ManualMode;
        dataChanged = dataChanged || Direction != msg.Direction;
        dataChanged = dataChanged || DistanceTraveled != msg.DistanceTraveled;

        Moving = msg.Moving;
        Dispensing = msg.Dispensing;
        StopOnEmpty = msg.StopOnEmpty;
        IsEmpty = msg.IsEmpty;
        ManualMode = msg.ManualMode;
        Direction = msg.Direction;
        DistanceTraveled = msg.DistanceTraveled;

        if (dataChanged)
            OnDataChanged?.Invoke();

        JustConnected = false;
    }
}
