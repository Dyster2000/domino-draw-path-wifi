/*
This file is part of DominoDrawWifi.

DominoDrawWifi is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation version 3 or later.

DominoDrawWifi is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with DominoDrawWifi. If not, see <https://www.gnu.org/licenses/>.
*/

namespace DominoPathDrawWifiApp;

public class DominoHereIAmMessage
{
    public static readonly UInt16 MSG_SIZE = 17;
    public static readonly UInt16 MSG_TYPE = 0xAA01;
    public static readonly UInt16 TEXT_SIZE = 15;

    public UInt16 MsgType;
    public string Msg;

    static public DominoHereIAmMessage Parse(byte[] data)
    {
        MessageBuffer parser = new MessageBuffer(data);
        int offset = 0;
        var msgType = parser.ReadUInt16(ref offset);

        if (msgType == DominoHereIAmMessage.MSG_TYPE && data.Length == DominoHereIAmMessage.MSG_SIZE)
        {
            DominoHereIAmMessage msg = new DominoHereIAmMessage();

            msg.Msg = parser.ReadString(ref offset, TEXT_SIZE);

            return msg;
        }

        return null;
    }

    private DominoHereIAmMessage()
    {
    }
}
