using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRExperience.Core;

namespace VRExperience.Meeting
{
    public enum MeetingFilter
    {
        All,
        Accepted,
        Invited,
        Rejected,
        Created
    }

    public enum MeetingCategory
    {
        CREATED,
        INVITED,
        REJECTED,
        ACCEPTED
    }

    public class MeetingAPIResponse
    {
        public List<MeetingInfo> MeetingDetailsByParticipantUser = new List<MeetingInfo>();
        public List<MeetingInfo> MeetingDetailsByCreatedUser = new List<MeetingInfo>();
    }

    [Serializable]
    public class MeetingInfo
    {
        public int Id = 8;
        //public string MeetingGUID = "c0233a62-deff-42ad-aa5a-f9626ef514bd";
        public string MeetingNumber = "string";
        //public string MeetingPassword = "string";
        public string FileToBeReviewed = "string";
        public string FileLocation = "string";
        public DateTime MeetingTime;
        public int MeetingDurationInMinutes = 0;
        public string Description = "";
        //public int ModelUploadTimeInSeconds = 0;
        //public bool Status = true;
        //public int CreatedUserId = 1;
        //public string CreatedDateTime = "2018-12-22T14:39:55.763";
        //public int ModifiedUserId = 1;
        //public string ModifiedDateTime = "2018-12-22T14:39:55.763";
        public MeetingCategory meetingType = MeetingCategory.INVITED;
    }

    [Serializable]
    public class MeetingResource : ExperienceResource
    {
        public MeetingInfo MeetingInfo;
    }

    [Serializable]
    public class UserResource : ExperienceResource
    {
        public UserInfo UserInfo;
    }

    [Serializable]
    public class MeetingGroup : ResourceComponent
    {
        public MeetingResource[] Resources;
    }

    [Serializable]
    public class UserGroup : ResourceComponent
    {
        public UserResource[] Resources;
    }
}
