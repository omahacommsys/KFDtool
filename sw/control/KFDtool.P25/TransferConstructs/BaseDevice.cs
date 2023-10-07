using KFDtool.Adapter.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KFDtool.P25.TransferConstructs
{
    public class BaseDevice
    {
        public enum DeviceTypeOptions
        {
            TwiKfdDevice,
            DliIp
        }

        public DeviceTypeOptions DeviceType { get; set; }

        public TwiKfdDevice KfdDeviceType { get; set; }

        public TwiKfdtoolDevice TwiKfdtoolDevice { get; set; }

        public DliIpDevice DliIpDevice { get; set; }
    }
}
