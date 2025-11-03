using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatIfAlchemy.Scripts
{
    public class ElementDestroyButton : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI設定")] [SerializeField] private Image buttonImage;

        [Header("視覺回饋")] [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color hoverColor = Color.red;
        [SerializeField] private Color dropZoneColor = Color.yellow;
        [SerializeField] private float colorTransitionSpeed = 5f;

        [Header("效果設定")] [SerializeField] private bool showVisualFeedback = true;

        [Header("音效設定")] [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip destroySound;

        // 私有變數
        private Color currentTargetColor;
        private bool isElementHovering = false;
        private RectTransform rectTransform;

        // 事件
        public System.Action<ElementView> OnElementDestroyed;

        #region Unity Lifecycle

        private void Awake()
        {
            // 快取組件參考
            if (buttonImage == null)
                buttonImage = GetComponent<Image>();

            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();

            rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            // 初始化顏色
            currentTargetColor = normalColor;
            if (buttonImage != null && showVisualFeedback)
            {
                buttonImage.color = normalColor;
            }
        }

        private void Update()
        {
            if (!showVisualFeedback) return;

            // 平滑顏色過渡（只在需要時進行）
            if (buttonImage != null && buttonImage.color != currentTargetColor)
            {
                buttonImage.color = Color.Lerp(buttonImage.color, currentTargetColor,
                    colorTransitionSpeed * Time.deltaTime);
            }

            // 檢測元素懸停狀態變化
            CheckElementHoverState();
        }

        #endregion

        #region Drop Interface Implementation

        /// <summary>
        /// 當物件被放置到此UI上時調用
        /// </summary>
        public void OnDrop(PointerEventData eventData)
        {
            // 檢查拖拽的物件是否是ElementView
            GameObject draggedObject = eventData.pointerDrag;
            if (draggedObject == null) return;

            ElementView elementView = draggedObject.GetComponent<ElementView>();
            if (elementView == null) return;

            // 銷毀元素（移除確認步驟）
            DestroyElement(elementView);

            // 重置視覺狀態
            ResetVisualState();
        }

        #endregion

        #region Hover Effects

        /// <summary>
        /// 滑鼠進入時調用
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isElementHovering)
            {
                currentTargetColor = hoverColor;
            }
        }

        /// <summary>
        /// 滑鼠離開時調用
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isElementHovering)
            {
                currentTargetColor = normalColor;
            }
        }

        #endregion

        #region Element Detection

        /// <summary>
        /// 檢測元素懸停狀態變化
        /// </summary>
        private void CheckElementHoverState()
        {
            bool elementCurrentlyHovering = IsElementHoveringOverThis();

            if (elementCurrentlyHovering != isElementHovering)
            {
                isElementHovering = elementCurrentlyHovering;
                currentTargetColor = isElementHovering ? dropZoneColor : normalColor;
            }
        }

        /// <summary>
        /// 檢查是否有ElementView懸停在此UI上方（優化版本）
        /// </summary>
        private bool IsElementHoveringOverThis()
        {
            // 快速檢查 - 如果EventSystem為空直接返回
            if (EventSystem.current?.currentSelectedGameObject == null) return false;

            GameObject currentDraggedObject = EventSystem.current.currentSelectedGameObject;
            ElementView draggedElement = currentDraggedObject.GetComponent<ElementView>();
            if (draggedElement == null) return false;

            // 使用快取的rectTransform避免重複GetComponent調用
            if (rectTransform == null) return false;

            // 檢查滑鼠是否在此UI範圍內
            return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, null);
        }

        #endregion

        #region Element Destruction

        /// <summary>
        /// 銷毀元素（優化版本）
        /// </summary>
        private void DestroyElement(ElementView elementView)
        {
            if (elementView == null) return;

            string elementName = elementView.GetElementName();

            // 播放銷毀音效
            if (audioSource != null && destroySound != null)
            {
                audioSource.PlayOneShot(destroySound);
            }

            // 觸發銷毀事件
            OnElementDestroyed?.Invoke(elementView);

            // 銷毀物件
            Destroy(elementView.gameObject);

            Debug.Log($"[ElementDestroyButton] 已銷毀元素: {elementName}");
        }

        #endregion

        #region Visual State Management

        /// <summary>
        /// 重置視覺狀態
        /// </summary>
        private void ResetVisualState()
        {
            isElementHovering = false;
            currentTargetColor = normalColor;
        }

        /// <summary>
        /// 設定視覺回饋是否啟用
        /// </summary>
        public void SetVisualFeedback(bool enabled)
        {
            showVisualFeedback = enabled;
            if (!enabled && buttonImage != null)
            {
                buttonImage.color = normalColor;
            }
        }

        /// <summary>
        /// 設定顏色配置
        /// </summary>
        public void SetColors(Color normal, Color hover, Color dropZone)
        {
            normalColor = normal;
            hoverColor = hover;
            dropZoneColor = dropZone;
            currentTargetColor = normalColor;
        }

        #endregion
    }
}