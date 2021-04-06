using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDK3.Components.Video;
using VRC.SDK3.Video.Components.Base;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using VRC.Udon;

public class VideoPlayerPanel : UdonSharpBehaviour
{

    public VideoController videoController;

    [Header("Video Control UI")]

    [Tooltip("Object Owner Text Field")]
    public Text textOwner;

    [Tooltip("Error Text Field")]
    public Text textError;

    [Tooltip("Timestamp Text Field")]
    public Text textTimestamp;

    [Tooltip("About Text Field")]
    public Text textAbout;

    [Tooltip("URL Input Field")]
    public VRCUrlInputField URLField;

    [Tooltip("World Master Lock Toggle")]
    public Toggle toggleMasterLock;

    [Tooltip("Timestamp Lock Toggle")]
    public Toggle toggleTimestampLock;

    [Tooltip("Volume Lock Toggle")]
    public Toggle toggleVolumeLock;

    [Tooltip("Brightness Lock Toggle")]
    public Toggle toggleBrightnessLock;
    
    [Tooltip("Video Brightness Slider")]
    public Slider sliderBrightness;

    [Tooltip("Timestamp Slider")]
    public Slider sliderTimestamp;

    [Tooltip("Video Volume Slider")]
    public Slider sliderVolume;

    [Tooltip("About Canvas")]
    public GameObject canvasAbout;

    [Tooltip("Play/Pause Image Field")]
    public Image imagePlayPause;

    [Tooltip("Mute Image Field")]
    public Image imageMute;

    [Tooltip("Loading Image Field")]
    public Image imageLoading;

    [Tooltip("Sprite for the Pause Button")]
    public Sprite spritePause;

    [Tooltip("Sprite for the Play Button")]
    public Sprite spritePlay;

    [Tooltip("Sprite for the Locked Button")]
    public Sprite spriteLocked;

    [Tooltip("Sprite for the Unlocked button")]
    public Sprite spriteUnlocked;

    [Tooltip("Sprite for the Mute Button")]
    public Sprite spriteMute;

    [Tooltip("Sprite for the Unmute button")]
    public Sprite spriteUnmute;

    [Header("DO NOT MODIFY")]

    public bool isDragging = false;
    public bool isBrightnessDragging = false;
    public bool isVolumeDragging = false;
    public bool loadThis = false;
    public bool masterLockClicked = false;
    private bool isTimestampDragging = false;

    private bool volumeLocked = false;
    private bool brightnessLocked = true;
    private bool timestampLocked = true;
    private string prevUrl = "";

    void Start()
    {




        if(toggleVolumeLock != null)
        {
            volumeLocked = toggleVolumeLock.isOn;
        }

        if (toggleBrightnessLock != null)
        {
            brightnessLocked = toggleBrightnessLock.isOn;
        }
        if (sliderBrightness != null)
        {
            sliderBrightness.interactable = !brightnessLocked;
        }
        if (sliderVolume != null)
        {
            sliderVolume.interactable = !volumeLocked;
        }

    }


    public void SetVideoPanel(VideoController controller)
    {
        videoController = controller;
    }

    public bool HasVideoPanel()
    {
        if(videoController == null)
        {
            return false;
        }
        return true;
    }

    public bool GetTimestampDragging()
    {
        return isTimestampDragging;
    }

    public void SetDraggingTrue()
    {
        isDragging = true;
    }

    public void SetDraggingFalse()
    {
        Debug.Log("Setting dragging to false..");
        isDragging = false;
        if (isTimestampDragging)
        {
            videoController.SetTimestampSliders();
            isTimestampDragging = false;
        }
    }

    public void SetVolume()
    {
        if (isDragging && sliderVolume != null)
        {
            if (sliderVolume.interactable)
            {
                videoController.SetVolume(sliderVolume.value);
            }
        }
    }

    public void SetBrightness()
    {
        if (isDragging && sliderBrightness != null)
        {
            if (sliderBrightness.interactable)
            {
                videoController.SetBrightness(sliderBrightness.value);
            }
        }
    }

    public void TogglePlaying()
    {
        videoController.TogglePlaying();
    }

    public void ToggleMute()
    {
        videoController.ToggleMute();
    }

    public void ToggleMasterLock()
    {
        videoController.ToggleMasterLock();
    }

    public void SetTimestamp()
    {
        //Debug.Log("Set timestamp");
        //Debug.Log("Dragging: " + isDragging);
        //Debug.Log("Null check: "+(sliderTimestamp == null));
        if (isDragging && sliderTimestamp != null)
        {
            //Debug.Log("Is dragging true");
            if (sliderTimestamp.interactable)
            {
                isTimestampDragging = true;
                videoController.SetTimestamp(sliderTimestamp.value);
            }
        }
        //Debug.Log("Done setting timestamp");
    }

    public void SetMasterLock(bool locked)
    {
        if(toggleMasterLock != null)
        {
            toggleMasterLock.isOn = locked;
        }
    }

    public void setMuteToggle(bool state)
    {
        if (imageMute != null && spriteMute != null && spriteUnmute != null)
        {
            if (state)
            {
                imageMute.sprite = spriteMute;
            }
            else
            {
                imageMute.sprite = spriteUnmute;
            }
        }
        
    }

    public void SetPlayPause(bool state)
    {
        if (imagePlayPause != null && spritePause != null && spritePlay != null)
        {
            if (state)
            {
                imagePlayPause.sprite = spritePause;
            }
            else
            {
                imagePlayPause.sprite = spritePlay;
            }
        }
    }

    public void LoadUrl()
    {
        videoController.LoadUrl(URLField.GetUrl());
    }

    public void TakeOwnership()
    {
        videoController.TakeOwnership();
    }
    public void ForceSync()
    {
        videoController.ForceSync();
    }




    public void ToggleVolumeLock()
    {
        volumeLocked = toggleVolumeLock.isOn;
        sliderVolume.interactable = !volumeLocked;
    }

    public void ToggleBrightnessLock()
    {
        brightnessLocked = toggleBrightnessLock.isOn;
        sliderBrightness.interactable = !brightnessLocked;
    }




    public void ToggleTimestampLock()
    {
        if (Networking.IsOwner(videoController.gameObject))
        {
            timestampLocked = toggleTimestampLock.isOn;
            sliderTimestamp.interactable = !timestampLocked;
        }
        else
        {
            LockTimestamp();
        }
    }

    public void LockTimestamp()
    {
        timestampLocked = true;
        toggleTimestampLock.isOn = timestampLocked;
        sliderTimestamp.interactable = !timestampLocked;
    }




    public void PlayPause()
    {
        videoController.SendCustomEvent("PlayPause");
    }
    public void LoadVideo()
    {
        videoController.SendCustomEvent("LoadVideo");
    }


    





    

    public void ToggleAbout()
    {
        canvasAbout.SetActive(!canvasAbout.activeSelf);
        if (canvasAbout.activeSelf)
        {
            textAbout.text = "X";
        }
        else
        {
            textAbout.text = "?";
        }
    }

}
