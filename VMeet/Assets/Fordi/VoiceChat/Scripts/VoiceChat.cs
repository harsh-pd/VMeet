using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using agora_gaming_rtc;
using UnityEngine.UI;
using System.Globalization;
using System.Runtime.InteropServices;
using System;
using Fordi.Networking;
using VRExperience.Common;
using Photon.Pun;
using Photon.Voice.Unity;
using Photon.Voice.DemoVoiceUI;

namespace Fordi.VoiceChat
{
    public interface IVoiceChat
    {
        void ToggleMute(bool val);
    }

    public class VoiceChat : MonoBehaviour, IVoiceChat
    {
        [SerializeField]
        private Recorder m_voiceRecorder = null;

        protected internal const string MutePropKey = "mute";

        private IEnumerator Start()
        {
            yield return null;
            SetDefaultMic();
        }

        private void SetDefaultMic()
        {
            List<MicRef> mics = new List<MicRef>();
            MicRef mic = new MicRef();

            if (Recorder.PhotonMicrophoneEnumerator.IsSupported)
            {
                for (int i = 0; i < Recorder.PhotonMicrophoneEnumerator.Count; i++)
                {
                    string n = Recorder.PhotonMicrophoneEnumerator.NameAtIndex(i);
                    MicRef item = new MicRef(n, Recorder.PhotonMicrophoneEnumerator.IDAtIndex(i));
                    mics.Add(item);
                    if (!n.ToLower().Contains("oculus"))
                        mic = item;
                }
            }

            if (mics.Count == 0)
            {
                foreach (string x in Microphone.devices)
                {
                    MicRef item = new MicRef(x);

                    mics.Add(item);
                    if (!x.Contains("oculus"))
                        mic = item;
                }
            }

            if (mics.Count == 0)
            {
                Debug.LogError("No mics found");
                return;
            }

            //Debug.LogError(mic.Name + " " + mic.MicType.ToString());

            this.m_voiceRecorder.MicrophoneType = mic.MicType;

            switch (mic.MicType)
            {
                case Recorder.MicType.Unity:
                    this.m_voiceRecorder.UnityMicrophoneDevice = mic.Name;
                    break;
                case Recorder.MicType.Photon:
                    this.m_voiceRecorder.PhotonMicrophoneDeviceId = mic.PhotonId;
                    break;
            }

            if (this.m_voiceRecorder.RequiresRestart)
            {
                this.m_voiceRecorder.RestartRecording();
            }
        }

        public void ToggleMute(bool val)
        {
            m_voiceRecorder.TransmitEnabled = val;
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { MutePropKey, val } }); // transmit is used as opposite of mute...
        }

    }
}