using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace AL.Theme
{
    public class AppTheme : MonoBehaviour
    {
        [SerializeField]
        Theme selectedTheme;
        public Theme SelectedTheme
        {
            get
            {
                return selectedTheme;
            }
        }
    }
}
