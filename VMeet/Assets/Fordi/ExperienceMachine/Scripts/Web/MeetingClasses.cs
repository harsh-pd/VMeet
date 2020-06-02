using Cornea.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fordi.Common;
using Fordi.Core;

namespace Fordi.Meeting
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

    public class Meeting
    {
        public string fileToBeReviewed = "NA";
        public string fileLocation = "NA";
        public string meetingTime = DateTime.UtcNow.ToShortDateString();
        public int meetingDurationInMinutes = 0;
        public int modelUploadTimeInSeconds = 0;
        public bool status = true;
        public int userid = 0;
        public string createddatetime;
        public List<MeetingParticipant> MeetingParticipants = new List<MeetingParticipant>();
        public string description = "Untitled";
    }

    public class MeetingParticipant
    {
        //public int Id = 0;
        //public int MeetingId = 0;
        public int InvitedByUserId = 0;
        public string InvitedDateTime = "2019-01-13T07:04:56.810Z";
        public int ParticipantUserId = 0;
        public int Status = 1;
        public string StatusUpdatedDateTime = "2019-01-13T07:04:56.810Z";

        public MeetingParticipant(int _ParticipantUserId)
        {
            ParticipantUserId = _ParticipantUserId;
            InvitedByUserId = IOC.Resolve<IWebInterface>().UserInfo.id;
        }
    }
}
