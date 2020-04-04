using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace AL
{
    public static class Extensions
    {
        #region TransformExtensions

        /// <summary>
        /// It will copy local position, width, height, pivot and anchors from reference
        /// </summary>
        /// <param name="target"></param>
        /// <param name="reference"></param>
        public static void Copy(this RectTransform target, RectTransform reference)
        {
            target.sizeDelta = reference.sizeDelta;
            target.pivot = reference.pivot;
            target.anchorMax = reference.anchorMax;
            target.anchorMin = reference.anchorMin;
            target.localPosition = reference.localPosition;
        }

        public static void Copy(this Transform target, Transform reference)
        {
            target.parent = reference.parent;
            target.localPosition = reference.localPosition;
            target.localRotation = reference.localRotation;
            target.localScale = reference.localScale;
        }

        #endregion

        public static string Style(this string str, string style)
        {
            return "<style=" + style + ">" + str + "</style>";
        }

        public static string MakeupSpace(this string str, int length)
        {
            for (int i = str.Length; i < length; i++)
                str += " ";
            return str;
        }
    }
}
