﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using agora_gaming_rtc;
using UnityEngine.UI;
using System.Globalization;
using System.Runtime.InteropServices;
using System;

namespace Fordi.ScreenSharing
{
    public interface IScreenShare
    {
        bool BroadcastScreen { get; set; }
        EventHandler<uint> OtherUserJoinedEvent { get; set; }
    }

    public class ScreenShare : MonoBehaviour, IScreenShare
    {
        Texture2D mTexture;
        private string appId = "397c1095001f4f88abe788a32dcd1570";
        private string channelName = "bt1";
        public IRtcEngine mRtcEngine;
        int i = 100;

        private uDesktopDuplication.Texture m_localMonitorView = null;
        private Color32[] colors;

        public bool BroadcastScreen { get; set; } = true;

        public EventHandler<uint> OtherUserJoinedEvent { get; set; }

        private void Awake()
        {
            m_localMonitorView = FindObjectOfType<uDesktopDuplication.Texture>();
        }

        void Start()
        {
            Debug.Log("ScreenShare Activated");
            mRtcEngine = IRtcEngine.getEngine(appId);
            // enable log
            mRtcEngine.SetLogFilter(LOG_FILTER.DEBUG | LOG_FILTER.INFO | LOG_FILTER.WARNING | LOG_FILTER.ERROR | LOG_FILTER.CRITICAL);
            // set callbacks (optional)
            mRtcEngine.SetParameters("{\"rtc.log_filter\": 65535}");
            //Configure the external video source
            mRtcEngine.SetExternalVideoSource(true, false);
            // Start video mode
            mRtcEngine.EnableVideo();
            // allow camera output callback
            mRtcEngine.EnableVideoObserver();
            // join channel
            mRtcEngine.JoinChannel(channelName, null, 0);
            //Create a rectangle width and height of the screen
            //mRect = new Rect(0, 0, Screen.width, Screen.height);
            //Create a texture the size of the rectangle you just created
            //mTexture = new Texture2D((int)mRect.width, (int)mRect.height, TextureFormat.RGBA32, false);
            mRtcEngine.OnUserJoined = OtherUserJoined;
        }

        [SerializeField]
        private VideoSurface m_videoSurfacePrefab = null;
        [SerializeField]
        private Renderer m_pixelPreview = null;

        private void CreateTextureIfNeeded()
        {
            if (!mTexture || mTexture.width != 1920 || mTexture.height != 1080)
            {
                colors = new Color32[1920 * 1080];
                mTexture = new Texture2D(1920, 1080, TextureFormat.BGRA32, false);
                Debug.LogError("Texture created");
            }
        }

        private static byte[] Color32ArrayToByteArray(Color32[] colors)
        {
            if (colors == null || colors.Length == 0)
                return null;

            int lengthOfColor32 = Marshal.SizeOf(typeof(Color32));
            int length = lengthOfColor32 * colors.Length;
            byte[] bytes = new byte[length];

            GCHandle handle = default(GCHandle);
            try
            {
                handle = GCHandle.Alloc(colors, GCHandleType.Pinned);
                IntPtr ptr = handle.AddrOfPinnedObject();
                Marshal.Copy(ptr, bytes, 0, length);
            }
            finally
            {
                if (handle != default(GCHandle))
                    handle.Free();
            }

            return bytes;
        }

        private void OtherUserJoined(uint uid, int elapsed)
        {
            //var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //var vs = cube.AddComponent<VideoSurface>();
            var vs = Instantiate(m_videoSurfacePrefab, FindObjectOfType<Canvas>().transform);
            vs.SetForUser(uid);
            vs.SetEnable(true);
            OtherUserJoinedEvent?.Invoke(this, uid);
        }

        void Update()
        {
            //Start the screenshare Coroutine
            if (m_localMonitorView != null)
                StartCoroutine(shareScreen());
        }
        //Screen Share
        IEnumerator shareScreen()
        {
            CreateTextureIfNeeded();
            uDesktopDuplication.Manager.primary.useGetPixels = true;
            yield return new WaitForEndOfFrame();


            ////Read the Pixels inside the Rectangle
            //mTexture.ReadPixels(mRect, 0, 0);
            ////Apply the Pixels read from the rectangle to the texture
            //mTexture.Apply();

            var monitor = m_localMonitorView.monitor;

            if (!monitor.hasBeenUpdated)
                yield break;

            if (monitor.GetPixels(colors, 0, 0, 1920, 1080))
            {
                mTexture.SetPixels32(colors);
                mTexture.Apply();
            }

            if (m_pixelPreview != null)
            {
                m_pixelPreview.material.mainTexture = mTexture;
            }

            // Get the Raw Texture data from the the from the texture and apply it to an array of bytes
            byte[] bytes = Color32ArrayToByteArray(colors);
            // Make enough space for the bytes array
            int size = Marshal.SizeOf(bytes[0]) * bytes.Length;
            // Check to see if there is an engine instance already created
            IRtcEngine rtc = IRtcEngine.QueryEngine();
            //if the engine is present
            if (rtc != null)
            {
                //Create a new external video frame
                ExternalVideoFrame externalVideoFrame = new ExternalVideoFrame();
                //Set the buffer type of the video frame
                externalVideoFrame.type = ExternalVideoFrame.VIDEO_BUFFER_TYPE.VIDEO_BUFFER_RAW_DATA;
                // Set the video pixel format
                externalVideoFrame.format = ExternalVideoFrame.VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_BGRA;
                //apply raw data you are pulling from the rectangle you created earlier to the video frame
                externalVideoFrame.buffer = bytes;
                //Set the width of the video frame (in pixels)
                externalVideoFrame.stride = (int)1920;
                //Set the height of the video frame
                externalVideoFrame.height = (int)1080;
                //Remove pixels from the sides of the frame
                externalVideoFrame.cropLeft = 10;
                externalVideoFrame.cropTop = 10;
                externalVideoFrame.cropRight = 10;
                externalVideoFrame.cropBottom = 10;
                //Rotate the video frame (0, 90, 180, or 270)
                externalVideoFrame.rotation = 180;
                // increment i with the video timestamp
                externalVideoFrame.timestamp = i++;
                //Push the external video frame with the frame we just created
                int a = rtc.PushVideoFrame(externalVideoFrame);
                Debug.Log(" pushVideoFrame =       " + a);
            }
        }

        private void OnDestroy()
        {
            if (mRtcEngine != null)
            {
                mRtcEngine.LeaveChannel();
                mRtcEngine.DisableVideoObserver();
                IRtcEngine.Destroy();
                mRtcEngine = null;
            }
        }
    }
}