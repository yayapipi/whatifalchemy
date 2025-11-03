using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Core;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace PolarAI.Scripts.AICore.FalAI
{
    public class FalAICore
    {
        public string FalApiKey = "";
        public string FalBaseUrl = "https://fal.run/";

        public void Initialize(string apiKey)
        {
            FalApiKey = apiKey;
        }

        public void CallFalApi(string endpoint, Dictionary<string, object> body, 
            Action<string> onSuccess, Action<string> onError) {
            CoroutineManager.Instance.StartCoroutine(CallFalCoroutine(endpoint, body, onSuccess, onError));
        }

        private IEnumerator CallFalCoroutine(string endpoint, Dictionary<string, object> body, 
            Action<string> onSuccess, Action<string> onError) {
            if (string.IsNullOrEmpty(FalApiKey))
            {
                Debug.LogWarning("Fal AI Core: API Key");
                yield break;
            }

            var url = FalBaseUrl + endpoint;
            var json = JsonConvert.SerializeObject(body);
            var payload = Encoding.UTF8.GetBytes(json);

            using var req = new UnityWebRequest(url, "POST");
            req.uploadHandler = new UploadHandlerRaw(payload);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Accept", "application/json");
            req.SetRequestHeader("Authorization", $"Key {FalApiKey}");

            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"Fal AI Core  [{endpoint}] Error：" +
                                 $"{req.responseCode} {req.error}\n{req.downloadHandler.text}");
                onError?.Invoke(req.downloadHandler.text);
                yield break;
            }

            var text = req.downloadHandler.text;
            onSuccess?.Invoke(text);
        }
    }
}