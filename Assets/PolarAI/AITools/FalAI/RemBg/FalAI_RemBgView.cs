using UnityEngine;
using UnityEngine.UI;
using YFrame.Runtime.Utility;

namespace PolarAI.Scripts.AICore.FalAI.RemBg
{
    public enum RemoveBgMode
    {
        Image,
        Url
    }
    public class FalAI_RemBgView : MonoBehaviour
    {
        public RemoveBgMode Mode;
        public Image InputImage;
        public string ImageUrl;
        public Image OutputImage;
        public Button SendBtn;

        public FalAI_RemBg FalAI_RemBg;
        private void Start()
        {
            SendBtn.onClick.AddListener(() =>
            {
                var imgUrl = Mode == RemoveBgMode.Image?InputImage.sprite.ToBase64DataUri():ImageUrl;
                FalAI_RemBg.RemoveBg(imgUrl, (url) =>
                {
                    SpritesUtility.DownloadImageFromURL(url, bytes =>
                    {
                        OutputImage.sprite = bytes.ToSprite();
                    });
                });
            });
        }
    }
}