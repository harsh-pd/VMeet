using Fordi.Common;
using Fordi.Core;
using Fordi.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fordi.Platforms
{
    public interface IPlatformModule
    {
        IPlayer Player { get; }
        IUserInterface UserInterface { get; }
        Platform Platform { get; }
    }

    public class PlatformModule : MonoBehaviour, IPlatformModule
    {
        [SerializeField]
        private Platform m_platform;

        public IPlayer Player { get; private set; }

        public IUserInterface UserInterface { get; private set; }

        public Platform Platform { get { return m_platform; } }

        private IExperienceMachine m_experienceMachine = null;

        private void Awake()
        {
            Player = GetComponentInChildren<IPlayer>(true);
            UserInterface = GetComponentInChildren<IUserInterface>(true);
            m_experienceMachine = IOC.Resolve<IExperienceMachine>();
            m_experienceMachine.RegisterPlatform(this);
        }
    }
}
