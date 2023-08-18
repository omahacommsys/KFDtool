using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KFDtool.Adapter.Protocol.Serial
{
    internal interface KfdSerialProtocol
    {
        void Open();

        void Close();

        void Clear();

        void Send(List<byte> date);

        List<byte> Read(int timeout);

        void Cancel();
    }
}
