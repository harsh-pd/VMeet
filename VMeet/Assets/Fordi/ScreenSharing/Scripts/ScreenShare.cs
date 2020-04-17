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

        private string channelName = "channeltest";
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

            mRtcEngine.OnUserJoined = OtherUserJoined;
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

        void Update()
        {
            //Start the screenshare Coroutine
            if (BroadcastScreen && m_localMonitorView != null)
                StartCoroutine(shareScreen());
        }

        private void OtherUserJoined(uint uid, int elapsed)
        {
            Debug.LogError("User Joined" + uid);
            OtherUserJoinedEvent?.Invoke(this, uid);
        }

        private void CreateTextureIfNeeded()
        {
            if (!mTexture || mTexture.width != MouseControl.SystemWidth || mTexture.height != MouseControl.SystemHeight)
            {
                colors = new Color32[MouseControl.SystemWidth * MouseControl.SystemHeight];
                mTexture = new Texture2D(MouseControl.SystemWidth, MouseControl.SystemHeight, TextureFormat.ARGB32, false);
            }
        }

        //Screen Share
        IEnumerator shareScreen()
        {
            CreateTextureIfNeeded();
            uDesktopDuplication.Manager.primary.useGetPixels = true;

            var monitor = m_localMonitorView.monitor;
            yield return new WaitForEndOfFrame();

            if (!monitor.hasBeenUpdated)
                yield break;

            if (monitor.GetPixels(colors, 0, 0, MouseControl.SystemWidth, MouseControl.SystemHeight))
            {
                mTexture.SetPixels32(colors);
                mTexture.Apply();
            }

            ////Read the Pixels inside the Rectangle
            //mTexture.ReadPixels(mRect, 0, 0);
            ////Apply the Pixels read from the rectangle to the texture
            //mTexture.Apply();
            // Get the Raw Texture data from the the from the texture and apply it to an array of bytes
            byte[] bytes = mTexture.GetRawTextureData();
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
                externalVideoFrame.stride = (int)MouseControl.SystemWidth;
                //Set the height of the video frame
                externalVideoFrame.height = (int)MouseControl.SystemHeight;
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
    }
}