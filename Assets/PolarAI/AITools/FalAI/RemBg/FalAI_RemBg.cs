using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace PolarAI.Scripts.AICore.FalAI.RemBg
{
    // Fal AI RemBG API : https://fal.ai/models/fal-ai/imageutils/rembg/api
    public class FalAI_RemBg : MonoBehaviour
    {
        public string FalApiKey = "";
        public string Endpoint = "fal-ai/imageutils/rembg";
        
        private FalAICore FalAICore = new FalAICore();

        private void Start()
        {
            FalAICore.Initialize(FalApiKey);
        }

        public void RemoveBg(string imageUrl, Action<string> onCompleteUrl)
        {
            var body = new Dictionary<string, object>
            {
                { "image_url", imageUrl },
            };

            FalAICore.CallFalApi(Endpoint, body, (json) =>
            {
                Debug.Log(json);
                var response = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                if (response.TryGetValue("image", out var value))
                {
                    if (value is JObject imageObj)
                    {
                        var url = imageObj["url"]?.ToString();
                        var contentType = imageObj["content_type"]?.ToString();
                        var width = imageObj["width"]?.ToObject<int>();
                        var height = imageObj["height"]?.ToObject<int>();
            
                        Debug.Log($"Image URL: {url}");
                        Debug.Log($"contentType: {contentType}");
                        Debug.Log($"Resolution: {width}x{height}");
                        onCompleteUrl?.Invoke(url);
                    }
                }

            }, (error) => { Debug.Log(error); });
        }
    }
}