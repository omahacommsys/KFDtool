#ifndef HAL_H_
#define HAL_H_

#include <Arduino.h>

// 16MHz, as most avr arduinos are
#define FCPU 16000000

#define ACTIVITY_LED_PIN 7  // if building without the shield, change this to 13 for an activity indicator on the built-in LED
#define SENSE_LED_PIN 6
#define DATA_TX 2   // TWI Data TX
#define DATA_RX 3   // TWI Data RX (INT0 Uno, INT5 Mega)
#define SNS_TX 4    // TWI Sense TX
#define SNS_RX 2    // TWI Sense RX (INT1 Uno, INT4 Mega)
#define GPIO1 8
#define GPIO2 9
#define UP_BUTTON_PIN 14
#define DOWN_BUTTON_PIN 15
#define ENTER_BUTTON_PIN 16

void halInit(void);

void halDelayUs(uint16_t us);

void halDelayMs(uint16_t ms);

void halEnterBsl(void);

void halReset(void);

void halActLedOn(void);

void halActLedOff(void);

void halActLedToggle(void);

void halSnsLedOn(void);

void halSnsLedOff(void);

void halSnsLedToggle(void);

void halGpio1High(void);

void halGpio1Low(void);

void halGpio1Toggle(void);

void halGpio2High(void);

void halGpio2Low(void);

void halGpio2Toggle(void);

void halKfdTxBusy(void);

void halKfdTxIdle(void);

void halSenTxConn(void);

void halSenTxDisc(void);

#endif /* HAL_H_ */