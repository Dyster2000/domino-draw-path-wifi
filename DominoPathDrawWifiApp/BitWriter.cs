/*
This file is part of DominoDrawWifi.

DominoDrawWifi is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation version 3 or later.

DominoDrawWifi is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with DominoDrawWifi. If not, see <https://www.gnu.org/licenses/>.
*/

using System.Text;

namespace DominoPathDrawWifiApp;

public static class BitWriter
{
    public static byte Read(this byte[] source, int offset)
    {
        return source[offset];
    }

    public static uint ReadUInt16(this byte[] source, int offset)
    {
        return BitConverter.ToUInt16(source, offset);
    }

    public static uint ReadUInt32(this byte[] source, int offset)
    {
        return BitConverter.ToUInt32(source, offset);
    }

    public static string ReadString(this byte[] source, int offset, int maxSize)
    {
        int size = 0;

        for (size = 0; size < maxSize; size++)
        {
            if (source[offset + size] == '\0')
                break;
        }
        return Encoding.ASCII.GetString(source, offset, size);
    }

    public static void Write(this byte[] buffer, int offset, byte value)
    {
        buffer[offset] = (byte)value;
    }

    public static void Write(this byte[] buffer, int offset, short value)
    {
        buffer[offset] = (byte)value;
        buffer[offset + 1] = (byte)(value >> 8);
    }

    public static void Write(this byte[] buffer, int offset, UInt16 value)
    {
        buffer[offset] = (byte)value;
        buffer[offset + 1] = (byte)(value >> 8);
    }

    public static void Write(this byte[] buffer, int offset, uint value)
    {
        buffer[offset] = (byte)value;
        buffer[offset + 1] = (byte)(value >> 0x08);
        buffer[offset + 2] = (byte)(value >> 0x10);
        buffer[offset + 3] = (byte)(value >> 0x18);
    }

    public static void Write(this byte[] buffer, int offset, int value)
    {
        buffer[offset] = (byte)value;
        buffer[offset + 1] = (byte)(value >> 0x08);
        buffer[offset + 2] = (byte)(value >> 0x10);
        buffer[offset + 3] = (byte)(value >> 0x18);
    }

    public static void Write(this byte[] buffer, int offset, byte[] value, int maxSize)
    {
        for (int i = 0; i < maxSize; i++)
        {
            if (i < value.Length)
                buffer[offset + i] = value[i];
            else
                buffer[offset + i] = 0;
        }
    }
}
