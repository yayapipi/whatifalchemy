using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PolarAI.Scripts.AICore.FalAI.AnyLLM
{
    public class FalAI_AnyLLMView : MonoBehaviour
    {
        public bool isVision;
        public List<string> imageUrls;
        public TMP_InputField inputField;
        public Text outputText;
        public Button sendButton;
        
        public FalAIAnyLlm FalAIAnyLlm;
        
        private void Start()
        {
            sendButton.onClick.AddListener(() =>
            {
                if (isVision)
                {
                    FalAIAnyLlm.RunAnyLLMVision(inputField.text, imageUrls, (response) =>
                    {
                        outputText.text = response["output"].ToString();
                    });
                    return;
                }
                
                FalAIAnyLlm.RunAnyLLM(inputField.text, (response) =>
                {
                    outputText.text = response["output"].ToString();
                });
            });
        }
        
    }
}