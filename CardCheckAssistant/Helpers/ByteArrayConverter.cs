using CardCheckAssistant.Models;
using Elatec.NET;

using System;
using System.Text;

public static class ByteArrayConverter
{
    // Bitmask for MIFARE Classic subtypes
    const int MIFARE_CLASSIC_MASK = 0x1F;

    // Bitmask for MIFARE DESFire subtypes
    const int MIFARE_DESFIRE_MASK = 0x70;


    // Method to convert a byte array to a hexadecimal string
    public static string GetStringFrom(byte[] byteArray)
    {
        if (byteArray == null || byteArray.Length == 0)
        {
            return string.Empty;
        }

        StringBuilder hexString = new StringBuilder(byteArray.Length * 2);
        foreach (byte b in byteArray)
        {
            hexString.AppendFormat("{0:X2}", b);
        }

        return hexString.ToString();
    }

    // Overloaded method to convert a single byte to a hexadecimal string
    public static string GetStringFrom(byte singleByte)
    {
        return singleByte.ToString("X2");
    }

    public static bool IsMifareClassic(CCChipType chipType)
    {
        int typeValue = (int)chipType;

        // Check for MIFARE Classic (0b0001 xxxx and 0b0011 01xx)
        return ((typeValue & 0xF0) == 0x10) || ((typeValue & 0xFC) == 0x34);
    }

    public static bool IsMifareDesfire(CCChipType chipType)
    {
        int typeValue = (int)chipType;

        // Check for MIFARE DESFire (0b01xx xxxx)
        return (typeValue & 0xC0) == 0x40;
    }
}