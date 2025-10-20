using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 測試用相機拖拽控制器
/// </summary>
public class TestCameraDrag : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] private float dragSensitivity = 5f;
    [SerializeField] private bool showDebugInfo = true;
    
    private Mouse mouse;
    private bool isDragging = false;
    private Vector3 lastMousePosition;
    
    private void Start()
    {
        mouse = Mouse.current;
        if (mouse == null)
        {
            Debug.LogError("找不到 Mouse 輸入設備！");
            enabled = false;
            return;
        }
        
        Debug.Log("TestCameraDrag 初始化完成");
    }
    
    private void Update()
    {
        if (mouse == null) return;
        
        bool mousePressed = mouse.leftButton.isPressed;
        
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
    
    private void StartDrag()
    {
        isDragging = true;
        lastMousePosition = mouse.position.ReadValue();
        Debug.Log("開始拖拽");
    }
    
    private void UpdateDrag()
    {
        Vector3 currentMousePosition = mouse.position.ReadValue();
        Vector3 mouseDelta = currentMousePosition - lastMousePosition;
        
        if (mouseDelta.magnitude > 0.1f) // 只有移動足夠大時才處理
        {
            Vector3 newPosition = transform.position;
            newPosition.x -= mouseDelta.x * dragSensitivity * 0.01f;
            newPosition.y -= mouseDelta.y * dragSensitivity * 0.01f;
            
            transform.position = newPosition;
            
            if (showDebugInfo)
            {
                Debug.Log($"拖拽: {mouseDelta}, 新位置: {transform.position}");
            }
        }
        
        lastMousePosition = currentMousePosition;
    }
    
    private void EndDrag()
    {
        isDragging = false;
        Debug.Log("結束拖拽");
    }
}
