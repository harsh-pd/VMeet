using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Cornea.Web;
using LargeFileDownloader;
using System.IO;
using System;
using Cornea.Networking;
using Cornea.Import;
using UnityEngine.XR;
using System.Threading;
using Crosstales.FB;
using Cornea.UI.Meetings;
using LitJson;
using Cornea.Serialization;
using Fordi;

namespace VRExperience.Meetings
{
    public enum MeetingStatus
    {
        IDLE,
        MAKING_API_REQUEST,
        FILE_DOWNLOADING,
        FILE_EXTRACTING,
        FILE_IMPORTING,
        SETTING_UP_ROOM,
        ONGOING
    }

    public class MeetingElement : MonoBehaviour, IResettable
    {
        #region FIELDS_AND_PROPERTIES
        public const string ROOT_FOLDER = "Root";

        [SerializeField]
        private TextMeshProUGUI titleField, descriptionField, progressLabel;
        [SerializeField]
        private ProgressPanel progressPanel;
        [SerializeField]
        private Image categoryIcon, actionResultIcon;
        [SerializeField]
        private RequestResultPanel requestResultPanel;
        [SerializeField]
        private ActionPanel actionPanel;
        [SerializeField]
        private DownloadPanel downloadPanel;
        [SerializeField]
        private ImportPanel importPanel;
        [SerializeField]
        private PiXYZMiniImportSettings pixyzImportSettingsPanel;
        [SerializeField]
        private TrilibMiniImportSettings trilibImportSettingsPanel;

        private MeetingInfo meetingInfo;
        private string downloadUrl = string.Empty;
        private APIRequest lastRequest = null;
        private FileDownloader lastFileDownloader = null;
        private bool initialized = false;
        private Thread decompressThread = null;
        private MeetingStatus status = MeetingStatus.IDLE;
        private static MeetingElement activeMeeting = null;

        private static MeetingElement ActiveMeeting {
            get
            {
                return activeMeeting;
            }
            set
            {
                activeMeeting = value;
                Coordinator.instance.vrSetup.ToggleVrButton(activeMeeting == null);
                Coordinator.instance.meetingInterface.ToggleMeetingListScreenClose(activeMeeting == null);
            }
        }

        public MeetingCategory MeetingType { get { return meetingInfo.meetingType; } }
        public MeetingStatus MeetingStatus { get { return status; } }
        public MeetingInfo Info { get { return meetingInfo; } }
        public string File
        {
            get
            {
                return meetingInfo.FileToBeReviewed;
            }
        }
        public int Id
        {
            get
            {
                return meetingInfo.Id;
            }
        }
        public DateTime MeetingTime
        {
            get
            {
                return meetingInfo.MeetingTime;
            }
        }

        private IEnumerator timeWatchEnumerator = null;
        private IEnumerator importProgressEnumerator = null;
        #endregion

        #region GENERAL_METHODS
        public void OnReset()
        {
            initialized = false;
            progressPanel.Close();
            requestResultPanel.Close();
            progressLabel.text = "";
            actionPanel.gameObject.SetActive(true);
            if (timeWatchEnumerator != null)
                StopCoroutine(timeWatchEnumerator);
        }

        private void SetupCategoryColor()
        {
            switch (meetingInfo.meetingType)
            {
                case MeetingCategory.CREATED:
                    categoryIcon.color = Coordinator.instance.appTheme.selectedTheme.meetingCreatedColor;
                    break;
                case MeetingCategory.INVITED:
                    categoryIcon.color = Coordinator.instance.appTheme.selectedTheme.meetingOnHoldColor;
                    break;
                case MeetingCategory.REJECTED:
                    categoryIcon.color = Coordinator.instance.appTheme.selectedTheme.meetingRejectedColor;
                    break;
                case MeetingCategory.ACCEPTED:
                    categoryIcon.color = Coordinator.instance.appTheme.selectedTheme.meetingAcceptedColor;
                    break;
            }
        }

        private void UpdateDescriptionText()
        {
            descriptionField.text = "";
            if (!string.IsNullOrEmpty(meetingInfo.Description))
            {
                descriptionField.text = meetingInfo.Description + " ";
                if (!meetingInfo.Description.EndsWith("."))
                    descriptionField.text += ". ";
            }

            descriptionField.text += VESDateTime.GetSchedule(meetingInfo.MeetingTime, meetingInfo.MeetingDurationInMinutes, meetingInfo.meetingType == MeetingCategory.CREATED);
            if ((MeetingType == MeetingCategory.ACCEPTED || MeetingType == MeetingCategory.CREATED) && VESDateTime.MeetingWithinNextFiveMinutes(MeetingTime, meetingInfo.MeetingDurationInMinutes))
            {
                if (Coordinator.instance.meetingRoomInterface.IsRoomAvailable(meetingInfo.MeetingNumber))
                    descriptionField.text += " Click join to collaborate.";
                else if (meetingInfo.meetingType == MeetingCategory.CREATED)
                    descriptionField.text += " Click host to setup room.";
            }
        }

        private void UpdateDescriptionText(bool roomAvailable)
        {
            descriptionField.text = "";
            if (!string.IsNullOrEmpty(meetingInfo.Description))
            {
                descriptionField.text = meetingInfo.Description + " ";
                if (!meetingInfo.Description.EndsWith("."))
                    descriptionField.text += ". ";
            }

            descriptionField.text += VESDateTime.GetSchedule(meetingInfo.MeetingTime, meetingInfo.MeetingDurationInMinutes, meetingInfo.meetingType == MeetingCategory.CREATED);
            if (VESDateTime.MeetingWithinNextFiveMinutes(MeetingTime, meetingInfo.MeetingDurationInMinutes))
            {
                if (roomAvailable)
                    descriptionField.text += " Click join to collaborate.";
                else if (meetingInfo.meetingType == MeetingCategory.CREATED)
                    descriptionField.text += " Click host to setup room.";
            }
        }

        private void ReceivedRoomListUpdate()
        {
            if (Coordinator.instance.meetingRoomInterface.IsRoomAvailable(meetingInfo.MeetingNumber))
            {
                NetworkManager.ReceivedRoomListUpdate -= ReceivedRoomListUpdate;
                UpdateDescriptionText(true);
                if (meetingInfo.meetingType != MeetingCategory.CREATED)
                    actionPanel.OnRoomCreated();
            }
        }

        private void OnMeetingTimeStart()
        {
            actionPanel.OnMeetingTimeStart(meetingInfo);
            UpdateDescriptionText();
            //if (meetingInfo.meetingType != MeetingCategory.CREATED && !Coordinator.instance.meetingRoomInterface.IsRoomAvailable(meetingInfo.MeetingNumber))
            //    NetworkManager.ReceivedRoomListUpdate += ReceivedRoomListUpdate;
        }

        private void OnMeetingTimeOver()
        {
            var directoryRoot = Path.Combine(Serializer.BaseProjectPath, meetingInfo.MeetingNumber);
            if (Directory.Exists(directoryRoot))
                Directory.Delete(directoryRoot, true);
        }

        private IEnumerator MeetingTimeWatchEnumerator()
        {
            float timeInterval = VESDateTime.GetIntervalInMinutes(meetingInfo.MeetingTime);

            if (timeInterval < 0)
                yield break;
            else if (timeInterval < 5)
                yield return null;
            else
            {
                //Debug.Log("MeetingTimeWatchEnumerator: "  + meetingInfo.MeetingNumber + " " + timeInterval);
                yield return new WaitForSeconds(timeInterval * 60 - 300);
            }

            OnMeetingTimeStart();

            timeInterval = VESDateTime.GetIntervalInMinutes(meetingInfo.MeetingTime);
            if (timeInterval < 0)
                UpdateDescriptionText();
            else
            {
                yield return new WaitForSeconds(timeInterval * 60);
                UpdateDescriptionText();
            }
        }

        private void OnEnable()
        {
            if (initialized && !VESDateTime.IsMeetingOver(meetingInfo.MeetingTime, meetingInfo.MeetingDurationInMinutes) && (meetingInfo.meetingType == MeetingCategory.CREATED || meetingInfo.meetingType == MeetingCategory.ACCEPTED))
            {
                timeWatchEnumerator = MeetingTimeWatchEnumerator();
                StartCoroutine(timeWatchEnumerator);
            }
        }

        private string GetFilePath()
        {
            var directoryPath = Path.Combine(Serializer.BaseProjectPath, meetingInfo.MeetingNumber);
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
            return Path.Combine(directoryPath, File);
        }

        public static string GetRootFilePath(string meetingNumber, string fileName)
        {
            var directoryRoot = Path.Combine(Serializer.BaseProjectPath, meetingNumber);
            if (!Directory.Exists(directoryRoot))
            {
                Directory.CreateDirectory(directoryRoot);
                return string.Empty;
            }
            var files = Directory.GetFiles(directoryRoot, Path.GetFileNameWithoutExtension(fileName), SearchOption.AllDirectories);
            if (files == null || files.Length == 0)
            {
                //Debug.LogError("files null or files.length = 0");
                return string.Empty;
            }
            //Debug.Log(files[0]);
            return files[0];
        }

        public string GetRootFilePath()
        {
            var directoryRoot = Path.Combine(Serializer.BaseProjectPath, meetingInfo.MeetingNumber);
            if (!Directory.Exists(directoryRoot))
            {
                Directory.CreateDirectory(directoryRoot);
                return string.Empty;
            }
            var files = Directory.GetFiles(directoryRoot, Path.GetFileNameWithoutExtension(meetingInfo.FileToBeReviewed), SearchOption.AllDirectories);
            if (files == null || files.Length == 0)
            {
                //Debug.LogError("files null or files.length = 0");
                return string.Empty;
            }
            //Debug.Log(files[0]);
            return files[0];
        }

        public void Init(MeetingInfo _meetingInfo)
        {
            var parameters = new Dictionary<string, int>();
            parameters.Add("meetingId", _meetingInfo.Id);
            downloadUrl = WebInterface.vesApiBaseUrl + WebInterface.downloadFile;
            downloadUrl = downloadUrl.AppendParameters(parameters);

            meetingInfo = _meetingInfo;
            titleField.text = _meetingInfo.MeetingNumber + (_meetingInfo.FileLocation.Contains("http") ? " ( " + Path.GetFileNameWithoutExtension(_meetingInfo.FileToBeReviewed) + " )" : "");
            UpdateDescriptionText();

            OnReset();

            actionPanel.Init(_meetingInfo);

            SetupCategoryColor();

            if (VESDateTime.IsMeetingOver(MeetingTime, meetingInfo.MeetingDurationInMinutes))
                categoryIcon.color = categoryIcon.color / 2;

            if (gameObject.activeInHierarchy && !VESDateTime.IsMeetingOver(meetingInfo.MeetingTime, meetingInfo.MeetingDurationInMinutes) && (meetingInfo.meetingType == MeetingCategory.CREATED || meetingInfo.meetingType == MeetingCategory.ACCEPTED))
            {
                timeWatchEnumerator = MeetingTimeWatchEnumerator();
                StartCoroutine(timeWatchEnumerator);
            }

            initialized = true;
        }

        public void InitOngoingMeeting(MeetingInfo _meetingInfo)
        {
            meetingInfo = _meetingInfo;
            SetupCategoryColor();
            //var meetingDurationString = _meetingInfo.MeetingDurationInMinutes / 60 > 0 ? _meetingInfo.MeetingDurationInMinutes / 60 + " Hours " : "" + _meetingInfo.MeetingDurationInMinutes % 60 + " Minutes";
            string meetingDurationString = "";
            var hours = _meetingInfo.MeetingDurationInMinutes / 60;
            var minutes = _meetingInfo.MeetingDurationInMinutes - hours * 60;
            if (hours > 0)
                meetingDurationString += hours.ToString() + " Hours ";
            if (minutes > 0)
                meetingDurationString += minutes.ToString() + " Minutes";
            titleField.text = "Meeting: " + _meetingInfo.MeetingNumber + "\n\n File: " + Path.GetFileNameWithoutExtension(_meetingInfo.FileToBeReviewed) + "\n\n Duration: " + meetingDurationString;
            actionPanel.InitOngoingMeeting();
        }

        public bool IsRelavant(string searchValue)
        {
            var lowerItemFileName = File.ToLower();
            var lowerMeetingNumberString = meetingInfo.MeetingNumber.ToLower();
            var lowerMeetingId = Id.ToString().ToLower();
            var lowerDateString = meetingInfo.MeetingTime.ToShortDateString();
            var lowerSearchValue = searchValue.ToLower();
            return (searchValue.Equals(lowerDateString) || (lowerMeetingId.Contains(lowerSearchValue) || lowerItemFileName.Contains(lowerSearchValue)) || lowerMeetingNumberString.Contains(lowerSearchValue));
        }
        #endregion
        
        #region API
        public void Accept()
        {
            progressPanel.Show("Accepting", true);
            actionPanel.gameObject.SetActive(false);

            APIRequest acceptRequest = null;
            acceptRequest = Coordinator.instance.webInterface.AcceptMeeting(Info.Id).OnRequestComplete(
                (isNetworkError, message) =>
                {
                    status = MeetingStatus.IDLE;
                    JsonData oututData = JsonMapper.ToObject(message);
                    if (!isNetworkError && (bool)oututData["success"] == true)
                    {
                        lastRequest.Kill();
                        lastRequest = null;
                        actionPanel.gameObject.SetActive(true);
                        progressPanel.Close();
                        //requestResultPanel.Show(false, "Meeting cancelled successfully.");
                        Coordinator.instance.meetingInterface.MeetingAccepted(this);
                    }
                    else
                    {
                        progressPanel.Close();
                        requestResultPanel.Show(false, "Failed to accept meeting.");
                    }
                }
            );

            lastRequest = acceptRequest;
            status = MeetingStatus.MAKING_API_REQUEST;
        }

        public void Ignore()
        {
            progressPanel.Show("Rejecting", true);
            actionPanel.gameObject.SetActive(false);
            APIRequest ignoreRequest = null;
            ignoreRequest = Coordinator.instance.webInterface.RejectMeeting(Info.Id).OnRequestComplete(
                (isNetworkError, message) =>
                {
                    status = MeetingStatus.IDLE;
                    JsonData oututData = JsonMapper.ToObject(message);
                    if (!isNetworkError && (bool)oututData["success"] == true)
                    {
                        lastRequest.Kill();
                        lastRequest = null;
                        actionPanel.gameObject.SetActive(true);
                        progressPanel.Close();
                        //requestResultPanel.Show(false, "Meeting cancelled successfully.");
                        Coordinator.instance.meetingInterface.MeetingIgnored(this);
                    }
                    else
                    {
                        progressPanel.Close();
                        requestResultPanel.Show(false, "Failed to ignore meeting.");
                    }
                }
            );
            lastRequest = ignoreRequest;
        }

        public void Reject()
        {
            progressPanel.Show("Rejecting", true);
            actionPanel.gameObject.SetActive(false);
            APIRequest rejectRequest = null;
            rejectRequest = Coordinator.instance.webInterface.RejectMeeting(Info.Id).OnRequestComplete(
                (isNetworkError, message) =>
                {
                    status = MeetingStatus.IDLE;
                    JsonData oututData = JsonMapper.ToObject(message);
                    if (!isNetworkError && (bool)oututData["success"] == true)
                    {
                        lastRequest.Kill();
                        lastRequest = null;
                        actionPanel.gameObject.SetActive(true);
                        progressPanel.Close();
                        //requestResultPanel.Show(false, "Meeting cancelled successfully.");
                        Coordinator.instance.meetingInterface.MeetingRejected(this);
                    }
                    else
                    {
                        progressPanel.Close();
                        requestResultPanel.Show(false, "Failed to reject meeting.");
                    }
                }
            );

            lastRequest = rejectRequest;
            status = MeetingStatus.MAKING_API_REQUEST;
        }

        public void Cancel()
        {
            progressPanel.Show("Cancelling", true);
            actionPanel.gameObject.SetActive(false);

            APIRequest cancelReq = null;
            cancelReq = Coordinator.instance.webInterface.CancelMeeting(Info.Id).OnRequestComplete(
                (isNetworkError, message) =>
                {
                    status = MeetingStatus.IDLE;
                    JsonData oututData = JsonMapper.ToObject(message);
                    if (!isNetworkError && (bool)oututData["success"] == true)
                    {
                        lastRequest.Kill();
                        lastRequest = null;
                        actionPanel.gameObject.SetActive(true);
                        progressPanel.Close();
                        //requestResultPanel.Show(false, "Meeting cancelled successfully.");
                        Coordinator.instance.meetingInterface.MeetingCancelled(this);
                    }
                    else
                    {
                        progressPanel.Close();
                        requestResultPanel.Show(false, "Failed to cancel meeting.");
                    }
                
                }
            );
            lastRequest = cancelReq;
            status = MeetingStatus.MAKING_API_REQUEST;
        }

        private IEnumerator onProgressEnumerator, postDownloadEnumerator;
        public void DownloadFile()
        {
            if (!Info.FileLocation.Contains("http"))
                return;
            importPanel.Close();
            actionPanel.gameObject.SetActive(false);
            downloadPanel.Init();


            //if (!Info.FileLocation.Contains("http"))
            //    return;
            //importPanel.Close();
            //actionPanel.gameObject.SetActive(false);
            //downloadPanel.Init();
            //lastFileDownloader = Coordinator.instance.uploadDownload.Download(Id, GetFilePath(), (e) => { OnProgress(e); }, (e) => { OnDownloadComplete(e); });

            lastRequest = Coordinator.instance.uploadDownload.Download(meetingInfo.FileLocation, GetFilePath()).OnRequestComplete(
                (error, message) =>
                {
                    if (lastRequest.isNetworkError || lastRequest.isHttpError)
                        downloadPanel.OnProcessComplete(false);
                    else
                    {
                        if (postDownloadEnumerator != null)
                            StopCoroutine(postDownloadEnumerator);
                        postDownloadEnumerator = PostDownloadEnumerator();
                        StartCoroutine(postDownloadEnumerator);
                    }
                }
            );
            if (onProgressEnumerator != null)
                StopCoroutine(onProgressEnumerator);
            onProgressEnumerator = OnDownloadProgress();
            StartCoroutine(onProgressEnumerator);
            status = MeetingStatus.FILE_DOWNLOADING;
        }

        private IEnumerator OnDownloadProgress()
        {
            while (lastRequest != null && !lastRequest.isDone)
            {
                int progressPercent = (int)(lastRequest.downloadProgress * 100);
                downloadPanel.UpdateProgress(progressPercent);
                //Debug.Log(progressPercent);
                yield return null;
            }
        }

        private void OnProgress(DownloadEvent e)
        {
            if (e.fileURL.Equals(downloadUrl))
                downloadPanel.UpdateProgress(e.progress);
            //Debug.Log(e.progress);
        }

        private void OnDownloadComplete(DownloadEvent e)
        {
            if (lastFileDownloader != null)
            {
                lastFileDownloader.Dispose();
                if (e.fileURL.Equals(downloadUrl))
                {
                    if (e.error == null)
                        Coordinator.instance.mainThreadDispatcher.Enqueue(PostDownloadEnumerator());
                    else
                        downloadPanel.OnProcessComplete(false);
                }
            }
        }

        private IEnumerator PostDownloadEnumerator()
        {
            yield return 5;
            downloadPanel.Close();
            progressPanel.Show("Hold on, extracting files.", false);
            Extract();
        }

        //private void OnProgress(DownloadEvent e)
        //{
        //    if (e.fileURL.Equals(downloadUrl))
        //        downloadPanel.UpdateProgress(e.progress);
        //    //Debug.Log(e.progress);
        //}
        #endregion

        #region MEETING
        private void SetupRoom(float waitTime, bool host)
        {
            if (!XRDevice.isPresent)
            {
                requestResultPanel.Show(false, "Failed to detect Oculus.", false, false);
                status = MeetingStatus.IDLE;
                ActiveMeeting = null;
                return;
            }

            progressPanel.Show("Hold on, setting up room.", false);
            actionPanel.gameObject.SetActive(false);
            NetworkManager.RoomJoined += RoomJoined;
            Coordinator.instance.meetingRoomInterface.StartRoom(meetingInfo, host, waitTime);
            status = MeetingStatus.SETTING_UP_ROOM;
            ActiveMeeting = this;
        }

        public void StartMeeting()
        {
            actionPanel.gameObject.SetActive(false);
            if (ActiveMeeting != null)
            {
                requestResultPanel.Show(false, "Failed to setup room. Meeting " + ActiveMeeting.Info.MeetingNumber + " active.", false, false);
                return;
            }
            if (!XRDevice.isPresent)
            {
                requestResultPanel.Show(false, "Failed to detect Oculus.", false, false);
                return;
            }
            if (!PhotonNetwork.connectedAndReady)
            {
                requestResultPanel.Show(false, "Failed to connect to network.", false, false);
                return;
            }

            var rootFilePath = GetRootFilePath();
            if (string.IsNullOrEmpty(rootFilePath))
            {
                PromptDownload();
                return;
            }

            var roomList = PhotonNetwork.GetRoomList();
            var currentMeetingRoom = Array.Find(roomList, item => item.Name.Equals(meetingInfo.MeetingNumber));

            if (currentMeetingRoom != null)
            {
                requestResultPanel.Show(false, "Room already available. Refresh to join.", false, false);
                return;
            }

            if (ImportInterface.LastImportOptions ==  null || !ImportInterface.LastImportOptions.chosenFile.Equals(rootFilePath) || (!rootFilePath.EndsWith(".rtprefab") && !ImportInterface.LastImportOptions.plugin.Equals(ImportPlugin.TRILIB)))
            {
                //if (ImportInterface.LastImportOptions != null)
                //    Debug.LogError(ImportInterface.LastImportOptions.chosenFile + " " + GetFilePath());
                if (rootFilePath.EndsWith(".rtprefab"))
                {
                    LoadPrefab(() => SetupRoom(Coordinator.instance.settings.SelectedPreferences.importInitialiseTime + .1f, true));
                    return;
                }
                //if (Coordinator.instance.settings.SelectedPreferences.importPlugin == ImportPlugin.TRILIB)
                trilibImportSettingsPanel.Toggle(true);
                //else
                //    pixyzImportSettingsPanel.Toggle(true);
                return;
            }
            Coordinator.instance.modelManager.ResetAllScale();
            SetupRoom(0, true);
        }

        public void OnRoomSetupCancellation()
        {
            importPanel.Close();
            actionPanel.gameObject.SetActive(true);
        }

        public void JoinMeeting()
        {
            actionPanel.gameObject.SetActive(false);
            if (ActiveMeeting != null)
            {
                requestResultPanel.Show(false, "Failed to join room. Meeting " + ActiveMeeting.Info.MeetingNumber + " active.", false, false);
                return;
            }
            if (!XRDevice.isPresent)
            {
                requestResultPanel.Show(false, "Failed to detect Oculus.", false, false);
                return;
            }
            if (!PhotonNetwork.connectedAndReady)
            {
                requestResultPanel.Show(false, "Failed to connect to network.", false, false);
                return;
            }

            var roomList = PhotonNetwork.GetRoomList();
            var currentMeetingRoom = Array.Find(roomList, item => item.Name.Equals(meetingInfo.MeetingNumber));

            if (currentMeetingRoom == null)
            {
                requestResultPanel.Show(false, "Failed to join. Room doesn't exist.", false, false);
                return;
            }

            ImportPlugin hostPlugin = ImportPlugin.PIXYZ;

            var file = GetRootFilePath();
            if (!file.EndsWith(".rtprefab"))
                hostPlugin = (ImportPlugin)currentMeetingRoom.CustomProperties[ImportOptions.pluginProperty];
            //if (hostPlugin == ImportPlugin.PIXYZ && !Coordinator.instance.pixyzImporter.cadLoader.CheckLicense())
            //{
            //    requestResultPanel.Show(false, "PiXYZ license not found.", false, false);
            //    return;
            //}

            ImportOptions importOptions;

            if (file.EndsWith(".rtprefab"))
            {
                importOptions = new ImportOptions()
                {
                    plugin = ImportPlugin.PIXYZ,
                    chosenFile = file,
                    automaticImport = true
                };
            }
            else if (hostPlugin == ImportPlugin.PIXYZ)
            {
                importOptions = new ImportOptions()
                {
                    chosenFile = file,
                    plugin = hostPlugin,
                    meshQuality = (int)currentMeetingRoom.CustomProperties[ImportOptions.meshQualityProperty],
                    rightHanded = (bool)currentMeetingRoom.CustomProperties[ImportOptions.rightHandedProperty],
                    scaleFactor = (float)currentMeetingRoom.CustomProperties[ImportOptions.scaleFactorProperty],
                    zUp = (bool)currentMeetingRoom.CustomProperties[ImportOptions.zUpProperty],
                    automaticImport = true
                };
            }
            else
            {
                importOptions = new ImportOptions()
                {
                    chosenFile = file,
                    plugin = hostPlugin,
                    scaleFactor = (float)currentMeetingRoom.CustomProperties[ImportOptions.scaleFactorProperty],
                    automaticImport = true
                };
            }

            if (ImportInterface.LastImportOptions != null && ImportInterface.LastImportOptions.Equals(importOptions))
            {
                Coordinator.instance.modelManager.ResetAllScale();
                SetupRoom(Coordinator.instance.settings.SelectedPreferences.importInitialiseTime + .1f, false);
                return;
            }
            var rootFilePath = GetRootFilePath();
            if (rootFilePath.EndsWith(".rtprefab"))
            {
                LoadPrefab(() => SetupRoom(Coordinator.instance.settings.SelectedPreferences.importInitialiseTime + .1f, false));
                return;
            }
            else
                ImportFile(() => SetupRoom(Coordinator.instance.settings.SelectedPreferences.importInitialiseTime + .1f, false), importOptions);
            return;
        }

        public void OnMeetingBegin()
        {
            actionPanel.OnMeetingBegin();
        }

        public void OnMeetingEnd()
        {
            actionPanel.OnMeetingEnd(meetingInfo);
        }

        public void OnMeetingExit()
        {
            status = MeetingStatus.IDLE;
        }

        public void ExitMeeting()
        {
            if (ActiveMeeting == null)
                return;
            ActiveMeeting.OnMeetingExit();
            ActiveMeeting = null;
            Coordinator.instance.meetingRoomInterface.ExitMeeting();
            status = MeetingStatus.IDLE;
        }

        public void RoomJoined(bool val)
        {
            Debug.LogError("Room Joined: " + Info.MeetingNumber);
            NetworkManager.RoomJoined -= RoomJoined;
            progressPanel.Close();
            if (val)
            {
                actionPanel.ExpandCollapseToggle();
                actionPanel.gameObject.SetActive(true);
                status = MeetingStatus.ONGOING;
            }
            else
            {
                ActiveMeeting = null;
                status = MeetingStatus.IDLE;
                requestResultPanel.Show(false, "Failed to join room.", false, false);
                return;
            }
        }
        #endregion
     
        #region API_REQUEST_MANAGEMENT
        public void Retry()
        {
            if (lastRequest != null)
            {
                requestResultPanel.Close();
                actionPanel.gameObject.SetActive(false);

                switch (lastRequest.requestType)
                {
                    case APIRequestType.Accept_Meeting:
                        progressPanel.Show("Accepting", true);
                        break;
                    case APIRequestType.Reject_Meeting:
                        progressPanel.Show("Rejecting", true);
                        break;
                    case APIRequestType.Cancel_Meeting:
                        progressPanel.Show("Cancelling", true);
                        break;
                }
                lastRequest.Refresh(false);
            }
        }

        public void CancelRequest()
        {
            progressPanel.Close();
            requestResultPanel.Close();
            actionPanel.gameObject.SetActive(true);
            if (lastRequest != null)
                lastRequest.Kill();
            status = MeetingStatus.IDLE;
        }

        public void Finish()
        {
            requestResultPanel.Close();
            actionPanel.gameObject.SetActive(true);
        }

        public void CancelDownload()
        {
            CancelRequest();
            DownloadFinish();
            //if (lastFileDownloader != null)
            //{
            //    lastFileDownloader.Cancel();
            //    lastFileDownloader.Dispose();
            //    DownloadFinish();
            //}
        }

        public void RetryDownload()
        {
            DownloadFile();
        }

        public void DownloadFinish()
        {
            downloadPanel.Close();
            actionPanel.gameObject.SetActive(true);
        }
        #endregion

        #region FILE_IMPORT
        private Action onImportFinish;

        private bool IsValidCADFile(string extension, ImportPlugin plugin)
        {
            if (extension.Length < 2)
                return false;
            extension = extension.Substring(1).ToLower();
            ExtensionFilter[] allowedExtensions;

            if (plugin == ImportPlugin.PIXYZ)
                allowedExtensions = new[] {
                    new ExtensionFilter("PiXYZ Files","fbx","igs","iges","stp","step","stepz","ifc","u3d","catproduct","catpart","cgr","catshape","sldasm","sldprt","prt","asm","neu","xas","xpr","prt.*","asm.*","neu.*","xas.*","xpr.*","par","pwd","psm","sat","sab","vda","rvt","rfa","3dm","3dxml","wrl","vrml","jt","dae","stl","x_t","x_b","p_t","p_b","xmt","xmt_txt","xmt_bin","plmxml","obj","csb","wire","skp","pdf","prc","3ds","dwg","dxf","pxz","e57","pts","ipt","iam","ipj","ptx","xyz","model","session")
                };
            else
                allowedExtensions = new[] {
                    new ExtensionFilter("Model Files","fbx","obj")
                };
            return Array.FindIndex(allowedExtensions[0].Extensions, item => item.ToLower().Equals(extension)) != -1;
        }

        public void LoadPrefab(Action _onImortFinished)
        {
            Coordinator.instance.vrSetup.ForceSetupVRMode();

            onImportFinish = _onImortFinished;
            ModelManager.OnImportProcessed += ImportEnded;
            ImportInterface.OnImportFailure += ImportFailed;

            actionPanel.gameObject.SetActive(false);
            progressPanel.Show("Hold on, loading file.", false);
            Coordinator.instance.importInterface.LoadPrefab(GetRootFilePath(), meetingInfo.MeetingNumber);
        }

        private void PromptDownload()
        {
            actionPanel.gameObject.SetActive(false);
            importPanel.Init(File, false);
            importPanel.OnProcessComplete(ImportError.FILE_PATH_EXCEPTION);
        }

        public void ImportFile(Action _onImportFinish, ImportOptions options)
        {
            actionPanel.gameObject.SetActive(false);
            importPanel.Init(File, false);
            var filePath = options.chosenFile;
            //Debug.Log("ImportFile: " + filePath);

            if (!System.IO.File.Exists(filePath))
            {
                importPanel.OnProcessComplete(ImportError.FILE_PATH_EXCEPTION);
                return;
            }
            else if(!IsValidCADFile(Path.GetExtension(filePath), options.plugin))
            {
                importPanel.OnProcessComplete(ImportError.INVALID_FILE_EXCEPTION);
                return;
            }
            else if (options.plugin == ImportPlugin.PIXYZ && !Coordinator.instance.pixyzImporter.cadLoader.CheckLicense())
            {
                importPanel.OnProcessComplete(ImportError.LICENSE_EXCEPTION);
                return;
            }

            Coordinator.instance.vrSetup.ForceSetupVRMode();

            status = MeetingStatus.FILE_IMPORTING;
            ActiveMeeting = this;
            onImportFinish = _onImportFinish;
            ModelManager.OnImportProcessed += ImportEnded;
            ImportInterface.OnImportFailure += ImportFailed;

            if (importProgressEnumerator != null)
                StopCoroutine(importProgressEnumerator);

            importProgressEnumerator = OnImportProgress(options.plugin);
            StartCoroutine(importProgressEnumerator);


            //Coordinator.instance.pixyzImporter.CADFileImport_start(filePath, options);
            Coordinator.instance.importInterface.Import(options);
        }

        public void OnImportProceedClick()
        {
            //var selectedPlugin = Coordinator.instance.settings.SelectedPreferences.importPlugin;
            var selectedPlugin = ImportPlugin.TRILIB;
            //if (Coordinator.instance.settings.SelectedPreferences.importPlugin == ImportPlugin.PIXYZ)
            //    pixyzImportSettingsPanel.Toggle(false);
            //else
            trilibImportSettingsPanel.Toggle(false);
            var userInput = selectedPlugin == ImportPlugin.TRILIB ? trilibImportSettingsPanel.FetchImportOptions() : pixyzImportSettingsPanel.FetchImportOptions();
            userInput.chosenFile = GetRootFilePath();
            userInput.automaticImport = true;
            ImportFile(() => SetupRoom(Coordinator.instance.settings.SelectedPreferences.importInitialiseTime + .1f, true), userInput);
        }

        public void OnImportCancelClick()
        {
            //if (Coordinator.instance.settings.SelectedPreferences.importPlugin == ImportPlugin.PIXYZ)
            //    pixyzImportSettingsPanel.Toggle(false);
            //else
            trilibImportSettingsPanel.Toggle(false);
            actionPanel.gameObject.SetActive(true);
        }

        private void ImportEnded()
        {
            ModelManager.OnImportProcessed -= ImportEnded;
            ImportInterface.OnImportFailure -= ImportFailed;

            importPanel.Close();

            if (importProgressEnumerator != null)
                StopCoroutine(importProgressEnumerator);

            //requestResultPanel.Show(true, "Import finished.");
            status = MeetingStatus.IDLE;

            if (onImportFinish != null)
            {
                onImportFinish.Invoke();
                onImportFinish = null;
            }
        }

        private void ImportFailed()
        {
            Debug.LogError("ImportFailed");
            ImportInterface.OnImportFailure -= ImportFailed;
            ModelManager.OnImportProcessed -= ImportEnded;

            progressPanel.Close();
            importPanel.gameObject.SetActive(true);
            importPanel.OnProcessComplete(ImportError.OTHER);
            status = MeetingStatus.IDLE;
            ActiveMeeting = null;
            onImportFinish = null;
        }

        private IEnumerator OnImportProgress(ImportPlugin plugin)
        {
            if (plugin == ImportPlugin.PIXYZ)
            {

                var cadLoader = Coordinator.instance.pixyzImporter.cadLoader;
                do
                {
                    //setProgressBar(cadLoader.Progress);
                    importPanel.UpdateProgress((int)(cadLoader.Progress * 100));
                    yield return null;
                }
                while (cadLoader != null && cadLoader.IsImporting);
            }
            else
            {
                var importInterface = Coordinator.instance.importInterface;
                do
                {
                    //setProgressBar(cadLoader.Progress);
                    importPanel.UpdateProgress((int)(importInterface.Progress * 100));
                    yield return null;
                }
                while (importInterface.IsImporting);
            }
        }
        
        public void OnImportPluginChange()
        {
            //if (Coordinator.instance.settings.SelectedPreferences.importPlugin == ImportPlugin.TRILIB && pixyzImportSettingsPanel.gameObject.activeSelf)
            //{
            //    pixyzImportSettingsPanel.Toggle(false);
            //    trilibImportSettingsPanel.Toggle(true);
            //}
            //else if (Coordinator.instance.settings.SelectedPreferences.importPlugin == ImportPlugin.PIXYZ && trilibImportSettingsPanel.gameObject.activeSelf)
            //{
            //    pixyzImportSettingsPanel.Toggle(true);
            //    trilibImportSettingsPanel.Toggle(false);
            //}
        }
        #endregion

        #region FILE_EXTRACTION
        private int[] progress = new int[1];
        private int[] progress2 = new int[1];

        public void OnImportPanelOkClick()
        {
            importPanel.Close();
            actionPanel.gameObject.SetActive(true);
        }

        private void MultThreadedDecompress(string compressedFilePath, string decompressedDirectoryPath, IEnumerator onCompleteAction)
        {
            string password = "d!3";
            //if (Path.GetFileNameWithoutExtension(compressedFilePath).EndsWith(".rtprefab"))
            //    password = "d!3";
            int res = lzip.decompress_File(compressedFilePath, decompressedDirectoryPath, progress, null, progress2, password);
            if (res == 1)
            {
                Debug.Log("multi-threaded ok");
                if (onCompleteAction != null)
                    Coordinator.instance.mainThreadDispatcher.Enqueue(onCompleteAction);
                Thread.CurrentThread.Abort();
            }
            else
            {
                Thread.CurrentThread.Abort();
                Debug.Log("multi-threading error");
                OnExtractionFailure("Download complete, failed to extract.");
                return;
            }
        }

        private IEnumerator PostExtractConfiguration()
        {
            //Debug.Log("Extraction complete");
            status = MeetingStatus.IDLE;
            progressPanel.Close();
            //Debug.LogError(meetingInfo.FileToBeReviewed);
            var file = Path.GetFileNameWithoutExtension(meetingInfo.FileToBeReviewed);
            if (file.EndsWith(".rtprefab"))
                downloadPanel.OnProcessComplete(true, "Done! Make sure to restart the app before collaboration.");
            else
                downloadPanel.OnProcessComplete(true);
            actionPanel.OnDownloadComplete(System.IO.File.Exists(GetRootFilePath()));
            yield return null;
        }

        protected virtual bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        private void OnExtractionFailure(string message)
        {
            progressPanel.Close();
            actionPanel.gameObject.SetActive(false);
            requestResultPanel.Show(false, message, false, false);
        }

        private void Extract()
        {
            var compressedFilePath = GetFilePath();
            //Debug.Log("Extract: " + compressedFilePath);
            if (IsFileLocked(new FileInfo(compressedFilePath)) || !Path.GetExtension(compressedFilePath).Equals(".zip"))
            {
                OnExtractionFailure("Download complete, failed to extract.");
                return;
            }

            var directoryRoot = Path.Combine(Serializer.BaseProjectPath, meetingInfo.MeetingNumber);
            //var decompressedDirectoryPath = Path.Combine(directoryRoot, Path.GetFileNameWithoutExtension(File));
            var decompressedDirectoryPath = Path.Combine(directoryRoot, ROOT_FOLDER);
            //Debug.Log(decompressedDirectoryPath);
            if (decompressThread != null)
            {
                decompressThread.Abort();
            }

#if !NETFX_CORE
            IEnumerator onExtractionCompleteEnumerator = PostExtractConfiguration();
            decompressThread = new Thread(() => MultThreadedDecompress(compressedFilePath, decompressedDirectoryPath, onExtractionCompleteEnumerator));
            decompressThread.Start();
#endif
#if NETFX_CORE && UNITY_WSA_10_0
			Task task = new Task(new Action(() => MultThreadedDecompress(compressedFilePath, decompressedDirectoryPath, onExtractionCompleteEnumerator))); task.Start();
#endif
            status = MeetingStatus.FILE_EXTRACTING;
        }
        #endregion
    }
}