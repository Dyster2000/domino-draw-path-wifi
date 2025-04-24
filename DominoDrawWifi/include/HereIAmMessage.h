/*
This file is part of DominoDrawBluetooth.

DominoDrawBluetooth is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation version 3 or later.

DominoDrawBluetooth is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with DominoDrawBluetooth. If not, see <https://www.gnu.org/licenses/>.
*/
#include <Arduino.h>

class DominoWifiServer;
struct DominoData;

#pragma pack(1)
struct HereIAmMessageData
{
  uint16_t Type{0};
  uint8_t Msg[15] {0};
};
#pragma pack()


class HereIAmMessage
{
public:
  HereIAmMessage();
  ~HereIAmMessage() = default;

  void Write(IPAddress destAddr, uint16_t port);

private:
  static constexpr uint16_t MSG_TYPE = 0xAA01;
  static const char *HelloMsg;
};