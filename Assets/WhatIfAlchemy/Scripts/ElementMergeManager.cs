using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using PolarAI.Scripts.AICore.FalAI.AnyLLM;
using PolarAI.Scripts.AICore.FalAI.NanoBanana;
using PolarAI.Scripts.AICore.FalAI.RemBg;
using UnityEngine;
using YFrame.Runtime.Utility;

namespace WhatIfAlchemy.Scripts
{
    public class ElementMergeManager : MonoBehaviour
    {
        [Header("Scene Object")] public GameObject mergedElementPrefab;
        public GameObject elementButtonPrefab;
        public Transform scrollViewContent;

        [Header("AI Model")] public FalAIAnyLlm AnyLLMAI;
        public FalAI_NanoBanana NanoBananaAI;
        public FalAI_RemBg RemBgAI;

        [Header("Generate Style")] 
        public bool AddRefImg;
        public List<Sprite> imageRefUrls;

        [Header("Save Config")]
        public string SavePath;
        public bool LoadGameOnStart;
        
        public static ElementMergeManager Instance;
        
        public List<string> ElementList = new List<string>();
        private Dictionary<string, string> MergeMappingDic = new Dictionary<string, string>();
        
        private Dictionary<string, ElementView> ElementObjDic = new Dictionary<string, ElementView>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            
            SavePath = Path.Combine(Application.persistentDataPath, SavePath);
        }

        private void Start()
        {
            if (LoadGameOnStart)
            {
                LoadElementList();
                BuildElementList();
            }
        }

        public static void MergeElement(ElementView draggedElement, ElementView targetElement)
        {
            var imgUrls = new List<string>
            {
                draggedElement.TargetSpriteRenderer.sprite.ToBase64DataUri(),
                targetElement.TargetSpriteRenderer.sprite.ToBase64DataUri()
            };

            if (Instance.AddRefImg)
            {
                foreach (var imgUrl in Instance.imageRefUrls)
                {
                    imgUrls.Add(imgUrl.ToBase64DataUri());
                }
            }

            var mergePosition = (draggedElement.transform.position + targetElement.transform.position) * 0.5f;
            var newElement = Instance.CreateMergedElement(mergePosition);
            draggedElement.gameObject.SetActive(false);
            targetElement.gameObject.SetActive(false);
            
            var matchResult = Instance.TrpMappingElement(draggedElement.GetElementName(), targetElement.GetElementName());
            if (!string.IsNullOrEmpty(matchResult))
            {
                newElement.UpdateGeneratedSprite(matchResult,
                    SpritesUtility.LoadImage(Instance.SavePath +  $"/img/{matchResult}.png").ToSprite());
                return;
            }
     
            Instance.CombinationPrompt(draggedElement.GetElementName(), targetElement.GetElementName(), (newName) =>
            {
                Instance.GenerateNewElementImage(newName, imgUrls, true, (newSprite) =>
                {
                    // update new result;
                    newElement.UpdateGeneratedSprite(newName, newSprite);
                    Instance.CreateUIButton(newElement);
                    SpritesUtility.SaveImage(newSprite.ToBytes(), Instance.SavePath + $"/img/{newName}.png");
                    Instance.MergeMappingDic.Add(draggedElement.GetElementName() + "+" + targetElement.GetElementName(), 
                        newName);
                    Instance.ElementList.Add(newName);
                    Instance.SaveElementList();
                    Destroy(draggedElement.gameObject);
                    Destroy(targetElement.gameObject);
                });
            });
        }

        private string TrpMappingElement(string name1, string name2)
        {
            var mapName = $"{name1}+{name2}";
            if (Instance.MergeMappingDic.TryGetValue(mapName, out var matchName))
            {
                return matchName;
            }
            
            var mapNameRev = $"{name2}+{name1}";
            return Instance.MergeMappingDic.GetValueOrDefault(mapNameRev);
        }

        private void CombinationPrompt(string name1, string name2, Action<string> onComplete)
        {
            string combinePrompt = "1. Combine this two element and give me new element. \n" +
                                   "2. It could be no new element if not reactable( return empty ) \n" +
                                   "3. It could be very creative element, such as dragon, anime character, ghost or robot\n" +
                                   "4. Reference Little Alchemy 2 game for as example \n" +
                                   "5. return as json format { result:\"new-element-name\" } \n" +
                                   "6. example: I give you {fire} and {water}, you return { result:\"steam\"} \n\n" +
                                   $"Given Element: {name1}, {name2}";

            AnyLLMAI.RunAnyLLM(combinePrompt, (result) =>
            {
                var jsonResult = result["output"].ToString();
                jsonResult = JsonExtractor.CleanJsonString(jsonResult);
                var response = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonResult);
                onComplete?.Invoke(response["result"].ToString());
            });
        }

        private void GenerateNewElementImage(string elementName, List<string> imageUrls, bool remBG,
            Action<Sprite> onComplete)
        {
            string generatePrompt = "1. generate this style of cute element\n" +
                                    "2. only generate single element\n" +
                                    "3. white/single color background\n" +
                                    $"4. generate element <{elementName}>";

            NanoBananaAI.EditImg(generatePrompt, imageUrls, (imgUrl, imgInfo) =>
            {
                if (remBG)
                {
                    RemoveBackground(imgUrl,
                        (remUrl) =>
                        {
                            SpritesUtility.DownloadImageFromURL(remUrl,
                                (spriteByte) => { onComplete?.Invoke(spriteByte.ToSprite()); });
                        });
                }
                else
                {
                    SpritesUtility.DownloadImageFromURL(imgUrl,
                        (spriteByte) => { onComplete?.Invoke(spriteByte.ToSprite()); });
                }
            });
        }

        private void RemoveBackground(string url, Action<string> onComplete)
        {
            RemBgAI.RemoveBg(url, (imgUrl) => { onComplete?.Invoke(imgUrl); });
        }

        private ElementView CreateMergedElement(Vector3 position)
        {
            var newElement = Instantiate(mergedElementPrefab, position, Quaternion.identity);
            var newElementView = newElement.GetComponent<ElementView>();
            if (newElementView != null)
            {
                newElementView.SetLoadingState();
            }

            return newElementView;
        }

        private void CreateUIButton(ElementView mergedElement)
        {
            var buttonObject = Instantiate(elementButtonPrefab, scrollViewContent);
            var buttonController = buttonObject.GetComponent<ElementButtonController>();
            buttonController.SetElementName(mergedElement.GetElementName());
            buttonController.SetElementSprite(mergedElement.TargetSpriteRenderer.sprite);
            buttonController.gameObject.name = $"{mergedElement.GetElementName()}Btn";
        }
        
        private void CreateUIButton(string elementName)
        {
            var buttonObject = Instantiate(elementButtonPrefab, scrollViewContent);
            var buttonController = buttonObject.GetComponent<ElementButtonController>();
            buttonController.SetElementName(elementName);
            buttonController.SetElementSprite(SpritesUtility.LoadImage(SavePath +  $"/img/{elementName}.png").ToSprite());
            buttonController.gameObject.name = $"{elementName}Btn";
        }


        private void SaveElementList()
        {
            var json = JsonConvert.SerializeObject(ElementList);
            System.IO.File.WriteAllText(SavePath + "/ElementList.json", json);
            
            var jsonMergeMapping = JsonConvert.SerializeObject(MergeMappingDic);
            System.IO.File.WriteAllText(SavePath + "/MergeMapping.json", jsonMergeMapping);
        }

        private void LoadElementList()
        {
            if (!System.IO.File.Exists(SavePath + "/ElementList.json"))
            {
                return;
            }
            
            var json = System.IO.File.ReadAllText(SavePath + "/ElementList.json");
            ElementList = JsonConvert.DeserializeObject<List<string>>(json);
            
            if (!System.IO.File.Exists(SavePath + "/MergeMapping.json"))
            {
                return;
            }
            
            var jsonMergeMapping = System.IO.File.ReadAllText(SavePath + "/MergeMapping.json");
            MergeMappingDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonMergeMapping);
        }

        private void BuildElementList()
        {
            foreach (var elementName in ElementList)
            {
                Instance.CreateUIButton(elementName);
            }
        }
    }
}