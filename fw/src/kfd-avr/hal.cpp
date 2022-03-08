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
#include "hal.h"

// ---------------------------------------------------------------------------
//  Global Functions
// ---------------------------------------------------------------------------
void(*resetFunc)(void) = 0;

/// <summary>
///
/// </summary>
void halInit(void)
{
    // ACT_LED
    pinMode(ACTIVITY_LED_PIN, OUTPUT);
    digitalWrite(ACTIVITY_LED_PIN, LOW);

    // SNS_LED
    pinMode(SENSE_LED_PIN, OUTPUT);
    digitalWrite(SENSE_LED_PIN, LOW);

    // MCU_DATA_OUT_3V3
    pinMode(DATA_TX, OUTPUT);
    digitalWrite(DATA_TX, LOW);

    // MCU_DATA_IN_3V3
    pinMode(DATA_RX, INPUT);
    // make it an interrupt later

    // MCU_SENSE_OUT_3V3
    pinMode(SNS_TX, OUTPUT);
    digitalWrite(SNS_TX, LOW);

    // MCU_SENSE_IN_3V3
    pinMode(SNS_RX, INPUT);

    // GPIO1
    pinMode(GPIO1, OUTPUT);
    digitalWrite(GPIO1, LOW);

    // GPIO2
    pinMode(GPIO2, OUTPUT);
    digitalWrite(GPIO2, LOW);

    // UP_BUTTON
    pinMode(UP_BUTTON_PIN, INPUT);

    // DOWN_BUTTON
    pinMode(DOWN_BUTTON_PIN, INPUT);

    // ENTER_BUTTON
    pinMode(ENTER_BUTTON_PIN, INPUT);   

    // blink the LEDs
    for (int i = 0; i < 3; i++) {
        halActLedOn();
        halSnsLedOn();
        halDelayMs(250);
        halActLedOff();
        halSnsLedOff();
        halDelayMs(250);
    }

}

/// <summary>
///
/// </summary>
void halDelayUs(uint16_t us)
{
    delayMicroseconds(us);
}

/// <summary>
///
/// </summary>
void halDelayMs(uint16_t ms)
{
    delay(ms);
}

/// <summary>
///
/// </summary>
void halEnterBsl(void)
{
    resetFunc();
}

/// <summary>
///
/// </summary>
void halReset(void)
{
    resetFunc();
}

/*
** BEGIN LED macros
*/
/// <summary>
///
/// </summary>
void halActLedOn(void)
{
    digitalWrite(ACTIVITY_LED_PIN, HIGH);
}

/// <summary>
///
/// </summary>
void halActLedOff(void)
{
    digitalWrite(ACTIVITY_LED_PIN, LOW);
}

/// <summary>
///
/// </summary>
void halActLedToggle(void)
{
    digitalWrite(ACTIVITY_LED_PIN, !digitalRead(ACTIVITY_LED_PIN));
}

/// <summary>
///
/// </summary>
void halSnsLedOn(void)
{
    digitalWrite(SENSE_LED_PIN, HIGH);
}

/// <summary>
///
/// </summary>
void halSnsLedOff(void)
{
    digitalWrite(SENSE_LED_PIN, LOW);
}

/// <summary>
///
/// </summary>
void halSnsLedToggle(void)
{
    digitalWrite(SENSE_LED_PIN, !digitalRead(SENSE_LED_PIN));
}

/*
** BEGIN GPIO macros
*/
/// <summary>
///
/// </summary>
void halGpio1High(void)
{
    digitalWrite(GPIO1, HIGH);
}

/// <summary>
///
/// </summary>
void halGpio1Low(void)
{
    digitalWrite(GPIO1, LOW);
}

/// <summary>
///
/// </summary>
void halGpio1Toggle(void)
{
    digitalWrite(GPIO1, !digitalRead(GPIO1));
}

/// <summary>
///
/// </summary>
void halGpio2High(void)
{
    digitalWrite(GPIO2, HIGH);
}

/// <summary>
///
/// </summary>
void halGpio2Low(void)
{
    digitalWrite(GPIO2, LOW);
}

/// <summary>
///
/// </summary>
void halGpio2Toggle(void)
{
    digitalWrite(GPIO2, !digitalRead(GPIO2));
}

/*
** BEGIN KFD macros
*/

/// <summary>
///
/// </summary>
void halKfdTxBusy(void)
{
    digitalWrite(DATA_TX, HIGH);
}

/// <summary>
///
/// </summary>
void halKfdTxIdle(void)
{
    digitalWrite(DATA_TX, LOW);
}

/// <summary>
///
/// </summary>
void halSenTxConn(void)
{
    digitalWrite(SNS_TX, HIGH);
}

/// <summary>
///
/// </summary>
void halSenTxDisc(void)
{
    digitalWrite(SNS_TX, LOW);
}

/*
** BEGIN BUTTON macros
*/

#define BUTTON_UP_PRESSED digitalRead(UP_BUTTON_PIN) == 0
#define BUTTON_UP_RELEASED digitalRead(UP_BUTTON_PIN) == 1

#define BUTTON_DOWN_PRESSED digitalRead(DOWN_BUTTON_PIN) == 0
#define BUTTON_DOWN_RELEASED digitalRead(DOWN_BUTTON_PIN) == 1

#define BUTTON_ENTER_PRESSED digitalRead(ENTER_BUTTON_PIN) == 0
#define BUTTON_ENTER_RELEASED digitalRead(ENTER_BUTTON_PIN) == 1
