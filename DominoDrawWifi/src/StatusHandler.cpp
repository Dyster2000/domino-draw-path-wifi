/*
This file is part of DominoDrawWifi.

DominoDrawWifi is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation version 3 or later.

DominoDrawWifi is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with DominoDrawWifi. If not, see <https://www.gnu.org/licenses/>.
*/
#include "StatusHandler.h"
#include "DominoWifiServer.h"
#include <Arduino.h>
#include <ArduinoJson.h>

StatusHandler::StatusHandler(DominoData &data)
  : m_Data{ data }
{
}

std::string StatusHandler::GetReturnData()
{
  JsonDocument doc;
  char output[256];

  JsonObject data = doc.to<JsonObject>();
  data["Moving"] = m_Data.Moving;
  data["Dispensing"] = m_Data.Dispensing;
  data["StopOnEmpty"] = m_Data.StopOnEmpty;
  data["IsEmpty"] = m_Data.IsEmpty;
  data["ManualMode"] = m_Data.ManualMode;
  data["Direction"] = m_Data.Direction;
  data["DistanceTraveledMM"] = m_Data.DistanceTraveledMM;

  //Serial.print("[StatusHandler::GetReturnData] Moving=");
  //Serial.print(m_Data.Moving);
  //Serial.print(", Dispensing=");
  //Serial.print(m_Data.Dispensing);
  //Serial.print(", StopOnEmpty=");
  //Serial.print(m_Data.StopOnEmpty);
  //Serial.print(", IsEmpty=");
  //Serial.print(m_Data.IsEmpty);
  //Serial.print(", ManualMode=");
  //Serial.print(m_Data.ManualMode);
  //Serial.print(", Direction=");
  //Serial.println(m_Data.Direction);

  serializeJson(doc, output);
  return output;
}