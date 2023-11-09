using KFDtool.KMF.TransferConstructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KFDtool.KMF.KMFConnection
{
    public interface IKMFConnection
    {
        Task<List<KMFKeyItem>> GetAllKeys();
    }
}
