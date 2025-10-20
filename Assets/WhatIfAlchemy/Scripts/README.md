# ElementView 拖拽合併系統

## 概述
這是一個類似 Little Alchemy 2 的元素拖拽合併系統，支援拖拽元素並與其他元素合併。

## 腳本說明

### ElementView.cs
掛在每個可拖拽的元素物件上，負責處理：
- 拖拽功能（IBeginDragHandler, IDragHandler, IEndDragHandler）
- 合併檢測和執行
- 視覺回饋（透明度、縮放、顏色變化）

#### 主要功能：
1. **拖拽系統**：支援滑鼠拖拽元素
2. **合併檢測**：當兩個元素靠近時自動檢測
3. **合併執行**：移除原元素，創建新元素
4. **視覺回饋**：拖拽時透明度變化，可合併時縮放效果

#### Inspector 設定：
- `dragAlpha`：拖拽時的透明度
- `snapDistance`：合併檢測距離
- `combineEffectPrefab`：合併特效預製體
- `combineEffectDuration`：特效持續時間

### ElementManager.cs
場景管理器，負責：
- 生成初始元素
- 管理元素位置
- 提供測試功能

#### 主要功能：
1. **元素生成**：自動生成隨機元素
2. **位置管理**：避免元素重疊
3. **測試工具**：清空、重新生成元素

#### Inspector 設定：
- `elementPrefab`：元素預製體
- `elementContainer`：元素容器
- `initialElementCount`：初始元素數量
- `spawnArea`：生成區域大小
- `minDistance`：元素間最小距離

## 使用方式

### 1. 基本設定
1. 將 `ElementView.cs` 掛在要拖拽的物件上
2. 確保物件有 `Collider2D` 組件
3. 將 `ElementManager.cs` 掛在場景中的空物件上

### 2. 創建預製體
1. 創建一個 GameObject
2. 添加 SpriteRenderer 和 Collider2D
3. 掛上 ElementView 腳本
4. 儲存為 Prefab

### 3. 測試功能
- 在 ElementManager 的 Inspector 中點擊 "清空所有元素" 或 "重新生成元素"
- 直接拖拽元素進行合併測試

## 未來擴展

### 資料庫整合
- 在 `CombineElements` 方法中整合 Supabase 資料庫
- 查詢現有合併規則
- 儲存新的合併結果

### AI 生成
- 當合併結果不存在於資料庫時
- 調用 Google AI 生成新元素
- 將新元素加入資料庫

### 視覺改進
- 添加更豐富的合併特效
- 實現元素動畫
- 添加音效

## 注意事項

1. 確保場景中有 Camera 和 EventSystem
2. 元素需要有 Collider2D 才能被拖拽
3. 合併距離可以根據需要調整
4. 使用 Unity 內建的協程系統進行動畫，無需額外依賴
