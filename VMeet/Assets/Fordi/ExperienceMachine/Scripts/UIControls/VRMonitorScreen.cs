using System.Collections;
using System.Collections.Generic;
using agora_gaming_rtc;
using Fordi.Sync;
using UnityEngine;
using VRExperience.Common;
using VRExperience.Core;
using VRExperience.UI.MenuControl;

namespace Fordi.ScreenSharing
{
    public class VRMonitorScreen : MonoBehaviour, IScreen
    {
        [SerializeField]
        private VideoSurface m_remoteMonitorViewPrefab = null;
        [SerializeField]
        private uDesktopDuplication.Texture m_localDesktop = null;

        #region INTERFACE_IMPLEMENTATION
    public bool Blocked { get; set; }

        public bool Persist { get; set; }

        public GameObject Gameobject { get { return gameObject; } }

        private IScreen m_pair = null;
        public IScreen Pair { get { return m_pair; } set { m_pair = value; } }

        public void AttachSyncView(SyncView syncView)
        {
            
        }

        public void Close()
        {
            Destroy(gameObject);
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }

        public void DisplayProgress(string text)
        {
            
        }

        public void DisplayResult(Error error)
        {
            
        }

        public void Hide()
        {
            transform.localScale = Vector3.zero;
        }

        public void Reopen()
        {
            gameObject.SetActive(true);
        }

        public void ShowPreview(Sprite sprite)
        {
            
        }

        public void ShowTooltip(string tooltip)
        {
            
        }

        public void UnHide()
        {
            transform.localScale = Vector3.one;
        }
        #endregion

        private IAppTheme m_appTheme = null;
        private IScreenShare m_screenShare = null;

        private VideoSurface m_remoteMonitorView;

        protected void Awake()
        {
            m_appTheme = IOC.Resolve<IAppTheme>();
            m_screenShare = IOC.Resolve<IScreenShare>();
            m_screenShare.OtherUserJoinedEvent += RemoteUserJoinedChannel;
            m_screenShare.RemoteScreenShareEvent += RemoteScreenShareNotification;
        }

        protected void OnDestroy()
        {
            m_screenShare.OtherUserJoinedEvent -= RemoteUserJoinedChannel;
            m_screenShare.RemoteScreenShareEvent -= RemoteScreenShareNotification;
        }

        private void RemoteUserJoinedChannel(object sender, uint e)
        {
            if (m_screenShare.ReceivingRemoteStream)
            {
                ToggleMonitor(true);
                m_remoteMonitorView.SetForUser(e);
                m_remoteMonitorView.SetEnable(true);
            }
        }

        private void ToggleMonitor(bool val)
        {
            if (!val && m_remoteMonitorView != null)
            {
                Destroy(m_remoteMonitorView.gameObject);
                m_localDesktop.gameObject.SetActive(true);
            }
            else if (val)
            {
                m_localDesktop.gameObject.SetActive(false);
                m_remoteMonitorView = Instantiate(m_remoteMonitorViewPrefab, transform);
                m_remoteMonitorView.transform.SetSiblingIndex(transform.childCount - 1);
            }
        }

        private void RemoteScreenShareNotification(object sender, ScreenEventArgs e)
        {
            if (!e.Streaming)
                ToggleMonitor(false);
            Debug.LogError(e.Streaming);
        }
    }
}
