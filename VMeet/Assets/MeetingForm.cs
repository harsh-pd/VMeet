using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using VRExperience.Core;
using VRExperience.Common;
using AudioType = VRExperience.Core.AudioType;
using Cornea.Web;
using VRExperience.Meeting;
using LitJson;
using System.Linq;

namespace VRExperience.UI.MenuControl
{
    public class MeetingForm : MenuScreen
    {
        [Header("Details")]
        [SerializeField]
        private TMP_InputField m_meetingTitle;
        [SerializeField]
        private TMP_InputField m_meetingDate;
        [SerializeField]
        private TMP_InputField m_meetingHour;
        [SerializeField]
        private TMP_InputField m_meetingMinute;
        [SerializeField]
        private TMP_InputField m_meetingDurationHour;
        [SerializeField]
        private TMP_InputField m_meetingDurationMinute;

        private TextMeshProUGUI m_hourPlaceholder, m_minutePlaceholder, m_durationHourPlaceholder, m_durationMinutePlaceholder, m_datePlaceholder;

        private IAudio m_audio;
        private ICommonResource m_commonResource;
        private IWebInterface m_webInterace;

        Fordi.Pool<OrganizationMember> memberPool;
        List<OrganizationMember> memberList = new List<OrganizationMember>();

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_settings = IOC.Resolve<ISettings>();
            m_audio = IOC.Resolve<IAudio>();
            m_commonResource = IOC.Resolve<ICommonResource>();
            m_webInterace = IOC.Resolve<IWebInterface>();
        }

        public override void SpawnMenuItem(MenuItemInfo menuItemInfo, GameObject prefab, Transform parent)
        {
            OrganizationMember menuItem = Instantiate(prefab, parent, false).GetComponentInChildren<OrganizationMember>();
            //menuItem.name = "MenuItem";
            menuItem.Item = menuItemInfo;
            memberList.Add(menuItem);
        }

        public void OpenForm(MenuItemInfo[] items)
        {
            OpenMenu(items, false, true);
            var time = DateTime.Now.Add(new TimeSpan(0, 10, 0));
            m_hourPlaceholder = m_meetingHour.placeholder.GetComponent<TextMeshProUGUI>();
            m_hourPlaceholder.text = time.ToString("hh");
            m_minutePlaceholder = m_meetingMinute.placeholder.GetComponent<TextMeshProUGUI>();
            m_minutePlaceholder.text = time.ToString("mm");

            m_durationHourPlaceholder = m_meetingDurationHour.placeholder.GetComponent<TextMeshProUGUI>();
            m_durationMinutePlaceholder = m_meetingDurationMinute.placeholder.GetComponent<TextMeshProUGUI>();

            m_datePlaceholder = m_meetingDate.placeholder.GetComponent<TextMeshProUGUI>();
            m_datePlaceholder.text = DateTime.Now.ToString("yyyy/MM/dd");
        }

        public void PopulateMemberList(List<UserInfo> users)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].emailAddress.Equals(m_webInterace.UserInfo.emailAddress))
                    users.Remove(users[i]);

                if (i >= users.Count)
                    break;

                if (memberList.Count > i)
                {
                    if (!memberList[i].gameObject.activeSelf)
                        memberPool.Retrieve(memberList[i]);
                }
                else
                {
                    var member = memberPool.FetchItem();
                    memberList.Add(member);
                }
                memberList[i].Init(users[i].name, users[i].emailAddress, users[i].id);
            }

            for (int i = users.Count; i < memberList.Count; i++)
            {
                memberPool.Surrender(memberList[i]);
                memberList.Remove(memberList[i]);
            }
        }

        public void OpenCalendar()
        {
            m_vrMenu.OpenCalendar((date) => {
                Debug.LogError(date);
                m_meetingDate.text = date;
            });
        }

        public void Submit()
        {
            m_webInterace.CreateMeeting(GenerateMeetingJson()).OnRequestComplete((networkError, message) =>
            {
                JsonData result = JsonMapper.ToObject(message);
                Error error = new Error();
                if (result["success"].ToString() == "True")
                {
                    error.ErrorCode = Error.OK;
                    error.ErrorText = "Meeting successfully created";
                }
                else
                {
                    error.ErrorCode = Error.E_Exception;
                    error.ErrorText = (string)result["error"]["message"];
                }
                Debug.LogError(error.ErrorText);
                m_vrMenu.DisplayResult(error);
            });
        }

        private string GenerateMeetingJson()
        {
            VRExperience.Meeting.Meeting newMeeting = new VRExperience.Meeting.Meeting();
            //if (fileSelectionScreen.EnteredUrl != null)
            //    newMeeting.fileLocation = fileSelectionScreen.EnteredUrl;
            //newMeeting.fileToBeReviewed = "Radial_Engine.jt.rtprefab.zip";

            string meetingHour = string.IsNullOrEmpty(m_meetingHour.text) ? m_hourPlaceholder.text : m_meetingHour.text;
            string meetingMinute = string.IsNullOrEmpty(m_meetingMinute.text) ? m_minutePlaceholder.text : m_meetingMinute.text;

            string meetingDurationHour = string.IsNullOrEmpty(m_meetingDurationHour.text) ? m_durationHourPlaceholder.text : m_meetingDurationHour.text;
            string meetingDurationMinute = string.IsNullOrEmpty(m_meetingDurationMinute.text) ? m_durationMinutePlaceholder.text : m_meetingDurationMinute.text;

            string date = string.IsNullOrEmpty(m_meetingDate.text) ? DateTime.Now.ToString("yyyy/MM/dd") : m_meetingDate.text;
            var localTime = date + " T " + meetingHour + ":" + meetingMinute;
            //Debug.LogError(localTime);
            newMeeting.meetingTime = DateTime.Parse(localTime).ToUniversalTime().ToString("MM/dd/yyyy hh:mm:ss tt");
            newMeeting.meetingDurationInMinutes = Int32.Parse(meetingDurationHour) * 60 + Int32.Parse(meetingDurationMinute);
            newMeeting.userid = m_webInterace.UserInfo.id;
            newMeeting.createddatetime = DateTime.Now.ToLongDateString();
            var members = memberList.Where(item => item.Selected == true).ToList();
            foreach (var item in members)
                newMeeting.MeetingParticipants.Add(new MeetingParticipant(item.UserId));
            newMeeting.description = m_meetingTitle.text;

            string meetingJson = JsonMapper.ToJson(newMeeting);
            Debug.LogError(meetingJson);
            return meetingJson;
        }
    }
}