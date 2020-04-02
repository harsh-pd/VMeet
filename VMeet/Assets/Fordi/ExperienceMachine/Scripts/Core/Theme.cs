using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRExperience.Core
{
    [CreateAssetMenu(fileName = "New Theme", menuName = "App Theme")]
    public class Theme : ScriptableObject
    {
        #region UI
        public Color panelInteractionOutline;
        public Color baseColor;
        public Color objectHighlightColor = new Color32(255, 99, 71, 255);
        public Color buttonHighlightTextColor = Color.white;
        public Color buttonNormalTextColor = new Color32(84, 84, 84, 255);
        public Color toggleSelectionColor = new Color32(0, 218, 231, 255);
        public Color toggleNormalColor = Color.white;
        public Color buttonDisabledTextColor = new Color32(0x32, 0x32, 0x32, 0xFF);
        public Color ErrorColor = new Color32(255, 0, 42, 255);
        public Color InputFieldSelection = new Color32(5, 5, 10, 255);
        public Color InputFieldNormalColor = new Color32(12, 12, 18, 255);
        #endregion
    }
}
