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
#if !defined(__CONTROL_OPCODES_H__)
#define __CONTROL_OPCODES_H__

// ---------------------------------------------------------------------------
//  Constants
// ---------------------------------------------------------------------------

/*
** Command Opcodes
*/
#define CMD_READ                    0x11U
#define CMD_WRITE_INFO              0x12U
#define CMD_ENTER_BSL_MODE          0x13U
#define CMD_RESET                   0x14U
#define CMD_SELF_TEST               0x15U
#define CMD_SEND_KEY_SIG            0x16U
#define CMD_SEND_BYTE               0x17U

/*
** Response Opcodes
*/
#define RSP_ERROR                   0x20U
#define RSP_READ                    0x21U
#define RSP_WRITE_INFO              0x22U
#define RSP_ENTER_BSL_MODE          0x23U
#define RSP_RESET                   0x24U
#define RSP_SELF_TEST               0x25U
#define RSP_SEND_KEY_SIG            0x26U
#define RSP_SEND_BYTE               0x27U

/* 
** Broadcast Opcodes
*/
#define BCST_RECEIVE_BYTE           0x31U

/*
** Read Opcodes
*/
#define READ_AP_VER                 0x01U
#define READ_FW_VER                 0x02U
#define READ_UNIQUE_ID              0x03U
#define READ_MODEL_ID               0x04U
#define READ_HW_REV                 0x05U
#define READ_SER_NUM                0x06U

/*
** Write Opcodes
*/
#define WRITE_MDL_REV               0x01U
#define WRITE_SER                   0x02U

/*
** Error Opcodes
*/
#define ERR_OTHER                   0x00U
#define ERR_INVALID_CMD_LENGTH      0x01U
#define ERR_INVALID_CMD_OPCODE      0x02U
#define ERR_INVALID_READ_OPCODE     0x03U
#define ERR_READ_FAILED             0x04U
#define ERR_INVALID_WRITE_OPCODE    0x05U
#define ERR_WRITE_FAILED            0x06U

#endif // __CONTROL_OPCODES_H__
