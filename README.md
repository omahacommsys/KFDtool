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
| Thales Liberty               | ✅ Tested        | ≥ 1.8.7          | ≥ 1.8.7      | Any                     |
| Tait TP/TM 9x00 series       | ❌ Not supported | —                | —            | —                       |


# Original Readme:

Open Source P25 Key Fill Device

Compliant with P25 standards (TIA-102.AACD-A)

Purchase Hardware: [online store](https://kfdtool.com/store)

Download Software: [latest release](https://github.com/KFDtool/KFDtool/releases)

Release Notifications: [subscribe](https://kfdtool.com/newsletter)

Demonstration: [video](https://www.youtube.com/watch?v=Oioa3xTQoE0)

Software Manual: [view](doc/KFDtool_Manual.pdf)

Security Considerations: [view](doc/SECURITY_CONSIDERATIONS.md)

Features
--------

**Key Fill Device (KFD)**

The KFDtool software supports KFD features through the KFDtool hardware adapter (TWI/3WI/Three Wire Interface), as well as through a IP (UDP) connection (DLI/Data Link Independent interface).

Keys and groups of keys can be saved to an AES-256 encrypted key container file, which can then be selected and loaded into a target device in one operation.

Supported Manual Rekeying Features (TIA-102.AACD-A)

* 2.3.1 Keyload
* 2.3.2 Key Erase
* 2.3.3 Erase All Keys
* 2.3.4 View Key Info
* 2.3.5 View Individual RSI
* 2.3.6 Load Individual RSI
* 2.3.7 View KMF RSI
* 2.3.8 Load KMF RSI
* 2.3.9 View MNP
* 2.3.10 Load MNP
* 2.3.11 View Keyset Info
* 2.3.12 Activate Keyset

Motorola refers to the P25 standard 3 wire interface (3WI) keyload protocol as ASTRO 25 mode or CKR mode.

The legacy Motorola proprietary keyloading formats SECURENET and ASN (Advanced SECURENET) are **NOT** supported by KFDtool. PID mode is also used to refer to ASN mode.

Key validators/generators are available for the following algorithms:

* AES-256 (Algorithm ID 0x84)
* DES-OFB (Algorithm ID 0x81)
* DES-XL (Algorithm ID 0x9F)
* ADP/RC4 (Algorithm ID 0xAA)

**Mobile Radio (MR) Emulator**

The KFDtool software only supports MR Emulator features through the KFDtool hardware adapter (TWI/3WI/Three Wire Interface) at this time.

This mode allows another keyloader to be connected to the KFDtool, and the keys retrieved.

Supported Manual Rekeying Features (TIA-102.AACD-A)

* 2.3.1 Keyload

Radio Compatibility
-------------------

*Any statements of compatibility do not imply endorsement by the vendor. Testing has not been performed by the vendor themselves.*

**A detailed list of compatible radios and adapters is available [here](doc/RADIO_COMPATIBILITY.md).**

Radios that are compatible with Motorola KVL3000/KVL3000+/KVL4000/KVL5000 keyloaders in ASTRO 25 mode should be compatible with KFDtool.

Keyloading cables made for other radios with MX (Motorola KVL) connectors can be modified by soldering an AC101 or AC102 Hirose pigtail in parallel with the MX connector according to [these](doc/MX_CONN_MOD_NOTES.md) instructions.

Operations encapsulated with encryption (commonly referred to as FIPS mode) are not supported at this time for either the KFD or MR emulation modes.

Hardware
--------

Assembled and tested KFDtool hardware is available from me directly. I can ship internationally. Please visit the [online store](https://kfdtool.com/store) to place an order.

**Proceeds from hardware sales enables me to further develop the software.**

| Part Number | Description |
| :---: | --- |
| KFD100 | Single Hirose port USB key fill device (includes 1 m / 3 ft USB A to USB B cable) |
| AC100 | 6 pin male plug Hirose to 6 pin male plug Hirose cable (0.5 m / 1.5 ft) |
| AC101 | 6 pin male plug Hirose pigtail for custom cables (0.5 m / 1.5 ft) |
| AC102 | 6 pin female jack Hirose pigtail for custom cables (0.5 m / 1.5 ft) |
| AC103 | Motorola R2670 compatible adapter (0.15 m / 6 in) |
| AC104 | Kenwood KPG-115 compatible adapter (0.15 m / 6 in) |
| AC105 | 4 way female jack passive Hirose splitter |
| AC106 | Kenwood KPG-93 compatible adapter (0.15 m / 6 in) |
| AC107 | Motorola XTS4000 compatible adapter (0.15 m / 6 in) |
| AC108 | Aeroflex/IFR 2975 compatible adapter (0.15 m / 6 in) |
| AC109 | Harris XG-100P/XL-150P/XL-185P/XL-200P compatible adapter |

OS Compatibility
----------------

* KFDtool software supports 32-bit and 64-bit Windows 7, Windows 8.1, and Windows 10

* The .NET Framework 4.7.2 or later compatible must be installed

* **The use of a virtual machine with USB passthrough is NOT supported at this time**
    * Changing the USB controller from USB 2.0 mode to USB 3.0 mode has been reported to resolve the issue
    * Do not attempt to update the adapter firmware or initialize an adapter using USB passthrough

Documentation
-------------

* [Software Changelog](doc/SW_CHANGELOG.txt)
* [Firmware Changelog](doc/FW_CHANGELOG.txt)
* [Hardware Changelog](doc/HW_CHANGELOG.txt)
* [Radio Compatibility](doc/RADIO_COMPATIBILITY.md)
* [TWI Cable Assembly Notes](doc/TWI_CABLE_ASSY_NOTES.md)
* [MX Connector Modification Notes](doc/MX_CONN_MOD_NOTES.md)
* [Developer Notes](doc/DEV_NOTES.md)
* [Security Considerations](doc/SECURITY_CONSIDERATIONS.md)

Contributors
------------

* [Ellie Dugger](https://github.com/duggerd)
* [Matt Ames](https://github.com/mattames)
* [Ilya Smirnov](https://github.com/ilyacodes)

License / Legal
---------------

KFDtool software, firmware, and hardware is distributed under the MIT License (see [LICENSE.txt](LICENSE.txt)).

KFDtool is a trademark of Florida Computer and Networking, Inc.

All product names, trademarks, registered trademarks, logos, and brands are property of their respective owners. All company, product, and service names used are for identification purposes only. Use of these names, trademarks, logos, and brands does not imply endorsement.

Note about hardware:

I request that no one else manufactures identical or compatible units **and sells them to others while I am still doing so** - I have put quite a bit of my own money into developing this hardware. I am totally fine with someone making a unit for themselves or a couple of extras to give to their friends, just that they don't charge for them. Proceeds from hardware sales enables me to further develop the software.

Included open-source components:

Software (see [doc/SW_LICENSE.txt](doc/SW_LICENSE.txt)):

* [NLog](https://github.com/NLog/NLog) - MIT License
* [Mono.Options](https://github.com/mono/mono/blob/master/mcs/class/Mono.Options/Mono.Options/Options.cs) - MIT License
* [HidLibrary](https://github.com/mikeobrien/HidLibrary) - MIT License
* [Microsoft Reference Source](https://github.com/microsoft/referencesource) - MIT License
* [InnovasubBSL430](https://github.com/corentinaltepe/InnovasubBSL430) - GPL v3+ License
* Texas Instruments - BSD 3 Clause License

Firmware (see [doc/FW_LICENSE.txt](doc/FW_LICENSE.txt)):

* Texas Instruments - BSD 3 Clause License


[^ch100]: XG-100M supports native TWI keyloading[^ch721] only via CH100
[^ch721]: XG-100M with CH721 supports TWI keyloading via Harris adapter cable 14002-0143-10
[^freon]: "Freon" (OMAP L-138) architecture only (APX 900, 6000 BN, 8000, 8500, NEXT, N-series, and BN mobiles)
[^harrisxg]: Harris XG radios require Harris adapter cable (portable: unknown part number; mobile: 14002-0143-10)
[^kenwood]: Requires SCM hardware module
