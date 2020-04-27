using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Fordi.UI
{
    [AddComponentMenu("UI/Toggle Group", 32), DisallowMultipleComponent]
    public class FordiToggleGroup : ToggleGroup
    {
        [SerializeField]
        private bool m_switchOffAllowed = false;

        protected List<Toggle> m_Toggles = new List<Toggle>();

        public new bool allowSwitchOff
        {
            get
            {
                return this.m_switchOffAllowed;
            }
            set
            {
                this.m_switchOffAllowed = value;
            }
        }

        protected FordiToggleGroup()
        {
        }

        protected override void Start()
        {
            this.EnsureValidState();
            base.Start();
        }

        private void ValidateToggleIsInGroup(Toggle toggle)
        {
            bool flag = toggle == null || !this.m_Toggles.Contains(toggle);
            if (flag)
            {
                throw new ArgumentException(string.Format("Toggle {0} is not part of ToggleGroup {1}", new object[]
                {
                    toggle,
                    this
                }));
            }
        }

        public new void NotifyToggleOn(Toggle toggle, bool sendCallback = true)
        {
            this.ValidateToggleIsInGroup(toggle);
            for (int i = 0; i < this.m_Toggles.Count; i++)
            {
                bool flag = this.m_Toggles[i] == toggle;
                if (!flag)
                {
                    if (sendCallback)
                    {
                        this.m_Toggles[i].isOn = false;
                    }
                    else
                    {
                        this.m_Toggles[i].SetIsOnWithoutNotify(false);
                    }
                }
            }
        }

        public new void UnregisterToggle(Toggle toggle)
        {
            bool flag = this.m_Toggles.Contains(toggle);
            if (flag)
            {
                this.m_Toggles.Remove(toggle);
            }
        }

        public new void RegisterToggle(Toggle toggle)
        {
            bool flag = !this.m_Toggles.Contains(toggle);
            if (flag)
            {
                this.m_Toggles.Add(toggle);
            }
        }

        public void EnsureValidState()
        {
            bool flag = !this.allowSwitchOff && !this.AnyTogglesOn() && this.m_Toggles.Count != 0;
            if (flag)
            {
                this.m_Toggles[0].isOn = true;
                this.NotifyToggleOn(this.m_Toggles[0], true);
            }
        }

        public new bool AnyTogglesOn()
        {
            return this.m_Toggles.Find((Toggle x) => x.isOn) != null;
        }

        public new IEnumerable<Toggle> ActiveToggles()
        {
            return from x in this.m_Toggles
                   where x.isOn
                   select x;
        }

        public new void SetAllTogglesOff(bool sendCallback = true)
        {
            bool allowSwitchOff = this.m_switchOffAllowed;
            this.m_switchOffAllowed = true;
            if (sendCallback)
            {
                for (int i = 0; i < this.m_Toggles.Count; i++)
                {
                    this.m_Toggles[i].isOn = false;
                }
            }
            else
            {
                for (int j = 0; j < this.m_Toggles.Count; j++)
                {
                    this.m_Toggles[j].SetIsOnWithoutNotify(false);
                }
            }
            this.m_switchOffAllowed = allowSwitchOff;
        }
    }
}
