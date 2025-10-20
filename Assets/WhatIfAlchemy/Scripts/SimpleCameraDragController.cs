using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 簡化版相機拖拽控制器，不依賴 EventSystem
/// </summary>
public class SimpleCameraDragController : MonoBehaviour
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
    
    [Header("除錯")]
    [SerializeField] private bool showDebugInfo = false;
    
    // 私有變數
    private Camera targetCamera;
    private Mouse mouse;
    private bool isDragging = false;
    private Vector3 lastMousePosition;
    private Vector3 dragStartPosition;
    
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
                Debug.LogError("SimpleCameraDragController: 找不到相機組件！");
                enabled = false;
                return;
            }
        }
        
        // 初始化 Input System
        mouse = Mouse.current;
        if (mouse == null)
        {
            Debug.LogError("SimpleCameraDragController: 找不到 Mouse 輸入設備！");
            enabled = false;
            return;
        }
    }
    
    private void Update()
    {
        HandleInput();
    }
    
    #endregion
    
    #region Input Handling
    
    /// <summary>
    /// 處理輸入事件
    /// </summary>
    private void HandleInput()
    {
        if (!enableDrag) return;
        
        bool mousePressed = mouse.leftButton.isPressed;
        
        if (showDebugInfo && mousePressed != isDragging)
        {
            Debug.Log($"滑鼠狀態: {mousePressed}, 拖拽中: {isDragging}");
        }
        
        if (mousePressed && !isDragging)
        {
            StartDrag();
        }
        else if (mousePressed && isDragging)
        {
            UpdateDrag();
        }
        else if (!mousePressed && isDragging)
        {
            EndDrag();
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
        
        if (showDebugInfo)
        {
            Debug.Log("開始拖拽相機");
        }
    }
    
    /// <summary>
    /// 更新拖拽
    /// </summary>
    private void UpdateDrag()
    {
        Vector3 currentMousePosition = mouse.position.ReadValue();
        Vector3 mouseDelta = currentMousePosition - lastMousePosition;
        
        // 應用拖拽靈敏度和反轉設定
        float deltaX = mouseDelta.x * dragSensitivity * 0.01f;
        float deltaY = mouseDelta.y * dragSensitivity * 0.01f;
        
        if (invertDragX) deltaX = -deltaX;
        if (invertDragY) deltaY = -deltaY;
        
        // 計算新的位置並直接更新相機位置
        Vector3 newPosition = dragStartPosition;
        newPosition.x -= deltaX;
        newPosition.y -= deltaY;
        
        transform.position = newPosition;
        lastMousePosition = currentMousePosition;
    }
    
    /// <summary>
    /// 結束拖拽
    /// </summary>
    private void EndDrag()
    {
        isDragging = false;
        
        if (showDebugInfo)
        {
            Debug.Log("結束拖拽相機");
        }
    }
    
    #endregion
    
    #region Public Methods
    
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
    /// 設定拖拽靈敏度
    /// </summary>
    public void SetDragSensitivity(float sensitivity)
    {
        dragSensitivity = Mathf.Max(0.1f, sensitivity);
    }
    
    #endregion
}
