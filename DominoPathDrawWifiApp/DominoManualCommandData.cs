/*
This file is part of DominoDrawWifi.

DominoDrawWifi is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation version 3 or later.

DominoDrawWifi is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with DominoDrawWifi. If not, see <https://www.gnu.org/licenses/>.
*/

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Diagnostics;
using Newtonsoft.Json;

namespace DominoPathDrawWifiApp;

public class DominoManualCommandData
{
    public bool Moving { get; set; } = false;
    public bool Dispensing { get; set; } = true;
    public bool StopOnEmpty { get; set; } = true;
    public byte Direction { get; set; } = 0;

    public DominoManualCommandData()
    {
    }

    public async Task Write(IPAddress addr, bool manualMode)
    {
        bool SendSuccess = false;
        ManualCommandRestData cmd = new ManualCommandRestData();

        cmd.Moving = Moving;
        cmd.Dispensing = Dispensing;
        cmd.StopOnEmpty = StopOnEmpty;
        cmd.ManualMode = manualMode;
        cmd.Direction = Direction;

        for (int tries = 0; tries < 5 && !SendSuccess; tries++)
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.BaseAddress = $"http://{addr}/";
                    client.Headers.Add("user-agent", "DominoApp");
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    string dataJson = JsonConvert.SerializeObject(cmd);
                    var response = client.UploadString("manual", dataJson);
                    SendSuccess = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DominoManualCommandData::Write] Error sending: {ex}");
                Thread.Sleep(100);
            }
        }
    }

    public void UpdateFromStatus(DominoStatusData statusData)
    {
        Moving = statusData.Moving;
        Dispensing = statusData.Dispensing;
        StopOnEmpty = statusData.StopOnEmpty;
        Direction = statusData.Direction;
    }
}
