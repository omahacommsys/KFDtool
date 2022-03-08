/**
* KFDTool - KFD-AVR
* GPLv2 Open Source. Use is subject to license terms.
* DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
*
* @package KFDTool / KFD-AVR Firmware
*
*/
//
// Based on code from the KFDtool project. (https://github.com/KFDtool/KFDtool)
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
//
/*
*   Copyright (C) 2019-2020 Daniel Dugger
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
#include "InfoData.h"

// ---------------------------------------------------------------------------
//  Constants
// ---------------------------------------------------------------------------

#define INFOB_START     0x0
#define INFOB_LENGTH    5
#define INFOB_HEADER    0x10
#define INFOB_FOOTER    0x11

#define INFOC_START     0x128
#define INFOC_LENGTH    8
#define INFOC_HEADER    0x20
#define INFOC_FOOTER    0x22

// ---------------------------------------------------------------------------
//  Global Functions
// ---------------------------------------------------------------------------

/// <summary>
///
/// </summary>
/// <param name="hwId"></param>
/// <param name="hwRevMaj"></param>
/// <param name="hwRevMin"></param>
/// <returns></returns>
uint16_t idWriteModelIdHwRev(uint8_t hwId, uint8_t hwRevMaj, uint8_t hwRevMin)
{
    uint8_t data[5U];
    data[0U] = INFOB_HEADER;
    data[1U] = hwId;
    data[2U] = hwRevMaj;
    data[3U] = hwRevMin;
    data[4U] = INFOB_FOOTER;
    
    for (int i = 0; i < INFOB_LENGTH; i++) {
        // use update instead of write to possibly save eeprom cells from unnecessary writes
        EEPROM.update(INFOB_START+i, data[i]);
    }

    return 1U;
}

/// <summary>
///
/// </summary>
/// <param name="serial"></param>
/// <returns></returns>
uint16_t idWriteSerNum(uint8_t ser0, uint8_t ser1, uint8_t ser2, uint8_t ser3, uint8_t ser4, uint8_t ser5)
{
    // TODO: Fix this mess and accept a array pointer for sanity reasons...
    uint8_t data[8U];
    data[0U] = INFOC_HEADER;
    data[1U] = ser0;
    data[2U] = ser1;
    data[3U] = ser2;
    data[4U] = ser3;
    data[5U] = ser4;
    data[6U] = ser5;
    data[7U] = INFOC_FOOTER;
    
    for (int i = 0; i < INFOC_LENGTH; i++) {
        // use update instead of write to possibly save eeprom cells from unnecessary writes
        EEPROM.update(INFOC_START+i, data[i]);
    }

    return 1U;
}

/// <summary>
///
/// </summary>
/// <param name="hwId">
/// <returns></returns>
uint16_t idReadModelId(uint8_t *hwId)
{
    uint8_t data[INFOB_LENGTH];
    for (int i = 0; i < INFOB_LENGTH; i++) {
        data[i] = EEPROM.read(INFOB_START + i);
    }
    
    uint8_t header = data[0];
    uint8_t footer = data[INFOB_LENGTH - 1];
    *hwId = data[1];
    
    if (header == INFOB_HEADER && footer == INFOB_FOOTER) {
        return 1U;
    }
    else {
        return 0U;
    }
}

/// <summary>
///
/// </summary>
/// <param name="hwRevMaj">
/// <param name="hwRevMin">
/// <returns></returns>
uint16_t idReadHwRev(uint8_t *hwRevMaj, uint8_t *hwRevMin)
{
    uint8_t data[INFOB_LENGTH];
    for (int i = 0; i < INFOB_LENGTH; i++) {
        data[i] = EEPROM.read(INFOB_START + i);
    }
    
    uint8_t header = data[0];
    uint8_t footer = data[INFOB_LENGTH - 1];
    *hwRevMaj = data[2];
    *hwRevMin = data[3];
    
    if (header == INFOB_HEADER && footer == INFOB_FOOTER) {
        return 1U;
    }
    else {
        return 0U;
    }
}

/// <summary>
///
/// </summary>
/// <param name="serial"></param>
/// <returns></returns>
uint16_t idReadSerNum(uint8_t *ser0, uint8_t *ser1, uint8_t *ser2, uint8_t *ser3, uint8_t *ser4, uint8_t *ser5)
{
    // TODO: Fix this mess and return a array pointer for sanity reasons...
    uint8_t data[INFOC_LENGTH];
    for (int i = 0; i < INFOC_LENGTH; i++) {
        data[i] = EEPROM.read(INFOC_START + i);
    }
    
    uint8_t header = data[0];
    uint8_t footer = data[INFOC_LENGTH - 1];
    *ser0 = data[1];
    *ser1 = data[2];
    *ser2 = data[3];
    *ser3 = data[4];
    *ser4 = data[5];
    *ser5 = data[6];
    
    if (header == INFOC_HEADER && footer == INFOC_FOOTER) {
        return 1U;
    }
    else {
        return 0U;
    }
}
