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
#include "hal.h"
#include "TwiProtocol.h"
#include "SerialProtocol.h"
#include "InfoData.h"
#include "ControlOpCodes.h"
#include "Versions.h"
#include "UID.h"

// ---------------------------------------------------------------------------
//  Globals
// ---------------------------------------------------------------------------

uint16_t cmdCount;
uint8_t cmdData[128];
uint16_t rxReady;
uint8_t rxTemp;

// ---------------------------------------------------------------------------
//  Global Functions
// ---------------------------------------------------------------------------

/// <summary>
///
/// </summary>
/// <param name="error"></param>
void writeSpTxError(uint8_t error)
{
    uint8_t rspData[2U];
    rspData[0U] = RSP_ERROR;
    rspData[1U] = error;

    spTxDataWait(rspData, sizeof(rspData));
}

void setup()
{
    halInit();
    spConnect();

    twiInit();

    halActLedOn();
}

void loop()
{
    cmdCount = spRxData(cmdData);

    // do we have a command?
    if (cmdCount > 0U) {
        // which command do we have?
        switch (cmdData[0U]) {
        case CMD_READ:                                                          // Read Command
            {
                if (cmdCount == 2U) {
                    switch (cmdData[1U]) {
                    case READ_AP_VER:                                           // Read Adapter Protocol Version
                        {
                            uint8_t rspData[5U];
                            rspData[0U] = RSP_READ;
                            rspData[1U] = READ_AP_VER;
                            rspData[2U] = VER_AP_MAJOR;
                            rspData[3U] = VER_AP_MINOR;
                            rspData[4U] = VER_AP_PATCH;
                            spTxDataWait(rspData, sizeof(rspData));
                            break;
                        }
                    case READ_FW_VER:                                           // Read Firmware Version
                        {
                            uint8_t rspData[5U];
                            rspData[0U] = RSP_READ;
                            rspData[1U] = READ_FW_VER;
                            rspData[2U] = VER_FW_MAJOR;
                            rspData[3U] = VER_FW_MINOR;
                            rspData[4U] = VER_FW_PATCH;
                            spTxDataWait(rspData, sizeof(rspData));
                            break;
                        }
                    case READ_UNIQUE_ID:                                        // Read Unique ID
                        {
                            uint8_t serial[8U];
                            ::memset(serial, 0x00U, 8U);

                            getUID8(&serial[0], &serial[1], &serial[2], &serial[3],
                                &serial[4], &serial[5], &serial[6], &serial[7]);

                            uint8_t rspData[12U];
                            rspData[0U] = RSP_READ;
                            rspData[1U] = READ_UNIQUE_ID;
                            rspData[2U] = 0x09U;    // id length
                            rspData[3U] = 0x10U;    // id source
                            rspData[4U] = serial[0U];
                            rspData[5U] = serial[1U];
                            rspData[6U] = serial[2U];
                            rspData[7U] = serial[3U];
                            rspData[8U] = serial[4U];
                            rspData[9U] = serial[5U];
                            rspData[10U] = serial[6U];
                            rspData[11U] = serial[7U];
                            spTxDataWait(rspData, sizeof(rspData));
                            break;
                        }
                    case READ_MODEL_ID:                                         // Read Model ID
                        {
                            uint8_t hwId;
                            uint16_t status = idReadModelId(&hwId);

                            // check if available
                            if (status)  {
                                uint8_t rspData[3U];
                                rspData[0U] = RSP_READ;
                                rspData[1U] = READ_MODEL_ID;
                                rspData[2U] = hwId;
                                spTxDataWait(rspData, sizeof(rspData));
                            }
                            else {
                                // no model id available
                                uint8_t rspData[3U];
                                rspData[0U] = RSP_READ;
                                rspData[1U] = READ_MODEL_ID;
                                rspData[2U] = 0x00U;
                                spTxDataWait(rspData, sizeof(rspData));
                            }
                            break;
                        }
                    case READ_HW_REV:                                           // Read Hardware Revision
                        {
                            uint8_t hwRevMaj;
                            uint8_t hwRevMin;
                            uint16_t status = idReadHwRev(&hwRevMaj, &hwRevMin);

                            // check if available
                            if (status == 1U) {
                                uint8_t rspData[4U];
                                rspData[0U] = RSP_READ;
                                rspData[1U] = READ_HW_REV;
                                rspData[2U] = hwRevMaj;
                                rspData[3U] = hwRevMin;
                                spTxDataWait(rspData, sizeof(rspData));
                            }
                            else {
                                // no hardware revision available
                                uint8_t rspData[4];
                                rspData[0U] = RSP_READ;
                                rspData[1U] = READ_HW_REV;
                                rspData[2U] = 0x00;
                                rspData[3U] = 0x00;
                                spTxDataWait(rspData, sizeof(rspData));
                            }
                            break;
                        }
                    case READ_SER_NUM:
                        {
                            uint8_t serial[6U];
                            ::memset(serial, 0x00U, 6U);

                            uint16_t status = idReadSerNum(&serial[0], &serial[1], &serial[2], 
                                &serial[3], &serial[4], &serial[5]);

                            // check if available
                            if (status == 1U) {
                                uint8_t rspData[9U];
                                rspData[0U] = RSP_READ;
                                rspData[1U] = READ_SER_NUM;
                                rspData[2U] = 0x06U; // serial length
                                rspData[3U] = serial[0U];
                                rspData[4U] = serial[1U];
                                rspData[5U] = serial[2U];
                                rspData[6U] = serial[3U];
                                rspData[7U] = serial[4U];
                                rspData[8U] = serial[5U];
                                spTxDataWait(rspData, sizeof(rspData));
                            }
                            else {
                                // no serial number available
                                uint8_t rspData[3U];
                                rspData[0U] = RSP_READ;
                                rspData[1U] = READ_SER_NUM;
                                rspData[2U] = 0x00U; // serial length
                                spTxDataWait(rspData, sizeof(rspData));
                            }
                            break;
                        }
                    default:
                        writeSpTxError(ERR_INVALID_READ_OPCODE);
                        break;
                    }
                }
                else {
                    // invalid command length
                    writeSpTxError(ERR_INVALID_CMD_LENGTH);
                }
                break;
            }
        case CMD_WRITE_INFO:                                                    // Write Info
            {
                if (cmdCount > 1U) {
                    switch (cmdData[1U]) {
                    case WRITE_MDL_REV:                                         // Write Model ID and Hardware Rev
                        {
                            if (cmdCount == 5U) {
                                uint16_t result = idWriteModelIdHwRev(cmdData[2], cmdData[3], cmdData[4]);
                                if (result == 1U) {
                                    uint8_t rspData[1U];
                                    rspData[0U] = RSP_WRITE_INFO;
                                    spTxDataWait(rspData, sizeof(rspData));
                                }
                                else  {
                                    // write failed
                                    writeSpTxError(ERR_WRITE_FAILED);
                                }
                            }
                            else {
                                // invalid command length
                                writeSpTxError(ERR_INVALID_CMD_LENGTH);
                            }
                            break;
                        }
                    case WRITE_SER:                                             // Write Serial Number
                        {
                            if (cmdCount == 8U) {
                                uint16_t result = idWriteSerNum(cmdData[2], cmdData[3], cmdData[4], 
                                    cmdData[5], cmdData[6], cmdData[7]);
                                if (result == 1U) {
                                    uint8_t rspData[1U];
                                    rspData[0U] = RSP_WRITE_INFO;
                                    spTxDataWait(rspData, sizeof(rspData));
                                }
                                else {
                                    // write failed
                                    writeSpTxError(ERR_WRITE_FAILED);
                                }
                            }
                            else {
                                // invalid command length
                                writeSpTxError(ERR_INVALID_CMD_LENGTH);
                            }
                            break;
                        }
                    default:
                        writeSpTxError(ERR_INVALID_WRITE_OPCODE);
                        break;
                    }
                }
                else {
                    // invalid command length
                    writeSpTxError(ERR_INVALID_CMD_LENGTH);
                }
                break;
            }
        case CMD_ENTER_BSL_MODE:                                                // Enter BSL Mode
            {
                if (cmdCount == 1U) {
                    uint8_t rspData[1U];
                    rspData[0U] = RSP_ENTER_BSL_MODE;
                    spTxDataWait(rspData, sizeof(rspData));

                    halDelayMs(1000); // wait 1 second
                    spDisconnect(); // disconnect usb
                    halDelayMs(3000); // wait 3 seconds
                    halActLedOff();
                    halEnterBsl();
                }
                else  {
                    // invalid command length
                    writeSpTxError(ERR_INVALID_CMD_LENGTH);
                }
                break;
            }
        case CMD_RESET:                                                         // Reset
            {
                if (cmdCount == 1U) {
                    uint8_t rspData[1U];
                    rspData[0U] = RSP_RESET;
                    spTxDataWait(rspData, sizeof(rspData));

                    halDelayMs(1000); // wait 1 second
                    spDisconnect(); // disconnect usb
                    halDelayMs(3000); // wait 3 seconds
                    halActLedOff();
                    halReset();
                }
                else {
                    // invalid command length
                    writeSpTxError(ERR_INVALID_CMD_LENGTH);
                }
                break;
            }
        case CMD_SELF_TEST:                                                     // Self Test
            {
                if (cmdCount == 1U) {
                    uint8_t result = twiSelfTest();

                    uint8_t rspData[2U];
                    rspData[0U] = RSP_SELF_TEST;
                    rspData[1U] = result;
                    spTxDataWait(rspData, sizeof(rspData));
                }
                else {
                    // invalid command length
                    writeSpTxError(ERR_INVALID_CMD_LENGTH);
                }
                break;
            }
        case CMD_SEND_KEY_SIG:                                                  // Send Key Signature
            {
                if (cmdCount == 2U) {
                    twiSendKeySig();

                    uint8_t rspData[1U];
                    rspData[0U] = RSP_SEND_KEY_SIG;
                    spTxDataWait(rspData, sizeof(rspData));
                }
                else {
                    // invalid command length
                    writeSpTxError(ERR_INVALID_CMD_LENGTH);
                }
                break;
            }
        case CMD_SEND_BYTE:
            {
                if (cmdCount == 3U) {
                    twiSendPhyByte(cmdData[2U]);

                    uint8_t rspData[1U];
                    rspData[0U] = RSP_SEND_BYTE;
                    spTxDataWait(rspData, sizeof(rspData));
                }
                else {
                    // invalid command length
                    writeSpTxError(ERR_INVALID_CMD_LENGTH);
                }
                break;
            }
        default:
            writeSpTxError(ERR_INVALID_CMD_OPCODE);
            break;
        }
    }

    rxReady = twiReceiveByte(&rxTemp);
    if (rxReady == 1)
    {
        uint8_t bcstData[3];

        bcstData[0] = BCST_RECEIVE_BYTE;
        bcstData[1] = 0x00; // reserved (set to 0x00)
        bcstData[2] = rxTemp;

        spTxDataBack(bcstData, sizeof(bcstData));
    }
}

// ---------------------------------------------------------------------------
//  Firmware Entry Point
// ---------------------------------------------------------------------------

int main(void)
{
    init();

#if defined(USBCON)
    USBDevice.attach();
#endif
    
    setup();
    
    for (;;) {
        loop();
        if (serialEventRun) serialEventRun();
    }
        
    return 0;
}
