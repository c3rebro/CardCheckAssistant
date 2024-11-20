﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardCheckAssistant.Models;

    /// <summary>
    /// Type of Chip from NXP AN10833
    /// </summary>
    /// <remarks>
    /// <code>
    /// Mifare Classic =    0x10xx - 0x1Fxx
    /// NXP SAM =           0x20xx - 0x2Fxx
    /// Mifare Plus =       0x30xx - 0x3Fxx
    /// Mifare Desfire =    0x40xx - 0x7Fxx
    /// Desfire Light =     0x80xx - 0x8Fxx
    /// Mifare Ultralight = 0x90xx - 0x9Fxx
    /// Mifare Mini =       0xA0xx - 0xAFxx
    /// NXP NTAG =          0xB0xx - 0xBFxx
    /// NXP ICODE =         0xC0xx - 0xCFxx
    ///
    /// 0bxxxx 0000 = nxp type
    /// 0b0000 xxxx = nxp subtype
    /// 0bxxxx 1xxx = smartmx variant
    /// </code>
    /// </remarks>
    [Flags]
    public enum CCChipType
    {
        NOTAG = 0,
        // LF Tags
        EM4102 = 0x40,    // "EM4x02/CASI-RUSCO" (aka IDRO_A)
        HITAG1S = 0x41,   // "HITAG 1/HITAG S"   (aka IDRW_B)
        HITAG2 = 0x42,    // "HITAG 2"           (aka IDRW_C)
        EM4150 = 0x43,    // "EM4x50"            (aka IDRW_D)
        AT5555 = 0x44,    // "T55x7"             (aka IDRW_E)
        ISOFDX = 0x45,    // "ISO FDX-B"         (aka IDRO_G)
        EM4026 = 0x46,    // N/A                 (aka IDRO_H)
        HITAGU = 0x47,    // N/A                 (aka IDRW_I)
        EM4305 = 0x48,    // "EM4305"            (aka IDRW_K)
        HIDPROX = 0x49,	// "HID Prox"
        TIRIS = 0x4A,	    // "ISO HDX/TIRIS"
        COTAG = 0x4B,	    // "Cotag"
        IOPROX = 0x4C,	// "ioProx"
        INDITAG = 0x4D,	// "Indala"
        HONEYTAG = 0x4E,	// "NexWatch"
        AWID = 0x4F,	    // "AWID"
        GPROX = 0x50,	    // "G-Prox"
        PYRAMID = 0x51,	// "Pyramid"
        KERI = 0x52,	    // "Keri"
        DEISTER = 0x53,	// "Deister"
        CARDAX = 0x54,	// "Cardax"
        NEDAP = 0x55,	    // "Nedap"
        PAC = 0x56,	    // "PAC"
        IDTECK = 0x57,	// "IDTECK"
        ULTRAPROX = 0x58,	// "UltraProx"
        ICT = 0x59,	    // "ICT"
        ISONAS = 0x5A,	// "Isonas"
        // HF Tags
        MIFARE = 0x80,	// "ISO14443A/MIFARE"
        ISO14443B = 0x81,	// "ISO14443B"
        ISO15693 = 0x82,	// "ISO15693"
        LEGIC = 0x83,	    // "LEGIC"
        HIDICLASS = 0x84,	// "HID iCLASS"
        FELICA = 0x85,	// "FeliCa"
        SRX = 0x86,	    // "SRX"
        NFCP2P = 0x87,	// "NFC Peer-to-Peer"
        BLE = 0x88,	    // "Bluetooth Low Energy"
        TOPAZ = 0x89,     // "Topaz"
        CTS = 0x8A,       // "CTS256 / CTS512"
        BLELC = 0x8B,     // "Bluetooth Low Energy LEGIC Connect"

        /* CUSTOM
         * 
         * Mifare Classic = 0x10 - 0x1F
         * NXP SAM = 0x20 - 0x2F
         * Mifare Plus = 0x30 - 0x3F
         * Mifare Desfire = 0x40 - 0x7F
         * Mifare Desfire Light = 0x80 - 0x8F
         * Mifare Ultralight = 0x90 - 0x9F
         * Mifare Mini = 0xA0 - 0xAF
         * NXP NTAG = 0xB0 - 0xBF
         * NXP ICODE = 0xC0 - 0xCF
         * 
         * 0bxxxx 0000 = mifare type
         * 0x0000 xxxx = mifare subtype
         */

        /* 
         * 0b0001 xxxx = mifare classic
         * 0b0001 1xxx = smartmx classic 
         *          
         * 0b0001 0000 = mifare classic - 1k
         * 0b0001 0001 = mifare classic - 2k
         * 0b0001 0010 = mifare classic - 4k
         * 
         * 0b0001 1000 = smartmx classic - 1k
         * 0b0001 1001 = smartmx classic - 2k
         * 0b0001 1010 = smartmx classic - 4k
         */
        Unspecified = 0x0100,

        MifareClassic = 0x1000,
        Mifare1K = 0x1100,
        Mifare2K = 0x1200,
        Mifare4K = 0x1300,

        SmartMX_Mifare_1K = 0x1900,
        SmartMX_Mifare_2K = 0x1A00,
        SmartMX_Mifare_4K = 0x1B00,

        /* 0b0010 0000 = MifareSAM
         * 
         * 0b0010 0001 = SAM_AV1
         * 0b0010 0010 = SAM_AV2
         */

        MifareSAM = 0x2000,
        SAM_AV1 = 0x2100,
        SAM_AV2 = 0x2200,

        /* 0b0011 xxxx = Mifare Plus
         * 
         * 0b0011 00xx = Mifare Plus SL0
         * 0b0011 0000 = Mifare Plus SL0 - 1k
         * 0b0011 0001 = Mifare Plus SL0 - 2k
         * 0b0011 0010 = Mifare Plus SL0 - 4k
         * 
         * 0b0011 01xx = Mifare Plus SL1
         * 0b0011 0100 = Mifare Plus SL1 - 1k
         * 0b0011 0101 = Mifare Plus SL1 - 2k
         * 0b0011 0110 = Mifare Plus SL1 - 4k
         * 
         * 0b0011 10xx = Mifare Plus SL2
         * 0b0011 1000 = Mifare Plus SL2 - 1k
         * 0b0011 1001 = Mifare Plus SL2 - 2k
         * 0b0011 1010 = Mifare Plus SL2 - 4k
         * 
         * 0b0011 11xx = Mifare Plus SL3
         * 0b0011 1100 = Mifare Plus SL3 - 1k
         * 0b0011 1101 = Mifare Plus SL3 - 2k
         * 0b0011 1110 = Mifare Plus SL3 - 4k
        */

        MifarePlus = 0x3000,
        MifarePlus_SL0_1K = 0x3100,
        MifarePlus_SL0_2K = 0x3200,
        MifarePlus_SL0_4K = 0x3300,

        MifarePlus_SL1_1K = 0x3400,
        MifarePlus_SL1_2K = 0x3500,
        MifarePlus_SL1_4K = 0x3600,

        MifarePlus_SL2_1K = 0x3800,
        MifarePlus_SL2_2K = 0x3900,
        MifarePlus_SL2_4K = 0x3A00,

        MifarePlus_SL3_1K = 0x3C00,
        MifarePlus_SL3_2K = 0x3D00,
        MifarePlus_SL3_4K = 0x3E00,

        /* 0b01xx xxxx = Mifare Desfire
         * 0b01xx 1xxx = SmartMX Desfire
         * 0b0100 x000 = EV0
         * 0b0101 x000 = EV1
         * 0b0110 x000 = EV2
         * 0b0111 x000 = EV3
         * 
         * 0b0100 0001 = Mifare Desfire EV0 - 256
         * 0b0100 0010 = Mifare Desfire EV0 - 1k
         * 0b0100 0011 = Mifare Desfire EV0 - 2k
         * 0b0100 0100 = Mifare Desfire EV0 - 4k
         * 
         * 0b0100 1xxx = SmartMX Desfire EV0
         * 0b0100 1001 = SmartMX Desfire EV0 - 256
         * 0b0100 1010 = SmartMX Desfire EV0 - 1k
         * 0b0100 1011 = SmartMX Desfire EV0 - 2k
         * 0b0100 1100 = SmartMX Desfire EV0 - 4k
         * 
         * 0b0101 0xxx = Mifare Desfire EV1
         * 0b0101 0000 = Mifare Desfire EV1 - 256
         * 0b0101 0001 = Mifare Desfire EV1 - 2k
         * 0b0101 0010 = Mifare Desfire EV1 - 4k
         * 0b0101 0011 = Mifare Desfire EV1 - 8k
         * 
         * 0b0101 1xxx = SmartMX Desfire EV1
         * 0b0101 1000 = SmartMX Desfire EV1 - 256
         * 0b0101 1001 = SmartMX Desfire EV1 - 2k
         * 0b0101 1010 = SmartMX Desfire EV1 - 4k
         * 0b0101 1011 = SmartMX Desfire EV1 - 8k
         * 
         * 0b0110 0xxx = Mifare Desfire EV2
         * 0b0110 0000 = Mifare Desfire EV2 - 2k
         * 0b0110 0001 = Mifare Desfire EV2 - 4k
         * 0b0110 0010 = Mifare Desfire EV2 - 8k
         * 0b0110 0011 = Mifare Desfire EV2 - 16k
         * 0b0110 0100 = Mifare Desfire EV2 - 32k
         * 
         * 0b0110 1xxx = SmartMX Desfire EV2
         * 0b0110 1000 = SmartMX Desfire EV2 - 2k
         * 0b0110 1001 = SmartMX Desfire EV2 - 4k
         * 0b0110 1010 = SmartMX Desfire EV2 - 8k
         * 0b0110 1011 = SmartMX Desfire EV2 - 16k
         * 0b0110 1100 = SmartMX Desfire EV2 - 32k
         * 
         * 0b0111 0xxx = Mifare Desfire EV3
         * 0b0111 0000 = Mifare Desfire EV3 - 2k
         * 0b0111 0001 = Mifare Desfire EV3 - 4k
         * 0b0111 0010 = Mifare Desfire EV3 - 8k
         * 0b0111 0011 = Mifare Desfire EV3 - 16k
         * 0b0111 0100 = Mifare Desfire EV3 - 32k
         * 
         * 0b0111 1xxx = SmartMX Desfire EV3
         * 0b0111 1000 = SmartMX Desfire EV3 - 1k
         * 0b0111 1001 = SmartMX Desfire EV3 - 2k
         * 0b0111 1010 = SmartMX Desfire EV3 - 4k
         * 0b0111 1011 = SmartMX Desfire EV3 - 16k
         * 0b0111 1100 = SmartMX Desfire EV3 - 32k
         * 
        */

        DESFire = 0x4000,
        DESFireEV0 = 0x4000,
        DESFireEV0_256 = 0x4100,
        DESFireEV0_1K = 0x4200,
        DESFireEV0_2K = 0x4300,
        DESFireEV0_4K = 0x4400,
        // 0x44 - 0x47 = RFU

        SmartMX_DESFire = 0x4800,
        SmartMX_DESFire_Generic = 0x4800,
        SmartMX_DESFireEV0_256 = 0x4900,
        SmartMX_DESFireEV0_1K = 0x4A00,
        SmartMX_DESFireEV0_2K = 0x4B00,
        SmartMX_DESFireEV0_4K = 0x4C00,
        // 0x4C - 0x4F = RFU

        DESFireEV1 = 0x5000,
        DESFireEV1_256 = 0x5100,
        DESFireEV1_2K = 0x5200,
        DESFireEV1_4K = 0x5300,
        DESFireEV1_8K = 0x5400,
        // 0x55 - 0x57 = RFU

        SmartMX_DESFireEV1_256 = 0x5900,
        SmartMX_DESFireEV1_2K = 0x5A00,
        SmartMX_DESFireEV1_4K = 0x5B00,
        SmartMX_DESFireEV1_8K = 0x5C00,
        // 0x5C - 0x5F = RFU

        DESFireEV2 = 0x6000,
        DESFireEV2_2K = 0x6100,
        DESFireEV2_4K = 0x6200,
        DESFireEV2_8K = 0x6300,
        DESFireEV2_16K = 0x6400,
        DESFireEV2_32K = 0x6500,
        // 0x5C - 0x5F = RFU

        SmartMX_DESFireEV2_2K = 0x6900,
        SmartMX_DESFireEV2_4K = 0x6A00,
        SmartMX_DESFireEV2_8K = 0x6B00,
        SmartMX_DESFireEV2_16K = 0x6C00,
        SmartMX_DESFireEV2_32K = 0x6D00,
        // 0x5C - 0x5F = RFU

        DESFireEV3 = 0x7000,
        DESFireEV3_2K = 0x7100,
        DESFireEV3_4K = 0x7200,
        DESFireEV3_8K = 0x7300,
        DESFireEV3_16K = 0x7400,
        DESFireEV3_32K = 0x7500,
        // 0x5C - 0x5F = RFU

        SmartMX_DESFireEV3_2K = 0x7900,
        SmartMX_DESFireEV3_4K = 0x7A00,
        SmartMX_DESFireEV3_8K = 0x7B00,
        SmartMX_DESFireEV3_16K = 0x7C00,
        SmartMX_DESFireEV3_32K = 0x7D00,
        // 0x5C - 0x5F = RFU

        DESFireLight = 0x8000,

        MifareUltralight = 0x9000,
        MifareUltralightC = 0x9100,
        MifareUltralightC_EV1 = 0x9200,

        NTAG_210 = 0xA000,
        NTAG_211 = 0xA100,
        NTAG_212 = 0xA200,
        NTAG_213 = 0xA300,
        NTAG_214 = 0xA400,
        NTAG_215 = 0xA500,
        NTAG_216 = 0xA600,
        // 0xA7 - 0xA9 = RFU
        NTAG_424 = 0xAA00,
        NTAG_426 = 0xAB00,

        MifareMini = 0xB000
    };