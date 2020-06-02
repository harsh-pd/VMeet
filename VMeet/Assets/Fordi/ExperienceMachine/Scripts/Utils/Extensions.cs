using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fordi.Core;

namespace Fordi.Common
{
    public static class Extensions
    {
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static T[] RemoveAll<T>(this T[] data, Predicate<T> match)
        {
            int validElementCount = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (match(data[i]))
                    validElementCount++;
            }

            T[] result = new T[validElementCount];
            int x = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (match(data[i]))
                    result[x++] = data[i];
            }
            return result;
        }

        public static T[] Concatenate<T>(this T[] first, T[] second)
        {
            if (first == null)
            {
                return second;
            }
            if (second == null)
            {
                return first;
            }

            return first.Concat(second).ToArray();
        }

        public static float Magnitude(this Color color)
        {
            return Vector3.Magnitude(new Vector3(color.r, color.g, color.b));
        }

        public static string Style(this string str, string style)
        {
            return "<style=" + style + ">" + str + "</style>";
        }
    }
}
