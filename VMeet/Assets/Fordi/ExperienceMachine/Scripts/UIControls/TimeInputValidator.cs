using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using VRExperience.UI.MenuControl;

namespace Fordi.UI
{
    public enum TimeType
    {
        HOUR,
        MINUTE,
    }

    [CreateAssetMenu(fileName = "TimeInutValidator", menuName = "Input Validator/Time Input")]
    public class TimeInputValidator : TMP_InputValidator
    {
        public ITimeForm m_timeForm = null;

        const char emptyChar = '\0';
        public TimeType timeType;

        protected char Validate(string text, int pos, char ch, int maximumValue, int minimumValue)
        {
            if (text.Length == 2 || !char.IsDigit(ch))
                return emptyChar;


            int digit = (int)char.GetNumericValue(ch);

            if (pos == 0)
            {
                if (digit > maximumValue / 10)
                    return emptyChar;
                return ch;
            }
            else if (pos == 1)
            {
                int expectedValue = 0;
                var expectedValueString = text + ch;
                Int32.TryParse(expectedValueString, out expectedValue);

                if (expectedValue > maximumValue || expectedValue < minimumValue)
                    return emptyChar;
                else
                    return ch;
            }

            return emptyChar;
        }

        public override char Validate(ref string text, ref int pos, char ch)
        {
            if (m_timeForm == null)
                return emptyChar;

            if (ch == emptyChar)
            {
                //Debug.LogError("Empty char coming as validator input");
                return emptyChar;
            }


            //Debug.Log(ch);
            //return ch;

            string updatedText = text + ch;
            int val = Convert.ToInt16(updatedText);
            string timeValue;
            string dateTimeString;
            int maximumVal;
            int maximumHour;

            switch (timeType)
            {
                case TimeType.HOUR:
                    if (Validate(text, pos, ch, 23, 0).Equals(emptyChar))
                        return emptyChar;
                    maximumVal = GetMaximumValue(updatedText, 0, 23);
                    if (val > maximumVal)
                        return emptyChar;
                    if (maximumVal >= 12)
                    {
                        maximumVal -= 12;
                        timeValue = maximumVal + ":" + m_timeForm.Minute + ":00 PM";
                    }
                    else if (maximumVal == 12)
                        timeValue = "12:" + m_timeForm.Minute + ":00 PM";
                    else
                        timeValue = maximumVal + ":" + m_timeForm.Minute + ":00 AM";

                    dateTimeString = m_timeForm.Date + " " + timeValue;

                    //Debug.Log(dateTimeString);
                    if (Convert.ToDateTime(dateTimeString) < DateTime.Now)
                        return emptyChar;
                    break;
                case TimeType.MINUTE:
                    if (Validate(text, pos, ch, 59, 0).Equals(emptyChar))
                        return emptyChar;
                    maximumVal = GetMaximumValue(updatedText, 0, 59);
                    if (val > maximumVal)
                        return emptyChar;
                    maximumHour = m_timeForm.Hour;
                    timeValue = (maximumHour < 13 ? maximumHour : maximumHour - 12) + ":" + maximumVal + (maximumHour < 12 ? ":00 AM" : ":00 PM");
                    dateTimeString = m_timeForm.Date + " " + timeValue;
                    //Debug.Log(dateTimeString);
                    if (Convert.ToDateTime(dateTimeString) < DateTime.Now)
                        return emptyChar;
                    break;
            }
            return ch;
        }

        private int GetMaximumValue(string input, int minimum, int maximum)
        {
            if (input.Length == 0)
                return maximum;

            var val = Convert.ToInt16(input);
            if (input.Length == 1)
                return Mathf.Clamp(val * 10 + 9, minimum, maximum);
            else
                return val;
        }
    }
}