# 相機拖拽控制器使用說明

## 功能概述

`CameraDragController` 是一個 Unity 腳本，提供以下功能：

1. **滑鼠拖拽相機移動** - 點擊背景拖拽相機
2. **滾輪縮放 FOV** - 使用滑鼠滾輪調整相機視野
3. **邊界限制** - 可選的相機移動範圍限制
4. **平滑移動** - 可選的平滑過渡效果

## 安裝步驟

1. 將 `CameraDragController.cs` 腳本添加到你的相機物件上
2. 在 Inspector 中調整各項設定
3. 確保場景中有 EventSystem（用於 UI 檢測）

## 設定參數

### 拖拽設定
- **Enable Drag**: 啟用拖拽功能
- **Drag Sensitivity**: 拖拽靈敏度（數值越大越敏感）
- **Invert Drag X/Y**: 反轉 X/Y 軸拖拽方向

### 縮放設定
- **Enable Zoom**: 啟用縮放功能
- **Zoom Sensitivity**: 縮放靈敏度
- **Min/Max FOV**: FOV 的最小和最大值限制
- **Invert Zoom**: 反轉縮放方向

### 邊界限制
- **Enable Bounds**: 啟用邊界限制
- **Min/Max Bounds**: 相機移動的最小和最大範圍

### 平滑設定
- **Enable Smoothing**: 啟用平滑移動
- **Smoothing Speed**: 平滑移動速度

## 使用方法

### 基本使用
1. 將腳本添加到相機上
2. 調整 Inspector 中的參數
3. 運行遊戲，點擊背景拖拽相機
4. 使用滑鼠滾輪縮放視野

### 程式化控制
```csharp
// 獲取相機拖拽控制器
CameraDragController dragController = Camera.main.GetComponent<CameraDragController>();

// 設定相機位置
dragController.SetCameraPosition(new Vector3(10, 5, -10));

// 設定相機 FOV
dragController.SetCameraFOV(45f);

// 設定拖拽靈敏度
dragController.SetDragSensitivity(3f);

// 設定 FOV 範圍
dragController.SetFOVRange(20f, 80f);

// 設定邊界限制
dragController.SetBounds(new Vector2(-50, -50), new Vector2(50, 50));

// 重置相機
dragController.ResetCamera();
```

## 注意事項

1. **UI 檢測**: 腳本會自動檢測滑鼠是否在 UI 上，避免在點擊 UI 時拖拽相機
2. **性能優化**: 使用 `Time.deltaTime` 確保幀率無關的平滑移動
3. **邊界檢查**: 啟用邊界限制時，相機移動會被限制在指定範圍內
4. **FOV 限制**: FOV 會被限制在 1-179 度之間，確保相機正常工作

## 除錯功能

啟用 **Show Debug Info** 可以在 Console 中看到拖拽和縮放的詳細資訊，方便除錯和調整參數。

## 自定義擴展

你可以通過繼承 `CameraDragController` 類別來添加自定義功能：

```csharp
public class CustomCameraController : CameraDragController
{
    protected override void HandleInput()
    {
        // 添加自定義輸入處理
        base.HandleInput();
    }
}
```

## 常見問題

**Q: 為什麼拖拽沒有反應？**
A: 檢查是否啟用了 `Enable Drag`，以及滑鼠是否點擊在 UI 上。

**Q: 縮放太快或太慢？**
A: 調整 `Zoom Sensitivity` 參數。

**Q: 相機移動不平滑？**
A: 啟用 `Enable Smoothing` 並調整 `Smoothing Speed`。

**Q: 如何限制相機移動範圍？**
A: 啟用 `Enable Bounds` 並設定 `Min/Max Bounds` 參數。
