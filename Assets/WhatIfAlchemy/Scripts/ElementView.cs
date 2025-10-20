using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 元素視圖控制器，負責處理元素的拖拽和碰撞檢測
/// </summary>
public class ElementView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("元素資訊")]
    [SerializeField] private string elementName = "Unknown Element";
    
    [Header("拖拽設定")]
    [SerializeField] private bool canDrag = true;
    [SerializeField] private bool returnToOriginalPosition = false;
    
    [Header("碰撞檢測")]
    [SerializeField] private float collisionRadius = 1f;
    [SerializeField] private LayerMask collisionLayerMask = -1;
    
    [Header("視覺回饋")]
    [SerializeField] private float dragScale = 1.1f;
    [SerializeField] private Color dragColor = Color.white;
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float hoverFadeTime = 0.2f;
    [SerializeField] private float overlapScale = 1.2f;
    [SerializeField] private float overlapFadeTime = 0.3f;
    
    
    // 私有變數
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Color originalColor;
    private Camera mainCamera;
    private SpriteRenderer spriteRenderer;
    private bool isDragging = false;
    private bool isHovering = false;
    private Collider2D[] collidersInRange;
    private Vector3 dragOffset;
    private int originalSortingOrder;
    private static int globalSortingOrder = 1000; // 全域排序層級計數器
    private ElementView lastOverlappedElement; // 記錄上次重疊的元素
    private bool isOverlapping = false; // 是否正在重疊
    
    // 事件
    public System.Action<ElementView, ElementView> OnElementCollision;
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        // 快取常用組件
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
        
        // 如果沒有找到主攝影機，嘗試查找
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        // 儲存原始狀態
        originalPosition = transform.position;
        originalScale = transform.localScale;
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            originalSortingOrder = spriteRenderer.sortingOrder;
        }
    }
    
    private void Start()
    {
        // 確保有 SpriteRenderer 組件
        if (spriteRenderer == null)
        {
            Debug.LogWarning($"ElementView on {gameObject.name} 缺少 SpriteRenderer 組件！");
        }
    }
    
    #endregion
    
    #region Drag Interface Implementation
    
    /// <summary>
    /// 開始拖拽時調用
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!canDrag) return;
        
        isDragging = true;
        
        // 計算拖拽偏移，讓物件保持在滑鼠點擊的相對位置
        Vector3 screenPosition = eventData.position;
        screenPosition.z = mainCamera.WorldToScreenPoint(transform.position).z;
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        dragOffset = transform.position - worldPosition;
        
        // 視覺回饋
        if (spriteRenderer != null)
        {
            spriteRenderer.color = dragColor;
            transform.localScale = originalScale * dragScale;
            spriteRenderer.sortingOrder = ++globalSortingOrder; // 拖拽時設定為最高排序層級
        }
        
        // 將物件移到最前面
        transform.SetAsLastSibling();
    }
    
    /// <summary>
    /// 拖拽過程中調用
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (!canDrag || !isDragging) return;
        
        // 將滑鼠位置轉換為世界座標
        Vector3 screenPosition = eventData.position;
        screenPosition.z = mainCamera.WorldToScreenPoint(transform.position).z;
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        
        // 應用拖拽偏移，讓物件保持在滑鼠點擊的相對位置
        transform.position = worldPosition + dragOffset;
        
        // 檢測碰撞
        CheckForCollisions();
    }
    
    /// <summary>
    /// 結束拖拽時調用
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        
        isDragging = false;
        
        // 檢查是否與其他元素重疊
        ElementView overlappedElement = CheckForOverlapOnEndDrag();
        
        // 如果與其他元素重疊，嘗試合併
        if (overlappedElement != null)
        {
            if (ElementMergeManager.TryMergeElements(this, overlappedElement))
            {
                return; // 合併成功，直接返回
            }
        }
        
        // 重置重疊狀態，並恢復被重疊物件的縮放
        if (lastOverlappedElement != null)
        {
            lastOverlappedElement.EndOverlapScaleFromOther();
            lastOverlappedElement = null;
        }
        
        if (isOverlapping)
        {
            isOverlapping = false;
        }
        
        // 恢復視覺狀態
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
            transform.localScale = originalScale;
            // 保持最高排序層級，不恢復原始層級
            spriteRenderer.sortingOrder = globalSortingOrder;
        }
        
        // 如果需要，返回原始位置
        if (returnToOriginalPosition)
        {
            transform.position = originalPosition;
        }
    }
    
    #endregion
    
    #region Hover Effects
    
    /// <summary>
    /// 滑鼠進入時調用
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isHovering || isDragging) return;
        
        isHovering = true;
        StartCoroutine(ScaleTo(originalScale * hoverScale, hoverFadeTime));
    }
    
    /// <summary>
    /// 滑鼠離開時調用
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isHovering || isDragging) return;
        
        isHovering = false;
        StartCoroutine(ScaleTo(originalScale, hoverFadeTime));
    }
    
    /// <summary>
    /// 縮放協程
    /// </summary>
    private System.Collections.IEnumerator ScaleTo(Vector3 targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        
        transform.localScale = targetScale;
    }
    
    #endregion
    
    #region Collision Detection
    
    /// <summary>
    /// 檢查拖拽結束時是否與其他元素重疊
    /// </summary>
    private ElementView CheckForOverlapOnEndDrag()
    {
        // 使用 OverlapCircle 檢測範圍內的碰撞體
        Collider2D[] collidersInRange = Physics2D.OverlapCircleAll(transform.position, collisionRadius, collisionLayerMask);
        
        foreach (Collider2D collider in collidersInRange)
        {
            // 跳過自己
            if (collider.gameObject == gameObject) continue;
            
            // 檢查是否有 ElementView 組件
            ElementView otherElement = collider.GetComponent<ElementView>();
            if (otherElement != null)
            {
                return otherElement;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// 檢測與其他元素的碰撞
    /// </summary>
    private void CheckForCollisions()
    {
        // 使用 OverlapCircle 檢測範圍內的碰撞體
        collidersInRange = Physics2D.OverlapCircleAll(transform.position, collisionRadius, collisionLayerMask);
        
        ElementView currentOverlappedElement = null;
        
        foreach (Collider2D collider in collidersInRange)
        {
            // 跳過自己
            if (collider.gameObject == gameObject) continue;
            
            // 檢查是否有 ElementView 組件
            ElementView otherElement = collider.GetComponent<ElementView>();
            if (otherElement != null)
            {
                currentOverlappedElement = otherElement;
                
                // 觸發碰撞事件
                OnElementCollision?.Invoke(this, otherElement);
                
                // 可以選擇是否在碰撞時停止檢測
                // break;
            }
        }
        
        // 檢查重疊狀態變化
        CheckOverlapStateChange(currentOverlappedElement);
    }
    
    /// <summary>
    /// 檢查重疊狀態變化並輸出 Debug Log
    /// </summary>
    private void CheckOverlapStateChange(ElementView currentOverlappedElement)
    {
        // 如果當前重疊的元素與上次不同
        if (currentOverlappedElement != lastOverlappedElement)
        {
            // 如果離開了之前重疊的元素，先結束其縮放效果
            if (lastOverlappedElement != null)
            {
                Debug.Log($"[{elementName}] 離開重疊 [{lastOverlappedElement.elementName}]");
                lastOverlappedElement.EndOverlapScaleFromOther();
            }
            
            // 如果開始重疊新的元素，對其進行縮放
            if (currentOverlappedElement != null)
            {
                Debug.Log($"[{elementName}] 開始重疊到 [{currentOverlappedElement.elementName}]");
                currentOverlappedElement.StartOverlapScaleFromOther();
            }
            
            // 更新記錄的重疊元素
            lastOverlappedElement = currentOverlappedElement;
        }
    }
    
    /// <summary>
    /// 開始重疊縮放效果
    /// </summary>
    private void StartOverlapScale()
    {
        if (isDragging) // 只有在拖拽時才進行縮放
        {
            StartCoroutine(ScaleTo(originalScale * overlapScale, overlapFadeTime));
        }
    }
    
    /// <summary>
    /// 結束重疊縮放效果
    /// </summary>
    private void EndOverlapScale()
    {
        if (isDragging) // 只有在拖拽時才進行縮放
        {
            StartCoroutine(ScaleTo(originalScale * dragScale, overlapFadeTime));
        }
    }
    
    /// <summary>
    /// 被其他物件重疊時開始縮放效果
    /// </summary>
    public void StartOverlapScaleFromOther()
    {
        if (!isOverlapping)
        {
            isOverlapping = true;
            StartCoroutine(ScaleTo(originalScale * overlapScale, overlapFadeTime));
        }
    }
    
    /// <summary>
    /// 被其他物件重疊時結束縮放效果
    /// </summary>
    public void EndOverlapScaleFromOther()
    {
        if (isOverlapping)
        {
            isOverlapping = false;
            StartCoroutine(ScaleTo(originalScale, overlapFadeTime));
        }
    }
    
    /// <summary>
    /// 手動觸發碰撞檢測（供外部調用）
    /// </summary>
    public void TriggerCollisionCheck()
    {
        CheckForCollisions();
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// 設定是否可以拖拽
    /// </summary>
    public void SetCanDrag(bool canDrag)
    {
        this.canDrag = canDrag;
    }
    
    /// <summary>
    /// 設定碰撞半徑
    /// </summary>
    public void SetCollisionRadius(float radius)
    {
        collisionRadius = radius;
    }
    
    /// <summary>
    /// 重置到原始位置
    /// </summary>
    public void ResetToOriginalPosition()
    {
        transform.position = originalPosition;
    }
    
    /// <summary>
    /// 設定原始位置
    /// </summary>
    public void SetOriginalPosition(Vector3 position)
    {
        originalPosition = position;
    }
    
    /// <summary>
    /// 設定元素名稱
    /// </summary>
    public void SetElementName(string name)
    {
        elementName = name;
    }
    
    /// <summary>
    /// 獲取元素名稱
    /// </summary>
    public string GetElementName()
    {
        return elementName;
    }
    
    /// <summary>
    /// 設定重疊縮放倍數
    /// </summary>
    public void SetOverlapScale(float scale)
    {
        overlapScale = scale;
    }
    
    /// <summary>
    /// 設定重疊縮放時間
    /// </summary>
    public void SetOverlapFadeTime(float fadeTime)
    {
        overlapFadeTime = fadeTime;
    }
    
    /// <summary>
    /// 手動觸發重疊縮放效果
    /// </summary>
    public void TriggerOverlapScale()
    {
        if (isDragging && !isOverlapping)
        {
            isOverlapping = true;
            StartOverlapScale();
        }
    }
    
    /// <summary>
    /// 手動結束重疊縮放效果
    /// </summary>
    public void EndOverlapScaleManually()
    {
        if (isOverlapping)
        {
            isOverlapping = false;
            EndOverlapScale();
        }
    }
    
    #endregion
    
    #region Gizmos
    
    /// <summary>
    /// 在 Scene 視圖中繪製碰撞範圍
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collisionRadius);
    }
    
    #endregion
}
