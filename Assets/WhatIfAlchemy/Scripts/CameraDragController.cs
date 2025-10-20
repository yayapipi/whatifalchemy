using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// 相機拖拽控制器，負責處理相機的右鍵拖拽移動和滾輪縮放功能
/// </summary>
public class CameraDragController : MonoBehaviour
{
    [Header("拖拽設定")]
    [SerializeField] private bool enableDrag = true;
    [SerializeField] private float dragSensitivity = 2f;
    [SerializeField] private bool invertDragX = false;
    [SerializeField] private bool invertDragY = false;
    
    [Header("縮放設定")]
    [SerializeField] private bool enableZoom = true;
    [SerializeField] private float zoomSensitivity = 5f;
    [SerializeField] private float minFOV = 10f;
    [SerializeField] private float maxFOV = 90f;
    [SerializeField] private bool invertZoom = false;
    
    
    [Header("平滑設定")]
    [SerializeField] private bool enableSmoothing = true;
    [SerializeField] private float smoothingSpeed = 5f;
    
    [Header("除錯")]
    [SerializeField] private bool showDebugInfo = false;
    
    // 私有變數
    private Camera targetCamera;
    private Vector3 lastMousePosition;
    private Vector3 targetPosition;
    private float targetFOV;
    private bool isDragging = false;
    private Vector3 dragStartPosition;
    private float dragStartFOV;
    private Vector3 dragOffset; // 拖拽偏移
    
    // 快取的組件
    private EventSystem eventSystem;
    
    // Input System 相關
    private Mouse mouse;
    private bool isMousePressed = false;
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        // 快取相機組件
        targetCamera = GetComponent<Camera>();
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                Debug.LogError("CameraDragController: 找不到相機組件！");
                enabled = false;
                return;
            }
        }
        
        // 快取 EventSystem
        eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            eventSystem = FindObjectOfType<EventSystem>();
        }
        
        // 初始化 Input System
        mouse = Mouse.current;
        if (mouse == null)
        {
            Debug.LogError("CameraDragController: 找不到 Mouse 輸入設備！");
            enabled = false;
            return;
        }
        
        Debug.Log("CameraDragController 初始化完成，Mouse 設備已找到");
        
        // 初始化目標位置和 FOV
        targetPosition = transform.position;
        targetFOV = targetCamera.fieldOfView;
    }
    
    private void Start()
    {
        // 確保 FOV 在合理範圍內
        targetFOV = Mathf.Clamp(targetFOV, minFOV, maxFOV);
        targetCamera.fieldOfView = targetFOV;
    }
    
    private void Update()
    {
        HandleInput();
        UpdateCamera();
        
        if (showDebugInfo)
        {
            ShowDebugInfo();
        }
    }
    
    #endregion
    
    #region Input Handling
    
    /// <summary>
    /// 處理輸入事件
    /// </summary>
    private void HandleInput()
    {
        // 檢查是否點擊在 UI 上
        if (IsPointerOverUI())
        {
            if (showDebugInfo)
            {
                Debug.Log("點擊在 UI 上，不處理相機拖拽");
            }
            return;
        }
        
        // 處理滑鼠拖拽
        if (enableDrag)
        {
            HandleDragInput();
        }
        
        // 處理滾輪縮放
        if (enableZoom)
        {
            HandleZoomInput();
        }
    }
    
    /// <summary>
    /// 處理拖拽輸入
    /// </summary>
    private void HandleDragInput()
    {
        if (mouse == null)
        {
            if (showDebugInfo)
            {
                Debug.LogError("Mouse 輸入設備為 null！");
            }
            return;
        }
        
        bool mousePressed = mouse.rightButton.isPressed; // 改為右鍵
        
        if (showDebugInfo && mousePressed != isMousePressed)
        {
            Debug.Log($"滑鼠右鍵狀態變化: {mousePressed}, 拖拽中: {isDragging}");
        }
        
        if (mousePressed && !isMousePressed)
        {
            StartDrag();
        }
        else if (mousePressed && isDragging)
        {
            UpdateDrag();
        }
        else if (!mousePressed && isMousePressed)
        {
            EndDrag();
        }
        
        isMousePressed = mousePressed;
    }
    
    /// <summary>
    /// 處理縮放輸入
    /// </summary>
    private void HandleZoomInput()
    {
        Vector2 scrollDelta = mouse.scroll.ReadValue();
        float scrollY = scrollDelta.y;
        
        if (Mathf.Abs(scrollY) > 0.01f)
        {
            float zoomDirection = invertZoom ? -scrollY : scrollY;
            float zoomAmount = zoomDirection * zoomSensitivity;
            
            targetFOV -= zoomAmount;
            targetFOV = Mathf.Clamp(targetFOV, minFOV, maxFOV);
            
            if (showDebugInfo)
            {
                Debug.Log($"縮放: {targetFOV:F1} (滾輪: {scrollY:F2})");
            }
        }
    }
    
    #endregion
    
    #region Drag Methods
    
    /// <summary>
    /// 開始拖拽
    /// </summary>
    private void StartDrag()
    {
        isDragging = true;
        lastMousePosition = mouse.position.ReadValue();
        dragStartPosition = transform.position;
        dragStartFOV = targetFOV;
        
        if (showDebugInfo)
        {
            Debug.Log($"開始右鍵拖拽相機 - 相機位置: {transform.position}, 滑鼠位置: {lastMousePosition}");
        }
    }
    
    /// <summary>
    /// 更新拖拽
    /// </summary>
    private void UpdateDrag()
    {
        Vector3 currentMousePosition = mouse.position.ReadValue();
        Vector3 mouseDelta = currentMousePosition - lastMousePosition;
        
        // 直接使用滑鼠移動量（基於 SimpleCameraDrag 的成功經驗）
        float deltaX = mouseDelta.x * dragSensitivity * 0.1f;
        float deltaY = mouseDelta.y * dragSensitivity * 0.1f;
        
        if (invertDragX) deltaX = -deltaX;
        if (invertDragY) deltaY = -deltaY;
        
        // 計算新的位置
        Vector3 newPosition = transform.position;
        newPosition.x -= deltaX;
        newPosition.y -= deltaY;
        
        // 直接更新相機位置
        transform.position = newPosition;
        targetPosition = newPosition;
        
        lastMousePosition = currentMousePosition;
        
        if (showDebugInfo)
        {
            Debug.Log($"右鍵拖拽中 - 滑鼠移動: {mouseDelta}, 相機位置: {transform.position}");
        }
    }
    
    /// <summary>
    /// 結束拖拽
    /// </summary>
    private void EndDrag()
    {
        isDragging = false;
        
        if (showDebugInfo)
        {
            Debug.Log("結束右鍵拖拽相機");
        }
    }
    
    #endregion
    
    #region Camera Update
    
    /// <summary>
    /// 更新相機位置和 FOV
    /// </summary>
    private void UpdateCamera()
    {
        // 只有在拖拽時才更新位置，避免相機回到原本位置
        if (isDragging)
        {
            // 更新位置
            if (enableSmoothing)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothingSpeed);
            }
            else
            {
                transform.position = targetPosition;
            }
        }
        
        // 更新 FOV
        if (enableSmoothing)
        {
            targetCamera.fieldOfView = Mathf.Lerp(targetCamera.fieldOfView, targetFOV, Time.deltaTime * smoothingSpeed);
        }
        else
        {
            targetCamera.fieldOfView = targetFOV;
        }
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// 將螢幕座標轉換為世界座標
    /// </summary>
    private Vector3 ScreenToWorldPoint(Vector3 screenPosition)
    {
        if (targetCamera == null) return Vector3.zero;
        
        // 設定 Z 軸距離，使用相機的遠近裁剪平面
        screenPosition.z = targetCamera.WorldToScreenPoint(transform.position).z;
        return targetCamera.ScreenToWorldPoint(screenPosition);
    }
    
    /// <summary>
    /// 檢查滑鼠是否在 UI 上
    /// </summary>
    private bool IsPointerOverUI()
    {
        if (eventSystem == null) return false;
        
        // 只檢查是否在 UI 上，暫時不檢查 ElementView
        return eventSystem.IsPointerOverGameObject();
    }
    
    /// <summary>
    /// 顯示除錯資訊
    /// </summary>
    private void ShowDebugInfo()
    {
        if (isDragging)
        {
            Debug.Log($"拖拽中 - 位置: {targetPosition}, FOV: {targetFOV:F1}");
        }
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// 設定相機位置
    /// </summary>
    public void SetCameraPosition(Vector3 position)
    {
        targetPosition = position;
        
        if (!enableSmoothing)
        {
            transform.position = position;
        }
    }
    
    /// <summary>
    /// 設定相機 FOV
    /// </summary>
    public void SetCameraFOV(float fov)
    {
        targetFOV = Mathf.Clamp(fov, minFOV, maxFOV);
        
        if (!enableSmoothing)
        {
            targetCamera.fieldOfView = targetFOV;
        }
    }
    
    /// <summary>
    /// 重置相機到初始位置
    /// </summary>
    public void ResetCamera()
    {
        targetPosition = Vector3.zero;
        targetFOV = 60f;
        targetFOV = Mathf.Clamp(targetFOV, minFOV, maxFOV);
    }
    
    /// <summary>
    /// 設定拖拽靈敏度
    /// </summary>
    public void SetDragSensitivity(float sensitivity)
    {
        dragSensitivity = Mathf.Max(0.1f, sensitivity);
    }
    
    /// <summary>
    /// 設定縮放靈敏度
    /// </summary>
    public void SetZoomSensitivity(float sensitivity)
    {
        zoomSensitivity = Mathf.Max(0.1f, sensitivity);
    }
    
    /// <summary>
    /// 設定 FOV 範圍
    /// </summary>
    public void SetFOVRange(float min, float max)
    {
        minFOV = Mathf.Max(1f, min);
        maxFOV = Mathf.Min(179f, max);
        
        // 確保當前 FOV 在範圍內
        targetFOV = Mathf.Clamp(targetFOV, minFOV, maxFOV);
    }
    
    
    /// <summary>
    /// 啟用或禁用拖拽功能
    /// </summary>
    public void SetDragEnabled(bool enabled)
    {
        enableDrag = enabled;
        
        if (!enabled && isDragging)
        {
            EndDrag();
        }
    }
    
    /// <summary>
    /// 啟用或禁用縮放功能
    /// </summary>
    public void SetZoomEnabled(bool enabled)
    {
        enableZoom = enabled;
    }
    
    /// <summary>
    /// 啟用或禁用平滑移動
    /// </summary>
    public void SetSmoothingEnabled(bool enabled)
    {
        enableSmoothing = enabled;
    }
    
    /// <summary>
    /// 設定平滑速度
    /// </summary>
    public void SetSmoothingSpeed(float speed)
    {
        smoothingSpeed = Mathf.Max(0.1f, speed);
    }
    
    #endregion
    
}
