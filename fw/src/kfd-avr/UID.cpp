/**
* KFDTool - KFD-AVR
* GPLv2 Open Source. Use is subject to license terms.
* DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
*
* @package KFDTool / KFD-AVR Firmware
*
*/
/*
*   Copyright (C) Luiz Henrique Cassettari. All rights reserved.
*   Copyright (C) 2021-2022 Nat Moore
*   Copyright (C) 2022 Bryan Biedenkapp N2PLL
*
*   Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
*   and associated documentation files (the "Software"), to deal in the Software without restriction, 
*   including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
*   and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
*   subject to the following conditions:
*
*   The above copyright notice and this permission notice shall be included in all copies or substantial 
*   portions of the Software.
*
*   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
*   LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN 
*   NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER 
*   IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
*   OR OTHER DEALINGS IN THE SOFTWARE.
*/
#include "UID.h"

// ---------------------------------------------------------------------------
//  Global Functions
// ---------------------------------------------------------------------------

/// <summary>
///
/// </summary>
/// <param name="id"></param>
/// <returns></returns>
void getUID8(uint8_t *id0, uint8_t *id1, uint8_t *id2, uint8_t *id3, uint8_t *id4, uint8_t *id5, uint8_t *id6, uint8_t *id7)
{
    // TODO: Fix this mess and return a array pointer for sanity reasons...
    uint8_t id[8];

#if defined(ARDUINO_ARCH_AVR)
    for (size_t i = 0; i < 8; i++) {
        id[i] = boot_signature_byte_get(0x0E + i + (UniqueIDsize == 9 && i > 5 ? 1 : 0));
    }
#elif defined(ARDUINO_ARCH_ESP8266)
    uint32_t chipid = ESP.getChipId();
    id[0] = 0;
    id[1] = 0;
    id[2] = 0;
    id[3] = 0;
    id[4] = chipid >> 24;
    id[5] = chipid >> 16;
    id[6] = chipid >> 8;
    id[7] = chipid;
#elif defined(ARDUINO_ARCH_ESP32)
    uint64_t chipid = ESP.getEfuseMac();
    id[0] = 0;
    id[1] = 0;
    id[2] = chipid;
    id[3] = chipid >> 8;
    id[4] = chipid >> 16;
    id[5] = chipid >> 24;
    id[6] = chipid >> 32;
    id[7] = chipid >> 40;
#endif

    *id0 = id[0];
    *id1 = id[1];
    *id2 = id[2];
    *id3 = id[3];
    *id4 = id[4];
    *id5 = id[5];
    *id6 = id[6];
    *id7 = id[7];
}
