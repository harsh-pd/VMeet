using UnityEngine;
using UnityEngine.UI;

using agora_gaming_rtc;
using agora_utilities;
using Fordi.UI;
using Fordi.Common;
using Fordi.UI.MenuControl;
using Fordi.Core;
using Photon.Pun;
using System.Collections;
using System;
using System.Collections.Generic;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Network = Fordi.Networking.Network;


// this is an example of using Agora Unity SDK
// It demonstrates:
// How to enable video
// How to join/leave channel
// 

namespace Fordi.VideoCall
{
    public interface IVideoCallEngine
    {
        EventHandler<VideoEventArgs> VideoPauseToggle { get; set; }
        AgoraUserInfo[] Users { get; }
        bool VideoEnabled { get; }
        void Join(string v);
        int EnableVideo(bool val);
        void Leave();
    }

    public class VideoEventArgs : EventArgs
    {
        public bool Pause;
        public uint UserId;
    }

    public class AgoraUserInfo
    {
        public uint UserId;
        public string Name;
        public bool MicOn;
        public bool VideoOn;
    }


    public class VideoCallEngine : MonoBehaviour, IVideoCallEngine, IMatchmakingCallbacks, IOnEventCallback
    {

        // instance of agora engine
        private static IRtcEngine mRtcEngine;
        private IUIEngine m_uiEngine;
        private IExperienceMachine m_experienceMachine;

        private const string APP_ID = "397c1095001f4f88abe788a32dcd1570";

        public EventHandler<VideoEventArgs> VideoPauseToggle { get; set; }

        private static Dictionary<uint, AgoraUserInfo> m_users = new Dictionary<uint, AgoraUserInfo>();

        public AgoraUserInfo[] Users
        {
            get {
                AgoraUserInfo[] users = new AgoraUserInfo[m_users.Count];
                m_users.Values.CopyTo(users, 0);
                return users;
            }
        }

        public bool VideoEnabled {
            get
            {
                if (!m_users.ContainsKey(0))
                    return false;
                return m_users[0].VideoOn;
            }
        }

        private uint m_localPlayerId;

        private void Awake()
        {
            //m_uiEngine = IOC.Resolve<IUIEngine>();
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnDestroy()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        // load agora engine
        public void LoadEngine()
        {
            // start sdk
            Debug.Log("initializeEngine");

            if (mRtcEngine != null)
            {
                Debug.Log("Engine exists. Please unload it first!");
                return;
            }

            // init engine
            mRtcEngine = IRtcEngine.GetEngine(APP_ID);

            // enable log
            mRtcEngine.SetLogFilter(LOG_FILTER.DEBUG | LOG_FILTER.INFO | LOG_FILTER.WARNING | LOG_FILTER.ERROR | LOG_FILTER.CRITICAL);

            //mRtcEngine.SetVideoEncoderConfiguration(new VideoEncoderConfiguration()
            //{
            //    orientationMode = ORIENTATION_MODE.ORIENTATION_MODE_FIXED_LANDSCAPE
            //});
        }

        public void Join(string channel)
        {
            Debug.Log("calling join (channel = " + channel + ")");

            if (mRtcEngine == null)
                LoadEngine();

            // set callbacks (optional)
            mRtcEngine.OnJoinChannelSuccess = OnJoinChannelSuccess;
            mRtcEngine.OnUserJoined = OnUserJoined;
            mRtcEngine.OnUserOffline = OnUserOffline;

            // enable video
            //var result = mRtcEngine.EnableVideo();
            //VideoEnabled = result >= 0;
            // allow camera output callback
            mRtcEngine.EnableVideoObserver();

            // join channel
            mRtcEngine.JoinChannel(channel, null, 0);

            // Optional: if a data stream is required, here is a good place to create it
            int streamID = mRtcEngine.CreateDataStream(true, true);
            Debug.Log("initializeEngine done, data stream id = " + streamID);
        }

        public string GetSdkVersion()
        {
            string ver = IRtcEngine.GetSdkVersion();
            if (ver == "2.9.1.45")
            {
                ver = "2.9.2";  // A conversion for the current internal version#
            }
            else
            {
                if (ver == "2.9.1.46")
                {
                    ver = "2.9.2.2";  // A conversion for the current internal version#
                }
            }
            return ver;
        }

        public void Leave()
        {
            Debug.Log("calling leave");

            if (mRtcEngine == null)
                return;

            // leave channel
            mRtcEngine.LeaveChannel();
            // deregister video frame observers in native-c code
            mRtcEngine.DisableVideoObserver();

            UnloadEngine();
        }

        // unload agora engine
        public void UnloadEngine()
        {
            Debug.Log("calling unloadEngine");

            // delete
            if (mRtcEngine != null)
            {
                IRtcEngine.Destroy();  // Place this call in ApplicationQuit
                mRtcEngine = null;
            }
        }

        public int EnableVideo(bool pauseVideo)
        {
            if (mRtcEngine != null)
            {
                int result;

                if (!pauseVideo)
                {
                   result = mRtcEngine.EnableVideo();
                }
                else
                {
                   result = mRtcEngine.DisableVideo();
                }

                if (result >= 0)
                {
                    m_users[0].VideoOn = !pauseVideo;

                    VideoPauseToggle?.Invoke(this, new VideoEventArgs()
                    {
                        Pause = pauseVideo,
                        UserId = 0
                    });

                    if (PhotonNetwork.InRoom)
                    {
                        object[] content = new object[2];
                        content[0] = m_localPlayerId;
                        content[1] = !pauseVideo;
                        PhotonNetwork.RaiseEvent(Network.videoMuteToggle, content, new RaiseEventOptions() { Receivers = ReceiverGroup.Others }, new SendOptions { Reliability = true });
                        Debug.LogError("____videoMuteToggle event fired");
                    }
                }

                return result;
            }
            return -1;
        }

        //accessing GameObject in Scnene1
        //set video transform delegate for statically created GameObject
        public void SetupLocalVideo()
        {
            // Attach the SDK Script VideoSurface for video rendering
            GameObject quad = GameObject.Find("Quad");
            if (ReferenceEquals(quad, null))
            {
                Debug.Log("BBBB: failed to find Quad");
                return;
            }
            else
            {
                quad.AddComponent<VideoSurface>();
            }

            GameObject cube = GameObject.Find("Cube");
            if (ReferenceEquals(cube, null))
            {
                Debug.Log("BBBB: failed to find Cube");
                return;
            }
            else
            {
                cube.AddComponent<VideoSurface>();
            }
        }

        // implement engine callbacks
        private void OnJoinChannelSuccess(string channelName, uint uid, int elapsed)
        {
            Debug.Log("JoinChannelSuccessHandler: uid = " + uid);

            m_localPlayerId = uid;

            if (m_uiEngine == null)
                m_uiEngine = IOC.Resolve<IUIEngine>();

            m_users[0] = new AgoraUserInfo()
            {
                UserId = 0,
                Name = PhotonNetwork.NickName,
                VideoOn = false
            };

            //m_uiEngine.AddVideo(new MenuItemInfo()
            //{
            //    Data = new AgoraUserInfo()
            //    {
            //         UserId = 0,
            //         Name = PhotonNetwork.NickName
            //    },
            //});

            //m_uiEngine.PresentVideo(new MenuItemInfo()
            //{
            //    Data = new AgoraUserInfo()
            //    {
            //        UserId = 0,
            //        Name = PhotonNetwork.NickName
            //    },
            //});
        }

        // When a remote user joined, this delegate will be called. Typically
        // create a GameObject to render video on it
        private void OnUserJoined(uint uid, int elapsed)
        {
            //Debug.Log("onUserJoined: uid = " + uid + " elapsed = " + elapsed);
            //// this is called in main thread

            //// find a game object to render video stream from 'uid'
            //GameObject go = GameObject.Find(uid.ToString());
            //if (!ReferenceEquals(go, null))
            //{
            //    return; // reuse
            //}

            //// create a GameObject and assign to this new user
            //VideoSurface videoSurface = MakeImageSurface(uid.ToString());
            //if (!ReferenceEquals(videoSurface, null))
            //{
            //    // configure videoSurface
            //    videoSurface.SetForUser(uid);
            //    videoSurface.SetEnable(true);
            //    videoSurface.SetVideoSurfaceType(AgoraVideoSurfaceType.RawImage);
            //    videoSurface.SetGameFps(30);
            //}

            m_users[uid] = new AgoraUserInfo
            {
                UserId = uid,
                VideoOn = true,
                MicOn = true
            };

            if (m_uiEngine == null)
                m_uiEngine = IOC.Resolve<IUIEngine>();

            m_uiEngine.AddVideo(new MenuItemInfo()
            {
                Data = new AgoraUserInfo()
                {
                    UserId = uid,
                    Name = "User: " + uid
                },
            });

            m_uiEngine.PresentVideo(new MenuItemInfo()
            {
                Data = new AgoraUserInfo()
                {
                    UserId = uid,
                    Name = "User: " + uid
                },
            });
        }

        public VideoSurface MakePlaneSurface(string goName)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Plane);

            if (go == null)
            {
                return null;
            }
            go.name = goName;
            // set up transform
            go.transform.Rotate(-90.0f, 0.0f, 0.0f);
            float yPos = UnityEngine.Random.Range(3.0f, 5.0f);
            float xPos = UnityEngine.Random.Range(-2.0f, 2.0f);
            go.transform.position = new Vector3(xPos, yPos, 0f);
            go.transform.localScale = new Vector3(0.25f, 0.5f, .5f);

            // configure videoSurface
            VideoSurface videoSurface = go.AddComponent<VideoSurface>();
            return videoSurface;
        }

        private const float Offset = 100;
        public VideoSurface MakeImageSurface(string goName)
        {
            GameObject go = new GameObject();

            if (go == null)
            {
                return null;
            }

            go.name = goName;

            // to be renderered onto
            go.AddComponent<RawImage>();

            // make the object draggable
            go.AddComponent<UIElementDragger>();
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas != null)
            {
                go.transform.parent = canvas.transform;
            }
            // set up transform
            go.transform.Rotate(0f, 0.0f, 180.0f);
            float xPos = UnityEngine.Random.Range(Offset - Screen.width / 2f, Screen.width / 2f - Offset);
            float yPos = UnityEngine.Random.Range(Offset, Screen.height / 2f - Offset);
            go.transform.localPosition = new Vector3(xPos, yPos, 0f);
            go.transform.localScale = new Vector3(3f, 4f, 1f);

            // configure videoSurface
            VideoSurface videoSurface = go.AddComponent<VideoSurface>();
            return videoSurface;
        }
        // When remote user is offline, this delegate will be called. Typically
        // delete the GameObject for this user
        private void OnUserOffline(uint uid, USER_OFFLINE_REASON reason)
        {
            //// remove video stream
            //Debug.Log("onUserOffline: uid = " + uid + " reason = " + reason);
            //// this is called in main thread
            //GameObject go = GameObject.Find(uid.ToString());
            //if (!ReferenceEquals(go, null))
            //{
            //    Destroy(go);
            //}

            m_users.Remove(uid);
            m_uiEngine.RemoveVideo(uid);
        }

        #region APP_EVENTS
        void OnApplicationPause(bool paused)
        {
            EnableVideo(paused);
        }

        void OnApplicationQuit()
        {
            UnloadEngine();
        }
        #endregion


        #region PUN_CALLBACKS
        public void OnFriendListUpdate(List<FriendInfo> friendList)
        {
            
        }

        public void OnCreatedRoom()
        {
            
        }

        public void OnCreateRoomFailed(short returnCode, string message)
        {
            
        }

        public void OnJoinedRoom()
        {
            Join(PhotonNetwork.CurrentRoom.Name);
        }

        public void OnJoinRoomFailed(short returnCode, string message)
        {
            
        }

        public void OnJoinRandomFailed(short returnCode, string message)
        {
            
        }

        public void OnLeftRoom()
        {
            Leave();
        }

        public void OnEvent(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                case Network.videoMuteToggle:
                    Debug.Log("_____videoMuteToggle event");
                    object[] videoMuteData = (object[])photonEvent.CustomData;

                    uint userId = (uint)videoMuteData[0];
                    bool videoOn = (bool)videoMuteData[1];

                    if (m_users.ContainsKey(userId))
                    {
                        m_users[userId].VideoOn = videoOn;
                        VideoPauseToggle?.Invoke(this, new VideoEventArgs()
                        {
                            UserId = userId,
                            Pause = !videoOn
                        });
                    }
                    break;
            }
        }
        #endregion
    }
}