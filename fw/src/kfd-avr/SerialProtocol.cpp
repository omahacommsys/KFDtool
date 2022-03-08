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
#include "SerialProtocol.h"
#include "hal.h"
#include <Arduino.h>

// ---------------------------------------------------------------------------
//  Constants
// ---------------------------------------------------------------------------

#define SOM             0x61
#define SOM_PLACEHOLDER 0x62
#define EOM             0x63
#define EOM_PLACEHOLDER 0x64
#define ESC             0x70
#define ESC_PLACEHOLDER 0x71

// ---------------------------------------------------------------------------
//  Globals
// ---------------------------------------------------------------------------

uint16_t inDataCount = 0;
uint8_t inData[128];

// ---------------------------------------------------------------------------
//  Global Functions
// ---------------------------------------------------------------------------

/// <summary>
///
/// </summary>
void spConnect(void)
{
    Serial.begin(115200);
}

/// <summary>
///
/// </summary>
void spDisconnect(void)
{
    //__disable_interrupt();
    Serial.end();
}

/// <summary>
///
/// </summary>
/// <param name="outData"></param>
/// <returns></returns>
uint16_t spRxData(uint8_t* outData)
{
    while (Serial.available() > 0) {
        uint8_t inByte = Serial.read();

        // reset the buffer if we have a start of message flag coming in
        if (inByte == SOM) {
            inDataCount = 0;
        }

        inData[inDataCount] = inByte;
        inDataCount++;
    }

    // don't process until we receive EOM
    if (inData[inDataCount - 1] != EOM) {
        return 0U;
    }

    uint16_t outIndex = 0U;
    for (uint16_t inIndex = 1; inIndex < inDataCount - 1; inIndex++) {
        // skip SOM and EOM
        if (inData[inIndex] == ESC) {
            inIndex++;

            if (inData[inIndex] == SOM_PLACEHOLDER) {
                outData[outIndex] = SOM;
            }   
            else if (inData[inIndex] == EOM_PLACEHOLDER) {
                outData[outIndex] = EOM;
            }
            else if (inData[inIndex] == ESC_PLACEHOLDER) {
                outData[outIndex] = ESC;
            }
        }
        else {
            outData[outIndex] = inData[inIndex];
        }

        outIndex++;
    }

    // we've already processed the message and set the pointer
    // reset the count (mark the buffer as free)
    inDataCount = 0;   
    return outIndex;
}

/// <summary>
///
/// </summary>
/// <param name="inData"></param>
/// <param name="inLength"></param>
/// <param name="outData"></param>
/// <returns></returns>
uint16_t spFrameData(const uint8_t* inData, uint16_t inLength, uint8_t* outData)
{
    uint16_t escCharsNeeded = 0;
    for (uint16_t i = 0; i < inLength; i++) {
        if ((inData[i] == SOM) || (inData[i] == EOM) || (inData[i] == ESC)) {
            escCharsNeeded++;
        }
    }

    uint16_t totalCharsNeeded = 1 + inLength + escCharsNeeded + 1;
    *(outData + 0) = SOM;

    uint16_t k = 1U;
    for (uint16_t j = 0; j < inLength; j++)
    {
        if (inData[j] == SOM) {
            *(outData + k) = ESC;
            k++;
            *(outData + k) = SOM_PLACEHOLDER;
            k++;
        }
        else if (inData[j] == EOM) {
            *(outData + k) = ESC;
            k++;
            *(outData + k) = EOM_PLACEHOLDER;
            k++;
        }
        else if (inData[j] == ESC) {
            *(outData + k) = ESC;
            k++;
            *(outData + k) = ESC_PLACEHOLDER;
            k++;
        }
        else {
            *(outData + k) = inData[j];
            k++;
        }
    }

    *(outData + (totalCharsNeeded - 1)) = EOM;
    return totalCharsNeeded;
}

/// <summary>
///
/// </summary>
/// <param name="inData"></param>
/// <param name="inLength"></param>
/// <returns></returns>
void spTxDataBack(const uint8_t* inData, uint16_t inLength)
{
    uint8_t outData[128];
    uint16_t outLength = spFrameData(inData, inLength, outData);
    Serial.write(outData, outLength);
}

/// <summary>
///
/// </summary>
/// <param name="inData"></param>
/// <param name="inLength"></param>
/// <returns></returns>
void spTxDataWait(const uint8_t* inData, uint16_t inLength)
{
    uint8_t outData[128];
    uint16_t outLength = spFrameData(inData, inLength, outData);
    Serial.write(outData, outLength);
}
