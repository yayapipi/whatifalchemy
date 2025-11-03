using System;
using PolarAI.Scripts.AICore.FalAI.AnyLLM;
using PolarAI.Scripts.AICore.FalAI.NanoBanana;
using PolarAI.Scripts.AICore.FalAI.RemBg;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WhatIfAlchemy.Scripts
{
    public class SettingPanel : MonoBehaviour
    {
        public TMP_InputField FalApiKeyInput;
        public Button OpenFalApiButton;
        public Button CloseBtn;
        public Button OpenSettingBtn;
        public Button SaveBtn;
        public Button DeleteSaveFileBtn;
        public Button MusicBtn;
        public Button SfxBtn;
        public GameObject SettingPanelObj;

        public FalAIAnyLlm FalAIAnyLlm;
        public FalAI_NanoBanana FalAI_NanoBanana;
        public FalAI_RemBg FalAI_RemBg;
        
        private void Start()
        {
            LoadApiKey();
            OpenFalApiButton.onClick.AddListener(() =>
            {
                Application.OpenURL("https://fal.ai/dashboard/keys");
            });
            
            OpenSettingBtn.onClick.AddListener(() =>
            {
                SettingPanelObj.SetActive(true);
            });

            CloseBtn.onClick.AddListener(() =>
            {
                SettingPanelObj.SetActive(false);
            });
            
            SaveBtn.onClick.AddListener(() =>
            {
                SetApiKey();
                SettingPanelObj.SetActive(false);
            });
            
            DeleteSaveFileBtn.onClick.AddListener(() =>
            {
                ElementMergeManager.Instance.RemoveAllSaveFile();
            });
            
        }

        private void SetApiKey()
        {
            FalAIAnyLlm.SetAPIKey(FalApiKeyInput.text);
            FalAI_NanoBanana.SetAPIKey(FalApiKeyInput.text);
            FalAI_RemBg.SetAPIKey(FalApiKeyInput.text);
            
            System.IO.File.WriteAllText(ElementMergeManager.Instance.SavePath + "/key.config", FalApiKeyInput.text);
        }
        
        private void LoadApiKey()
        {
            if (!System.IO.File.Exists(ElementMergeManager.Instance.SavePath + "/key.config"))
            {
                Debug.Log("No key.config file found");
                SettingPanelObj.SetActive(true);
                return;
            }
            
            var apiKey = System.IO.File.ReadAllText(ElementMergeManager.Instance.SavePath + "/key.config");
            FalApiKeyInput.text = apiKey;
            SetApiKey();
        }
        
        
    }
}