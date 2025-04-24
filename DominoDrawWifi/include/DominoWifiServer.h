/*
This file is part of DominoDrawWifi.

DominoDrawWifi is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation version 3 or later.

DominoDrawWifi is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with DominoDrawWifi. If not, see <https://www.gnu.org/licenses/>.
*/
#pragma once

#include <WiFi.h>
#include <string>
#include <vector>

class StatusHandler;
class ManualControlHandler;
class DrawControlHandler;
struct DominoData;

#pragma pack(1)
struct MessageHeader
{
  uint8_t Type{0};
  uint16_t Len{0};
};
#pragma pack()

enum class MsgTypes
{
  HereIAm = 1,
  Hello = 10,
  Status = 11,
  ManualCommand = 12,
  DrawCommand = 13
};

class DominoWifiServer
{
public:
  DominoWifiServer(std::string ssid, std::string password, DominoData &data);

  void Init();
  void Loop(uint32_t deltaTimeUS);

  IPAddress GetWifiAddress();
  bool IsConnected();

private:
  void HandleUdpBroadcaster(uint32_t deltaTimeUS);
  void HandleWebServer();

  void SendHeader(WiFiClient &client, size_t len);
  void SendReturnGoodNoData(WiFiClient &client);
  void SendBadReturn(WiFiClient &client);
  void SendReturn(WiFiClient &client, std::string body);

  bool HandleManualCommand(const char *data);
  bool HandleDrawCommand(const char *data);

  std::string ToLower(std::string s);
  std::vector<std::string> Split(const std::string &s, char delim);

private:
  static constexpr uint16_t BROADCAST_PORT = 11005;
  static constexpr uint16_t SERVER_PORT = 80;
  //static constexpr uint16_t APP_PORT = 11006;
  static constexpr int HEADER_SIZE = 3;

  std::string m_Ssid;
  std::string m_Password;
  WiFiServer m_Server;
  bool m_ValidClientConnected;
  bool m_HaveHeader;
  MessageHeader m_Header;
  uint8_t *m_RawDataUdp;
  size_t m_RawDataSizeUdp;
  uint8_t *m_RawDataTcp;
  size_t m_RawDataSizeTcp;
  size_t m_ReadIndexTcp;

  uint32_t m_TimeSinceLastBroadcastUS;

  DominoData &m_Data;
  StatusHandler *m_pStatus;
  ManualControlHandler *m_pManualControl;
  DrawControlHandler *m_pDrawControl;
};
