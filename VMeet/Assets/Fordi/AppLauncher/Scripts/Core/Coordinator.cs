using AL.Theme;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AL.X
{
    public class Coordinator : MonoBehaviour {

        public static Coordinator instance;

        [Header("Scripts")]
        public AppTheme appTheme;
        public Settings settings;

        [Header("Objects")]
        public TMPro.TextMeshProUGUI debugText;


        private void Awake()
        {
            instance = this;
        }

        public void OnApplicationQuit()
        {
#if !UNITY_EDITOR
            System.Diagnostics.Process.GetCurrentProcess().Kill();
#endif
        }

    }
}
