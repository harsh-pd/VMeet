using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fordi.UI
{
    [RequireComponent(typeof(ToggleGroup))]
    public class ToggleGroupFix : MonoBehaviour
    {
        private ToggleGroup m_Group;
        private List<Toggle> m_toggles = new List<Toggle>();
        private Toggle m_activeToggle = null;

        private void Start()
        {
            m_Group = this.GetComponent<ToggleGroup>();

            if (m_Group.allowSwitchOff)
            {
                return;
            }
            m_Group.allowSwitchOff = true;


            var toggleListMember = typeof(ToggleGroup).GetField("m_Toggles", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (toggleListMember == null)
                throw new System.Exception("UnityEngine.UI.ToggleGroup Has become public, so this code must be changed!");

            m_toggles = toggleListMember.GetValue(m_Group) as List<Toggle>;
            if (m_toggles != null)
            {
                foreach (Toggle tt in m_toggles)
                {
                    tt.onValueChanged.AddListener((bool on) => { ToggleChanged(tt, on); });
                    if (tt.isOn)
                        m_activeToggle = tt;
                }
            }

        }

        private void OnEnable()
        {
            StartCoroutine(ActivateOldSelectedToggle());
        }

        private void FindActiveToggle()
        {
            foreach (var item in m_toggles)
            {
                if (item.isOn)
                {
                    m_activeToggle = item;
                    return;
                }
            }
        }

        private IEnumerator ActivateOldSelectedToggle()
        {
            yield return null;
            yield return null;
            m_activeToggle.isOn = true;
        }

        private void ToggleChanged(Toggle tt, bool on)
        {
            if (on)
                m_activeToggle = tt;
        }
    }
}