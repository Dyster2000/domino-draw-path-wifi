/*
This file is part of DominoDrawWifi.

DominoDrawWifi is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation version 3 or later.

DominoDrawWifi is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with DominoDrawWifi. If not, see <https://www.gnu.org/licenses/>.
*/
#include "ManualControlHandler.h"
#include "DominoWifiServer.h"
#include <Arduino.h>

ManualControlHandler::ManualControlHandler(DominoData &data, DominoWifiServer &server)
  : m_Data(data)
  , m_Server(server)
  , m_ReceivedDataUpdated(false)
{
}

void ManualControlHandler::Loop(uint32_t deltaTime)
{
  if (m_ReceivedDataUpdated)
  {
    m_ReceivedDataUpdated = false;
    m_Data.StopOnEmpty = m_ReceivedData.StopOnEmpty;
    m_Data.Dispensing = m_ReceivedData.Dispensing;
    m_Data.ManualMode = m_ReceivedData.ManualMode;
    
    // Only take the direction if in manual mode. Draw mode will control direction with the path list
    //Serial.print("[ManualControlHandler::Loop] ManualMode=");
    //Serial.println(m_Data.ManualMode);
    if (m_Data.ManualMode)
      m_Data.Direction = m_ReceivedData.Direction;

    // Check if not moving and requesting to move
    if (m_ReceivedData.Moving && !m_Data.Moving)
    {
      if (!m_Data.IsEmpty || !m_Data.StopOnEmpty)
      {
        //Serial.print("[ManualControlHandler::Loop] Set Moving=true");
        m_Data.Moving = true;
      }
    }
    else if (!m_ReceivedData.Moving && m_Data.Moving)
    {
      //Serial.print("[ManualControlHandler::Loop] Set Moving=false");
      m_Data.Moving = false;
    }
  }
}

void ManualControlHandler::OnRecvMessage(const ManualCommandData &data)
{
  m_ReceivedData = data;
  m_ReceivedDataUpdated = true;
}