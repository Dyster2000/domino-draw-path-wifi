/*
This file is part of DominoDrawWifi.

DominoDrawWifi is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation version 3 or later.

DominoDrawWifi is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with DominoDrawWifi. If not, see <https://www.gnu.org/licenses/>.
*/
#pragma once

struct DominoData
{
  uint8_t Moving{false};
  uint8_t Dispensing{true};
  uint8_t StopOnEmpty{true};
  uint8_t IsEmpty{false};
  uint8_t ManualMode{true}; // Manual vs Draw control
  int8_t Direction{0};
  uint32_t DistanceTraveledMM{0};
};

struct ManualCommandData
{
  uint8_t Moving{false};
  uint8_t Dispensing{true};
  uint8_t StopOnEmpty{true};
  uint8_t ManualMode{true}; // Manual vs Draw control
  int8_t Direction{0};
};

struct DrawPathStep
{
  uint16_t DistanceMM{0};
  uint16_t Angle{0};
};
