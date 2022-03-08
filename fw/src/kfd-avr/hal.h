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
#if !defined(__HAL_H__)
#define __HAL_H__

#include <Arduino.h>

// ---------------------------------------------------------------------------
//  Constants
// ---------------------------------------------------------------------------

// 16MHz, as most avr arduinos are
#define FCPU                16000000

#define ACTIVITY_LED_PIN    7  // if building without the shield, change this to 13 for an activity indicator on the built-in LED
#define SENSE_LED_PIN       6
#define DATA_TX             5   // TWI Data TX
#define DATA_RX             3   // TWI Data RX (INT0)
#define SNS_TX              4    // TWI Sense TX
#define SNS_RX              2    // TWI Sense RX (INT1)
#define GPIO1               8
#define GPIO2               9
#define UP_BUTTON_PIN       14
#define DOWN_BUTTON_PIN     15
#define ENTER_BUTTON_PIN    16

// ---------------------------------------------------------------------------
//  Global Functions
// ---------------------------------------------------------------------------
/// <summary></summary>
void halInit(void);

/// <summary></summary>
void halDelayUs(uint16_t us);

/// <summary></summary>
void halDelayMs(uint16_t ms);

/// <summary></summary>
void halEnterBsl(void);

/// <summary></summary>
void halReset(void);

/// <summary></summary>
void halActLedOn(void);

/// <summary></summary>
void halActLedOff(void);

/// <summary></summary>
void halActLedToggle(void);

/// <summary></summary>
void halSnsLedOn(void);

/// <summary></summary>
void halSnsLedOff(void);

/// <summary></summary>
void halSnsLedToggle(void);

/// <summary></summary>
void halGpio1High(void);

/// <summary></summary>
void halGpio1Low(void);

/// <summary></summary>
void halGpio1Toggle(void);

/// <summary></summary>
void halGpio2High(void);

/// <summary></summary>
void halGpio2Low(void);

/// <summary></summary>
void halGpio2Toggle(void);

/// <summary></summary>
void halKfdTxBusy(void);

/// <summary></summary>
void halKfdTxIdle(void);

/// <summary></summary>
void halSenTxConn(void);

/// <summary></summary>
void halSenTxDisc(void);

#endif // __HAL_H__
