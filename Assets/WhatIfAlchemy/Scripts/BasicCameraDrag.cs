using UnityEngine;

/// <summary>
/// 基礎相機拖拽控制器，使用傳統 Input 系統
/// </summary>
public class BasicCameraDrag : MonoBehaviour
{
    [Header("拖拽設定")]
    [SerializeField] private bool enableDrag = true;
    [SerializeField] private float dragSensitivity = 2f;
    [SerializeField] private bool invertDragX = false;
    [SerializeField] private bool invertDragY = false;
    
    [Header("除錯")]
    [SerializeField] private bool showDebugInfo = true;
    
    // 私有變數
    private Camera targetCamera;
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
                Debug.LogError("BasicCameraDrag: 找不到相機組件！");
                enabled = false;
                return;
            }
        }
        
        Debug.Log("BasicCameraDrag 初始化完成");
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
        
        // 使用傳統 Input 系統
        bool mousePressed = Input.GetMouseButton(0);
        
        if (showDebugInfo && mousePressed != isDragging)
        {
            Debug.Log($"滑鼠狀態: {mousePressed}, 拖拽中: {isDragging}");
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            StartDrag();
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            UpdateDrag();
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
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
        lastMousePosition = Input.mousePosition;
        dragStartPosition = transform.position;
        
        Debug.Log("開始拖拽相機");
    }
    
    /// <summary>
    /// 更新拖拽
    /// </summary>
    private void UpdateDrag()
    {
        Vector3 currentMousePosition = Input.mousePosition;
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
        
        if (showDebugInfo)
        {
            Debug.Log($"拖拽中 - 位置: {transform.position}");
        }
    }
    
    /// <summary>
    /// 結束拖拽
    /// </summary>
    private void EndDrag()
    {
        isDragging = false;
        
        Debug.Log("結束拖拽相機");
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
