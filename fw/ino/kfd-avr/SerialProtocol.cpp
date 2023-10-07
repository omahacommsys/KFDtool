#include "SerialProtocol.h"
#include "hal.h"
#include <Arduino.h>


#define SOM 0x61
#define SOM_PLACEHOLDER 0x62
#define EOM 0x63
#define EOM_PLACEHOLDER 0x64
#define ESC 0x70
#define ESC_PLACEHOLDER 0x71

uint16_t inDataCount = 0;
uint8_t inData[512];

void spConnect(void)
{
    Serial.begin(115200);
}

void spDisconnect(void)
{
    //__disable_interrupt();

    Serial.end();
}

uint16_t spRxData(uint8_t* outData)
{
  uint8_t inByte;
    while (Serial.available() > 0) {
        inByte = Serial.read();

        // reset the buffer if we have a start of message flag coming in
        if (inByte == SOM)
        {
          inDataCount = 0;
        }
        
        inData[inDataCount] = inByte;
        inDataCount++;
    }

    // don't process until we receive EOM
    if (inData[inDataCount - 1] != EOM)
    {
        return 0;
    }

    uint16_t inIndex;
    uint16_t outIndex;
    outIndex = 0;

    for (inIndex = 1; inIndex < inDataCount - 1; inIndex++) // skip SOM and EOM
    {
        if (inData[inIndex] == ESC)
        {
            inIndex++;

            if (inData[inIndex] == SOM_PLACEHOLDER)
            {
                outData[outIndex] = SOM;
            }
            else if (inData[inIndex] == EOM_PLACEHOLDER)
            {
                outData[outIndex] = EOM;
            }
            else if (inData[inIndex] == ESC_PLACEHOLDER)
            {
                outData[outIndex] = ESC;
            }
            
        }
        else
        {
            outData[outIndex] = inData[inIndex];
        }

        outIndex++;
    }

    // we've already processed the message and set the pointer
    // reset the count (mark the buffer as free)
    inDataCount = 0;   

    return outIndex;
}

uint16_t spFrameData(const uint8_t* inData,
                     uint16_t inLength,
                     uint8_t* outData)
{
    uint16_t escCharsNeeded = 0;
    uint16_t i;

    for (i = 0; i < inLength; i++)
    {
        if ((inData[i] == SOM) || (inData[i] == EOM) || (inData[i] == ESC))
        {
            escCharsNeeded++;
        }
    }

    uint16_t totalCharsNeeded = 1 + inLength + escCharsNeeded + 1;

    *(outData + 0) = SOM;

    uint16_t j;
    uint16_t k = 1;

    for (j = 0; j < inLength; j++)
    {
        if (inData[j] == SOM)
        {
            *(outData + k) = ESC;
            k++;
            *(outData + k) = SOM_PLACEHOLDER;
            k++;
        }
        else if (inData[j] == EOM)
        {
            *(outData + k) = ESC;
            k++;
            *(outData + k) = EOM_PLACEHOLDER;
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

    *(outData + (totalCharsNeeded - 1)) = EOM;

    return totalCharsNeeded;
}

void spTxDataBack(const uint8_t* inData,
                            uint16_t inLength)
{
    uint16_t outLength;
    uint8_t outData[512];

    outLength = spFrameData(inData, inLength, outData);

    Serial.write(outData, outLength);
}

void spTxDataWait(const uint8_t* inData,
                            uint16_t inLength)
{
    uint16_t outLength;
    uint8_t outData[512];

    outLength = spFrameData(inData, inLength, outData);

    Serial.write(outData, outLength);
}