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

        [SerializeField]
        private TMP_InputField m_organization, m_name;

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

        public void OpenForm(MenuItemInfo[] items)
        {
            OpenMenu(items, false, true);
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

    }
}