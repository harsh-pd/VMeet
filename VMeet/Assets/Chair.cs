using Fordi.Common;
using Fordi.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fordi.VR
{
    public class Chair : MonoBehaviour
    {
        [SerializeField]
        private Transform m_playerAnchor;

        private void Awake()
        {
            var experienceMachinie = IOC.Resolve<IExperienceMachine>();
            var currentExperience = experienceMachinie.GetExperience(experienceMachinie.CurrentExperience);
            if (currentExperience != null)
            {
                currentExperience.AddTeleportAnchor(m_playerAnchor);
            }
        }
    }
}
