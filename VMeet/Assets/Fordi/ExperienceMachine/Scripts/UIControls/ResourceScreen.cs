using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fordi.UI.MenuControl
{
    public class ResourceScreen : MenuScreen
    {
        protected override void OnDisable()
        {
            base.OnDisable();
            ShowPreview(null);
            ShowTooltip("");
        }
    }
}
