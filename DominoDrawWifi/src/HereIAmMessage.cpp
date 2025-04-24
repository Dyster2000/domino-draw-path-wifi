/*
This file is part of DominoDrawWifi.

DominoDrawWifi is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation version 3 or later.

DominoDrawWifi is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with DominoDrawWifi. If not, see <https://www.gnu.org/licenses/>.
*/
#include "HereIAmMessage.h"
#include <Arduino.h>
#include <WiFi.h>
#include <string>

const char *HereIAmMessage::HelloMsg = "Domino Hello";

HereIAmMessage::HereIAmMessage()
{
}

void HereIAmMessage::Write(IPAddress destAddr, uint16_t port)
{
  HereIAmMessageData msg;
  WiFiUDP udp;

  memset(&msg, 0, sizeof(msg));
  msg.Type = MSG_TYPE;
  strcpy((char *)msg.Msg, HelloMsg);

  udp.beginPacket(destAddr, port);
  udp.write((uint8_t *)&msg, sizeof(msg));
  udp.endPacket();
}