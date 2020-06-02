using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using LargeFileDownloader;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.Events;
using Fordi.Common;

namespace Cornea.Web
{
    public class CustomDownloadCertificateHandler : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }

    public class UploadDownload : MonoBehaviour {

        private WebClient currentWebClient;
        private FileDownloader downloader;
        private APIRequest lastUploadRequest;

        private IWebInterface m_webInterface = null;

        private void Awake()
        {
            m_webInterface = IOC.Resolve<IWebInterface>();
        }

        private void Start()
        {
            downloader = new FileDownloader();
        }

        #region UPLOAD
        private void WebClientUploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            //Debug.Log("Upload {0}% complete. " + e.ProgressPercentage);
            Debug.Log("Upload progress");
        }

        private void WebClientUploadCompleted(object sender, UploadFileCompletedEventArgs e)
        {
            //Debug.Log("WebClientUploadCompleted");
            if (e.Error != null)
            {
                Debug.Log(e.Error);
            }
            string reply = System.Text.Encoding.UTF8.GetString(e.Result);
            Debug.Log(reply);
        }

        //public void Upload(string url, string filePath, UploadFileCompletedEventHandler onComplete)
        //{
        //    WebClient webClient = new WebClient();
        //    currentWebClient = webClient;
        //    var authorizationHeaderValue = "Bearer: " + Coordinator.instance.webInterface.access_token;
        //    webClient.Headers.Add("Authorization", authorizationHeaderValue);
        //    webClient.UploadProgressChanged += WebClientUploadProgressChanged;
        //    webClient.UploadFileCompleted += onComplete;
        //    webClient.UploadFileAsync(new System.Uri(url), filePath);
        //}

        private IEnumerator uploadProgressEnumerator;
        public APIRequest Upload(string url, string filePath, UnityAction<float> onProgressAction)
        {
            WWWForm form = new WWWForm();
            form.AddField("file", "file");
            form.AddBinaryData("file", File.ReadAllBytes(filePath), Path.GetFileName(filePath), "application/octet-stream");
            APIRequest uploadFileRequest = APIRequest.Prepare(form, url, UnityWebRequest.kHttpVerbPOST, APIRequestType.Upload_File);
            //uploadFileRequest.uploadHandler = new UploadHandlerFile(filePath);
            uploadFileRequest.Run(this).OnRequestComplete(
            (isNetworkError, message) =>
                {
                    print(message);
                }
            );

            if (uploadProgressEnumerator != null)
                StopCoroutine(uploadProgressEnumerator);
            uploadProgressEnumerator = UploadProgressEnumerator(uploadFileRequest, onProgressAction);
            StartCoroutine(uploadProgressEnumerator);
            lastUploadRequest = uploadFileRequest;
            return uploadFileRequest;
        }

        private IEnumerator UploadProgressEnumerator(APIRequest request, UnityAction<float> onProgressAction)
        {
            while (request != null && !request.isDone && onProgressAction != null)
            {
                onProgressAction.Invoke(request.uploadProgress);
                yield return null;
            }
        }

        //public void Cancel()
        //{
        //    if (currentWebClient == null)
        //        return;

        //    try
        //    {
        //        currentWebClient.CancelAsync();
        //    }
        //    catch(Exception e)
        //    {
        //        Debug.LogException(e);
        //    }

        //}

        public void CanceFileUpload()
        {
            if (lastUploadRequest != null)
                lastUploadRequest.Kill();
        }
        #endregion

        #region DOWNLOAD
        public FileDownloader Download(int id, string filePath, FileDownloader.OnProgress onProgress, FileDownloader.OnComplete onComplete)
        {
            var parameters = new Dictionary<string, int>();
            parameters.Add("meetingId", id);

            var url = WebInterface.vesApiBaseUrl + WebInterface.downloadFile;
            url = url.AppendParameters(parameters);

            FileDownloader downloader = new FileDownloader();
            var authorizationHeaderValue = "Bearer " + m_webInterface.AccessToken;
            downloader.AddHeader("Authorization", authorizationHeaderValue);
            Debug.Log("Downloading: " + url + " at: " + filePath);
            FileDownloader.onProgress += onProgress;
            FileDownloader.onComplete += onComplete;
            downloader.Download(url, filePath);
            return downloader;
            //DownloadFile();
        }

        public APIRequest Download(string fileUrl, string filePath)
        {
            APIRequest downloadReq = APIRequest.Prepare(fileUrl, APIRequestType.Download_File, false);
            var downloadHandler = new DownloadHandlerFile(filePath);
            downloadHandler.removeFileOnAbort = true;
            downloadReq.downloadHandler = downloadHandler;
            downloadReq.certificateHandler = new CustomDownloadCertificateHandler();
            downloadReq.Run(this, false).OnRequestComplete((error, message) =>
            {
                //Debug.Log(downloadReq.downloadHandler.text);

                // Or retrieve results as binary data
                //byte[] results = downloadReq.downloadHandler.data;
                //File.WriteAllBytes(filePath, results);
            });
            return downloadReq;
        }

        #endregion
    }
}