/*
This file is part of DominoDrawWifi.

DominoDrawWifi is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation version 3 or later.

DominoDrawWifi is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with DominoDrawWifi. If not, see <https://www.gnu.org/licenses/>.
*/
#include <Arduino.h>
#include "DominoData.h"
#include <vector>

class DominoWifiServer;

constexpr uint16_t NumPoints = 8;

class DrawControlHandler
{
public:
  DrawControlHandler(DominoData &data, DominoWifiServer &server);
  ~DrawControlHandler() = default;

  void Loop(uint32_t deltaTime);

  void OnRecvMessage(std::vector<DrawPathStep> data);

  void SetNextStep();

private:
  static constexpr float MicrosPerTurnDegree = 1.0/5000000; // 90 degrees over 5 seconds at max turn rate

  DominoData &m_Data;
  DominoWifiServer &m_Server;

  bool m_ReceivedDataUpdated;
  std::vector<DrawPathStep> m_Steps;

  int m_CurrentIndex;
  bool m_PathActive;
  uint32_t m_DistanceForNextStep;
  float m_CurrentAngle;
  float m_TargetAngle;
  float m_LastDiff;
  uint32_t m_StepElapsedTimeUS;
};