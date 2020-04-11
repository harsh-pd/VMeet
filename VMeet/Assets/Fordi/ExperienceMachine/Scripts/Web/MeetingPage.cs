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
using VRExperience.Meeting;
using VRExperience.UI.MenuControl;
using VRExperience.Common;
using VRExperience.Core;
using VRExperience.UI;

namespace VRExperience.Meetings.UI
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
        private Button m_actionButton;

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

        #region GENERAL_METHODS
        private IWebInterface m_webInterface = null;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_webInterface = IOC.Resolve<IWebInterface>();
        }

        public void OnReset()
        {
            
        }

        //private void ReceivedRoomListUpdate()
        //{
        //    if (Coordinator.instance.meetingRoomInterface.IsRoomAvailable(meetingInfo.MeetingNumber))
        //    {
        //        NetworkManager.ReceivedRoomListUpdate -= ReceivedRoomListUpdate;
        //        UpdateDescriptionText(true);
        //        if (meetingInfo.meetingType != MeetingCategory.CREATED)
        //            actionPanel.OnRoomCreated();
        //    }
        //}

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

        public void OpenMeeting(MeetingInfo meetingInfo)
        {
            m_title.text = "MEETING";
            m_meetingInfo = meetingInfo;
            var text = m_actionButton.GetComponentInChildren<TextMeshProUGUI>();
            switch (meetingInfo.meetingType)
            {
                case MeetingCategory.CREATED:
                    text.text = "Cancel Meeting";
                    m_actionButton.onClick.AddListener(() => Cancel());
                    break;
                case MeetingCategory.INVITED:
                    text.text = "Accept Meeting";
                    m_actionButton.onClick.AddListener(() => Accept());
                    break;
                case MeetingCategory.REJECTED:
                    text.text = "Accept Meeting";
                    m_actionButton.onClick.AddListener(() => Accept());
                    break;
                case MeetingCategory.ACCEPTED:
                    text.text = "Ignore Meeting";
                    m_actionButton.onClick.AddListener(() => Reject());
                    break;
                default:
                    break;
            }

            m_description.text = meetingInfo.MeetingNumber + "\n\n" + (string.IsNullOrEmpty(meetingInfo.Description) ? "" : string.IsNullOrEmpty(meetingInfo.Description) + "\n\n") + meetingInfo.MeetingTime + "\n\n" + meetingInfo.MeetingDurationInMinutes / 60 + " Hours, " + meetingInfo.MeetingDurationInMinutes % 60 + " Minutes";
        }

        #region API

        public void Accept()
        {
            m_vrMenu.DisplayProgress("Accepting Meeting: " + Info.MeetingNumber);

            APIRequest rejectRequest = null;
            rejectRequest = m_webInterface.AcceptMeeting(Info.Id).OnRequestComplete(
                (isNetworkError, message) =>
                {
                    status = MeetingStatus.IDLE;
                    JsonData oututData = JsonMapper.ToObject(message);
                    if (!isNetworkError && (bool)oututData["success"] == true)
                    {
                        m_vrMenu.CloseLastScreen();
                    }
                    else
                    {
                        Error error = new Error(Error.E_Exception);
                        error.ErrorText = (string)oututData["error"]["message"];
                        m_vrMenu.DisplayResult(error);
                    }
                }
            );
        }

        public void Reject()
        {
            m_vrMenu.DisplayProgress("Rejecting Meeting: " + Info.MeetingNumber);

            APIRequest rejectRequest = null;
            rejectRequest = m_webInterface.RejectMeeting(Info.Id).OnRequestComplete(
                (isNetworkError, message) =>
                {
                    status = MeetingStatus.IDLE;
                    JsonData oututData = JsonMapper.ToObject(message);
                    if (!isNetworkError && (bool)oututData["success"] == true)
                    {
                        m_vrMenu.CloseLastScreen();
                    }
                    else
                    {
                        Error error = new Error(Error.E_Exception);
                            error.ErrorText = (string)oututData["error"]["message"];
                            m_vrMenu.DisplayResult(error);
                    }
                }
            );
        }

        public void Cancel()
        {
            m_vrMenu.DisplayProgress("Cancelling Meeting: " + Info.MeetingNumber);

            APIRequest rejectRequest = null;
            rejectRequest = m_webInterface.CancelMeeting(Info.Id).OnRequestComplete(
                (isNetworkError, message) =>
                {
                    status = MeetingStatus.IDLE;
                    JsonData oututData = JsonMapper.ToObject(message);
                    if (!isNetworkError && (bool)oututData["success"] == true)
                    {
                        m_vrMenu.CloseLastScreen();
                    }
                    else
                    {
                        Error error = new Error(Error.E_Exception);
                        error.ErrorText = (string)oututData["error"]["message"];
                        m_vrMenu.DisplayResult(error);
                    }
                }
            );
        }

        public void Ignore()
        {
            m_vrMenu.DisplayProgress("Rejecting Meeting: " + Info.MeetingNumber);

            APIRequest rejectRequest = null;
            rejectRequest = m_webInterface.RejectMeeting(Info.Id).OnRequestComplete(
                (isNetworkError, message) =>
                {
                    status = MeetingStatus.IDLE;
                    JsonData oututData = JsonMapper.ToObject(message);
                    if (!isNetworkError && (bool)oututData["success"] == true)
                    {
                        m_vrMenu.CloseLastScreen();
                    }
                    else
                    {
                        Error error = new Error(Error.E_Exception);
                        error.ErrorText = (string)oututData["error"]["message"];
                        m_vrMenu.DisplayResult(error);
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

    }
}