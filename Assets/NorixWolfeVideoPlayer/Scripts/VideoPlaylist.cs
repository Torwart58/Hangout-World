using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDK3.Components.Video;
using VRC.SDK3.Video.Components.Base;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using VRC.Udon;

public class VideoPlaylist : UdonSharpBehaviour
{
    public UdonSharpBehaviour videoController;
    public Text textObject;
    public VRCUrlInputField newInput;
    public Button submitButton;
    public Button nextButton;
    public VRCUrlInputField emptyInput;
    public Toggle toggleAddQueue;


    public VRCUrl[] urls = new VRCUrl[100];
    private bool syncing = false;
    private bool initializing = true;
    private bool canSync = false;
    private double syncTimer = 0;
    private bool isOwner = false;
    private bool tryOwnButton = false;
    private int removeFirstVideoQueue = 0;
    private bool waitingForUrlLength = false;
    private bool localLocked = false;

    private VRCUrl videoSyncingLocal = VRCUrl.Empty;
    private int urlSyncingLocal = -1;
    private int urlLengthLocal = 0;

    [UdonSynced] VRCUrl videoSyncing;
    [UdonSynced] int urlSyncing;
    [UdonSynced] int urlLength;
    [UdonSynced] bool canSubmitUrl;
    [UdonSynced] bool addSongLock;

    public VRCUrl nextVideo = VRCUrl.Empty;


    void Start()
    {
        videoSyncingLocal = emptyInput.GetUrl();
        videoSyncing = emptyInput.GetUrl();
        SetNextVideo();
        if (Networking.IsOwner(gameObject))
        {
            addSongLock = false;
            isOwner = true;
            videoSyncing = VRCUrl.Empty;
            urlSyncing = -1;
            initializing = false;
            canSubmitUrl = true;
            //initialize here.
            SetUrlLength();
            Debug.Log("URL LENGTH: "+urlLength);
            SetPlaylistText();
            Debug.Log("Finished settings up the object");
        }
        else
        {
            // request url sync
            for(int i = 0; i < urls.Length; i++)
            {
                urls[i] = emptyInput.GetUrl();
            }
            SyncUrls();
        }

    }

    public void SetPlaylistText()
    {
        string tempText = "";
        for (var i = 0; i < urlLength; i++)
        {
            if(urls[i] != null)
            {
                tempText += (i+1)+" - "+urls[i].Get() + " \n";
            }
        }
        textObject.text = tempText;
    }

    public void ToggleAddQueue()
    {
        if (Networking.IsMaster)
        {
            addSongLock = !addSongLock;
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        toggleAddQueue.isOn = addSongLock;
    }

    public void AddUrl()
    {

        if (addSongLock)
        {
            if (Networking.IsMaster)
            {
                AddingUrl();
            }
        }
        else
        {
            AddingUrl();
        }
    }

    private void AddingUrl()
    {
        if (newInput.GetUrl().Get() != "" && urlSyncing == -1 && syncTimer == 0)
        {
            Debug.Log("Can submit url: " + canSubmitUrl);
            if (canSubmitUrl)
            {
                if (Networking.IsOwner(gameObject))
                {
                    Debug.Log("Is already owner");
                    urlLength++;
                    urlLengthLocal = urlLength;
                    urls[urlLength - 1] = newInput.GetUrl();
                    urlSyncing = urlLength - 1;
                    videoSyncing = urls[urlLength - 1];
                    Debug.Log("This is the Video it's trying to sync: " + videoSyncing.Get());
                    newInput.SetUrl(VRCUrl.Empty);
                    SetPlaylistText();
                    syncTimer = 5;
                    SetNextVideo();
                    SetCanSubmitUrl(false);
                }
                else
                {
                    Debug.Log("Set owner as self");
                    Networking.SetOwner(Networking.LocalPlayer, gameObject);
                    tryOwnButton = true;
                    syncTimer = 1f;
                }
            }

        }
    }

    public void StartSync()
    {
        syncing = true;
    }

    public void SyncUrls()
    {
        if (!Networking.IsOwner(gameObject))
        {
            Debug.Log("Requesting sync..");
            syncing = true;
            SendCustomNetworkEvent(NetworkEventTarget.Owner, "StartFetchSyncedUrl");
        }
    }

    public void CompleteSync()
    {
        bool didComplete = true;
        for (var i = 0; i < urlLength; i++)
        {
            if(urls[i] == null)
            {
                Debug.Log("url " + i + " was null: '" + urls[i] + "'");
                i = urlLength;
                didComplete = false;
            }else if(urls[i].Get() == "")
            {
                Debug.Log("url " + i + " was: '" + urls[i] + "'");
                i = urlLength;
                didComplete = false;
            }
        }
        if (didComplete)
        {
            Debug.Log("Completed!!!!!!!!");
            SendCustomNetworkEvent(NetworkEventTarget.Owner, "CompleteSyncedUrls");
            syncing = false;
            initializing = false;
            urlLengthLocal = urlLength;
            SetPlaylistText();
            if(removeFirstVideoQueue > 0)
            {
                RemoveFirstEntry();
            }
        }
        else
        {
            if (!Networking.IsOwner(gameObject))
            {
                syncing = true;
                SendCustomNetworkEvent(NetworkEventTarget.Owner, "ForceFetchSyncedUrl");
            }
        }
    }

    private void SetCanSubmitUrl(bool can)
    {
        if (Networking.IsOwner(gameObject))
        {
            Debug.Log("Setting the can submit to: " + can);
            Debug.Log("And the current URL to sync is: " + videoSyncing.Get());
            canSubmitUrl = can;
            if (canSubmitUrl)
            {
                SendCustomNetworkEvent(NetworkEventTarget.All, "UnlockButton");
                UnlockButton();
                if (removeFirstVideoQueue > 0)
                {
                    RemoveFirstEntry();
                }
            }
            else
            {
                SendCustomNetworkEvent(NetworkEventTarget.All, "LockButton");
                LockButton();
            }
        }
    }

    public void StartFetchSyncedUrl()
    {
        Debug.Log("STARTING THE FETCH SYNECD URLS");
        if (Networking.IsOwner(gameObject))
        {
            if (urlLength > 0 && urlSyncing == -1)
            {
                urlSyncing = 0;
                videoSyncing = urls[urlSyncing];
                syncTimer = 10;
                SetCanSubmitUrl(false);
            }
            else if(urlSyncing == -1)
            {
                urlSyncing = 0;
                videoSyncing = urls[urlSyncing];
                syncTimer = 0.1f;
                SendCustomNetworkEvent(NetworkEventTarget.All, "CompleteEmptyPlaylist");
            }
        }
    }

    public void CompleteEmptyPlaylist()
    {
        if (initializing)
        {
            CompleteSync();
        }
    }

    public void ForceFetchSyncedUrl()
    {
        if (Networking.IsOwner(gameObject))
        {
            if (urlLength > 0)
            {
                urlSyncing = 0;
                videoSyncing = urls[urlSyncing];
                syncTimer = 10;
                SetCanSubmitUrl(false);
            }
        }
    }

    public void FetchSyncedUrlOdd()
    {
        if (Networking.IsOwner(gameObject))
        {
            if (urlSyncing + 1 < urlLength && urlSyncing % 2 != 0)
            {
                urlSyncing++;
                videoSyncing = urls[urlSyncing];
                syncTimer = 10;
            }
        }
    }

    public void FetchSyncedUrlEven()
    {
        if (Networking.IsOwner(gameObject))
        {
            if (urlSyncing + 1 < urlLength && urlSyncing % 2 == 0)
            {
                urlSyncing++;
                videoSyncing = urls[urlSyncing];
                syncTimer = 10;
            }
        }
    }

    public void AdvancePlaylist()
    {
        if (Networking.IsOwner(gameObject))
        {
            //Do thing to advance the playlist here.
            if(urlLength > 0)
            {
                urlLength--;
                SendCustomNetworkEvent(NetworkEventTarget.All, "RemoveFirstEntry");
                SetNextVideo();
            }
        }
    }

    public void RemoveFirstEntry()
    {
        if (!initializing && canSubmitUrl)
        {
            if(removeFirstVideoQueue > 0)
            {
                removeFirstVideoQueue--;
            }
            for (var i = 0; i < urls.Length - 1; i++)
            {
                urls[i] = VRCUrl.Empty;
                if (urls[i + 1] != null)
                {
                    if (urls[i + 1].Get() != "")
                    {
                        urls[i] = urls[i + 1];
                    }
                }
                if (!Networking.IsOwner(gameObject))
                {
                    urlLengthLocal--;
                }
            }
            if (removeFirstVideoQueue > 0)
            {
                RemoveFirstEntry();
            }
            else
            {
                SetNextVideo();
            }
            SetPlaylistText();
        }
        else
        {
            removeFirstVideoQueue++;
        }
    }

    private void SetNextVideo()
    {
        //Set the next video to urls[0]
        nextVideo = urls[0];
        //nextVideo = VRCUrl.Empty;
    }

    public void CompleteSyncedUrls()
    {
        syncTimer = 2;
    }

    public void Update()
    {
        if(syncTimer > 0)
        {
            syncTimer -= Time.deltaTime;

            if (syncTimer <= 0)
            {
                syncTimer = 0;
                if (tryOwnButton)
                {
                    tryOwnButton = false;
                    if (Networking.IsOwner(gameObject))
                    {
                        Debug.Log("Adding url proper.");
                        AddUrl();
                    }
                }
                else
                {
                    if (Networking.IsOwner(gameObject))
                    {
                        urlSyncing = -1;
                        videoSyncing = VRCUrl.Empty;
                        SetCanSubmitUrl(true);
                    }
                    else
                    {
                        if (initializing)
                        {
                            SyncUrls();
                        }
                    }
                }

                
                
            }
                
        }
    }

    public override void OnDeserialization()
    {
        VariableChange();
    }
    
    public override void OnOwnershipTransferred()
    {
        
        if (Networking.IsOwner(gameObject))
        {
            isOwner = true;
        }
        else
        {
            isOwner = false;
        }
    }
    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (Networking.IsOwner(gameObject) && isOwner == false)
        {
            Debug.Log("You're the new owner!");
            SetUrlLength();
            syncTimer = 3;
            isOwner = true;
            tryOwnButton = false;
        }
    }

    
    public void NextItem()
    {
        videoController.SendCustomEvent("SkipPlaylist");
    }

    public void LockButton()
    {
        submitButton.interactable = false;
        nextButton.interactable = false;
    }

    public void UnlockButton()
    {
        submitButton.interactable = true;
        nextButton.interactable = true;
    }

    public void SetUrlLength()
    {
        bool isEnd = false;
        for(int i = 0; i < urls.Length; i++)
        {
            if(urls[i] == null)
            {
                isEnd = true;
            }
            else if (urls[i].Get() == null)
            {
                isEnd = true;
            }
            else if(urls[i].Get().Length == 0)
            {
                isEnd = true;
            }
            if (isEnd)
            {
                urlLength = i;
                urlLengthLocal = urlLength;
                i = urls.Length;
            }
            else
            {
                Debug.Log("Url " + i + " is '" + urls[i].Get() + "'");
            }
        }
    }

    public void VariableChange()
    {
        if (!Networking.IsOwner(gameObject))
        {
            if (urlLengthLocal > urlLength)
            {
                urlLengthLocal = urlLength;
            }
            if (urlSyncing == -1)
            {
                urlSyncingLocal = urlSyncing;
            }
            if (videoSyncingLocal.Get() != videoSyncing.Get() && urlSyncingLocal != urlSyncing)
            {
                Debug.Log("There was a difference in the urls!");
                if (initializing)
                {
                    if (urlSyncing == 0)
                    {
                        canSync = true;
                    }
                    if (canSync == true)
                    {
                        syncTimer = 10;
                        videoSyncingLocal = videoSyncing;
                        urlSyncingLocal = urlSyncing;
                        urls[urlSyncing] = videoSyncing;
                        Debug.Log("Sycing url #" + urlSyncing + " as '" + videoSyncing.Get() + "'");
                    }

                    if (urlSyncing + 1 >= urlLength)
                    {
                        if (canSync == true)
                        {
                            CompleteSync();
                        }
                        else
                        {
                            SyncUrls();
                        }
                    }
                    else
                    {
                        SetPlaylistText();
                        if (urlSyncing % 2 == 0)
                        {

                            SendCustomNetworkEvent(NetworkEventTarget.Owner, "FetchSyncedUrlEven");
                        }
                        else
                        {
                            SendCustomNetworkEvent(NetworkEventTarget.Owner, "FetchSyncedUrlOdd");
                        }
                    }
                }
                else
                {
                    Debug.Log("Current local length: " + urlLengthLocal + ", But it should be: " + urlLength+", and we're trying to sync: "+urlSyncing);
                    if (urlSyncing > urlLengthLocal-1)
                    {                    //Not initializing
                        Debug.Log("Current length is: " + urlLengthLocal);
                        Debug.Log("Current synced length is: " + urlLength);
                        Debug.Log("Current syncing is: " + urlSyncing);
                        Debug.Log("Current Video Syncing: " + videoSyncing);
                        videoSyncingLocal = videoSyncing;
                        urlSyncingLocal = urlSyncing;
                        urlLengthLocal = urlLength;
                        urls[urlSyncing] = videoSyncing;
                        SetPlaylistText();
                        SendCustomNetworkEvent(NetworkEventTarget.Owner, "CompleteSyncedUrls");
                    }
                }


            }
        }
        if (localLocked != addSongLock)
        {
            toggleAddQueue.isOn = addSongLock;
            localLocked = addSongLock;
        }
    }


}
