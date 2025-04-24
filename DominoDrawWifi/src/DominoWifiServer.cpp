/*
This file is part of DominoDrawWifi.

DominoDrawWifi is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation version 3 or later.

DominoDrawWifi is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with DominoDrawWifi. If not, see <https://www.gnu.org/licenses/>.
*/
#include "DominoWifiServer.h"
#include "DrawControlHandler.h"
#include "ManualControlHandler.h"
#include "StatusHandler.h"
#include "HereIAmMessage.h"
#include <ArduinoJson.h>
#include <sstream>
#include <algorithm>
#include <vector>

DominoWifiServer::DominoWifiServer(std::string ssid, std::string password, DominoData &data)
  : m_Ssid{ ssid }
  , m_Password{ password }
  , m_ValidClientConnected{ false }
  , m_Data{ data }
  , m_Server{ SERVER_PORT }
  , m_HaveHeader{ false }
  , m_RawDataUdp{ nullptr }
  , m_RawDataSizeUdp{ 0 }
  , m_RawDataTcp{ nullptr }
  , m_RawDataSizeTcp{ 0 }
  , m_ReadIndexTcp{ 0 }
  , m_TimeSinceLastBroadcastUS{ 0 }
{
}

void DominoWifiServer::Init()
{
  // Connect to WiFi
  Serial.print("Connecting to WiFi...");
  WiFi.begin(m_Ssid.c_str(), m_Password.c_str());
  while (WiFi.status() != WL_CONNECTED)
  {
    delay(1000);
    Serial.print(".");
  }
  Serial.println("\nWiFi Connected!");
  Serial.print("IP Address: ");
  Serial.println(WiFi.localIP());

  // Start the server
  m_Server.begin();
  Serial.println("DominoWifiServer started.");

  // Create message handlers
  m_pStatus = new StatusHandler(m_Data);
  m_pManualControl = new ManualControlHandler(m_Data, *this);
  m_pDrawControl = new DrawControlHandler(m_Data, *this);
}

IPAddress DominoWifiServer::GetWifiAddress()
{
  return WiFi.localIP();
}

bool DominoWifiServer::IsConnected()
{
  WiFiClient client = m_Server.available();

  // Check if a client is connected
  return client.connected();
}

void DominoWifiServer::Loop(uint32_t deltaTimeUS)
{
  HandleUdpBroadcaster(deltaTimeUS);
  HandleWebServer();

  m_pManualControl->Loop(deltaTimeUS);
  m_pDrawControl->Loop(deltaTimeUS);
}

void DominoWifiServer::HandleUdpBroadcaster(uint32_t deltaTimeUS)
{
  m_TimeSinceLastBroadcastUS += deltaTimeUS;

  if (m_TimeSinceLastBroadcastUS > 1000000) // Send every 1 second
  {
    WiFiUDP udp;

    m_TimeSinceLastBroadcastUS -= 1000000;

    auto broadcastAddr = WiFi.calculateBroadcast(WiFi.localIP(), WiFi.subnetMask());
    HereIAmMessage msg;

    msg.Write(broadcastAddr, BROADCAST_PORT);
  }
}

void DominoWifiServer::HandleWebServer()
{
  WiFiClient client = m_Server.available();

  // Check if there is a client connected and we have received data
  if (client && client.connected() && client.available())
  {
    std::stringstream requestBuff;
    std::string header;
    std::string headerRequest;
    std::string body;
    bool readingHeader = true;
    bool blankLine = true;

    //Serial.print("[WebServer] available=");
    //Serial.println(client.available());

    // Read HTTP Request
    while (client.available())
    {
      char c = client.read();
      requestBuff << c;
      if (c != '\r' && c != '\n')
        blankLine = false;
      if ((readingHeader) && (c == '\n'))
      {
        if (blankLine)
        {
          header = requestBuff.str();
          requestBuff = std::stringstream();
          readingHeader = false;
        }
        else
        {
          blankLine = true;
          if (headerRequest.empty())
            headerRequest = requestBuff.str();
        }
      }
    }
    body = requestBuff.str();

    //Serial.print("[WebServer] HeaderLen= ");
    //Serial.print(headerRequest.size());
    //Serial.print(", BodyLen=");
    //Serial.print(body.size());
    //Serial.print(", Header=");
    //Serial.println(headerRequest.c_str());

    //Expect:
    // GET /status
    // POST /manual
    // POST /draw

    auto parts = Split(headerRequest, ' '); 
    if (parts.size() >= 2)
    {
      auto verb = parts[0];
      auto target = ToLower(parts[1]);

      //Serial.print("[WebServer] Verb= ");
      //Serial.print(verb.c_str());
      //Serial.print(", target=");
      //Serial.println(target.c_str());

      if (verb == "GET")
      {
        if (target == "/status")
        {
          //Serial.println("[WebServer] Return status");
          SendReturn(client, m_pStatus->GetReturnData());
        }
        else
        {
          //Serial.println("[WebServer] Return bad");
          SendBadReturn(client);
        }
      }
      else if (verb == "POST")
      {
        // Find start & end of json body
        auto start = body.find('{');
        auto end = body.rfind('}');

        if (start == std::string::npos || end == std::string::npos)
        {
          //Serial.println("[WebServer] Bad json, Return bad");
          SendBadReturn(client);
          return;
        }
        body = body.substr(start, end - start + 1);

        // Check for which endpoint
        if (target == "/manual")
        {
          if (HandleManualCommand(body.c_str()))
            SendReturnGoodNoData(client);
          else
            SendBadReturn(client);
        }
        else if (target == "/draw")
        {
          if (HandleDrawCommand(body.c_str()))
            SendReturnGoodNoData(client);
          else
            SendBadReturn(client);
        }
        else
        {
          //Serial.println("[WebServer] Return bad");
          SendBadReturn(client);
        }
      }
      else
      {
        //Serial.println("[WebServer] Return bad");
        SendBadReturn(client);
      }
    }
  }
}

void DominoWifiServer::SendHeader(WiFiClient &client, size_t len)
{
  client.println("HTTP/1.1 200 OK");
  client.println("Content-type:application/json");
  client.print("Content-Length: ");
  client.println(len);
  client.println("Connection: close");
  client.println();  
}

void DominoWifiServer::SendReturnGoodNoData(WiFiClient &client)
{
  client.println("HTTP/1.1 204 OK");
  client.println("Content-type:text/html");
  client.println("Connection: close");
  client.println();  
}

void DominoWifiServer::SendBadReturn(WiFiClient &client)
{
  client.println("HTTP/1.1 404 OK");
  client.println("Content-type:text/html");
  client.println("Connection: close");
  client.println();  
}

void DominoWifiServer::SendReturn(WiFiClient &client, std::string body)
{
  SendHeader(client, body.length());
  client.println(body.c_str());
}

bool DominoWifiServer::HandleManualCommand(const char *data)
{
  JsonDocument doc;

  //Serial.println("[HandleManualCommand] Enter");

  // Deserialize the JSON document
  DeserializationError error = deserializeJson(doc, data);
  ManualCommandData recvData;

  // Test if parsing succeeds
  if (error)
  {
    //Serial.print(F("[HandleManualCommand] deserializeJson() failed: "));
    //Serial.println(error.f_str());
    //Serial.print(F("[HandleManualCommand] Data: "));
    //Serial.println(data);
    return false;
  }
  //Serial.println("[HandleManualCommand] Load data");

  recvData.Moving = doc["Moving"];
  recvData.Dispensing = doc["Dispensing"];
  recvData.StopOnEmpty = doc["StopOnEmpty"];
  recvData.ManualMode = doc["ManualMode"];
  recvData.Direction = doc["Direction"];

  //Serial.print("[HandleManualCommand] Moving=");
  //Serial.print(recvData.Moving);
  //Serial.print(", Dispensing=");
  //Serial.print(recvData.Dispensing);
  //Serial.print(", StopOnEmpty=");
  //Serial.print(recvData.StopOnEmpty);
  //Serial.print(", ManualMode=");
  //Serial.print(recvData.ManualMode);
  //Serial.print(", Direction=");
  //Serial.println(recvData.Direction);

  m_pManualControl->OnRecvMessage(recvData);

  return true;
}

bool DominoWifiServer::HandleDrawCommand(const char *data)
{
  JsonDocument doc;

  //Serial.println("[HandleDrawCommand] Enter");

  // Deserialize the JSON document
  DeserializationError error = deserializeJson(doc, data);

  // Test if parsing succeeds
  if (error)
  {
    //Serial.print(F("[HandleDrawCommand] deserializeJson() failed: "));
    //Serial.println(error.f_str());
    return false;
  }
  //Serial.println("[HandleDrawCommand] Load data");

  JsonArray drawData = doc["DrivePath"];
  std::vector<DrawPathStep> recvData(drawData.size());
  size_t index = 0;

  for (JsonVariant item : drawData)
  {
    DrawPathStep entry;
    entry.DistanceMM = item["DistanceMM"];
    entry.Angle = item["Angle"];
     
    recvData[index++] = entry;
  }

  //Serial.print("[HandleDrawCommand] Size=");
  //Serial.println(recvData.size());
  index = 0;
  for (DrawPathStep &item : recvData)
  {
    //Serial.print(index);
    //Serial.print(": DistanceMM=");
    //Serial.print(item.DistanceMM);
    //Serial.print(", Angle=");
    //Serial.println(item.Angle);
    index++;
  }

  m_pDrawControl->OnRecvMessage(recvData);

  return true;
}

std::string DominoWifiServer::ToLower(std::string s)
{
  std::transform(s.begin(), s.end(), s.begin(),
                  [](unsigned char c){ return std::tolower(c); }
                );
  return s;
}

std::vector<std::string> DominoWifiServer::Split(const std::string &s, char delim) 
{
  std::vector<std::string> result;
  std::stringstream ss(s);
  std::string item;

  while (getline(ss, item, delim))
    result.push_back(item);

  return result;
}
