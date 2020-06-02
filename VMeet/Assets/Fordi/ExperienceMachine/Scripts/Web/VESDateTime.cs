using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fordi.Meetings
{ 
    public class VESDateTime
    {
        private const int roomSetupTimeInMinutes = 10;

        public static float GetIntervalInMinutes(DateTime dateTime)
        {
            TimeSpan span = dateTime < DateTime.Now ? -DateTime.Now.Subtract(dateTime) : dateTime.Subtract(DateTime.Now);
            return (float)span.TotalMinutes;
        }

        public static bool IsMeetingOver(DateTime dateTime, int duration)
        {
            var meetingEndTime = dateTime.Add(new TimeSpan(0, duration, 0));
            return meetingEndTime < DateTime.Now;
        }

        public static bool MeetingWithinNextFiveMinutes(DateTime dateTime, int duration)
        {
            var meetingEndTime = dateTime.Add(new TimeSpan(0, duration, 0));
            var roomSetupTime = dateTime.Subtract(new TimeSpan(0, roomSetupTimeInMinutes, 0));
            //Debug.Log("roomSetupTime : " + roomSetupTime.ToShortTimeString() + " " + DateTime.Now.ToShortTimeString() + " meetingEndTime " + (meetingEndTime >= DateTime.Now));
            return roomSetupTime <= DateTime.Now && meetingEndTime >= DateTime.Now;
        }

        public static bool MeetingScheduledNow(DateTime dateTime, int duration)
        {
            var meetingEndTime = dateTime.Add(new TimeSpan(0, duration, 0));
            return dateTime <= DateTime.Now && meetingEndTime >= DateTime.Now;
        }

        public static string GetShortTimeZone()
        {
            var longForm = TimeZone.CurrentTimeZone.StandardName;
            string shortForm = "";
            for (int i = 0; i < longForm.Length; i++)
                if (i == 0 || longForm[i-1].ToString().Equals(" "))
                    shortForm += longForm[i];
            return shortForm;
        }

        public static string GetSchedule(DateTime dateTime, int duration, bool host)
        {
            var meetingEndTime = dateTime.Add(new TimeSpan(0, duration, 0));
            var interval = GetIntervalInMinutes(dateTime);

            if (IsMeetingOver(dateTime, duration) && interval > -1440)
                return "Was scheduled today";
            else if (IsMeetingOver(dateTime, duration) && interval > -2880)
                return "Was scheduled yesterday";
            else if (IsMeetingOver(dateTime, duration))
                return "Was scheduled on " + dateTime.ToShortDateString() + " at " + dateTime.ToShortTimeString() + " " + GetShortTimeZone();
            else if (MeetingScheduledNow(dateTime, duration))
                return "Scheduled now.";
            else if (MeetingWithinNextFiveMinutes(dateTime, duration))
                return "Scheduled in less than five minutes.";
            else if (interval < 60)
                return "Scheduled in less than an hour at " + dateTime.ToShortTimeString() + " " + GetShortTimeZone();
            else if (interval < 1440)
                return "Scheduled today at " + dateTime.ToShortTimeString() + " " + GetShortTimeZone();
            else if (interval < 2880)
                return "Scheduled tomorrow at " + dateTime.ToShortTimeString() + " " + GetShortTimeZone();
            else
                return "Scheduled on " + dateTime.ToShortDateString() + " at " + dateTime.ToShortTimeString() + " " + GetShortTimeZone();
        }
    }
}
