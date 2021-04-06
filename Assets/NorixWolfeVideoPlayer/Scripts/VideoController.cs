/* 
 * 
 * Code by NorixWolfe
 * 
 * 
 * 
 * 
 * 
 */
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDK3.Components.Video;
using VRC.SDK3.Video.Components.Base;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using VRC.Udon;

public class VideoController : UdonSharpBehaviour
{

    [UdonSynced] private VRCUrl syncedUrl = VRCUrl.Empty;
    [UdonSynced] private int URLSyncType = 0;
    [UdonSynced] private int videoLoadID = 0;
    [UdonSynced] private float videoTime = 0f;
    [UdonSynced] private float videoEnd = 0f;
    [UdonSynced] private double serverTimestamp = 0f;
    [UdonSynced] private bool isPlaying = true;
    [UdonSynced] private bool masterLocked = false;

    [Header("Video Objects")]

    [Tooltip("Full list of Speaker objects")]
    public AudioSource[] speakers;

    [Tooltip("Full list of Video Materials")]
    public Material[] screenMaterials;

    [Tooltip("Video Player Panels")]
    public VideoPlayerPanel[] videoPlayerPanels;

    private BaseVRCVideoPlayer videoPlayer;
    private int loadID = 0;
    private VRCUrl currentUrl = VRCUrl.Empty;
    private bool muted = false;
    private float brightness = 1f;
    private float volume = 0.5f;
    private int maxUrlLength = 80;
   // private float videoEnd = 0f;
    private bool syncingVideo = false;
    private bool localLocked = false;
    private int syncRequests = 0;
    private int localURLSyncType = 0;
    private bool requestingSync = false;
    private float curVideoTime = 0f;
    private float videoTimeout = 0f;
    private float syncTimeout = 0f;
    private bool buffering = false;
    private float bufferingTick = 0f;
    private float prevVideoTime = 0f;


    void Start()
    {
        videoPlayer = (BaseVRCVideoPlayer)gameObject.GetComponent(typeof(BaseVRCVideoPlayer));
        SetOwnerText();
        SetVolume(volume);
        for (int i = 0; i < videoPlayerPanels.Length; i++)
        {
            if(!videoPlayerPanels[i].HasVideoPanel())
            {
                videoPlayerPanels[i].SetVideoPanel(this);
            }
        }
    }

    //TODO: Replace this with the built-in tool
    private string SecondsToTimestamp(float time)
    {
        string timestamp = "";

        int seconds = (int)Mathf.Floor(time % 60);
        int minutes = (int)Mathf.Floor((time / 60) % 60);
        int hours = (int)Mathf.Floor((time / 60) / 60);
        string secondsString = "" + seconds;
        string minutesString = "" + minutes;
        string hoursString = "" + hours;

        if (seconds <= 0)
        {
            secondsString = "00";
        }
        else if (seconds > 0 && seconds < 10)
        {
            secondsString = "0" + seconds;
        }

        if (minutes <= 0)
        {
            minutesString = "00";
        }
        else if (minutes > 0 && minutes < 10)
        {
            minutesString = "0" + minutes;
        }

        timestamp = hoursString + ":" + minutesString + ":" + secondsString;
        return timestamp;
    }





    public void SetOwnerText()
    {
        for (int i = 0; i < videoPlayerPanels.Length; i++)
        {
            Text textOwner = videoPlayerPanels[i].textOwner;
            if (textOwner != null)
            {
                textOwner.text = "Owner: " + Networking.GetOwner(gameObject).displayName;
            }
        }
    }

    public bool IsMasterLocked()
    {
        if (masterLocked)
        {
            if (Networking.IsMaster)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else
        {
            return false;
        }
    }

    public void SetVolume(float vol)
    {
        volume = vol;
        for (int i = 0; i < videoPlayerPanels.Length; i++)
        {
            Slider sliderVolume = videoPlayerPanels[i].sliderVolume;
            if (sliderVolume != null)
            {
                sliderVolume.value = volume;
            }
        }

        for (int i = 0; i < speakers.Length; i++)
        {
            speakers[i].volume = (muted ? 0f : volume);
        }
    }

    public void SetBrightness(float bright)
    {
        brightness = bright;
        for (int i = 0; i < videoPlayerPanels.Length; i++)
        {
            Slider sliderBrightness = videoPlayerPanels[i].sliderBrightness;
            if (sliderBrightness != null)
            {
                sliderBrightness.value = brightness;
            }
        }

        for (var i = 0; i < screenMaterials.Length; i++)
        {
            screenMaterials[i].SetFloat("_Emission", brightness);
        }
    }

    public void SetTimestamp(float time)
    {
        if (Networking.IsOwner(gameObject) && !IsMasterLocked() && videoEnd >= 1f)
        {
            videoTime = time * videoEnd;
            Debug.Log(videoTime);
            videoPlayer.SetTime(videoTime);
        }
    }

    public void ToggleMute()
    {
        muted = !muted;
        for (int i = 0; i < videoPlayerPanels.Length; i++)
        {
            videoPlayerPanels[i].setMuteToggle(muted);
        }
        SetVolume(volume);
    }

    public void TogglePlaying()
    {
        if (Networking.IsOwner(gameObject))
        {
            if (videoPlayer.IsPlaying)
            {
                videoPlayer.Pause();
            }
            else
            {
                videoPlayer.Play();
            }
            isPlaying = videoPlayer.IsPlaying;
            SyncVideoTimestamp();
        }
        SetPlayButton();
    }

    public void SetPlayButton()
    {
        for (int i = 0; i < videoPlayerPanels.Length; i++)
        {
            videoPlayerPanels[i].SetPlayPause(videoPlayer.IsPlaying);
        }
    }

    public void ToggleMasterLock()
    {
        if (Networking.IsMaster)
        {
            masterLocked = !masterLocked;
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        for (int i = 0; i < videoPlayerPanels.Length; i++)
        {
            videoPlayerPanels[i].SetMasterLock(masterLocked);
        }
    }

    public void SyncVideoTimestamp()
    {
        if (videoEnd > 1f)
        {
            if (!Networking.IsOwner(gameObject))
            {
                Debug.Log("Video Time: " + videoTime);
                Debug.Log("Server Time: " + Networking.GetServerTimeInMilliseconds());
                Debug.Log("Video time calc'd: " + ((Networking.GetServerTimeInMilliseconds() - (videoTime * 1000f)) / 1000f));
                videoPlayer.SetTime(videoTime);
                if (isPlaying)
                {
                    videoPlayer.Play();
                }
                SetTimestampText();
            }
        }
        else if(videoEnd > 0f)
        {
            videoPlayer.SetTime(float.MaxValue);
            SetTimestampText();
        }
        syncingVideo = false;

    }



    public override void OnDeserialization()
    {
        VariableChange();
    }

    public void VariableChange()
    {
        if (!Networking.IsOwner(gameObject))
        {
            if (isPlaying != videoPlayer.IsPlaying)
            {
                if (isPlaying)
                {
                    videoPlayer.Play();
                }
                else
                {
                    videoPlayer.Pause();
                }
                SetPlayButton();
            }
            if (masterLocked != localLocked)
            {
                localLocked = masterLocked;
                ToggleMasterLock();
            }
            if (videoLoadID != loadID)
            {
                if(URLSyncType == 1 && requestingSync && syncedUrl.Get() != "")
                {
                    Debug.Log("URL Sync was 1! Setting video proper.");
                    requestingSync = false;
                    loadID = videoLoadID;
                    videoEnd = 0f;
                    currentUrl = syncedUrl;
                    Debug.Log("Current synced url is: "+syncedUrl.Get());
                    syncingVideo = true;
                    videoPlayer.SetTime(0);
                    SendCustomNetworkEvent(NetworkEventTarget.Owner, "VideoSyncComplete");
                    ReloadVideo();
                }else if(requestingSync == false)
                {
                    Debug.Log("Requesting a sync..");
                    SendCustomNetworkEvent(NetworkEventTarget.Owner, "RequestVideoSync");
                    requestingSync = true;
                    VariableChange();
                }
            }
        }

    }
    
    public void RequestVideoSync()
    {
        Debug.Log("User requested video sync.");
        if (Networking.IsOwner(gameObject) && (URLSyncType == 0 || URLSyncType == 1))
        {
            syncedUrl = currentUrl;
            URLSyncType = 1;
            syncRequests++;
            syncTimeout = 10;
        }
    }

    public void VideoSyncComplete()
    {
        if (Networking.IsOwner(gameObject))
        {
            if (URLSyncType == 1)
            {
                syncRequests--;
                if (syncRequests <= 0)
                {
                    syncTimeout = 0;
                    URLSyncType = 0;
                    syncedUrl = VRCUrl.Empty;
                }
                else
                {
                    syncTimeout = 10;
                }
            }
        }
    }

    public bool LoadUrl(VRCUrl url)
    {
        Debug.Log("Loading url");
        TakeOwnership();
        Debug.Log("URLSync Type: " + URLSyncType);
        Debug.Log("Is Owner: " + Networking.IsOwner(gameObject));
        Debug.Log("Is master locked: " + IsMasterLocked());
        if (Networking.IsOwner(gameObject) && !IsMasterLocked() && URLSyncType == 0)
        {
            Debug.Log("Is owner and we're good on url sync type");
            if (url.Get().Length <= maxUrlLength)
            {
                syncTimeout = 10;
                URLSyncType = 1;
                videoLoadID++;
                loadID = videoLoadID;
                videoEnd = 0f;
                videoTime = 0;
                syncedUrl = url;
                currentUrl = url;
                curVideoTime = 0;
                ReloadVideo();
            }
            else
            {
                ShowError("URL longer than " + maxUrlLength + " characters");
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    public void ReloadVideo()
    {
        videoPlayer.Stop();
        videoPlayer.LoadURL(currentUrl);
        if (isPlaying)
        {
            videoPlayer.Play();
        }
        SyncVideoTimestamp();
        SetPlayButton();
    }

    public void TakeOwnership()
    {
        if (!Networking.IsOwner(gameObject))
        {
            if (!masterLocked)
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }
        }
    }

    public void ShowError(string error)
    {
        Debug.Log("ERR:" + error);
        for (int i = 0; i < videoPlayerPanels.Length; i++)
        {
            Text textError = videoPlayerPanels[i].textError;
            if (textError != null)
            {
                textError.text = "Video Errors: " + error;
            }
        }
    }


    public override void OnOwnershipTransferred()
    {
        SetOwnerText();
        for (int i = 0; i < videoPlayerPanels.Length; i++)
        {
            videoPlayerPanels[i].LockTimestamp();
        }
        
        //Maybe the timestamp slider should be locked here
    }

    public void ForceSync()
    {
        if(currentUrl.Get() != "")
        {
            if (!Networking.IsOwner(gameObject) && !syncingVideo)
            {
                Debug.Log("Forcing a Sync");
                loadID = videoLoadID;
                videoEnd = 0f;
                videoPlayer.SetTime(0);
                ReloadVideo();
                SetPlayButton();
            }
            else
            {
                videoPlayer.Stop();
                videoPlayer.LoadURL(currentUrl);
                videoPlayer.SetTime(videoTime);
                SetPlayButton();
            }
        }
    }

    public void PlayVideo()
    {
        videoPlayer.Play();
        SetPlayButton();
        SyncVideoTimestamp();
    }

    public override void OnVideoReady()
    {
        Debug.Log("--------------------------Video Ready!!");
        PlayVideo();
    }


    public void SetTimestampText()
    {
        for (int i = 0; i < videoPlayerPanels.Length; i++)
        {
            if(videoEnd > 1 || videoEnd <= 0)
            {
                videoPlayerPanels[i].textTimestamp.text = SecondsToTimestamp(videoPlayer.GetTime()) + " / " + SecondsToTimestamp(videoEnd);
            }
            else
            {
                videoPlayerPanels[i].textTimestamp.text = "livestream";
            }
        }
            

    }

    public void Update()
    {
        if (videoPlayer.IsPlaying)
        {
            if (!Networking.IsOwner(gameObject))
            {
                if (Mathf.Abs(videoPlayer.GetTime() - (videoTime + (float)(Networking.GetServerTimeInSeconds() - serverTimestamp))) > 2 && videoEnd > 1)
                {
                    SyncVideoTimestamp();
                }
            }
            else if(videoEnd <= 0)
            {
                Debug.Log("Setting end time");
                videoPlayer.SetTime(float.MaxValue);
                videoEnd = videoPlayer.GetTime();
                if (videoEnd > 0 && videoEnd < 1)
                {
                    videoEnd = 0.1f;
                }
                else
                {
                    videoEnd = Mathf.Round(videoEnd * 10f) / 10f;
                }
                Debug.Log(videoEnd);
                SyncVideoTimestamp();
            }


            if(videoEnd > 0 && (curVideoTime - videoEnd) > 2 && !syncingVideo)
            {
                videoEnd = 0.1f;
                SetPlayButton();
            }
            if (videoEnd > 0 && !syncingVideo)
            {
                if (Mathf.Abs(videoPlayer.GetTime() - curVideoTime) > 1)
                {
                    curVideoTime = videoPlayer.GetTime();
                    SetTimestampText();
                }
            }

            if (Mathf.Abs((float)(serverTimestamp - Networking.GetServerTimeInSeconds())) > 1)
            {
                if (Networking.IsOwner(gameObject))
                {
                    videoTime = videoPlayer.GetTime();
                    serverTimestamp = Networking.GetServerTimeInSeconds();
                }
                SetTimestampSliders();
            }


        }
        if (videoTimeout > 0)
        {
            Debug.Log("START 6");
            videoTimeout -= Time.deltaTime;
            if (videoTimeout <= 0)
            {
                videoTimeout = 0;
                videoPlayer.LoadURL(currentUrl);
                Debug.Log("START 7");
            }
        }
        if (syncTimeout > 0)
        {
            Debug.Log("Sync TImeout 1");
            syncTimeout -= Time.deltaTime;
            if (syncTimeout <= 0)
            {
                syncTimeout = 0;
                VideoSyncComplete();
                Debug.Log("Sync TImeout 2");
            }
        }
        if (buffering)
        {
            for (int i = 0; i < videoPlayerPanels.Length; i++)
            {
                Image imageLoading = videoPlayerPanels[i].imageLoading;
                if (imageLoading != null)
                {
                    imageLoading.transform.Rotate(new Vector3(0.0f, 0.0f, -2f));
                }
            }
        }
        if (isPlaying)
        {

            if (videoPlayer.GetTime() == prevVideoTime)
            {
                bufferingTick+=Time.deltaTime;
                if (bufferingTick >= 1f && buffering == false)
                {
                    buffering = true;
                    for (int i = 0; i < videoPlayerPanels.Length; i++)
                    {
                        Image imageLoading = videoPlayerPanels[i].imageLoading;
                        if (imageLoading != null)
                        {
                            imageLoading.enabled = true;
                        }
                    }
                }
            }
            else
            {
                if (buffering)
                {
                    ShowError("");
                }
                buffering = false;
                bufferingTick = 0;
                for (int i = 0; i < videoPlayerPanels.Length; i++)
                {
                    Image imageLoading = videoPlayerPanels[i].imageLoading;
                    if (imageLoading != null)
                    {
                        imageLoading.enabled = false;
                    }
                }

            }
            prevVideoTime = videoPlayer.GetTime();
        }else if (buffering)
        {
            if (buffering)
            {
                ShowError("");
            }
            buffering = false;
            bufferingTick = 0;
            for (int i = 0; i < videoPlayerPanels.Length; i++)
            {
                Image imageLoading = videoPlayerPanels[i].imageLoading;
                if (imageLoading != null)
                {
                    imageLoading.enabled = false;
                }
            }
        }
    }

    public void SetTimestampSliders()
    {
        for (int i = 0; i < videoPlayerPanels.Length; i++)
        {
            Slider sliderTimestamp = videoPlayerPanels[i].sliderTimestamp;
            if (sliderTimestamp != null)
            {
                if (!videoPlayerPanels[i].GetTimestampDragging())
                {
                    sliderTimestamp.value = videoPlayer.GetTime() / videoEnd;
                }
            }
        }
    }
    public override void OnVideoEnd()
    {

    }

    public override void OnVideoLoop()
    {
        if (Networking.IsOwner(gameObject))
        {
            if (videoPlayer.Loop)
            {
                videoPlayer.SetTime(0);
            }
        }
    }



    //TODO add method to check if you're dragging the timestamp scrubber to make it not sync.

    public override void OnVideoError(VideoError videoError)
    {

        ShowError(videoError.ToString());

        if (videoError == VideoError.RateLimited)
        {
            videoTimeout = 4;
            if (!Networking.IsOwner(gameObject))
            {
                //loadID -= 1;
            }
        }

        if (videoError == VideoError.PlayerError)
        {
            videoTimeout = 6;
            if (!Networking.IsOwner(gameObject))
            {
                //loadID -= 1;
            }
        }

        if (videoError == VideoError.InvalidURL)
        {
            //Probably do Playlist stuff here.
        }

    }

}
