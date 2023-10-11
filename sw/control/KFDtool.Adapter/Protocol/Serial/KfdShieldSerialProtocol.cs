using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KFDtool.Adapter.Protocol.Serial
{
    public class KfdShieldSerialProtocol : KfdSerialProtocol
    {
        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        const byte SOM = 0x61;
        const byte SOM_PLACEHOLDER = 0x62;
        const byte EOM = 0x63;
        const byte EOM_PLACEHOLDER = 0x64;
        const byte ESC = 0x70;
        const byte ESC_PLACEHOLDER = 0x71;

        private static AutoResetEvent CancelRead = new AutoResetEvent(false);

        private List<byte> FrameBuffer;

        private bool FoundStart;

        private List<List<byte>> PacketBuffer;

        private static ManualResetEvent PacketReady = new ManualResetEvent(false);

        private SerialPort Port;

        public KfdShieldSerialProtocol(string portName)
        {
            FrameBuffer = new List<byte>();

            FoundStart = false;

            PacketBuffer = new List<List<byte>>();

            Port = new SerialPort();

            Port.PortName = portName;
            Port.BaudRate = 115200;
            Port.Parity = Parity.None;
            Port.DataBits = 8;
            Port.StopBits = StopBits.One;

            Port.DataReceived += new SerialDataReceivedEventHandler(OnDataReceived);
        }

        public void Open()
        {
            // don't open the port if it is open already
            if (!Port.IsOpen)
            {
                Port.Open();
            }
            else
            {
                Logger.Debug("Not opening already open serial port");
            }
        }

        public void Close()
        {
            Port.Close();
        }

        public void Clear()
        {
            FrameBuffer.Clear();
            PacketBuffer.Clear();
            PacketReady.Reset();
            FoundStart = false;

            while (Port.BytesToRead > 0)
            {
                Port.ReadByte();
            }
        }

        public void Send(List<byte> data)
        {
            List<byte> frameData = new List<byte>();

            frameData.Add(SOM);

            foreach (byte b in data)
            {
                if (b == ESC)
                {
                    frameData.Add(ESC);
                    frameData.Add(ESC_PLACEHOLDER);
                }
                else if (b == SOM)
                {
                    frameData.Add(ESC);
                    frameData.Add(SOM_PLACEHOLDER);
                }
                else if (b == EOM)
                {
                    frameData.Add(ESC);
                    frameData.Add(EOM_PLACEHOLDER);
                }
                else
                {
                    frameData.Add(b);
                }
            }

            frameData.Add(EOM);

            byte[] outData = frameData.ToArray();

            Logger.Debug("out len: {0}", outData.Length);
            Logger.Debug("out data: {0}", BitConverter.ToString(outData));

            Port.Write(outData, 0, outData.Length);
        }

        private byte[] ReadPacketFromPacketBuffer()
        {
            Logger.Trace("packet buffer before length: {0}", PacketBuffer.Count);

            if (PacketBuffer.Count == 0)
            {
                throw new Exception("no packet in packet buffer");
            }

            byte[] packet = PacketBuffer[0].ToArray();

            Logger.Trace("packet contents: {0}", BitConverter.ToString(packet));

            PacketBuffer.RemoveAt(0);

            Logger.Trace("packet buffer after length: {0}", PacketBuffer.Count);

            return packet;
        }

        public List<byte> Read(int timeout)
        {
            // if there are no packets in the buffer, wait until there is one
            if (PacketBuffer.Count == 0)
            {
                if (timeout > 0)
                {
                    if (!PacketReady.WaitOne(timeout))
                    {
                        throw new TimeoutException("timeout waiting for data");
                    }
                }
                else if (timeout == 0)
                {
                    WaitHandle[] handles = new WaitHandle[] { PacketReady, CancelRead };

                    if (WaitHandle.WaitAny(handles) == 1)
                    {
                        throw new OperationCanceledException("read was canceled");
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException("timeout can not be negative");
                }
            }

            List<byte> data = new List<byte>();

            byte[] packet = ReadPacketFromPacketBuffer();

            data.AddRange(packet);

            PacketReady.Reset();

            return data;
        }

        public void Cancel()
        {
            CancelRead.Set();
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = sender as SerialPort;

            int toRead = sp.BytesToRead;

            Logger.Debug("in len: {0}", toRead);

            if (toRead == 0)
            {
                Logger.Warn("no data to read");
                return;
            }

            byte[] inData = new byte[toRead];

            sp.Read(inData, 0, inData.Length);

            Logger.Debug("in data: {0}", BitConverter.ToString(inData));

            foreach (byte b in inData)
            {
                Logger.Trace("new byte: 0x{0:X2}", b);

                if (b == SOM)
                {
                    FoundStart = true;
                    Logger.Trace("found start byte");
                }
                else if (b == EOM)
                {
                    for (int i = 0; i < FrameBuffer.Count; i++)
                    {
                        if (FrameBuffer[i] == ESC)
                        {
                            FrameBuffer.RemoveAt(i);

                            if (i == FrameBuffer.Count)
                            {
                                throw new Exception("escape character at end");
                            }

                            if (FrameBuffer[i] == ESC_PLACEHOLDER)
                            {
                                FrameBuffer[i] = ESC;
                            }
                            else if (FrameBuffer[i] == SOM_PLACEHOLDER)
                            {
                                FrameBuffer[i] = SOM;
                            }
                            else if (FrameBuffer[i] == EOM_PLACEHOLDER)
                            {
                                FrameBuffer[i] = EOM;
                            }
                            else
                            {
                                throw new Exception("invalid character after escape character");
                            }
                        }
                    }

                    List<byte> packet = new List<byte>();

                    packet.AddRange(FrameBuffer);

                    PacketBuffer.Add(packet);

                    Logger.Debug("packet contents: {0}", BitConverter.ToString(packet.ToArray()));

                    FrameBuffer.Clear();

                    Logger.Debug("packet buffer length: {0}", PacketBuffer.Count);
                }
                else
                {
                    if (FoundStart)
                    {
                        Logger.Trace("added 0x{0:X2}", b);
                        FrameBuffer.Add(b);
                    }
                }
            }

            if (PacketBuffer.Count > 0)
            {
                PacketReady.Set();
            }
        }
    }
}
