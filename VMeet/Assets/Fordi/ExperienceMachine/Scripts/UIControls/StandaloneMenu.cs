using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRExperience.Common;
using VRExperience.Core;

namespace VRExperience.UI.MenuControl
{
    public class StandaloneMenu : MonoBehaviour
    {
        public void OpenMenu()
        {
            IOC.Resolve<IExperienceMachine>().OpenSceneMenu();
            Destroy(gameObject);
        }
    }
}
