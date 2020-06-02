using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Fordi.Core;
using Fordi.Common;
using AudioType = Fordi.Core.AudioType;
using Cornea.Web;
using Fordi.Meeting;
using LitJson;
using System.Linq;
using Fordi.UI;
using Fordi;

namespace Fordi.UI.MenuControl
{
    public interface ITimeForm : IForm
    {
        string SelectedTime { get; }
        int Hour { get; }
        int Minute { get; }
        string Date { get; }
    }
    public class MeetingForm : MenuScreen, ITimeForm
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
        [SerializeField]
        private TimeInputValidator m_hourValidator, m_minuteValidator;

        private TextMeshProUGUI m_hourPlaceholder, m_minutePlaceholder, m_durationHourPlaceholder, m_durationMinutePlaceholder, m_datePlaceholder;

        private IAudio m_audio;
        private ICommonResource m_commonResource;
        private IWebInterface m_webInterace;

        private Fordi.Pool<OrganizationMember> memberPool;
        private List<OrganizationMember> memberList = new List<OrganizationMember>();
        private List<TMP_InputField> m_inputs = new List<TMP_InputField>();

        private string m_currentSearchValue = string.Empty;

        public string SelectedTime {
            get
            {
                string hour = string.IsNullOrEmpty(m_meetingHour.text) ? m_hourPlaceholder.text : m_meetingHour.text;
                if (hour.Length < 2)
                    hour = 0 + hour;
                string minute = string.IsNullOrEmpty(m_meetingMinute.text) ? m_minutePlaceholder.text : m_meetingMinute.text;
                if (minute.Length < 2)
                    minute = 0 + minute;
                return hour + ":" + minute;
            }
        }

        public int Hour {
            get
            {
                string hourText = string.IsNullOrEmpty(m_meetingHour.text) ? m_hourPlaceholder.text : m_meetingHour.text;
                return Convert.ToInt32(hourText);
            }
        }

        public int Minute
        {
            get
            {
                string minuteText = string.IsNullOrEmpty(m_meetingMinute.text) ? m_minutePlaceholder.text : m_meetingMinute.text;
                return Convert.ToInt32(minuteText);
            }
        }

        public string Date
        {
            get
            {
                return string.IsNullOrEmpty(m_meetingDate.text) ? m_datePlaceholder.text : m_meetingDate.text;
            }
        }

        private int m_inputIndex = 0;

        protected override void Update()
        {
            base.Update();
            if (m_inputs.Count == 0)
                return;

            if (m_vrMenu.ActiveModule == InputModule.STANDALONE && !m_blocker.gameObject.activeSelf && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Tab))
            {
                m_inputIndex--;
                if (m_inputIndex < 0)
                    m_inputIndex = m_inputs.Count - 1;
                m_inputs[m_inputIndex].Select();
                return;
            }

            if (m_vrMenu.ActiveModule == InputModule.STANDALONE && !m_blocker.gameObject.activeSelf && Input.GetKeyDown(KeyCode.Tab))
            {
                m_inputIndex++;
                if (m_inputIndex > m_inputs.Count - 1)
                    m_inputIndex = 0;
                m_inputs[m_inputIndex].Select();
            }
        }

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_settings = IOC.Resolve<ISettings>();
            m_audio = IOC.Resolve<IAudio>();
            m_commonResource = IOC.Resolve<ICommonResource>();
            m_webInterace = IOC.Resolve<IWebInterface>();

            m_hourValidator.m_timeForm = this;
            m_minuteValidator.m_timeForm = this;
            if (m_meetingHour.inputValidator != null && m_meetingHour.inputValidator is TimeInputValidator hourValidator)
                hourValidator.m_timeForm = this;
            if (m_meetingMinute.inputValidator != null && m_meetingMinute.inputValidator is TimeInputValidator minuteValidator)
                minuteValidator.m_timeForm = this;
            m_inputs.Clear();
            m_inputs = new List<TMP_InputField>() { m_meetingTitle, m_meetingDate, m_meetingHour, m_meetingMinute, m_meetingDurationHour, m_meetingDurationMinute };

            for (int i = 0; i < m_inputs.Count; i++)
            {
                int index = i;
                m_inputs[i].onSelect.AddListener((val) => m_inputIndex = index);
            }
        }

        public override IMenuItem SpawnMenuItem(MenuItemInfo menuItemInfo, GameObject prefab, Transform parent)
        {
            OrganizationMember menuItem = Instantiate(prefab, parent, false).GetComponentInChildren<OrganizationMember>();
            //menuItem.name = "MenuItem";
            menuItem.Item = menuItemInfo;
            memberList.Add(menuItem);
            return menuItem;
        }

        public void OpenForm(MenuItemInfo[] items)
        {
            if (memberPool == null)
                memberPool = new Pool<OrganizationMember>(m_contentRoot, m_menuItem);

            OpenMenu("", false, true);

            PopulateMemberList(items);

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


        public void PopulateMemberList(MenuItemInfo[] items)
        {
            for (int i = 0; i < items.Length; i++)
            {
                var userInfo = ((UserResource)items[i].Data).UserInfo;

                if (i >= items.Length)
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
                memberList[i].Item = items[i];
            }

            for (int i = items.Length; i < memberList.Count; i++)
            {
                memberPool.Surrender(memberList[i]);
                memberList.Remove(memberList[i]);
            }

            SearchMembers("");
        }

        public void OpenCalendar()
        {
            m_vrMenu.OpenCalendar((date) => {
                Debug.LogError(date);
                m_meetingDate.text = date;
            }, this);
        }

        public void SearchMembers(string searchValue)
        {
            m_currentSearchValue = searchValue;
            foreach (var item in memberList)
            {
                if (item.IsRelavant(searchValue) && !item.gameObject.activeSelf)
                    memberPool.Retrieve(item);
                else if (!item.IsRelavant(searchValue) && item.gameObject.activeSelf)
                    memberPool.Surrender(item);
            }
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
                //Debug.LogError(error.ErrorText);
                m_vrMenu.DisplayResult(error);
                Invoke("CloseSelf", 1.0f);
            });
        }

        public override void DisplayResult(Error error)
        {
            base.DisplayResult(error);
            if (m_blocker != null && error.ErrorCode != Error.OK)
                m_blocker.gameObject.SetActive(true);
        }

        private void CloseSelf()
        {
            m_vrMenu.CloseLastScreen();
        }

        private string GenerateMeetingJson()
        {
            Fordi.Meeting.Meeting newMeeting = new Fordi.Meeting.Meeting();
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
            //Debug.LogError(meetingJson);
            return meetingJson;
        }

        public void OpenForm(FormArgs args, bool blocked, bool persist)
        {
            memberPool = new Pool<OrganizationMember>(m_contentRoot, m_menuItem);
            OpenForm(args.FormItems);
        }

        public override void WebRefresh()
        {
            m_webInterface.GetCategories(ResourceType.USER, (val) =>
            {
                ExternalChangesDone?.Invoke(this, EventArgs.Empty);
            }, true);
        }

        protected override void OnExternalChanges(object sender, EventArgs e)
        {
            var users = m_webInterace.GetResource(ResourceType.USER, "").Where(item => ((UserResource)item).UserInfo.id != m_webInterace.UserInfo.id).ToArray();
            PopulateMemberList(ResourceToMenuItems(users));
        }
    }
}