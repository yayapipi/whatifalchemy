using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 元素按鈕控制器，負責處理 UI 按鈕的懸停顯示和點擊生成功能
/// </summary>
public class ElementButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI 組件")]
    [SerializeField] private Button button;
    [SerializeField] private Image buttonImage;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("懸停顯示設定")]
    [SerializeField] private GameObject hoverDisplayObject;
    [SerializeField] private bool enableHoverDisplay = true;
    [SerializeField] private Vector3 hoverOffset = Vector3.zero;
    [SerializeField] private float hoverScale = 1.2f;
    [SerializeField] private float hoverFadeTime = 0.2f;
    
    [Header("生成設定")]
    [SerializeField] private GameObject elementViewPrefab;
    [SerializeField] private Transform spawnParent;
    [SerializeField] private Vector3 spawnOffset = Vector3.zero;
    [SerializeField] private bool autoStartDrag = true;
    [SerializeField] private bool generateOnDrag = true;
    
    [Header("拖拽視覺效果")]
    [SerializeField] private float dragTransparency = 0.5f;
    [SerializeField] private float dragFadeTime = 0.2f;
    
    
    // 私有變數
    private Vector3 originalHoverScale;
    private CanvasGroup hoverCanvasGroup;
    private bool isHovering = false;
    private Camera mainCamera;
    private ElementView currentDraggedElement;
    private bool isDragging = false;
    private float originalAlpha;
    
    // 事件
    public System.Action<ElementView> OnElementSpawned;
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        // 快取組件
        if (button == null)
            button = GetComponent<Button>();
        if (buttonImage == null)
            buttonImage = GetComponent<Image>();
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        
        mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = FindObjectOfType<Camera>();
        
        // 儲存原始透明度
        if (canvasGroup != null)
        {
            originalAlpha = canvasGroup.alpha;
        }
        
        // 設定懸停顯示物件
        SetupHoverDisplay();
    }
    
    private void Start()
    {
        // 確保有按鈕組件
        if (button == null)
        {
            Debug.LogWarning($"ElementButtonController on {gameObject.name} 缺少 Button 組件！");
        }
        
        // 確保有預製體
        if (elementViewPrefab == null)
        {
            Debug.LogWarning($"ElementButtonController on {gameObject.name} 沒有設定 elementViewPrefab！");
        }
        
        // 設定生成父物件
        if (spawnParent == null)
        {
            spawnParent = transform.parent;
        }
    }
    
    #endregion
    
    #region Hover Display Setup
    
    /// <summary>
    /// 設定懸停顯示物件
    /// </summary>
    private void SetupHoverDisplay()
    {
        if (hoverDisplayObject == null) return;
        
        // 儲存原始縮放
        originalHoverScale = hoverDisplayObject.transform.localScale;
        
        // 設定初始狀態
        hoverDisplayObject.SetActive(false);
        
        // 添加 CanvasGroup 用於淡入淡出
        hoverCanvasGroup = hoverDisplayObject.GetComponent<CanvasGroup>();
        if (hoverCanvasGroup == null)
        {
            hoverCanvasGroup = hoverDisplayObject.AddComponent<CanvasGroup>();
        }
        
        hoverCanvasGroup.alpha = 0f;
    }
    
    #endregion
    
    #region Pointer Event Handlers
    
    /// <summary>
    /// 滑鼠進入按鈕時調用
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isHovering || !enableHoverDisplay) return;
        
        isHovering = true;
        ShowHoverDisplay();
    }
    
    /// <summary>
    /// 滑鼠離開按鈕時調用
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isHovering) return;
        
        isHovering = false;
        HideHoverDisplay();
    }
    
    /// <summary>
    /// 點擊按鈕時調用
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!generateOnDrag) // 只有在不啟用拖拽生成時才點擊生成
        {
            SpawnElementView(eventData);
        }
    }
    
    /// <summary>
    /// 開始拖拽按鈕時調用
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!generateOnDrag) return;
        
        isDragging = true;
        
        // 開始半透明效果
        if (canvasGroup != null)
        {
            StartCoroutine(FadeCanvasGroup(canvasGroup, originalAlpha, dragTransparency, dragFadeTime));
        }
        
        currentDraggedElement = SpawnElementView(eventData);
    }
    
    /// <summary>
    /// 拖拽按鈕過程中調用
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || currentDraggedElement == null) return;
        
        // 將拖拽事件傳遞給生成的元素
        currentDraggedElement.OnDrag(eventData);
    }
    
    /// <summary>
    /// 結束拖拽按鈕時調用
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging || currentDraggedElement == null) return;
        
        isDragging = false;
        
        // 恢復透明度
        if (canvasGroup != null)
        {
            StartCoroutine(FadeCanvasGroup(canvasGroup, dragTransparency, originalAlpha, dragFadeTime));
        }
        
        // 將拖拽結束事件傳遞給生成的元素
        currentDraggedElement.OnEndDrag(eventData);
        
        currentDraggedElement = null;
    }
    
    #endregion
    
    #region Hover Display Methods
    
    /// <summary>
    /// 顯示懸停物件
    /// </summary>
    private void ShowHoverDisplay()
    {
        if (hoverDisplayObject == null) return;
        
        hoverDisplayObject.SetActive(true);
        
        // 設定位置
        Vector3 worldPosition = transform.position + hoverOffset;
        hoverDisplayObject.transform.position = worldPosition;
        
        // 設定縮放
        hoverDisplayObject.transform.localScale = originalHoverScale * hoverScale;
        
        // 淡入效果
        StartCoroutine(FadeCanvasGroup(hoverCanvasGroup, 0f, 1f, hoverFadeTime));
    }
    
    /// <summary>
    /// 隱藏懸停物件
    /// </summary>
    private void HideHoverDisplay()
    {
        if (hoverDisplayObject == null) return;
        
        // 淡出效果
        StartCoroutine(FadeCanvasGroup(hoverCanvasGroup, 1f, 0f, hoverFadeTime, () =>
        {
            hoverDisplayObject.SetActive(false);
        }));
    }
    
    /// <summary>
    /// 淡入淡出協程
    /// </summary>
    private System.Collections.IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float from, float to, float duration, System.Action onComplete = null)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            canvasGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }
        
        canvasGroup.alpha = to;
        onComplete?.Invoke();
    }
    
    #endregion
    
    #region Spawn Methods
    
    /// <summary>
    /// 生成 ElementView 預製體
    /// </summary>
    private ElementView SpawnElementView(PointerEventData eventData)
    {
        // 檢查預製體
        if (elementViewPrefab == null)
        {
            Debug.LogError("沒有設定 elementViewPrefab！");
            return null;
        }
        
        // 計算生成位置（滑鼠位置）
        Vector3 spawnPosition = CalculateSpawnPosition(eventData);
        
        // 生成預製體
        GameObject spawnedObject = Instantiate(elementViewPrefab, spawnPosition, Quaternion.identity, spawnParent);
        
        // 獲取 ElementView 組件
        ElementView elementView = spawnedObject.GetComponent<ElementView>();
        if (elementView == null)
        {
            Debug.LogWarning($"生成的物件 {spawnedObject.name} 沒有 ElementView 組件！");
            return null;
        }
        
        // 自動開始拖拽
        if (autoStartDrag)
        {
            StartAutoDrag(elementView, eventData);
        }
        
        // 觸發事件
        OnElementSpawned?.Invoke(elementView);
        
        Debug.Log($"生成了新的元素: {spawnedObject.name}");
        return elementView;
    }
    
    /// <summary>
    /// 計算生成位置（滑鼠位置）
    /// </summary>
    private Vector3 CalculateSpawnPosition(PointerEventData eventData)
    {
        // 使用 PointerEventData 獲取滑鼠位置
        Vector3 mousePosition = eventData.position;
        
        // 如果有攝影機，將滑鼠位置轉換為世界座標
        if (mainCamera != null)
        {
            // 使用攝影機的遠近裁剪平面來計算正確的 Z 軸距離
            float distance = mainCamera.WorldToScreenPoint(Vector3.zero).z;
            mousePosition.z = distance;
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
            worldPosition.z = 0f; // 確保 Z 軸為 0
            return worldPosition + spawnOffset;
        }
        
        // 如果沒有攝影機，使用滑鼠位置加上偏移
        return mousePosition + spawnOffset;
    }
    
    /// <summary>
    /// 開始自動拖拽
    /// </summary>
    private void StartAutoDrag(ElementView elementView, PointerEventData eventData)
    {
        // 模擬滑鼠點擊事件來開始拖拽
        StartCoroutine(SimulateDragStart(elementView, eventData));
    }
    
    /// <summary>
    /// 模擬拖拽開始的協程
    /// </summary>
    private System.Collections.IEnumerator SimulateDragStart(ElementView elementView, PointerEventData eventData)
    {
        // 等待一幀確保物件完全初始化
        yield return null;
        
        // 使用傳入的 PointerEventData，確保位置正確
        elementView.OnBeginDrag(eventData);
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// 設定懸停顯示物件
    /// </summary>
    public void SetHoverDisplayObject(GameObject displayObject)
    {
        hoverDisplayObject = displayObject;
        SetupHoverDisplay();
    }
    
    /// <summary>
    /// 設定元素預製體
    /// </summary>
    public void SetElementViewPrefab(GameObject prefab)
    {
        elementViewPrefab = prefab;
    }
    
    /// <summary>
    /// 設定懸停顯示開關
    /// </summary>
    public void SetEnableHoverDisplay(bool enable)
    {
        enableHoverDisplay = enable;
        
        // 如果關閉懸停顯示且正在懸停，則隱藏顯示物件
        if (!enable && isHovering)
        {
            isHovering = false;
            HideHoverDisplay();
        }
    }
    
    /// <summary>
    /// 設定拖拽透明度
    /// </summary>
    public void SetDragTransparency(float transparency)
    {
        dragTransparency = Mathf.Clamp01(transparency);
    }
    
    /// <summary>
    /// 設定拖拽淡入淡出時間
    /// </summary>
    public void SetDragFadeTime(float fadeTime)
    {
        dragFadeTime = Mathf.Max(0f, fadeTime);
    }
    
    /// <summary>
    /// 手動生成元素
    /// </summary>
    public void ManualSpawnElement()
    {
        // 創建一個模擬的 PointerEventData，使用當前滑鼠位置
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Vector2.zero, // 使用螢幕中心位置
            delta = Vector2.zero,
            scrollDelta = Vector2.zero,
            button = PointerEventData.InputButton.Left
        };
        
        // 如果有攝影機，使用攝影機中心位置
        if (mainCamera != null)
        {
            eventData.position = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        }
        
        SpawnElementView(eventData);
    }
    
    #endregion
}
