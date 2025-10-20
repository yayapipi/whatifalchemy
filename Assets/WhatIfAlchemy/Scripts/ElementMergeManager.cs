using UnityEngine;
using UnityEngine.UI;

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
    
    /// <summary>
    /// 創建合併後的新元素
    /// </summary>
    private ElementView CreateMergedElement(Vector3 position)
    {
        // 創建新元素
        GameObject newElement = Instantiate(mergedElementPrefab, position, Quaternion.identity);
        
        // 獲取 ElementView 組件並初始化
        ElementView newElementView = newElement.GetComponent<ElementView>();
        if (newElementView != null)
        {
            // 設定元素名稱
            newElementView.SetElementName(mergedElementName);
            
            // 設定 Sprite
            if (mergedElementSprite != null)
            {
                SpriteRenderer newSpriteRenderer = newElementView.GetComponent<SpriteRenderer>();
                if (newSpriteRenderer != null)
                {
                    newSpriteRenderer.sprite = mergedElementSprite;
                }
            }
            
            Debug.Log($"[ElementMergeManager] 創建了新的合併元素: {mergedElementName}");
        }
        else
        {
            Debug.LogWarning("[ElementMergeManager] 合併元素預製體沒有 ElementView 組件！");
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
