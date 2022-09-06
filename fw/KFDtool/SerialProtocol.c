// KFDtool
// Copyright 2019-2022 KFDtool, LLC

#include "USB_config/descriptors.h"
#include "USB_API/USB_Common/usb.h"
#include "USB_app/usbConstructs.h"

#include "SerialProtocol.h"

#define SOM_EOM 0x61
#define SOM_EOM_PLACEHOLDER 0x62
#define ESC 0x63
#define ESC_PLACEHOLDER 0x64

uint8_t rxIndex;
uint8_t rxStartFlag;
uint8_t rxEscapeFlag;

void spConnect(void)
{
    rxIndex = 0;
    rxStartFlag = 0;
    rxEscapeFlag = 0;

    USB_setup(TRUE, TRUE);
}

void spDisconnect(void)
{
    __disable_interrupt();

    /*
    USBKEYPID = 0x9628; // Unlock USB configuration registers
    USBCNF &= ~PUR_EN; // Set PUR pin to hi-Z, logically disconnect from host
    USBPWRCTL &= ~VBOFFIE; // Disable VUSBoff interrupt
    USBKEYPID = 0x9600; // Lock USB configuration register
    */

    USB_disconnect(); // PUR high, disable VBUS interrupt
    USB_disable(); // Disable USB module, disable PLL
}

uint16_t spRxData(uint8_t *rxBuffer)
{
    uint8_t inData[1];
    uint16_t inDataCount;

    while (1)
    {
        inDataCount = cdcReceiveDataInBuffer(inData, sizeof(inData), CDC0_INTFNUM);

        // no data to read
        if (inDataCount < 1)
        {
            return 0;
        }

        // reset if buffer overrun
        if (rxIndex == sizeof(rxBuffer))
        {
            rxIndex = 0;
            rxStartFlag = 0;
            rxEscapeFlag = 0;

            return 0;
        }

        // got SOM/EOM
        if (inData[0] == SOM_EOM)
        {
            // not started, set start flag, clear other flags
            if (rxStartFlag == 0)
            {
                rxIndex = 0;
                rxStartFlag = 1;
                rxEscapeFlag = 0;

                return 0;
            }
            // started, clear start flag and return message length
            else
            {
                rxStartFlag = 0;

                return rxIndex;
            }
        }

        // not started, do not continue
        if (rxStartFlag == 0)
        {
            return 0;
        }

        // escape byte
        if (rxEscapeFlag)
        {
            if (inData[0] == SOM_EOM_PLACEHOLDER)
            {
                rxBuffer[rxIndex++] = SOM_EOM;
            }
            else if (inData[0] == ESC_PLACEHOLDER)
            {
                rxBuffer[rxIndex++] = ESC;
            }

            rxEscapeFlag = 0;
        }
        // escape byte, set flag for next byte to be escaped
        else if (inData[0] == ESC)
        {
            rxEscapeFlag = 1;
        }
        // normal byte, save as is
        else
        {
            rxBuffer[rxIndex++] = inData[0];
        }
    }
}

uint16_t spFrameData(const uint8_t *inData, uint16_t inLength, uint8_t *outData)
{
    uint16_t escCharsNeeded = 0;
    uint16_t i;

    for (i = 0; i < inLength; i++)
    {
        if ((inData[i] == SOM_EOM) || (inData[i] == ESC))
        {
            escCharsNeeded++;
        }
    }

    uint16_t totalCharsNeeded = 1 + inLength + escCharsNeeded + 1;

    *(outData + 0) = SOM_EOM;

    uint16_t j;
    uint16_t k = 1;

    for (j = 0; j < inLength; j++)
    {
        if (inData[j] == SOM_EOM)
        {
            *(outData + k) = ESC;
            k++;
            *(outData + k) = SOM_EOM_PLACEHOLDER;
            k++;
        }
        else if (inData[j] == ESC)
        {
            *(outData + k) = ESC;
            k++;
            *(outData + k) = ESC_PLACEHOLDER;
            k++;
        }
        else
        {
            *(outData + k) = inData[j];
            k++;
        }
    }

    *(outData + (totalCharsNeeded - 1)) = SOM_EOM;

    return totalCharsNeeded;
}

void spTxDataBack(const uint8_t *inData, uint16_t inLength)
{
    uint16_t outLength;
    uint8_t outData[128];

    outLength = spFrameData(inData, inLength, outData);

    cdcSendDataInBackground(outData, outLength, CDC0_INTFNUM, 1000);
}

void spTxDataWait(const uint8_t *inData, uint16_t inLength)
{
    uint16_t outLength;
    uint8_t outData[128];

    outLength = spFrameData(inData, inLength, outData);

    cdcSendDataWaitTilDone(outData, outLength, CDC0_INTFNUM, 1000);
}
