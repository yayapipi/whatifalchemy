using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PolarAI.Scripts.AICore.FalAI.Sora.Model;
using UnityEngine;

namespace PolarAI.Scripts.AICore.FalAI.Sora
{
    public class FalAI_Sora : MonoBehaviour
    {
        public string FalApiKey = "";
        public string OpenAIApiKey = "";
        public string T2VEndpoint = "fal-ai/sora-2/text-to-video";
        public string I2VEndpoint = "fal-ai/sora-2/image-to-video";
        
        private FalAICore FalAICore = new FalAICore();

        private void Start()
        {
            FalAICore.Initialize(FalApiKey);
        }

        public void TextToVideo(string prompt, string aspectRatio, int duration, Action<string> onCompleteUrl)
        {
            var body = new Dictionary<string, object>
            {
                { "prompt", prompt },
                { "resolution", "720p" },
                { "aspect_ratio", aspectRatio }, // 16:9 or 9:16
                { "duration", duration }, // 4,8 or 12
            };
            
            // Use OpenAI Key Will No Charge For Fal Ai Token
            if (OpenAIApiKey != "")
            {
                body.Add("api_key", OpenAIApiKey);
            }
            
            Debug.Log($"Generating Video");
            
            FalAICore.CallFalApi(T2VEndpoint, body, (json) =>
            {
                Debug.Log(json);
                var response = JsonConvert.DeserializeObject<FalAI_SoraModel>(json);
                onCompleteUrl?.Invoke(response.video?.url);

            }, (error) => { Debug.Log(error); });
        }
        
        public void ImageToVideo(string imageUrl, string prompt, string aspectRatio, int duration, Action<string> onCompleteUrl)
        {
            var body = new Dictionary<string, object>
            {
                { "image_url", imageUrl },
                { "prompt", prompt },
                { "resolution", "720p" },
                { "aspect_ratio", aspectRatio }, // 16:9 or 9:16
                { "duration", duration }, // 4,8 or 12
            };
            
            // Use OpenAI Key Will No Charge For Fal Ai Token
            if (OpenAIApiKey != "")
            {
                body.Add("api_key", OpenAIApiKey);
            }
            
            Debug.Log($"Generating Video");
            
            FalAICore.CallFalApi(I2VEndpoint, body, (json) =>
            {
                Debug.Log(json);
                var response = JsonConvert.DeserializeObject<FalAI_SoraModel>(json);
                onCompleteUrl?.Invoke(response.video?.url);

            }, (error) => { Debug.Log(error); });
        }
    }
}