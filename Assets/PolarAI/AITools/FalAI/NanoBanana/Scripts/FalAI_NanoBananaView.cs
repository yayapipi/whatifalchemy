using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YFrame.Runtime.Utility;

namespace PolarAI.Scripts.AICore.FalAI.NanoBanana
{
    public enum NanoBananaMode
    {
        Image,
        Url,
        TextOnly
    }
    
    public class FalAI_NanoBananaView : MonoBehaviour
    {
        public NanoBananaMode Mode;
        
        public List<Image> InputImages;
        public List<string> ImageUrls;
        public Text OutputText;
        public TMP_InputField InputPrompt;
        public Image OutputImage;
        
        public Button SendBtn;
        public FalAI_NanoBanana FalAI_NanoBanana;

        private void Start()
        {
            SendBtn.onClick.AddListener(() =>
            {
                if (Mode == NanoBananaMode.TextOnly)
                {
                    GenImage();
                }
                else
                {
                    EditImage();
                }
            });
        }

        private void EditImage()
        {
            var imageUrls = new List<string>();
            if (Mode == NanoBananaMode.Image)
            {
                foreach (var image in InputImages)
                {
                    imageUrls.Add(image.sprite.ToBase64DataUri());
                }
            }else if (Mode == NanoBananaMode.Url)
            {
                imageUrls.AddRange(ImageUrls);
            }
            
            FalAI_NanoBanana.EditImg(InputPrompt.text, imageUrls, (url, info) =>
            {
                Debug.Log(url);
                Debug.Log(info);
                SpritesUtility.DownloadImageFromURL(url, bytes =>
                {
                    OutputImage.sprite = bytes.ToSprite();
                });
                OutputText.text = info;
            });
        }

        private void GenImage()
        {
            FalAI_NanoBanana.GenImg(InputPrompt.text, (url, info) =>
            {
                Debug.Log(url);
                Debug.Log(info);
                SpritesUtility.DownloadImageFromURL(url, bytes =>
                {
                    OutputImage.sprite = bytes.ToSprite();
                });
                OutputText.text = info;
            });
            
        }
    }
}