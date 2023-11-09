using KFDtool.KMF.TransferConstructs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KFDtool.KMF.KMFConnection
{
    public class OPKMFConnection : IKMFConnection
    {
        private readonly HttpClient _client;
        private readonly string _baseUrl;
        public OPKMFConnection(string serverUrl, string secretKey)
        {
            _client = new HttpClient();
            _baseUrl = serverUrl;
        }

        public async Task<List<KMFKeyItem>> GetAllKeys()
        {
            List<KMFKeyItem> keys = new List<KMFKeyItem>();
            HttpResponseMessage response = await _client.GetAsync(_baseUrl + "/api/keys");
            if (response.IsSuccessStatusCode)
            {
                keys = JsonConvert.DeserializeObject<List<KMFKeyItem>>(await response.Content.ReadAsStringAsync());
            }
            return keys;
        }
    }
}
