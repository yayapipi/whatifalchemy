using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PolarAI.Scripts.AICore.FalAI.AnyLLM;
using PolarAI.Scripts.AICore.FalAI.NanoBanana;
using PolarAI.Scripts.AICore.FalAI.RemBg;
using UnityEngine;
using UnityEngine.UI;
using YFrame.Runtime.Utility;

/// <summary>
/// 元素合併管理器，負責處理元素之間的合併邏輯
/// </summary>
public class ElementMergeManager : MonoBehaviour
{
    [Header("合併設定")]
    [SerializeField] private bool enableMerge = true;
    [SerializeField] private GameObject mergedElementPrefab;
    [SerializeField] private string mergedElementName = "Merged Element";
    [SerializeField] private Sprite mergedElementSprite;
    
    [Header("UI 合併設定")]
    [SerializeField] private bool enableUIButtonCreation = true;
    [SerializeField] private GameObject elementButtonPrefab;
    [SerializeField] private Transform scrollViewContent;

    public FalAIAnyLlm AnyLLMAI;
    public FalAI_NanoBanana NanoBananaAI;
    public FalAI_RemBg RemBgAI;

    public bool AddRefImg;
    public List<Sprite> imageRefUrls;
    
    // 靜態實例
    private static ElementMergeManager instance;
    
    // 事件
    public static System.Action<ElementView, ElementView, ElementView> OnElementsMerged;
    
    /// <summary>
    /// 靜態實例屬性
    /// </summary>
    public static ElementMergeManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ElementMergeManager>();
                if (instance == null)
                {
                    Debug.LogError("ElementMergeManager: 找不到 ElementMergeManager 實例！請確保場景中有一個 ElementMergeManager。");
                }
            }
            return instance;
        }
    }
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        // 確保只有一個實例
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// 設定合併功能開關
    /// </summary>
    public static void SetEnableMerge(bool enable)
    {
        if (Instance != null)
        {
            Instance.enableMerge = enable;
        }
    }
    
    /// <summary>
    /// 設定合併元素預製體
    /// </summary>
    public static void SetMergedElementPrefab(GameObject prefab)
    {
        if (Instance != null)
        {
            Instance.mergedElementPrefab = prefab;
        }
    }
    
    /// <summary>
    /// 設定合併元素名稱
    /// </summary>
    public static void SetMergedElementName(string name)
    {
        if (Instance != null)
        {
            Instance.mergedElementName = name;
        }
    }
    
    /// <summary>
    /// 設定合併元素 Sprite
    /// </summary>
    public static void SetMergedElementSprite(Sprite sprite)
    {
        if (Instance != null)
        {
            Instance.mergedElementSprite = sprite;
        }
    }
    
    /// <summary>
    /// 設定 UI 按鈕創建開關
    /// </summary>
    public static void SetEnableUIButtonCreation(bool enable)
    {
        if (Instance != null)
        {
            Instance.enableUIButtonCreation = enable;
        }
    }
    
    /// <summary>
    /// 設定元素按鈕預製體
    /// </summary>
    public static void SetElementButtonPrefab(GameObject prefab)
    {
        if (Instance != null)
        {
            Instance.elementButtonPrefab = prefab;
        }
    }
    
    /// <summary>
    /// 設定 ScrollView Content
    /// </summary>
    public static void SetScrollViewContent(Transform content)
    {
        if (Instance != null)
        {
            Instance.scrollViewContent = content;
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
        
        Instance.CombinationPrompt(draggedElement.GetElementName(), targetElement.GetElementName(), (newName) =>
        {
            Instance.GenerateNewElementImage(newName, imgUrls, true, (newSprite) =>
            {
                // update new result;
                newElement.UpdateGeneratedSprite(newName, newSprite);;
                Destroy(draggedElement.gameObject);
                Destroy(targetElement.gameObject);
                
            });
        });
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

    private void GenerateNewElementImage(string elementName, List<string> imageUrls, bool remBG,  Action<Sprite> onComplete)
    {
        string generatePrompt = "1. generate this style of cute element\n" +
                                "2. only generate single element\n" +
                                "3. white/single color background\n" +
                                $"4. generate element <{elementName}>";
        
        NanoBananaAI.EditImg(generatePrompt, imageUrls, (imgUrl,imgInfo) =>
        {
            if (remBG)
            {
                RemoveBackground(imgUrl, (remUrl) =>
                {
                    SpritesUtility.DownloadImageFromURL(remUrl, (spriteByte) =>
                    {
                        onComplete?.Invoke(spriteByte.ToSprite());
                    });
                });
            }
            else
            {
                SpritesUtility.DownloadImageFromURL(imgUrl, (spriteByte) =>
                {
                    onComplete?.Invoke(spriteByte.ToSprite());
                });
            }
     
        });
    }

    private void RemoveBackground(string url,  Action<string> onComplete)
    {
        RemBgAI.RemoveBg(url, (imgUrl) =>
        {
            onComplete?.Invoke(imgUrl);
        });
    }
    
    /// <summary>
    /// 檢查並處理元素合併
    /// </summary>
    public static bool TryMergeElements(ElementView draggedElement, ElementView targetElement)
    {
        if (Instance == null)
        {
            Debug.LogError("ElementMergeManager: 實例不存在！");
            return false;
        }
        
        // 檢查是否啟用合併
        if (!Instance.enableMerge)
        {
            return false;
        }
        
        // 檢查是否有設定合併元素預製體
        if (Instance.mergedElementPrefab == null)
        {
            Debug.LogError("ElementMergeManager: 沒有設定 mergedElementPrefab，無法創建合併元素！");
            return false;
        }
        
        // 檢查兩個元素是否有效
        if (draggedElement == null || targetElement == null)
        {
            return false;
        }
        
        // 執行合併
        Instance.MergeElements(draggedElement, targetElement);
        return true;
    }
    
    /// <summary>
    /// 合併兩個元素
    /// </summary>
    private void MergeElements(ElementView element1, ElementView element2)
    {
        Debug.Log($"[ElementMergeManager] {element1.GetElementName()} 與 {element2.GetElementName()} 合併！");
        
        // 計算合併位置（兩個元素的中間位置）
        Vector3 mergePosition = (element1.transform.position + element2.transform.position) * 0.5f;
        
        // 創建新的合併元素
        ElementView mergedElement = CreateMergedElement(mergePosition);
        
        // 創建 UI 按鈕（如果啟用）
        if (enableUIButtonCreation)
        {
            CreateUIButton(mergedElement);
        }
        
        // 摧毀兩個原始元素
        if (element1 != null)
        {
            Destroy(element1.gameObject);
        }
        if (element2 != null)
        {
            Destroy(element2.gameObject);
        }
        
        // 觸發合併事件
        OnElementsMerged?.Invoke(element1, element2, mergedElement);
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
    
    /// <summary>
    /// 創建 UI 按鈕
    /// </summary>
    private void CreateUIButton(ElementView mergedElement)
    {
        // 檢查是否有設定按鈕預製體
        if (elementButtonPrefab == null)
        {
            Debug.LogWarning("[ElementMergeManager] 沒有設定 elementButtonPrefab，無法創建 UI 按鈕！");
            return;
        }
        
        // 檢查是否有設定 ScrollView Content
        if (scrollViewContent == null)
        {
            Debug.LogWarning("[ElementMergeManager] 沒有設定 scrollViewContent，無法創建 UI 按鈕！");
            return;
        }
        
        // 創建按鈕物件
        GameObject buttonObject = Instantiate(elementButtonPrefab, scrollViewContent);
        
        // 獲取 ElementButtonController 組件
        ElementButtonController buttonController = buttonObject.GetComponent<ElementButtonController>();
        if (buttonController != null)
        {
            // 初始化按鈕設定
            InitializeButtonController(buttonController, mergedElement);
            Debug.Log($"[ElementMergeManager] 在 ScrollView 中創建了按鈕: {mergedElementName}");
        }
        else
        {
            Debug.LogWarning("[ElementMergeManager] 按鈕預製體沒有 ElementButtonController 組件！");
        }
    }
    
    /// <summary>
    /// 初始化按鈕控制器
    /// </summary>
    private void InitializeButtonController(ElementButtonController buttonController, ElementView mergedElement)
    {
        // 設定元素名稱
        buttonController.SetElementName(mergedElementName);
        
        // 設定元素 Sprite
        if (mergedElementSprite != null)
        {
            buttonController.SetElementSprite(mergedElementSprite);
        }
        
        // 設定按鈕名稱
        buttonController.gameObject.name = $"Button_{mergedElementName}";
        buttonController.GetComponent<Image>().sprite = mergedElementSprite;
    }
    
    /// <summary>
    /// 檢查兩個元素是否應該合併
    /// </summary>
    public static bool ShouldMerge(ElementView element1, ElementView element2)
    {
        if (Instance == null || !Instance.enableMerge || element1 == null || element2 == null)
        {
            return false;
        }
        
        // 這裡可以添加更複雜的合併條件邏輯
        // 例如：檢查元素類型、等級等
        return true;
    }
    
    /// <summary>
    /// 獲取合併設定信息
    /// </summary>
    public static string GetMergeSettingsInfo()
    {
        if (Instance == null)
        {
            return "ElementMergeManager 實例不存在";
        }
        
        return $"Merge Enabled: {Instance.enableMerge}, " +
               $"Prefab: {(Instance.mergedElementPrefab != null ? Instance.mergedElementPrefab.name : "None")}, " +
               $"Name: {Instance.mergedElementName}, " +
               $"UI Button Creation: {Instance.enableUIButtonCreation}, " +
               $"Button Prefab: {(Instance.elementButtonPrefab != null ? Instance.elementButtonPrefab.name : "None")}, " +
               $"ScrollView Content: {(Instance.scrollViewContent != null ? Instance.scrollViewContent.name : "None")}";
    }
    
    #endregion
}
