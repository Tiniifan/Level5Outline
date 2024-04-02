using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Level5Outline.Level5.Outline
{
    public class OutlineMeshData
    {
        [JsonProperty("OutlineValue")]
        public object[] Values;

        [JsonProperty("MeshHashes")]
        public List<string> Hashes;

        public OutlineMeshData()
        {
            Hashes = new List<string>();
        }
    }
}
