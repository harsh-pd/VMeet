using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fordi.Common;
using Fordi.Core;

namespace Fordi.UI.MenuControl
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
