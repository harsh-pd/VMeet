using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Fordi;
using VRExperience.UI.MenuControl;

namespace VRExperience.Meeting
{
    public class OrganizationMember : MenuItem, IResettable
    {
        [SerializeField]
        TextMeshProUGUI nameField, emailField;

        private int userId;

        public string Name
        {
            get
            {
                return nameField.text;
            }
        }

        public string Email
        {
            get
            {
                return emailField.text;
            }
        }

        public int UserId
        {
            get
            {
                return userId;
            }
        }

        [SerializeField]
        Toggle selectionToggle;

        //private void OnEnable()
        //{
        //    selectionToggle.onValueChanged.AddListener(Coordinator.instance.meetingInterface.OnMemberSelectionChange);
        //}

        protected override void OnDisableOverride()
        {
            selectionToggle.onValueChanged.RemoveAllListeners();
        }

        public bool Selected
        {
            get
            {
                return selectionToggle.isOn;
            }
        }

        public void ToggleSelection(bool val)
        {
            selectionToggle.isOn = val;
        }

        public void OnMaximumSelectionSet()
        {
            if (!selectionToggle.isOn)
                selectionToggle.interactable = false;
        }

        public void OnMaximumSelectionReset()
        {
            selectionToggle.interactable = true;
        }

        public void Init(string _name, string _email, int _userId)
        {
            nameField.text = _name;
            emailField.text = _email;
            userId = _userId;
            selectionToggle.isOn = false;
            selectionToggle.interactable = true;
        }
    }
}
