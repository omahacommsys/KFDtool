/*
*
*   Copyright 2019-2022 KFDtool, LLC
*   Copyright 2023 Natalie Moore
*   Copyright (C) 2023 by Bryan Biedenkapp N2PLL
*
*   Permission is hereby granted, free of charge, to any person obtaining a copy
*   of this software and associated documentation files (the "Software"), to deal
*   in the Software without restriction, including without limitation the rights
*   to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
*   copies of the Software, and to permit persons to whom the Software is
*   furnished to do so, subject to the following conditions:
*   
*   The above copyright notice and this permission notice shall be included in all
*   copies or substantial portions of the Software.
*/

using KFDtool.P25.TransferConstructs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace KFDtool.P25.Kmm
{
    /* TIA 102.AACA-A 10.2.19 and TIA 102.AACD-A 3.9.2.17 */
    public class ModifyKeyCommand : KmmBody
    {
        private int _keysetId;

        private int _algorithmId;

        public CmdKeyItem Kek;

        public int KeysetId
        {
            get
            {
                return _keysetId;
            }
            set
            {
                if (value < 0 || value > 0xFF)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _keysetId = value;
            }
        }

        public int AlgorithmId
        {
            get
            {
                return _algorithmId;
            }
            set
            {
                if (value < 0 || value > 0xFF)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _algorithmId = value;
            }
        }

        public List<KeyItem> KeyItems { get; set; }

        public override MessageId MessageId
        {
            get
            {
                return MessageId.ModifyKeyCommand;
            }
        }

        public override ResponseKind ResponseKind
        {
            get
            {
                return ResponseKind.Immediate;
            }
        }

        public ModifyKeyCommand()
        {
            KeyItems = new List<KeyItem>();
            Kek = null;
        }

        public override byte[] ToBytes()
        {
            bool hasMi = false;
            int kekAlgid = Kek != null ? Kek.AlgorithmId: 0x80;

            // encrypt keys if necessary
            if (kekAlgid != 0x80)
            {

                List<KeyItem> encKeys = new List<KeyItem>();
                foreach (KeyItem keyIn in KeyItems)
                {
                    if (kekAlgid == 0x84)
                    {
                        keyIn.Key = WrapKeyFrameAes(keyIn.Key, Kek.Key.ToArray());
                        //encKeys.Add(keyIn);
                    }
                    else
                    {
                        throw new Exception("Only AES-256 encrypted keyloads are supported at this time.");
                    }
                }
            }

            List<byte> keys = new List<byte>();

            foreach (KeyItem key in KeyItems)
            {
                keys.AddRange(key.ToBytes());
            }

            List<byte> contents = new List<byte>();

            /* decryption instruction format */

            // add Message Indicator/Initilization Vector flag
            BitArray decFormatArr = new BitArray(8, false);
            decFormatArr.Set(6, hasMi);

            int[] decFormat = new int[1];
            decFormatArr.CopyTo(decFormat, 0);
            contents.Add((byte)decFormat[0]);

            /* extended decryption instruction format */
            contents.Add(0x00);

            /* algorithm id */
            contents.Add((byte) kekAlgid);

            /* key id */
            contents.Add((byte) (((Kek != null ? Kek.KeyId : 0x00) >> 8) & 0xff));
            contents.Add((byte) ((Kek != null ? Kek.KeyId : 0x00) & 0xff));

            /* keyset id */
            contents.Add((byte)KeysetId);

            /* algorithm id */
            contents.Add((byte)AlgorithmId);

            /* key length */
            contents.Add((byte)KeyItems[0].Key.Length);

            /* number of keys */
            contents.Add((byte)KeyItems.Count);

            /* keys */
            contents.AddRange(keys);

            return contents.ToArray();
        }

        public override void Parse(byte[] contents)
        {
            if (contents.Length < 9)
            {
                throw new ArgumentOutOfRangeException(string.Format("length mismatch - expected at least 9, got {0} - {1}", contents.Length.ToString(), BitConverter.ToString(contents)));
            }

            Kek = new CmdKeyItem();

            /* alg id */
            Kek.AlgorithmId = contents[2];

            if (Kek.AlgorithmId != 0x80)
            {
                throw new Exception(string.Format("Unable to decrypt algid {0}", Kek.AlgorithmId));
            }

            /* key id */
            //TODO


            /* keyset id */
            KeysetId = contents[5];

            /* algorithm id */
            AlgorithmId = contents[6];

            /* key length */
            int keyLength = contents[7];

            /* number of keys */
            int keyCount = contents[8];

            /* keys */
            if ((keyCount == 0) && (contents.Length == 9))
            {
                return;
            }
            else if (((keyCount * (5 + keyLength)) % (contents.Length - 9)) == 0)
            {
                for (int i = 0; i < keyCount; i++)
                {
                    byte[] item = new byte[5 + keyLength];
                    Array.Copy(contents, 9 + (i * (5 + keyLength)), item, 0, 5 + keyLength);
                    KeyItem item2 = new KeyItem();
                    item2.Parse(item, keyLength);
                    KeyItems.Add(item2);
                }
            }
            else
            {
                throw new Exception("number of keys field and length mismatch");
            }
        }

        private static byte[] WrapKeyFrameAes(byte[] tek, byte[] kek, byte[] iv = null)
        {
            if (tek == null)
                throw new ArgumentNullException("input");
            if (kek == null)
                throw new ArgumentNullException("key");
            if (iv == null)
                iv = new byte[8] { 0xA6, 0xA6, 0xA6, 0xA6, 0xA6, 0xA6, 0xA6, 0xA6 };

            // initialize variables
            const int blockLength = 8; // in bytes
            int x = (int)(Math.Ceiling((double)(tek.Length / blockLength)));
            byte[][] R = new byte[x][];
            for (int i = 0; i < x; i++)
            {
                R[i] = new byte[blockLength];
                Buffer.BlockCopy(tek, i * blockLength, R[i], 0, blockLength);
            }

            RijndaelManaged aes = new RijndaelManaged();
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.None;

            ICryptoTransform cryptor = aes.CreateEncryptor(kek, new byte[16]);

            // calculate intermediate values
            byte[] input = new byte[blockLength * 2];
            Buffer.BlockCopy(iv, 0, input, 0, blockLength);
            for (int j = 0; j < 6; j++)
            {
                for (int i = 0; i < x; i++)
                {
                    Buffer.BlockCopy(input, 0, input, 0, blockLength);
                    Buffer.BlockCopy(R[i], 0, input, blockLength, blockLength);

                    byte[] B = new byte[blockLength * 2];
                    cryptor.TransformBlock(input, 0, input.Length, B, 0);

                    byte[] MSB = new byte[blockLength];
                    Buffer.BlockCopy(B, 0, MSB, 0, blockLength);
                    byte[] LSB = new byte[8];
                    Buffer.BlockCopy(B, blockLength, LSB, 0, blockLength);

                    // perform MSB XOR
                    byte[] t = BitConverter.GetBytes((ulong)((x * j) + i + 1));
                    Array.Reverse(t);
                    for (int k = 0; k < blockLength; k += 2)
                    {
                        ushort v = (ushort)((MSB[k] << 8) + MSB[k + 1]);
                        ushort tv = (ushort)((t[k] << 8) + t[k + 1]);
                        v = (ushort)(v ^ tv);

                        input[k] = (byte)((v >> 8) & 0xFF);
                        input[k + 1] = (byte)(v & 0xFF);
                    }

                    R[i] = LSB;
                }
            }

            // assemble key frame
            int len = x * blockLength + blockLength;
            byte[] frame = new byte[len];
            Buffer.BlockCopy(input, 0, frame, 0, blockLength);
            for (int i = 0; i < x; i++)
                Buffer.BlockCopy(R[i], 0, frame, (i * blockLength) + blockLength, blockLength);

            return frame;
        }
    }
}
