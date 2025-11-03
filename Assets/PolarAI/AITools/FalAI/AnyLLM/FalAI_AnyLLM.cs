using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace PolarAI.Scripts.AICore.FalAI.AnyLLM
{
    // Fal AI Model: https://fal.ai/models/fal-ai/any-llm
    // Fal AI Vision Model: https://fal.ai/models/fal-ai/any-llm/vision/api
    
    /* Available Model List
     * deepseek/deepseek-r1, anthropic/claude-sonnet-4.5, anthropic/claude-haiku-4.5, anthropic/claude-3.7-sonnet,
     * anthropic/claude-3.5-sonnet, anthropic/claude-3-5-haiku, anthropic/claude-3-haiku, google/gemini-pro-1.5,
     * google/gemini-flash-1.5, google/gemini-flash-1.5-8b, google/gemini-2.0-flash-001, google/gemini-2.5-flash,
     * google/gemini-2.5-flash-lite, google/gemini-2.5-pro, meta-llama/llama-3.2-1b-instruct,
     * meta-llama/llama-3.2-3b-instruct, meta-llama/llama-3.1-8b-instruct, meta-llama/llama-3.1-70b-instruct,
     * openai/gpt-oss-120b, openai/gpt-4o-mini, openai/gpt-4o, openai/gpt-4.1, openai/o3, openai/gpt-5-chat,
     * openai/gpt-5-mini, openai/gpt-5-nano, meta-llama/llama-4-maverick, meta-llama/llama-4-scout
     */
    public class FalAIAnyLlm : MonoBehaviour
    {
        public string FalApiKey = "";
        public string AnyLLMEndpoint = "fal-ai/any-llm";
        public string AnyLLMVisionEndpoint = "fal-ai/any-llm/vision";

        public string DefaultModel = "google/gemini-2.0-flash-001";

        private FalAICore FalAICore;

        private void Start()
        {
            FalAICore = new FalAICore();
            FalAICore.Initialize(FalApiKey);
            // RunAnyLLM("Give me 3 eerie quest ideas for a rogue-like dungeon crawler in chinese",null);
        }

        public void RunAnyLLM(string prompt, Action<Dictionary<string, object>> onComplete,
            string model = null, bool reasoning = false, int maxToken = 300, float temperature = 0.7f)
        {
            model ??= DefaultModel;
            var body = new Dictionary<string, object>
            {
                { "model", model },
                { "prompt", prompt },
                { "reasoning", reasoning },
                { "max_tokens", maxToken },
                { "temperature", temperature }
            };

            FalAICore.CallFalApi(AnyLLMEndpoint, body, (json) =>
            {
                Debug.Log(json);
                var response = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                onComplete?.Invoke(response);

                Debug.Log("output:" + response["output"]);
                Debug.Log("reasoning:" + response["reasoning"]);
                Debug.Log(response["partial"]);
                Debug.Log(response["error"]);
            }, (error) => { Debug.Log(error); });
        }
        
        public void RunAnyLLMVision(string prompt, List<string> imageUrls, Action<Dictionary<string, object>> onComplete,
            string model = null, bool reasoning = false, int maxToken = 300, float temperature = 0.7f)
        {
            model ??= DefaultModel;
            var body = new Dictionary<string, object>
            {
                { "model", model },
                { "prompt", prompt },
                { "image_urls", imageUrls },
                { "reasoning", reasoning },
                { "max_tokens", maxToken },
                { "temperature", temperature }
            };

            FalAICore.CallFalApi(AnyLLMVisionEndpoint, body, (json) =>
            {
                Debug.Log(json);
                var response = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                onComplete?.Invoke(response);

                Debug.Log("output:" + response["output"]);
                Debug.Log("reasoning:" + response["reasoning"]);
                Debug.Log(response["partial"]);
                Debug.Log(response["error"]);
            }, (error) => { Debug.Log(error); });
        }
    }
}