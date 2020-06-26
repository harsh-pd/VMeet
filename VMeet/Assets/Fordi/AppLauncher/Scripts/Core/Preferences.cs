using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AL
{
    [CreateAssetMenu(fileName = "NewALSetting", menuName = "AL Settings")]
    public class Preferences : ScriptableObject
    {
        [Header("Update")]
        public int updateInterval = 120;
        [Header("Repository")]
        public string owner = "harsh-priyadarshi";
        [Header("Download")]
        public int bufferSize = 8192;
    }
}
