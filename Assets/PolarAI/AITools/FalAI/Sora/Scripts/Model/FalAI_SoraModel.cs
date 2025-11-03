namespace PolarAI.Scripts.AICore.FalAI.Sora.Model
{
    public class FalAI_SoraModel
    {
        public SoraVideoInfo video { get; set; }
        public string video_id { get; set; }
    }

    public class SoraVideoInfo
    {
        public string url { get; set; }
        public string content_type { get; set; }
        public string file_name { get; set; }
        public object file_size { get; set; }
    }
}