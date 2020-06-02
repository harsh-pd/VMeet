using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using LitJson;
using System;
using UnityEngine.UI;
using System.Net;
using System.IO;
using UnityEngine.Events;
using System.Text;
using Fordi.Meeting;
using Fordi.UI.MenuControl;
using Fordi.Common;
using Fordi.Core;

namespace Cornea.Web
{
    public interface IWebInterface
    {
        string AccessToken { get; }
        void ValidateUserLogin(string organization, string username, string password);
        void RegisterRequestFailure(string errorMessage, APIRequest req);
        void RemoveRequest(APIRequest req);
        APIRequest ListAllMeetingDetails(MeetingFilter meetingFilter);
        APIRequest CreateMeeting(string meetingJson);
        APIRequest AcceptMeeting(int meetingId);
        APIRequest RejectMeeting(int meetingId);
        APIRequest CancelMeeting(int meetingId);
        List<UserInfo> ParseUserListJson(string userListJson);
        List<MeetingInfo> ParseMeetingListJson(string meetingListJson, MeetingCategory category);
        ExperienceResource[] GetResource(ResourceType resourceType, string category);
        void GetCategories(ResourceType type, UnityAction<ResourceComponent[]> done, bool requireWebRefresh = false);
        UserInfo UserInfo { get; }
    }

    public enum APIRequestType
    {
        Check_License_Valid,
        Activate_User_License,
        Validate_User_Login,
        Get_Login_User_Details,
        Get_User_Information,
        Get_New_AccessToken,
        Get_Users_By_Organization,
        Upload_File,
        Save_Meeting,
        List_All_Meeting_Details,
        Accept_Meeting,
        Reject_Meeting,
        Cancel_Meeting,
        Authenticate,
        Download_File
    }

    public delegate void OnCompleteAction(bool isNetworkError, string text);

    public static class APIRequestExtentions
    {
        public static T OnRequestComplete <T> (this T apiReq, OnCompleteAction action) where T : APIRequest
        {
            apiReq.AddOnCompleteListener(action);
            return apiReq;
        }
        
        public static string AppendParameters<T>(this string url, Dictionary<string, T> parameters)
        {
            url += "?";
            int i = 0;
            foreach (KeyValuePair<string, T> entry in parameters)
            {
                url += entry.Key + "=" + entry.Value.ToString();
                if (i < parameters.Count - 1)
                    url += "&";
                i++;
            }

            return url;
        }
    }

    [Serializable]
    public struct Intermediates
    {
        [SerializeField]
        private IntermediatePanel loadingPanel, networkErrorPanel;
        [SerializeField]
        private TextMeshProUGUI networkErrorText;

        Intermediates(IntermediatePanel _loadingPanel, IntermediatePanel _networkErrorPanel)
        {
            loadingPanel = _loadingPanel;
            networkErrorPanel = _networkErrorPanel;
            networkErrorText = networkErrorPanel.DynamicText;
        }

        public void SwtichToLoader(Action action)
        {
            loadingPanel.Activate(action);
            networkErrorPanel.Deactivate();
        }

        public void SwtichToError(Action action)
        {
            //Debug.Log("SwtichToError");
            networkErrorPanel.Activate(action);
            loadingPanel.Deactivate();
        }

        public void Deactivate()
        {
            networkErrorPanel.Deactivate();
            loadingPanel.Deactivate();
            networkErrorText.text = "";
        }

        public void SwtichToError(Action action, string errorMessage)
        {
            //Debug.Log("SwitchToError");
            networkErrorPanel.Activate(action);
            loadingPanel.Deactivate();
            networkErrorText.text = errorMessage;
        }
    }

    public enum NetworkState
    {
        IDLE,
        PROGRESS,
        ERROR
    }

    //[Serializable]
    //public class VESNetworkInterface
    //{
    //    public Action postMacAddressAction;
    //    public TextMeshProUGUI networkErrorText;

    //    private NetworkState state = NetworkState.IDLE;
    //    private List<APIRequest> failedRequestsStack = new List<APIRequest>();
    //    private List<Intermediates> intermediatesStack = new List<Intermediates>();

    //    public void SetIntermediatePanels(Intermediates _intermediates)
    //    {
    //        if (intermediatesStack.Contains(_intermediates))
    //            intermediatesStack.Remove(_intermediates);
    //        intermediatesStack.Add(_intermediates);
    //    }

    //    public void ResetIntermediatePanels(Intermediates _intermediates)
    //    {
    //        if (intermediatesStack.Contains(_intermediates))
    //            intermediatesStack.Remove(_intermediates);
    //    }

    //    public void DeactivateErrorScreen()
    //    {
    //        state = NetworkState.IDLE;
    //        postMacAddressAction = null;
    //        if (intermediatesStack.Count > 0)
    //        {
    //            intermediatesStack[intermediatesStack.Count - 1].Deactivate();
    //        }
    //    }

    //    public void RemoveRequest(APIRequest req)
    //    {
    //        if (failedRequestsStack.Contains(req))
    //        {
    //            failedRequestsStack.Remove(req);
    //            if (failedRequestsStack.Count == 0)
    //                DeactivateErrorScreen();
    //        }
    //        else if (state != NetworkState.IDLE)
    //        {
    //            DeactivateErrorScreen();
    //        }
    //    }

    //    //public void ActivateErrorScreen(string errorMessage, APIRequest req)
    //    //{
    //    //    int reqIndex = failedRequestsStack.FindIndex(item => item.requestType == req.requestType);

    //    //    if (reqIndex != -1)
    //    //        failedRequestsStack[reqIndex] = req;
    //    //    else
    //    //        failedRequestsStack.Add(req);

    //    //    if (intermediatesStack.Count > 0)
    //    //    {
    //    //        state = NetworkState.ERROR;
    //    //        intermediatesStack[intermediatesStack.Count - 1].SwtichToError(() => req.Kill(), errorMessage);
    //    //    }
              
    //    //}

    //    /// <summary>
    //    /// Only to be used in case of mac address fetch failure
    //    /// </summary>
    //    /// <param name="postMacAddressAction"></param>
    //    public void ActivateErrorScreen(Action _postMacAddressAction)
    //    {
    //        Debug.Log("ActivateErrorScreen");
    //        postMacAddressAction = _postMacAddressAction;
    //        if (intermediatesStack.Count > 0)
    //        {
    //            state = NetworkState.ERROR;
    //            intermediatesStack[intermediatesStack.Count - 1].SwtichToError(null);
    //        }
    //    }

    //    public void ShowUnderProgress(Action action)
    //    {
    //        state = NetworkState.PROGRESS;
    //        intermediatesStack[intermediatesStack.Count - 1].SwtichToLoader(action);
    //    }

    //    public void  MacAddressFetched()
    //    {
    //        if (postMacAddressAction != null)
    //            postMacAddressAction.Invoke();
    //    }

    //    public void Refresh()
    //    {
    //        for (int i = 0; i < failedRequestsStack.Count; i++)
    //        {
    //            Debug.Log("Refreshing: " + failedRequestsStack[i].requestType.ToString());
    //            var req = failedRequestsStack[i].Refresh();
    //            failedRequestsStack[i] = req;
    //        }
    //    }

    //    public void AbortAll()
    //    {
    //        for (int i = 0; i < failedRequestsStack.Count; i++)
    //            failedRequestsStack[i].Kill();
    //    }
    //}

    public class WebInterface : MonoBehaviour, IWebInterface
    {
        public static string vesApiBaseUrl = "https://corneaapi.caresoftglobal.com";
        //public const string vesApiBaseUrl = "http://vmeet.work";
        public const string isTenantAvailable = "/api/services/app/Account/IsTenantAvailable";
        public const string tokenAuth = "/api/TokenAuth/Authenticate";

        public const string validateUserLogin = "/api/services/app/UserLogin/ValidateUserLogin";
        public const string activateUserLicense = "/api/services/app/Licenses/ActivateUserLicense";
        public const string checkLicenseValid = "/api/services/app/Licenses/CheckLicenseValid";
        public const string getLoginUserDetails = "/api/services/app/User/GetUserForEdit";
        public const string getUserInformation = "/api/services/app/User/GetUserInformation";
        public const string getUsersByOrganization = "/api/services/app/User/GetUsersByTenant";
        public const string token = "/api/token";
        public const string saveMeeting = "/api/services/app/Meetings/CreateOrEdit";
        public const string listAllMeetingDetails = "/api/services/app/MeetingParticipants/getAllMeetingDetails";
        public const string acceptMeeting = "/api/services/app/MeetingParticipants/acceptMeeting";
        public const string cancelMeeting = "/api/services/app/MeetingParticipants/cancelMeeting";
        public const string rejectMeeting = "/api/services/app/MeetingParticipants/rejectMeeting";
        public const string uploadFile = "/Meeting/UploadFileToBlobAsync";
        public const string downloadFile = "/Meeting/DownloadFileFromBlobAsync";

        private const string APP_CONFIG = "app.config";

        [SerializeField]
        private Button meetingButton;

        [Header("Web Interface Input")]
        public TMP_InputField organizationCode;
        public TMP_InputField userName;
        public TMP_InputField password;
        public TMP_InputField licenseKey;
        public TMP_InputField licenseActivationEmail;

        [Header("User Info")]
        public TMP_InputField Name;
        public TMP_InputField Email;
        public TMP_InputField OrganisationId;

        private static UserInfo m_userInfo = new UserInfo();

        private List<MeetingGroup> m_meetings = new List<MeetingGroup>();
        public MeetingGroup[] Meetings { get { return m_meetings.ToArray(); } }


        private List<UserGroup> m_users = new List<UserGroup>();
        public UserGroup[] Users { get { return m_users.ToArray(); } }


        [HideInInspector]
        private static string access_token = "";

        private bool m_requireMeetingListRefresh = true;

        private string MacAddress {
            get
            {
                //return "6dc53fb71be5e6b9762a4053e49fa0f28b3f54a4";
                //Debug.LogError(SystemInfo.deviceUniqueIdentifier);
                return SystemInfo.deviceUniqueIdentifier;
            }
        }

        public string AccessToken { get { return access_token; } }

        public UserInfo UserInfo { get { return m_userInfo; } }

        private IUserInterface m_vrMenu;

        private IExperienceMachine m_experienceMachine;

        private List<APIRequest> m_failedRequestStack = new List<APIRequest>();

        private void Awake()
        {
            m_vrMenu = IOC.Resolve<IUserInterface>();
            m_experienceMachine = IOC.Resolve<IExperienceMachine>();

            var configFilePath = Path.Combine(Application.persistentDataPath, APP_CONFIG);
            if (File.Exists(configFilePath))
            {
                vesApiBaseUrl = File.ReadAllText(configFilePath);
                Debug.LogError("Using: " + vesApiBaseUrl);
            }
            else
            {
                File.WriteAllText(configFilePath, vesApiBaseUrl);
            }
        }

        private void Start()
        {
            //Screen initialization is now handled by individual experiences.
        }

        public void NetworkRefresh()
        {
            throw new NotImplementedException();
            //networkInterface.ShowUnderProgress(() => networkInterface.AbortAll());
            //networkInterface.Refresh();
        }

        private string TruncateString(string input)
        {
            if (input.Length > 88)
            {
                var firstDotIndex = input.IndexOf('.');
                if (firstDotIndex > -1)
                    input = input.Substring(0, firstDotIndex + 1);
                if (input.Length > 88)
                    return input.Substring(0, 88);
            }
            return input;
        }

#region API_REQUESTS

        public void TokenAuthenticate(string organization, string username, string password)
        {
            var jsonString = "{\"UserNameOrEmailAddress\":\"" + username + "\",\"Password\":\"" + password + "\",\"TenancyName\":\"" + organization + "\"}";
            APIRequest loginReq = new APIRequest(vesApiBaseUrl + tokenAuth, UnityWebRequest.kHttpVerbPOST)
            {
                requestType = APIRequestType.Authenticate,
                uploadHandler = new UploadHandlerRaw(Encoding.ASCII.GetBytes(jsonString)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            loginReq.SetRequestHeader("Content-Type", "application/json");
            m_vrMenu.DisplayProgress("Connecting to VMeet server...");
            loginReq.Run(this).OnRequestComplete(
                    (isNetworkError, message) =>
                    {
                        //Debug.LogError(message);
                        try
                        {
                            JsonData tokenAuthResult = JsonMapper.ToObject(message);
                            if (tokenAuthResult["success"].ToString() == "True")
                            {
                                var token = tokenAuthResult["result"]["accessToken"];
                                if (token == null)
                                {
                                    Error error = new Error(Error.E_Exception);
                                    error.ErrorText = "Login failed. Please change your password in web console first.";
                                    m_vrMenu.DisplayResult(error);
                                    //Coordinator.instance.screenManager.OnLoginFailure("Login failed. Please change your password in web console first.");
                                    return;
                                }
                                access_token = token.ToString();
                                ValidateUserLogin(organization, username, password);
                            }
                            else
                            {


                                var loginFailureMessage = TruncateString((string)tokenAuthResult["error"]["message"]);

                                Error error = new Error(Error.E_Exception);
                                error.ErrorText = loginFailureMessage;
                                m_vrMenu.DisplayResult(error);

                                //Coordinator.instance.screenManager.OnLoginFailure(loginFailureMessage);
                                Debug.Log("Error " + tokenAuthResult["error"]["message"].ToString() + "Ok");
                            }
                        }
                        catch(Exception e)
                        {
                            m_vrMenu.DisplayResult(new Error()
                            {
                                ErrorCode = Error.E_NetworkIssue,
                                ErrorText = e.Message
                            });
                        }
                    }
            );
        }

        private void OpenLicensePage()
        {
            var organizationInput = new MenuItemInfo
            {
                Path = "Organization",
                Text = "Organization",
                Command = "Organization",
                Icon = null,
                Data = TMP_InputField.ContentType.Standard,
                CommandType = MenuCommandType.FORM_INPUT
            };

            var keyInput = new MenuItemInfo
            {
                Path = "License key",
                Text = "License key",
                Command = "License key",
                Icon = null,
                Data = TMP_InputField.ContentType.Standard,
                CommandType = MenuCommandType.FORM_INPUT
            };

            MenuItemInfo[] formItems = new MenuItemInfo[] { organizationInput, keyInput };
            FormArgs args = new FormArgs(formItems, "ACTIVATE LICENSE", "Activate", (inputs) => { Debug.LogError("Form button click"); });
            m_vrMenu.OpenForm(args);
        }

        public void ValidateUserLogin(string organization, string username, string password)
        {
            if (access_token.Equals(""))
            {
                TokenAuthenticate(organization, username, password);
                return;
            }

            var jsonString = "{\"UserNameOrEmailAddress\":\"" + username + "\",\"Password\":\"" + password + "\",\"MacAddress\":\"" + MacAddress + "\",\"TenancyName\":\"" + organization + "\"}";

            APIRequest validateUserLoginRequest = new APIRequest(vesApiBaseUrl + validateUserLogin, UnityWebRequest.kHttpVerbPOST)
            {
                requestType = APIRequestType.Validate_User_Login,
                uploadHandler = new UploadHandlerRaw(Encoding.ASCII.GetBytes(jsonString)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            validateUserLoginRequest.SetRequestHeader("Content-Type", "application/json");
            validateUserLoginRequest.SetRequestHeader("Authorization", "Bearer " + access_token);
            m_vrMenu.DisplayProgress("Validating user...");
            validateUserLoginRequest.Run(this).OnRequestComplete(
                (isNetworkError, message) =>
                {
                    //Debug.LogError(message);
                    JsonData validateUserLoginResult = JsonMapper.ToObject(message);
                    if (validateUserLoginResult["success"].ToString() == "True")
                    {
                        m_experienceMachine.OpenSceneMenu();
                        ZPlayerPrefs.SetInt("LoggedIn", 1);
                        SetUserData(validateUserLoginResult["result"]);
                        m_experienceMachine.OpenSceneMenu();
                        //Coordinator.instance.screenManager.SwitchToHomeScreen();
                    }
                    else
                    {
                        Debug.Log("Error " + validateUserLoginResult["error"]["message"].ToString() + " Ok");
                        if (validateUserLoginResult["error"]["message"].ToString() == "No valid license is activated from this system")
                        {
                            OpenLicensePage();
                            //Coordinator.instance.screenManager.InitScreen(Enums.StartLevel.license_key);
                        }
                        else
                        {
                            var loginFailureMessage = TruncateString((string)validateUserLoginResult["error"]["message"]);

                            Error error = new Error(Error.E_Exception);
                            error.ErrorText = loginFailureMessage;
                            m_vrMenu.DisplayResult(error);

                            //Coordinator.instance.screenManager.OnLoginFailure(loginFailureMessage);
                        }
                    }
                }
            );
        }

        public void ActivateUserLicense()
        {
            var jsonString = "{\"macAddress\":\"" + MacAddress + "\",\"userEmailAddress\":\"" + licenseActivationEmail.text + "\",\"licenseKey\":\"" + licenseKey.text + "\"}";
            byte[] byteData = System.Text.Encoding.ASCII.GetBytes(jsonString.ToCharArray());

            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            };

            APIRequest activateLicenseRequest = new APIRequest(vesApiBaseUrl + activateUserLicense, UnityWebRequest.kHttpVerbPOST)
            {
                requestType = APIRequestType.Activate_User_License,
                uploadHandler = new UploadHandlerRaw(Encoding.ASCII.GetBytes(jsonString)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            activateLicenseRequest.SetRequestHeader("Content-Type", "application/json");
            activateLicenseRequest.SetRequestHeader("Authorization", "Bearer " + access_token);
            activateLicenseRequest.Run(this).OnRequestComplete(
                (isNetworkError, message) =>
                {
                    Debug.Log(message);
                    JsonData activateLicenseResult = JsonMapper.ToObject(message);
                    if (activateLicenseResult["success"].ToString() == "True")
                    {
                        Debug.Log("License has been successfully activated. Ok");

                        Error error = new Error();
                        error.ErrorText = "License has been successfully activated.";
                        m_vrMenu.DisplayResult(error);

                        //Coordinator.instance.screenManager.LicenseValidation(true, "License has been successfully activated.");
                    }
                    else
                    {
                        Error error = new Error(Error.E_NotFound);
                        error.ErrorText = activateLicenseResult["error"]["message"].ToString();
                        m_vrMenu.DisplayResult(error);

                        Debug.LogFormat("Error", activateLicenseResult["error"]["message"].ToString(), "Ok");

                        //Coordinator.instance.screenManager.LicenseValidation(false, (string)activateLicenseResult["error"]["message"]);
                    }
                }
            );
        }

        public APIRequest GetUsersByOrganization()
        {
            var parameters = new Dictionary<string, int>
            {
                { "tenantId", m_userInfo.organizationId }
            };
            var url = vesApiBaseUrl + getUsersByOrganization;
            url = url.AppendParameters(parameters);
            Debug.Log(url);
            APIRequest getUserListReq = APIRequest.Prepare(url, APIRequestType.Get_Users_By_Organization);
            getUserListReq.Run(this).OnRequestComplete(
                (isNetworkError, message) =>
                {
                    //Debug.LogError(message);
                    //ParseUserListJson(message);
                }
            );
            return getUserListReq;
        }

        public List<UserInfo> ParseUserListJson(string userListJson)
        {
            //Debug.Log(userListJson);
            var userData = JsonMapper.ToObject(userListJson);

            List<UserInfo> users = new List<UserInfo>();
            var userList = userData["result"];
            for (int i = 0; i < userList.Count; i++)
            {
                var user = new UserInfo
                {
                    emailAddress = (string)userList[i]["emailAddress"],
                    id = (int)userList[i]["id"],
                    name = (string)userList[i]["name"],
                    //UserRoletype = (int)userData[i]["UserRoletype"]
                };
                users.Add(user);
            }
            return users;
        }

        public APIRequest CreateMeeting(string meetingJson)
        {
            m_vrMenu.DisplayProgress("Submitting details. Please wait...");
            var url = vesApiBaseUrl + saveMeeting;
            var jsonData = Encoding.ASCII.GetBytes(meetingJson);

            APIRequest saveMeetingReq = new APIRequest(url, UnityWebRequest.kHttpVerbPOST)
            {
                requestType = APIRequestType.Save_Meeting,
                uploadHandler = new UploadHandlerRaw(jsonData),
                downloadHandler = new DownloadHandlerBuffer()
            };

            saveMeetingReq.SetRequestHeader("Authorization", "Bearer " + access_token);
            saveMeetingReq.SetRequestHeader("Content-Type", "application/json");

            saveMeetingReq.Run(this).OnRequestComplete(
                (isNetworkError, message) =>
                {
                    JsonData result = JsonMapper.ToObject(message);
                    if (result["success"].ToString() == "True")
                        m_requireMeetingListRefresh = true;
                }
            );

            return saveMeetingReq;
        }

        public void UploadMeetingFile(Byte[] bytes, int meetingId)
        {
            var url = vesApiBaseUrl + uploadFile;
            APIRequest saveMeetingReq = new APIRequest(url, UnityWebRequest.kHttpVerbPOST)
            {
                uploadHandler = new UploadHandlerRaw(bytes),
                downloadHandler = new DownloadHandlerBuffer()
            };
            saveMeetingReq.SetRequestHeader("Content-Type", "application/octet-stream");

            saveMeetingReq.Run(this).OnRequestComplete(
                (isNetworkError, message) =>
                {
                    print(message);
                }
            );
        }

        public APIRequest ListAllMeetingDetails(MeetingFilter meetingFilter)
        {
            var parameters = new Dictionary<string, string>
            {
                { "userId", m_userInfo.id.ToString() },
                { "status", meetingFilter.ToString() }
            };
            var url = vesApiBaseUrl + listAllMeetingDetails;
            url = url.AppendParameters(parameters);
            APIRequest getMeetingListReq = APIRequest.Prepare(url, APIRequestType.List_All_Meeting_Details);
            getMeetingListReq.Run(this).OnRequestComplete((networkIssue, message) =>
            {
                //Debug.LogError(message);
            });

            return getMeetingListReq;
        }

        public APIRequest AcceptMeeting(int meetingId)
        {
            var parameters = new Dictionary<string, int>
            {
                { "meetingId", meetingId },
                { "userId", m_userInfo.id }
            };

            var url = vesApiBaseUrl + acceptMeeting;
            url = url.AppendParameters(parameters);

            APIRequest acceptMeetingRequest = APIRequest.Prepare((WWWForm)null, url, UnityWebRequest.kHttpVerbPOST, APIRequestType.Accept_Meeting);
            acceptMeetingRequest.Run(this, false).OnRequestComplete(
                (isNetworkError, message) =>
                {
                    var meetingResource = ShuffleMeetings(meetingId, MeetingFilter.Invited.ToString(), MeetingFilter.Accepted.ToString(), MeetingFilter.Rejected.ToString());
                    if (meetingResource != null)
                        meetingResource.MeetingInfo.meetingType = MeetingCategory.ACCEPTED;
                }
            );
            return acceptMeetingRequest;
        }

        public APIRequest RejectMeeting(int meetingId)
        {
            var parameters = new Dictionary<string, int>
            {
                { "meetingId", meetingId },
                { "userId", m_userInfo.id }
            };

            var url = vesApiBaseUrl + rejectMeeting;
            url = url.AppendParameters(parameters);

            APIRequest rejectMeetingRequest = APIRequest.Prepare((WWWForm)null, url, UnityWebRequest.kHttpVerbPOST, APIRequestType.Reject_Meeting);
            rejectMeetingRequest.Run(this, false).OnRequestComplete(
                (isNetworkError, message) =>
                {
                    Debug.Log(message);
                    var meetingResource = ShuffleMeetings(meetingId, MeetingFilter.Accepted.ToString(), MeetingFilter.Rejected.ToString(), MeetingFilter.Invited.ToString());
                    if (meetingResource != null)
                        meetingResource.MeetingInfo.meetingType = MeetingCategory.REJECTED;
                }
            );

            return rejectMeetingRequest;
        }

        private MeetingResource ShuffleMeetings(int meetingId, string fromGroupName, string toGroupName, string alternateFromGroupName)
        {
            MeetingGroup fromGroup = null, toGroup = null, alternateFromGroup = null;

            fromGroup = m_meetings.Find(item => item.Name  == fromGroupName);
            if (!string.IsNullOrEmpty(toGroupName))
                toGroup = m_meetings.Find(item => item.Name == toGroupName);

            if (!string.IsNullOrEmpty(toGroupName))
                alternateFromGroup = m_meetings.Find(item => item.Name == alternateFromGroupName);

            MeetingResource meetingResource = null;
            if (fromGroup != null && fromGroup.Resources != null && fromGroup.Resources.Length > 0)
            {
                meetingResource = Array.Find(fromGroup.Resources, item => item.MeetingInfo.Id == meetingId);
                if (meetingResource != null)
                {
                    fromGroup.Resources = fromGroup.Resources.Where(item => item.MeetingInfo.Id != meetingId).ToArray();
                }
            }

            if (meetingResource == null && alternateFromGroup != null && alternateFromGroup.Resources != null && alternateFromGroup.Resources.Length > 0)
            {
                meetingResource = Array.Find(alternateFromGroup.Resources, item => item.MeetingInfo.Id == meetingId);
                if (meetingResource != null)
                {
                    alternateFromGroup.Resources = alternateFromGroup.Resources.Where(item => item.MeetingInfo.Id != meetingId).ToArray();
                }
            }

            if (toGroup != null && meetingResource != null)
                toGroup.Resources = toGroup.Resources.Concatenate(new MeetingResource[] { meetingResource });

            return meetingResource;
        }

        public APIRequest CancelMeeting(int meetingId)
        {
            var parameters = new Dictionary<string, int>
            {
                { "meetingId", meetingId },
                { "userId", m_userInfo.id }
            };

            var url = vesApiBaseUrl + cancelMeeting;
            url = url.AppendParameters(parameters);

            APIRequest cancelMeetingRequest = APIRequest.Prepare((WWWForm)null, url, UnityWebRequest.kHttpVerbPOST, APIRequestType.Cancel_Meeting);
            cancelMeetingRequest.Run(this, false).OnRequestComplete(
                (isNetworkError, message) =>
                {
                    var createdMeetingsGroup = m_meetings.Find(item => item.Name == MeetingFilter.Created.ToString());
                    if (createdMeetingsGroup != null && createdMeetingsGroup.Resources != null && createdMeetingsGroup.Resources.Length > 0)
                    {
                        createdMeetingsGroup.Resources = createdMeetingsGroup.Resources.Where(item => item.MeetingInfo.Id != meetingId).ToArray();
                    }
                }
            );

            return cancelMeetingRequest;
        }

        #endregion

        public static bool ConnectedToNetwork()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://clients3.google.com/generate_204"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public void ShowUploadProgress()
        {

        }

        private void SetUserData(JsonData loginValidationData)
        {
            //Coordinator.instance.authoringManager.ToggleAuthoring(true);
            //print(loginValidationData["name"] + " " + loginValidationData["emailAddress"]);
            m_userInfo.emailAddress = Convert.ToString(loginValidationData["emailAddress"]);
            m_userInfo.id = Convert.ToInt32(Convert.ToString(loginValidationData["id"]));
            m_userInfo.organizationId = Convert.ToInt32(Convert.ToString(loginValidationData["tenantId"]));
            m_userInfo.name = Convert.ToString(loginValidationData["name"]);
            m_userInfo.phoneNumber = Convert.ToString(loginValidationData["phoneNumber"]);
            //userInfo.UserRoletype = (int)loginValidationData["UserRoletype"];

            ZPlayerPrefs.SetString("name", m_userInfo.name);
            ZPlayerPrefs.SetInt("id", m_userInfo.id);
            ZPlayerPrefs.SetInt("organizationId", m_userInfo.organizationId);
            ZPlayerPrefs.SetString("emailAddress", m_userInfo.emailAddress);
            //ZPlayerPrefs.SetInt("UserRoleType", userInfo.UserRoletype);

            //List<string> allowedFeatures = new List<string>();
            //for (int i=0; i<loginValidationData["permissions"].Count; i++)  
            //{
            //    var permission = loginValidationData["permissions"][i];
            //    allowedFeatures.Add(Convert.ToString(permission["displayName"]));
            //    //Debug.LogError(Convert.ToString(permission["displayName"]));
            //}
            //Coordinator.instance.permissionHandler.RefreshPermissions(allowedFeatures);
        }

        public string GetHtmlFromUri(string resource)
        {
            string html = string.Empty;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(resource);
            try
            {
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    bool isSuccess = (int)resp.StatusCode < 299 && (int)resp.StatusCode >= 200;
                    if (isSuccess)
                    {
                        using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                        {
                            //We are limiting the array to 80 so we don't have
                            //to parse the entire html document feel free to 
                            //adjust (probably stay under 300)
                            char[] cs = new char[80];
                            reader.Read(cs, 0, cs.Length);
                            foreach (char ch in cs)
                            {
                                html += ch;
                            }
                        }
                    }
                }
            }
            catch
            {
                return "";
            }
            return html;
        }

        bool CheckInternetConnection()
        {
            string HtmlText = GetHtmlFromUri("http://google.com");
            if (HtmlText == "")
            {
                return false;
            }
            else if (!HtmlText.Contains("schema.org/WebPage"))
            {
                //Redirecting since the beginning of googles html contains that 
                //phrase and it was not found
                return false;
            }
            else
            {
                //success
                return true;
            }
        }

        #region NETWORK_HANDLING
        public void RegisterRequestFailure(string errorMessage, APIRequest req)
        {
            int reqIndex = m_failedRequestStack.FindIndex(item => item.requestType == req.requestType);

            if (reqIndex != -1)
                m_failedRequestStack[reqIndex] = req;
            else
                m_failedRequestStack.Add(req);

            Error error = new Error(Error.E_NetworkIssue)
            {
                ErrorText = errorMessage
            };
            m_vrMenu.DisplayResult(error);
        }

        public void RemoveRequest(APIRequest req)
        {
            if (m_failedRequestStack.Contains(req))
            {
                m_failedRequestStack.Remove(req);
            }
        }
        #endregion

        #region PARSING
        public List<MeetingInfo> ParseMeetingListJson(string meetingListJson, MeetingCategory category)
        {
            //Debug.LogError(meetingListJson);
            JsonData meetingListData = JsonMapper.ToObject(meetingListJson);

            List<MeetingInfo> _meetingInfoList = new List<MeetingInfo>();

            bool arrayTraverseFinished = false;

            for (int i = 0; !arrayTraverseFinished; i++)
            {
                try
                {
                    MeetingInfo meetingInfo = null;

                    meetingInfo = new MeetingInfo
                    {
                        Id = (int)meetingListData["result"][i]["meetingParticipant"]["meetingId"],
                        //MeetingGUID = (string)meetingListData["MeetingDetailsByCreatedUser"][i]["MeetingGUID"],
                        MeetingNumber = (string)meetingListData["result"][i]["meetingMeetingNumber"],
                        //MeetingPassword = (string)meetingListData["MeetingDetailsByCreatedUser"][i]["MeetingPassword"],
                        FileToBeReviewed = (string)meetingListData["result"][i]["fileToBeReviewed"],
                        FileLocation = (string)meetingListData["result"][i]["fileLocation"],
                        MeetingTime = Convert.ToDateTime((string)meetingListData["result"][i]["meetingTime"]).ToLocalTime(),
                        MeetingDurationInMinutes = (int)meetingListData["result"][i]["meetingDuration"],
                        //ModelUploadTimeInSeconds = (int)meetingListData["MeetingDetailsByCreatedUser"][i]["ModelUploadTimeInSeconds"],
                        //Status = (bool)meetingListData["MeetingDetailsByCreatedUser"][i]["Status"],
                        //CreatedUserId = (int)meetingListData["MeetingDetailsByCreatedUser"][i]["CreatedUserId"],
                        //CreatedDateTime = (string)meetingListData["MeetingDetailsByCreatedUser"][i]["CreatedDateTime"],
                        //ModifiedUserId = (int)meetingListData["MeetingDetailsByCreatedUser"][i]["ModifiedUserId"],
                        //ModifiedDateTime = (string)meetingListData["MeetingDetailsByCreatedUser"][i]["ModifiedDateTime"],
                        meetingType = category
                    };
                    //meetingInfo.Description = "Temp description.";
                    _meetingInfoList.Add(meetingInfo);
                    meetingInfo.Description = (string)meetingListData["result"][i]["description"];
                }
                catch (NullReferenceException)
                {
                    Debug.LogError("description null");
                }
                catch (KeyNotFoundException e)
                {
                    Debug.LogError(e.Message);
                    continue;
                }
                catch (ArgumentOutOfRangeException)
                {
                    arrayTraverseFinished = true;
                }
            }

            return _meetingInfoList;
        }

        public ExperienceResource[] GetResource(ResourceType resourceType, string category)
        {
            try
            {
                switch (resourceType)
                {
                    case ResourceType.MEETING:
                        return m_meetings.Find(item => item.Name.Equals(category)).Resources;
                    case ResourceType.USER:
                        return m_users.Find(item => item.Name.Equals(category)).Resources;
                    default:
                        Debug.LogError("Returning null as resource list: " + resourceType.ToString());
                        return null;
                }
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        public void GetCategories(ResourceType type, UnityAction<ResourceComponent[]> done, bool requireWebRefresh = false)
        {
            //Debug.LogError("GetCategories: " + type.ToString());

            m_vrMenu.DisplayProgress("Hold on, fetching details...");
            switch (type)
            {
                case ResourceType.MEETING:
                    if (!m_requireMeetingListRefresh && !requireWebRefresh && m_meetings.Count == 4)
                    {
                        m_vrMenu.DisplayResult(new Error(Error.OK));
                        done?.Invoke(Meetings);
                        break;
                    }
                    m_meetings.Clear();
                    ListAllMeetingDetails(MeetingFilter.Created).OnRequestComplete((isNetworkError, message) =>
                    {
                        JsonData result = JsonMapper.ToObject(message);
                        if (result["success"].ToString() == "True")
                        {
                            //Debug.LogError(message);
                            var allCreatedMeetings = ParseMeetingListJson(message, MeetingCategory.CREATED);
                            m_meetings.Add(GetMeetingGroup(MeetingFilter.Created, allCreatedMeetings));
                            if (m_meetings.Count == 4)
                            {
                                m_vrMenu.DisplayResult(new Error(Error.OK));
                                done?.Invoke(Meetings);
                                m_requireMeetingListRefresh = false;
                            }
                            //Coordinator.instance.meetingInterface.CleanupCache();
                        }
                        else
                        {
                            m_vrMenu.DisplayResult(new Error()
                            {
                                ErrorText = (string)result["error"]["message"],
                                ErrorCode = Error.E_Exception
                            });
                        }
                    });
                    ListAllMeetingDetails(MeetingFilter.Accepted).OnRequestComplete((isNetworkError, message) =>
                    {
                        JsonData result = JsonMapper.ToObject(message);
                        if (result["success"].ToString() == "True")
                        {
                            //Debug.LogError(message);
                            var meetings = ParseMeetingListJson(message, MeetingCategory.ACCEPTED);
                            m_meetings.Add(GetMeetingGroup(MeetingFilter.Accepted, meetings));
                            if (m_meetings.Count == 4)
                            {
                                m_vrMenu.DisplayResult(new Error(Error.OK));
                                done?.Invoke(Meetings);
                                m_requireMeetingListRefresh = false;
                            }
                            //Coordinator.instance.meetingInterface.CleanupCache();
                        }
                        else
                        {
                            m_vrMenu.DisplayResult(new Error()
                            {
                                ErrorText = (string)result["error"]["message"],
                                ErrorCode = Error.E_Exception
                            });
                        }
                    });
                    ListAllMeetingDetails(MeetingFilter.Invited).OnRequestComplete((isNetworkError, message) =>
                    {
                        JsonData result = JsonMapper.ToObject(message);
                        if (result["success"].ToString() == "True")
                        {
                            //Debug.LogError(message);
                            var meetings = ParseMeetingListJson(message, MeetingCategory.INVITED);
                            m_meetings.Add(GetMeetingGroup(MeetingFilter.Invited, meetings));
                            if (m_meetings.Count == 4)
                            {
                                m_vrMenu.DisplayResult(new Error(Error.OK));
                                done?.Invoke(Meetings);
                                m_requireMeetingListRefresh = false;
                            }
                            //Coordinator.instance.meetingInterface.CleanupCache();
                        }
                        else
                        {
                            m_vrMenu.DisplayResult(new Error()
                            {
                                ErrorText = (string)result["error"]["message"],
                                ErrorCode = Error.E_Exception
                            });
                        }
                    });
                    ListAllMeetingDetails(MeetingFilter.Rejected).OnRequestComplete((isNetworkError, message) =>
                    {
                        JsonData result = JsonMapper.ToObject(message);
                        if (result["success"].ToString() == "True")
                        {
                            //Debug.LogError(message);
                            var meetings = ParseMeetingListJson(message, MeetingCategory.REJECTED);
                            m_meetings.Add(GetMeetingGroup(MeetingFilter.Rejected, meetings));
                            if (m_meetings.Count == 4)
                            {
                                m_vrMenu.DisplayResult(new Error(Error.OK));
                                done?.Invoke(Meetings);
                                m_requireMeetingListRefresh = false;
                            }
                            //Coordinator.instance.meetingInterface.CleanupCache();
                        }
                        else
                        {
                            m_vrMenu.DisplayResult(new Error()
                            {
                                ErrorText = (string)result["error"]["message"],
                                ErrorCode = Error.E_Exception
                            });
                        }
                    });
                    break;
                case ResourceType.USER:
                    if (!requireWebRefresh && m_users.Count > 0)
                    {
                        m_vrMenu.DisplayResult(new Error());
                        done?.Invoke(Users);
                        return;
                    }
                    m_users.Clear();
                    GetUsersByOrganization().OnRequestComplete(
                    (isNetworkError, message) =>
                    {
                    //Debug.LogError(message);

                        JsonData result = JsonMapper.ToObject(message);
                        if (result["success"].ToString() == "True")
                        {
                            var userGroup = GetUserGroup(ParseUserListJson(message));
                            m_users.Add(userGroup);
                            m_vrMenu.DisplayResult(new Error(Error.OK));
                            done?.Invoke(Users);
                        }
                        else
                        {
                            m_vrMenu.DisplayResult(new Error()
                            {
                                ErrorText = (string)result["error"]["message"],
                                ErrorCode = Error.E_Exception
                            });
                        }
                    });
                    break;
                default:
                    break;
            }
        }

        private MeetingGroup GetMeetingGroup(MeetingFilter filter, List<MeetingInfo> meetings)
        {
            MeetingResource[] meetingResources = new MeetingResource[meetings.Count];
            for (int i = 0; i < meetings.Count; i++)
            {
                meetingResources[i] = new MeetingResource
                {
                    Name = meetings[i].MeetingNumber,
                    Description = meetings[i].MeetingNumber + "\n\n" + (string.IsNullOrEmpty(meetings[i].Description) ? "" : string.IsNullOrEmpty(meetings[i].Description) + "\n\n") + meetings[i].MeetingTime + "\n\n" + meetings[i].MeetingDurationInMinutes/60 + " Hours, " + meetings[i].MeetingDurationInMinutes % 60 + " Minutes",
                    MeetingInfo = meetings[i],
                    ResourceType = ResourceType.MEETING
                };
            }

            MeetingGroup group = new MeetingGroup
            {
                Name = filter.ToString(),
                Description = "",
                ResourceType = ResourceType.MEETING,
                Resources = meetingResources.Length == 0 ? new MeetingResource[] { } : meetingResources
            };
            return group;
        }

        private UserGroup GetUserGroup(List<UserInfo> users)
        {
            UserResource[] userResources = new UserResource[users.Count];
            for (int i = 0; i < users.Count; i++)
            {
                userResources[i] = new UserResource
                {
                    Name = users[i].name,
                    Description = users[i].emailAddress,
                    UserInfo = users[i],
                    ResourceType = ResourceType.USER
                };
            }

            UserGroup group = new UserGroup
            {
                Name = "",
                Description = "",
                ResourceType = ResourceType.USER,
                Resources = userResources.Length == 0 ? new UserResource[] { } : userResources
            };
            return group;
        }

        #endregion
    }

    public class ErrorResult
    {
        public bool IsHttpError { get; set; }
        public bool IsNetworkError { get; set; }
        public string Response { get; set; }
        public string ServerMessage { get; set; }
        public int StatusCode { get; set; }
    }
}
