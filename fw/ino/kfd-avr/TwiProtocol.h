#ifndef TWIPROTOCOL_H_
#define TWIPROTOCOL_H_

#include <Arduino.h>

void twiInit(void);

uint8_t twiSelfTest(void);

uint16_t twiReceiveByte(uint8_t *c);

void twiSendKeySig(void);

void twiSendPhyBytes(uint8_t* byteToSend, uint16_t count);
void twiSendPhyByte(uint8_t byteToSend);

void Port_1(void);

void TimerCallback(void);
#endif /* TWIPROTOCOL_H_ */
