using Fordi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fordi.Core
{
    public interface IAppTheme
    {
        Theme GetSelectedTheme(Platform platform);
    }

    public class AppTheme : MonoBehaviour, IAppTheme
    {
        [SerializeField]
        private Theme m_desktopTheme, m_vrTheme, m_arTheme;

        public Theme GetSelectedTheme(Platform platform)
        {
            switch (platform)
            {
                case Platform.DESKTOP:
                    return m_desktopTheme;
                case Platform.VR:
                    return m_vrTheme;
                default:
                    return null;
            }
        }
    }
}
