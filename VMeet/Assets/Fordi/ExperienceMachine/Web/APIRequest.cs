using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using VRExperience.Common;
using VRExperience.Core;
using VRExperience.UI.MenuControl;

namespace Cornea.Web
{
    public class APIRequest : UnityWebRequest
    {
        private OnCompleteAction OnRequestCompleteAction = null;
        private IEnumerator requestCoroutine = null;
        private MonoBehaviour callingScript = null;

        protected WWWForm form = null;

        public APIRequestType requestType;

        private static void SetupPost(UnityWebRequest request, WWWForm formData)
        {
            byte[] array = null;
            if (formData != null)
            {
                array = formData.data;
                if (array.Length == 0)
                {
                    array = null;
                }
            }
            request.uploadHandler = new UploadHandlerRaw(array);
            request.downloadHandler = new DownloadHandlerBuffer();
            if (formData != null)
            {
                Dictionary<string, string> headers = formData.headers;
                foreach (KeyValuePair<string, string> current in headers)
                {
                    request.SetRequestHeader(current.Key, current.Value);
                }
            }
        }

        private static void SetupPost(UnityWebRequest request, List<IMultipartFormSection> multipartFormSections, byte[] boundary)
        {
            byte[] data = null;
            if (multipartFormSections != null && multipartFormSections.Count != 0)
            {
                data = SerializeFormSections(multipartFormSections, boundary);
            }
            request.uploadHandler = new UploadHandlerRaw(data)
            {
                contentType = "multipart/form-data; boundary=" + Encoding.UTF8.GetString(boundary, 0, boundary.Length)
            };
            request.downloadHandler = new DownloadHandlerBuffer();
        }

        private IEnumerator POST(bool autoErrorHandling)
        {
            if (form != null && form.headers.ContainsKey("Content-Type"))
            {
                form.headers["Content-Type"] = "application/json";
                //form.headers["Content-Type"] = "application/json";
            }
            else if (form != null)
                form.headers.Add("Content-Type", "application/json");

            yield return SendWebRequest();

            if (!autoErrorHandling)
            {
                if (OnRequestCompleteAction != null)
                {
                    try
                    {
                        OnRequestCompleteAction(isNetworkError || isHttpError, downloadHandler.text.ToString());
                    }
                    catch (NotSupportedException)
                    {
                        OnRequestCompleteAction(isNetworkError || isHttpError, "");
                    }
                }
                yield break;
            }

            if (isNetworkError)
            {
                if (!error.Contains("abort"))
                    IOC.Resolve<IWebInterface>().NetworkInterface.ActivateErrorScreen(error, this);
            }
            else
            {
                IOC.Resolve<IWebInterface>().NetworkInterface.RemoveRequest(this);
                if (OnRequestCompleteAction != null)
                    OnRequestCompleteAction(false, downloadHandler.text.ToString());
            }
        }

        private IEnumerator GET(bool autoErrorHandling)
        {
            yield return SendWebRequest();

            if (!autoErrorHandling)
            {
                if (OnRequestCompleteAction != null)
                {
                    try
                    {
                        OnRequestCompleteAction(isNetworkError || isHttpError, downloadHandler.text.ToString());
                    }
                    catch(NotSupportedException)
                    {
                        OnRequestCompleteAction(isNetworkError || isHttpError, "");
                    }
                }
                yield break;
            }

            if (isNetworkError)
            {
                Debug.Log("isNetworkError: " + error + " " + requestType.ToString());

                if (!error.Contains("abort"))
                {
                    Error errorHandler = new Error(Error.E_NetworkIssue);
                    errorHandler.ErrorText = error;
                    IOC.Resolve<IVRMenu>().DisplayError(errorHandler, true);
                    //Coordinator.instance.webInterface.networkInterface.ActivateErrorScreen(error, this);
                }
            }
            else
            {
                //Debug.Log("Request success: " + requestType.ToString());
                IOC.Resolve<IWebInterface>().NetworkInterface.RemoveRequest(this);
                if (OnRequestCompleteAction != null)
                    OnRequestCompleteAction(false, downloadHandler.text.ToString());
            }
        }

        public void AddOnCompleteListener(OnCompleteAction _OnCompleteAction)
        {
            OnRequestCompleteAction += _OnCompleteAction;
        }

        public APIRequest(string url, string method, DownloadHandler downloadHandler, UploadHandler uploadHandler) : base(url, method, downloadHandler, uploadHandler)
        {
        }

        public APIRequest(string url, string method) : base(url, method)
        {
        }

        public static APIRequest Prepare(WWWForm _form, string url, string reqMethod, APIRequestType _requestType)
        {
            APIRequest req;

            req = new APIRequest(" ", UnityWebRequest.kHttpVerbPOST);

            if (reqMethod.Equals(kHttpVerbGET))
                req = Get(url);
            else
                req = Post(url, _form);
            req.SetRequestHeader("Authorization", "Bearer " + IOC.Resolve<IWebInterface>().AccessToken);
            req.form = _form;
            req.requestType = _requestType;
            return req;
        }

        /// <summary>
        /// Only for plain get requests
        /// </summary>
        /// <param name="url"></param>
        /// <param name="reqMethod"></param>
        /// <param name="_requestType"></param>
        /// <returns></returns>
        public static APIRequest Prepare(string url, APIRequestType _requestType, bool setToken = true)
        {
            APIRequest req;
            req = Get(url);
            if (setToken)
                req.SetRequestHeader("Authorization", "Bearer " + IOC.Resolve<IWebInterface>().AccessToken);
            req.form = null;
            req.requestType = _requestType;

            return req;
        }

        public static APIRequest Prepare(List<IMultipartFormSection> data, string url, string reqMethod, APIRequestType _requestType)
        {
            APIRequest req;

            if (reqMethod.Equals(kHttpVerbGET))
                req = Get(url);
            else
                req = Post(url, data);
            req.SetRequestHeader("Authorization", "Bearer " + IOC.Resolve<IWebInterface>().AccessToken);

            //req.form = _form;
            req.requestType = _requestType;

            return req;
        }

        public new static APIRequest Post(string uri, List<IMultipartFormSection> multipartFormSections)
        {
            byte[] boundary = GenerateBoundary();
            return Post(uri, multipartFormSections, boundary);
        }

        public new static APIRequest Post(string uri, List<IMultipartFormSection> multipartFormSections, byte[] boundary)
        {
            APIRequest req = new APIRequest(uri, kHttpVerbPOST);
            SetupPost(req, multipartFormSections, boundary);
            return req;
        }

        public new static APIRequest Get(string uri)
        {
            return new APIRequest(uri, kHttpVerbGET, new DownloadHandlerBuffer(), null);
        }

        public new static APIRequest Post(string uri, WWWForm formData)
        {
            APIRequest req = new APIRequest(uri, kHttpVerbPOST);
            APIRequest.SetupPost(req, formData);
            return req;
        }

        public APIRequest Run(MonoBehaviour _callingClass, bool autoErrorHandling = true)
        {
            //Debug.Log("Run: " + url + " " + requestType.ToString());
            callingScript = _callingClass;
            if (method.Equals(kHttpVerbPOST))
                requestCoroutine = POST(autoErrorHandling);
            else
                requestCoroutine = GET(autoErrorHandling);

            if (_callingClass == null)
                return this;

            _callingClass.StartCoroutine(requestCoroutine);
            return this;
        }

        public APIRequest Refresh(bool autoErrorHandling = true)
        {
            Debug.Log("Refresh: " + url + " " + requestType.ToString());
            APIRequest newReq = APIRequest.Prepare(form, url, method, requestType);
            if (uploadHandler != null && uploadHandler.data != null)
                newReq.uploadHandler = new UploadHandlerRaw(uploadHandler.data);
            var contentType = GetRequestHeader("Content-Type");
            if (contentType != null)
                newReq.SetRequestHeader("Content-Type", contentType);
            newReq.Run(callingScript, autoErrorHandling).OnRequestComplete(OnRequestCompleteAction);
            Kill();
            return newReq;
        }

        public void Kill()
        {
            OnRequestCompleteAction = null;
            try
            {
                this.Abort();
                //this.OnRequestCompleteAction = null;
                //this.Dispose();
            }
            catch(Exception)
            {

            }
        }
    }
}
