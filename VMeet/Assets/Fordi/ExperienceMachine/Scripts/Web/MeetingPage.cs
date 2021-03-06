using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Cornea.Web;
using LargeFileDownloader;
using System.IO;
using System;
using UnityEngine.XR;
using System.Threading;
using LitJson;
using Fordi;
using Fordi.Meeting;
using Fordi.UI.MenuControl;
using Fordi.Common;
using Fordi.Core;
using Fordi.UI;
using Photon.Pun;
using Fordi.Networking;

namespace Fordi.Meetings.UI
{
    public enum MeetingStatus
    {
        IDLE,
        MAKING_API_REQUEST,
        FILE_DOWNLOADING,
        FILE_EXTRACTING,
        FILE_IMPORTING,
        SETTING_UP_ROOM,
        ONGOING
    }

    public class MeetingPage : MenuScreen, IResettable
    {
        #region FIELDS_AND_PROPERTIES
        [SerializeField]
        private GameObject m_actionButtonPrefab;

        public const string ROOT_FOLDER = "Root";

        private MeetingInfo m_meetingInfo;
        public MeetingCategory MeetingType { get { return m_meetingInfo.meetingType; } }
        private MeetingStatus status = MeetingStatus.IDLE;
        public MeetingStatus MeetingStatus { get { return status; } }
        public MeetingInfo Info { get { return m_meetingInfo; } }
        public string File
        {
            get
            {
                return m_meetingInfo.FileToBeReviewed;
            }
        }
        public int Id
        {
            get
            {
                return m_meetingInfo.Id;
            }
        }
        public DateTime MeetingTime
        {
            get
            {
                return m_meetingInfo.MeetingTime;
            }
        }
        #endregion

        private INetwork m_network = null;

        #region GENERAL_METHODS

        private int m_buttonRootLevel = 0;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_network = IOC.Resolve<INetwork>();
            m_network.RoomListUpdateEvent += RoomListUpdated;
            var button = m_actionButtonPrefab.GetComponentInChildren<Button>();
            if (m_actionButtonPrefab.transform == button.transform)
                m_buttonRootLevel = 0;
            else if (m_actionButtonPrefab.transform == button.transform.parent)
                m_buttonRootLevel = 1;
            else
                throw new InvalidDataException("Action button prefab not in proper format");
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            m_network.RoomListUpdateEvent -= RoomListUpdated;
        }


        protected override void OnExternalChanges(object sender, EventArgs e)
        {
            ReleaseActionButton(m_secondMeetingButton);
            m_meetingButton.onClick.RemoveAllListeners();
            var buttonLable = m_meetingButton.GetComponentInChildren<TextMeshProUGUI>();

            switch (m_meetingInfo.meetingType)
            {
                case MeetingCategory.INVITED:
                    buttonLable.text = "Accept";
                    m_meetingButton.onClick.AddListener(() => Accept());
                    m_secondMeetingButton = Instantiate(m_actionButtonPrefab, m_contentRoot).GetComponentInChildren<Button>();
                    m_secondMeetingButton.GetComponentInChildren<TextMeshProUGUI>().text = "Ignore";
                    m_secondMeetingButton.onClick.AddListener(() => Ignore());
                    break;
                case MeetingCategory.REJECTED:
                    buttonLable.text = "Accept";
                    m_meetingButton.onClick.AddListener(() => Accept());
                    break;
                case MeetingCategory.ACCEPTED:
                    buttonLable.text = "Reject";
                    m_meetingButton.onClick.AddListener(() => Reject());
                    break;
                default:
                    break;
            }
        }

        public void OnReset()
        {

        }

        private void ReleaseActionButton(Button button)
        {
            if (button == null)
                return;

            button.onClick.RemoveAllListeners();
            if (m_buttonRootLevel == 0)
                Destroy(button.gameObject);
            else if (m_buttonRootLevel == 1)
                Destroy(button.transform.parent.gameObject);
            else
                throw new InvalidDataException("Action button prefab not in proper format");
        }

        private void RoomListUpdated(object sender, EventArgs e)
        {
            if (m_meetingInfo.meetingType == MeetingCategory.INVITED || m_meetingInfo.meetingType == MeetingCategory.REJECTED)
                return;

            if (PhotonNetwork.InRoom && m_roomButton != null)
            {
                ReleaseActionButton(m_roomButton);
                return;
            }

            if (Array.FindIndex(Fordi.Networking.Network.Rooms, item => item.Name == m_meetingInfo.MeetingNumber) != -1)
            {
                Debug.LogError(Fordi.Networking.Network.Rooms.Length);
                if (m_roomButton != null)
                    m_roomButton.onClick.RemoveAllListeners();
                else
                    m_roomButton = Instantiate(m_actionButtonPrefab, m_contentRoot).GetComponentInChildren<Button>(); ;
                m_roomButton.GetComponentInChildren<TextMeshProUGUI>().text = "Join";
                m_roomButton.onClick.AddListener(() => Join());
            }
            else
            {
                if (m_meetingInfo.meetingType == MeetingCategory.ACCEPTED)
                    ReleaseActionButton(m_roomButton);
                else
                {
                    if (m_roomButton != null)
                        m_roomButton.onClick.RemoveAllListeners();
                    else
                        m_roomButton = Instantiate(m_actionButtonPrefab, m_contentRoot).GetComponentInChildren<Button>(); ;
                    m_roomButton.GetComponentInChildren<TextMeshProUGUI>().text = "Host";
                    m_roomButton.onClick.AddListener(() => Host());
                }
            }
        }

        public bool IsRelavant(string searchValue)
        {
            var lowerItemFileName = File.ToLower();
            var lowerMeetingNumberString = m_meetingInfo.MeetingNumber.ToLower();
            var lowerMeetingId = Id.ToString().ToLower();
            var lowerDateString = m_meetingInfo.MeetingTime.ToShortDateString();
            var lowerSearchValue = searchValue.ToLower();
            return (searchValue.Equals(lowerDateString) || (lowerMeetingId.Contains(lowerSearchValue) || lowerItemFileName.Contains(lowerSearchValue)) || lowerMeetingNumberString.Contains(lowerSearchValue));
        }
        #endregion

        private Button m_roomButton, m_meetingButton, m_secondMeetingButton = null;

        public void OpenMeeting(IUserInterface userInterface, MeetingArgs args)
        {
            m_title.text = "MEETING";
            m_meetingInfo = args.MeetingInfo;
            m_userInterface = userInterface;

            switch (args.MeetingInfo.meetingType)
            {
                case MeetingCategory.CREATED:
                    m_roomButton = Instantiate(m_actionButtonPrefab, m_contentRoot).GetComponentInChildren<Button>();
                    if (Array.FindIndex(Fordi.Networking.Network.Rooms, item => item.Name == m_meetingInfo.MeetingNumber) != -1)
                    {
                        m_roomButton.GetComponentInChildren<TextMeshProUGUI>().text = "Join";
                        m_roomButton.onClick.AddListener(() => Join());
                    }
                    else
                    {
                        m_meetingButton = Instantiate(m_actionButtonPrefab, m_contentRoot).GetComponentInChildren<Button>();
                        m_meetingButton.GetComponentInChildren<TextMeshProUGUI>().text = "Cancel";
                        m_meetingButton.onClick.AddListener(() => Cancel());
                        m_roomButton.GetComponentInChildren<TextMeshProUGUI>().text = "Host";
                        m_roomButton.onClick.AddListener(() => Host());
                    }
                    break;
                case MeetingCategory.INVITED:
                    m_meetingButton = Instantiate(m_actionButtonPrefab, m_contentRoot).GetComponentInChildren<Button>();
                    m_meetingButton.GetComponentInChildren<TextMeshProUGUI>().text = "Accept";
                    m_meetingButton.onClick.AddListener(() => Accept());
                    m_secondMeetingButton = Instantiate(m_actionButtonPrefab, m_contentRoot).GetComponentInChildren<Button>();
                    m_secondMeetingButton.GetComponentInChildren<TextMeshProUGUI>().text = "Ignore";
                    m_secondMeetingButton.onClick.AddListener(() => Ignore());
                    break;
                case MeetingCategory.REJECTED:
                    m_meetingButton = Instantiate(m_actionButtonPrefab, m_contentRoot).GetComponentInChildren<Button>();
                    m_meetingButton.GetComponentInChildren<TextMeshProUGUI>().text = "Accept";
                    m_meetingButton.onClick.AddListener(() => Accept());
                    break;
                case MeetingCategory.ACCEPTED:
                    m_meetingButton = Instantiate(m_actionButtonPrefab, m_contentRoot).GetComponentInChildren<Button>();
                    m_meetingButton.GetComponentInChildren<TextMeshProUGUI>().text = "Reject";
                    m_meetingButton.onClick.AddListener(() => Reject());
                    if (Array.FindIndex(Fordi.Networking.Network.Rooms, item => item.Name == m_meetingInfo.MeetingNumber) != -1)
                    {
                        Debug.LogError(Fordi.Networking.Network.Rooms.Length);
                        m_roomButton = Instantiate(m_actionButtonPrefab, m_contentRoot).GetComponentInChildren<Button>(); ;
                        m_roomButton.GetComponentInChildren<TextMeshProUGUI>().text = "Join";
                        m_roomButton.onClick.AddListener(() => Join());
                    }
                    break;
                default:
                    break;
            }

            m_description.text = args.MeetingInfo.MeetingNumber + "\n\n" + (string.IsNullOrEmpty(args.MeetingInfo.Description) ? "" : string.IsNullOrEmpty(args.MeetingInfo.Description) + "\n\n") + args.MeetingInfo.MeetingTime + "\n\n" + args.MeetingInfo.MeetingDurationInMinutes / 60 + " Hours, " + args.MeetingInfo.MeetingDurationInMinutes % 60 + " Minutes";
        }

        #region API

        public void Accept()
        {
            m_uiEngine.DisplayProgress("Accepting Meeting: " + Info.MeetingNumber);

            APIRequest rejectRequest = null;
            rejectRequest = m_webInterface.AcceptMeeting(Info.Id).OnRequestComplete(
                (isNetworkError, message) =>
                {
                    status = MeetingStatus.IDLE;
                    JsonData oututData = JsonMapper.ToObject(message);
                    if (!isNetworkError && (bool)oututData["success"] == true)
                    {
                        ExternalChangesDone?.Invoke(this, EventArgs.Empty);

                        if (Array.FindIndex(Fordi.Networking.Network.Rooms, item => item.Name == m_meetingInfo.MeetingNumber) != -1)
                        {
                            if (m_roomButton != null)
                                m_roomButton.onClick.RemoveAllListeners();
                            else
                                m_roomButton = Instantiate(m_actionButtonPrefab, m_contentRoot).GetComponentInChildren<Button>(); ;
                            m_roomButton.GetComponentInChildren<TextMeshProUGUI>().text = "Join";
                            m_roomButton.onClick.AddListener(() => Join());
                        }

                        //m_uiEngine.CloseLastScreen();
                        Error error = new Error(Error.OK);
                        error.ErrorText = "";
                        m_uiEngine.DisplayResult(error);
                    }
                    else
                    {
                        Error error = new Error(Error.E_Exception);
                        error.ErrorText = (string)oututData["error"]["message"];
                        m_uiEngine.DisplayResult(error);
                    }
                }
            );
        }

        public void Reject()
        {
            m_uiEngine.DisplayProgress("Rejecting Meeting: " + Info.MeetingNumber);

            APIRequest rejectRequest = null;
            rejectRequest = m_webInterface.RejectMeeting(Info.Id).OnRequestComplete(
                (isNetworkError, message) =>
                {
                    status = MeetingStatus.IDLE;
                    JsonData oututData = JsonMapper.ToObject(message);
                    if (!isNetworkError && (bool)oututData["success"] == true)
                    {
                        ExternalChangesDone?.Invoke(this, EventArgs.Empty);

                        //m_uiEngine.CloseLastScreen();
                        Error error = new Error(Error.OK);
                        error.ErrorText = "";
                        m_uiEngine.DisplayResult(error);
                    }
                    else
                    {
                        Error error = new Error(Error.E_Exception);
                        error.ErrorText = (string)oututData["error"]["message"];
                        m_uiEngine.DisplayResult(error);
                    }
                }
            );
        }

        public void Cancel()
        {
            m_uiEngine.DisplayProgress("Cancelling Meeting: " + Info.MeetingNumber);

            APIRequest rejectRequest = null;
            rejectRequest = m_webInterface.CancelMeeting(Info.Id).OnRequestComplete(
                (isNetworkError, message) =>
                {
                    status = MeetingStatus.IDLE;
                    JsonData oututData = JsonMapper.ToObject(message);
                    if (!isNetworkError && (bool)oututData["success"] == true)
                    {
                        ExternalChangesDone?.Invoke(this, EventArgs.Empty);
                        m_uiEngine.CloseLastScreen();
                    }
                    else
                    {
                        Error error = new Error(Error.E_Exception);
                        error.ErrorText = (string)oututData["error"]["message"];
                        m_uiEngine.DisplayResult(error);
                    }
                }
            );
        }

        public void Ignore()
        {
            m_uiEngine.DisplayProgress("Rejecting Meeting: " + Info.MeetingNumber);

            APIRequest rejectRequest = null;
            rejectRequest = m_webInterface.RejectMeeting(Info.Id).OnRequestComplete(
                (isNetworkError, message) =>
                {
                    status = MeetingStatus.IDLE;
                    JsonData oututData = JsonMapper.ToObject(message);
                    if (!isNetworkError && (bool)oututData["success"] == true)
                    {
                        ExternalChangesDone?.Invoke(this, EventArgs.Empty);

                        //m_uiEngine.CloseLastScreen();
                        Error error = new Error(Error.OK);
                        error.ErrorText = "";
                        m_uiEngine.DisplayResult(error);
                    }
                    else
                    {
                        Error error = new Error(Error.E_Exception);
                        error.ErrorText = (string)oututData["error"]["message"];
                        m_uiEngine.DisplayResult(error);
                    }
                }
            );
        }

        //private void OnProgress(DownloadEvent e)
        //{
        //    if (e.fileURL.Equals(downloadUrl))
        //        downloadPanel.UpdateProgress(e.progress);
        //    //Debug.Log(e.progress);
        //}
        #endregion

        #region ROOM
        public void Host()
        {
            m_network.CreateRoom(m_meetingInfo.MeetingNumber);
        }

        public void Join()
        {
            m_network.JoinRoom(m_meetingInfo.MeetingNumber);
        }
        #endregion
    }
}