# KFDTool-AVR

<img src="https://github.com/omahacommsys/KFDtool/blob/master/doc/pic/darkmode.png" width="75%">

A fork of the amazing [KFDTool](https://github.com/KFDTool/KFDTool) software for ATmega devices, plus schematics and board files for an Arduino-compatible shield.

⚠️ **You must use the build of the KFDTool software attached to this GitHub repo in order for your homebrew adapter to work.** This build also supports the original [KFDTool](https://github.com/KFDTool/KFDTool). The original [KFDTool](https://github.com/KFDTool/KFDTool) software will not find your Arduino-based KFD.

The software officially supports the following devices:
* Original [duggerd](https://github.com/duggerd) [KFDTool](https://store.kfdtool.com/products/kfdtool-kfd100)
* [OmahaCommSys](https://github.com/omahacommsys) [KFDshield](https://store.omahacommunicationsys.com)
* [W3AXL](https://github.com/w3axl) [KFDMicro](https://store.w3axl.com/products/kfdmicro-3d-printed-case-1)
* [rentfrowj](https://github.com/rentfrowj) [KFDnano](https://www.ebay.com/usr/rentfrowj)
* [alexhanyuan](https://github.com/alexhanyuan) KFDnano
* [alexhanyuan](https://github.com/alexhanyuan) [KFDpico](https://www.ebay.com/usr/alexhyuan)

Also supports homebrew keyloaders using the following Arduino boards and their direct clones (ATmega328P, ATmega32u4, and ATmega2560 based boards):
* Arduino Uno Rev3
* Arduino Nano
* Arduino Micro
* Arduino Leonardo
* Arduino Mega 2560

The following boards have been reported to work with minor modifications:

* LGT8F328P - need to adjust frequency and remove all of the EEPROM references

Further testing and problem reporting by the community is encouraged to further improve compatibility with other devices.

## Setup/Installation

Setup consists of three parts:
1. Hardware Setup
2. Firmware Flashing
3. Software Setup

### Hardware Setup

The hardware for the KFD-AVR fork is very simple. You can either purchase a KFDShield from the [online store](https://store.omahacommunicationsys.com), or build your own circuit on a breadboard. The bare minimum you need to get going is this:

![schematic](doc/pic/basic_hw_schematic.png)

Where PIN3 is Arduino pin 3 for receive data, PIN5 is Arduino pin 5 for transmit data, DATA is the radio data line, SNS, is the radio sense line, and GND is the radio ground line. VCC is 5 volts (make sure you are using a 5 volt tolerant board).

On the 3.5mm TRS connector, tip is DATA, ring is SNS, and shield is GND.

⚠️ KFDmini (eBay clones) cables and accessories are NOT compatible with any of the officially supported KFD-AVR devices, and vice-versa.

### Firmware Flashing

Head to the **Releases** page and download the latest Firmware zip. Unzip the Firmware, and open the kfd-avr.ino file in [Arduino IDE](https://www.arduino.cc/en/software/). It should then automatically open the associated files. Select your board type and COM port from the **`Tools`** menu. Some Arduino clones may require you to select **`ATmega328P (Old Bootloader)`** under the **`Processor`** option. Try this if you are having trouble uploading the sketch.


If you are using a board that is not explicitly supported, or DIYing your own rather than using a shield, you should confirm that all options in the [hal.h](fw/ino/kfd-avr/hal.h) file are correct - namely, CPU speed and DATA/LED pins. Once you are done, hit upload.

⚠️ If you have a KFDpico and it is BLUE, please use the KFDpico firmware in the releases folder. The DATA TX pin is Arduino pin 2. There is also a KFDpico specific branch in this repository. If you have a GREEN KFDpico, it likely does not have the USB bootloader to allow updating over USB. Please contact [Alex](https://github.com/alexhanyuan).

For all other versions (KFDShield, KFDMicro, and KFDnano), please use the normal firmware in the releases folder.

### Software Setup

The software distributed with the KFD-AVR release is "flat pack", meaning there is no installer supplied - all you need to do to run the software is unzip the Software zip and run KFDToolGui.exe. You may see an "Error - timeout while waiting for data" when first opening the software. **This is completely normal, especially if you have many COM ports on your system. The warning will be suppressed in a future release.** Select the COM port corresponding to your KFD, and you should see information populate in the bottom bar of the screen indicating that the KFDTool software is connected to your device.

In order to validate that every part of the chain - hardware, firmware, and software - is working, go to the Utility - Adapter Self Test menu, and click "Detect MR". This will send a signal to your radio asking it to reply and confirm it is there. If you get a success message, congrats! You are ready to load keys.

If you do not have the circuit shown in the **Hardware Setup** section for the TWI sense line, you may get an error saying that Sense is shorted to ground. This is expected and does not mean your KFD is not working.

## Radio Compatibility

Radios that have been specifically tested with KFD devices are listed below, along with any notes or firmware restrictions.

If you have used KFDtool with a radio that is not listed, please get in touch so we can update this list!

| Product line                 | Support status   | KFDtool software | KFD firmware | Radio firmware          | Notes |
|------------------------------|------------------|------------------|--------------|-------------------------|-------|
| Bendix King BK               | ❔ Untested      | —                | —            | —                       |
| Bendix King KNG              | ❔ Untested      | —                | —            | —                       |
| EF Johnson VP/VM 8000        | ✅ Tested        | ≥ 1.8.7          | ≥ 1.8.7      | Any                     |
| EF Johnson VP/VM x000        | ❔ Untested      | —                | —            | —                       |
| EF Johnson VP/VM x00         | ✅ Tested        | ≥ 1.8.7          | ≥ 1.8.7      | ≥ 8.28.5                |
| EF Johnson 5100/ES           | ✅ Tested        | ≥ 1.7.3          | ≥ 1.7.3      | ≥ 6.12.4                |
| Harris XL                    | ✅ Tested        | ≥ 1.8.7          | ≥ 1.8.7      | Any                     |
| Harris Unity (XG-100)        | ✅ Tested        | ≥ 1.8.7          | ≥ 1.8.7      | Any                     | [^ch100]
| Harris XG                    | ❔ Untested      | —                | —            | —                       | [^harrisxg]
| Kenwood NX                   | ✅ Tested        | ≥ 1.8.7          | ≥ 1.8.7      | Any                     | [^kenwood]
| Kenwood TK                   | ❔ Untested      | —                | —            | —                       | [^kenwood]
| Motorola APX (TWI)           | ✅ Tested        | ≥ 1.8.7          | ≥ 1.8.7      | Any                     |
| Motorola APX (DLI IP)        | ✅ Tested        | ≥ 1.8.7          | ≥ 1.8.7      | ≥ R27.03                | [^freon]
| Motorola ASTRO 25 (MACE)     | ✅ Tested        | ≥ 1.8.7          | ≥ 1.8.7      | Any                     |
| Motorola ASTRO 25 (UCM)      | ✅ Tested        | ≥ 1.8.7          | ≥ 1.8.7      | Any                     |
| Motorola ASTRO               | ✅ Tested        | ≥ 1.8.7          | ≥ 1.8.7      | ≥ R07.71.06, EMC R03.56 |
| Tait TP/TM 9x00 series       | ✅ Tested        | ≥ 1.8.7          | ≥ 1.8.7      | Any                     | [^kiwikey][^taitbox]
| Thales Liberty               | ✅ Tested        | ≥ 1.8.7          | ≥ 1.8.7      | Any                     |


[^ch100]: XG-100M supports native TWI keyloading[^ch721] only via CH100
[^ch721]: XG-100M with CH721 supports TWI keyloading via Harris adapter cable 14002-0143-10
[^freon]: "Freon" (OMAP L-138) architecture only (APX 900, 6000 BN, 8000, 8500, NEXT, N-series, and BN mobiles)
[^harrisxg]: Requires Harris adapter cable (portable: unknown part number; mobile: 14002-0143-10)
[^kenwood]: Requires SCM hardware module
[^kiwikey]: Requires [kiwi**key**](https://github.com/beepbooplabsltd/kiwikey) keyloading adapter
[^taitbox]: Tait TPA-SV-020 keyloading adapter box not currently supported
