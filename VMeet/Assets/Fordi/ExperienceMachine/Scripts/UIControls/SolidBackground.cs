using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fordi.Common;
using Fordi.UI.MenuControl;

namespace Fordi.UI
{
    public class SolidBackground : MonoBehaviour
    {
        public void Enlarge()
        {
            transform.localScale = new Vector3(28, 7, 48);
        }
    }
}
