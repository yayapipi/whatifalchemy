using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using YFrame.Runtime.Utility;

namespace PolarAI.Scripts.AICore.FalAI.Sora
{
    public enum SoraMode
    {
        Image,
        Url,
        TextOnly
    }

    public class FalAI_SoraView : MonoBehaviour
    {
        public SoraMode Mode;
        public string AspectRatio = "16:9";
        public int Duration = 4;
        public VideoPlayer videoPlayer;
        public Image InputImage;
        public string ImageUrl;
        public TMP_InputField InputPrompt;
        public Button SendBtn;
        public FalAI_Sora FalAI_Sora;


        private void Start()
        {
            SendBtn.onClick.AddListener(() =>
            {
                if (Mode == SoraMode.TextOnly)
                {
                    TextToVideo();
                }
                else
                {
                    ImageToVideo();
                }
            });
        }

        private void ImageToVideo()
        { 
            string imageUrls = Mode == SoraMode.Image?InputImage.sprite.ToBase64DataUri(): ImageUrl;
            FalAI_Sora.ImageToVideo(imageUrls, InputPrompt.text, AspectRatio, Duration, url => 
            {
               videoPlayer.url = url;
               videoPlayer.Play(); 
            });
        }

        private void TextToVideo()
        {
            FalAI_Sora.TextToVideo(InputPrompt.text, AspectRatio, Duration, (url) => 
            {
                videoPlayer.url = url;
                videoPlayer.Play(); 
            });
        }
    }
}