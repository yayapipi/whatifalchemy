using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace PolarAI.Scripts.AICore.FalAI.NanoBanana
{
    public class FalAI_NanoBanana : MonoBehaviour
    {
        public string FalApiKey = "";
        public string TextEndpoint = "fal-ai/nano-banana";
        public string EditEndpoint = "fal-ai/nano-banana/edit";
        
        private FalAICore FalAICore = new FalAICore();

        private void Start()
        {
            FalAICore.Initialize(FalApiKey);
        }

        public void GenImg(string prompt, Action<string,string> onCompleteUrl)
        {
            var body = new Dictionary<string, object>
            {
                { "prompt", prompt },
            };
            Debug.Log($"Generating Img");
            FalAICore.CallFalApi(TextEndpoint, body, (json) =>
            {
                Debug.Log(json);
                var response = JsonConvert.DeserializeObject<FalNanoBananaResponseModel>(json);
                onCompleteUrl?.Invoke(response.images.FirstOrDefault()?.url, response.description);

            }, (error) => { Debug.Log(error); });
        }
        
        public void EditImg(string prompt, List<string> imageUrls, Action<string,string> onCompleteUrl)
        {
            var body = new Dictionary<string, object>
            {
                { "prompt", prompt },
                { "image_urls", imageUrls },
            };

            Debug.Log($"Editing Img");
            FalAICore.CallFalApi(EditEndpoint, body, (json) =>
            {
                Debug.Log(json);
                var response = JsonConvert.DeserializeObject<FalNanoBananaResponseModel>(json);
                onCompleteUrl?.Invoke(response.images.FirstOrDefault()?.url, response.description);

            }, (error) => { Debug.Log(error); });
        }
    }
}