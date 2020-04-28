using AL.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRExperience.UI;

namespace Fordi.UI
{
    [RequireComponent(typeof(ToggleGroup))]
    public class ToggleGroupFix : MonoBehaviour
    {
        [SerializeField]
        private ToggleGroup m_Group;
        [SerializeField]
        private bool m_switchOffAllowed = false;

        private List<Toggle> m_toggles = new List<Toggle>();
        private Toggle m_activeToggle = null;

        private IEnumerator Start()
        {
            if (m_switchOffAllowed)
                yield break;

            yield return null;

            var toggleListMember = typeof(ToggleGroup).GetField("m_Toggles", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (toggleListMember == null)
                throw new System.Exception("UnityEngine.UI.ToggleGroup Has become public, so this code must be changed!");

            var toggles = toggleListMember.GetValue(m_Group) as List<Toggle>;
            if (toggles != null)
            {
                foreach (Toggle tt in toggles)
                {
                    m_toggles.Add(tt);
                    if (tt.isOn)
                        m_activeToggle = tt;
                }
            }

        }

        private void OnEnable()
        {
            VRTabInteraction.TabChangeInitiated += TabChangeInitiated;
            TabInteraction.TabChangeInitiated += TabChangeInitiated;
            StartCoroutine(ConfigureToggleGroup());
        }

        private IEnumerator ConfigureToggleGroup()
        {
            yield return null;
            m_Group.allowSwitchOff = m_switchOffAllowed;
        }

        private void TabChangeInitiated(object sender, EventArgs e)
        {
            FindActiveToggle();
            m_Group.allowSwitchOff = true;
            //Debug.LogError("Switch off allowed");
        }

        private void OnDisable()
        {
            VRTabInteraction.TabChangeInitiated -= TabChangeInitiated;
            TabInteraction.TabChangeInitiated -= TabChangeInitiated;
            m_Group.allowSwitchOff = true;
        }

        private void FindActiveToggle()
        {
            //Debug.LogError("FindActiveToggle");
            foreach (var item in m_toggles)
            {
                if (item.isOn)
                {
                    //if (item != m_activeToggle)
                    //{
                    //    Debug.LogError("Old active Toggle value changing");
                    //}
                    m_activeToggle = item;
                    return;
                }
            }
        }
    }
}