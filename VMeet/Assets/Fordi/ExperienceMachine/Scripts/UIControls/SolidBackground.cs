using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRExperience.Common;
using VRExperience.UI.MenuControl;

namespace VRExperience.UI
{
    public class SolidBackground : MonoBehaviour
    {
        public void Enlarge()
        {
            transform.localScale = new Vector3(28, 7, 48);
        }
    }
}
