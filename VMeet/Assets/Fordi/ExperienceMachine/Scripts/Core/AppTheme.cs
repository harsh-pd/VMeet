using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRExperience.Core
{
    public interface IAppTheme
    {
        Theme SelectedTheme { get; }
    }

    public class AppTheme : MonoBehaviour, IAppTheme
    {
        [SerializeField]
        Theme m_selectedTheme;

        public Theme SelectedTheme { get { return m_selectedTheme; } }
    }
}
