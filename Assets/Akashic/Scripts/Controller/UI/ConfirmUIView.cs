using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Akashic.Scripts.Controller.UI
{
    public class ConfirmUIView : MonoBehaviour
    {
        public GameObject ConfirmPanel;
        public Animator ConfirmPanelAnimator;
        public TMP_Text Title;
        public TMP_Text Content;
        public Button ConfirmBtn;
        public Button CancelBtn;
        
        public static ConfirmUIView Instance;
        
        private void Awake()
        {
            Instance = this;
        }
        
        public void Open(string title, string content, Action onConfirm, Action onCancel)
        {
            Title.text = title;
            Content.text = content;
            ConfirmPanel.SetActive(true);
            ConfirmPanelAnimator.Play("OpenConfirmPanel");
            ConfirmBtn.onClick.AddListener(()=>
            {
                onConfirm?.Invoke();
                Close();
            });
            CancelBtn.onClick.AddListener(()=>
            {
                onCancel?.Invoke();
                Close();
            });
        }
        
        public void Close()
        {
            ConfirmPanelAnimator.Play("CloseConfirmPanel");
            ConfirmBtn.onClick.RemoveAllListeners();
            CancelBtn.onClick.RemoveAllListeners();
            StartCoroutine(WaitClose());
        }
        
        private IEnumerator WaitClose()
        {
            yield return new WaitForSeconds(.35f);
            ConfirmPanel.SetActive(false);
        }
        
    }
}