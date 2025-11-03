using System.Collections.Generic;

namespace PolarAI.Scripts.AICore.FalAI.NanoBanana
{
    public class FalNanoBananaResponseModel
    {
        public List<NanoImageInfo> images { get; set; }
        public string description { get; set; }
    }
    
    public class NanoImageInfo
    {
        public string url { get; set; }
        public string content_type { get; set; }
        public string file_name { get; set; }
        public object file_size { get; set; }
    }
}