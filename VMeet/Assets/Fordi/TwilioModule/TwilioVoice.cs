//using Fordi.Common;
//using Fordi.Core;
//using Fordi.Plugins;

//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Twilio;
//using Twilio.Types;
//using Twilio.Rest.Api.V2010.Account;
//using Twilio.TwiML;
//using System;
//using Twilio.TwiML.Voice;
//using Twilio.Http;

//using System.Net;
//using System.ComponentModel.Composition;
//using UniRx;


//namespace Fordi.VMeet.Twilio
//{
//    [Export(typeof(IFordiModule))]
//    [ExportMetadata("name", "VmeetTwilioModule")]
//    public class TwilioVoice : IFordiModule
//    {
//        public string Version { get { return "v1.0.0"; } }
//        public string DepsKey
//        {
//            get
//            {
//                var experienceMachine = IOC.Resolve<IExperienceMachine>();
//                var experienceType = experienceMachine.CurrentExperience;
//                //Debug.LogError("Requesting deps for: " + experienceType.ToString());

//                switch (experienceType)
//                {
//                    case ExperienceType.HOME:
//                        return "7723c6a301c4c404d99a27a6bf62a29e";
//                    case ExperienceType.MEETING:
//                        return "c05fee30e5be7384ba058cac9776385a";
//                    case ExperienceType.LOBBY:
//                        return "c05fee30e5be7384ba058cac9776385a";
//                    default:
//                        return null;
//                }
//            }
//        }

//        public TwilioVoice()
//        {
//            //Observable.TimerFrame(600).Subscribe(_ => Start());
//            Start();
//            Debug.LogError("TwilioVoice " + Version);
//        }



//        private const string ACCOUNT_SID = "AC1026592709fdffc1ebc5361ed23f2496";
//        private const string AUTH_TOKEN = "8ad56360b75ec4713399e8e30407389f";

//        // Start is called before the first frame update
//        void Start()
//        {
//            Debug.LogError("Stasrt");
//            TwilioClient.Init(ACCOUNT_SID, AUTH_TOKEN);
//            var to = new PhoneNumber("+12058594469");
//            var from = new PhoneNumber("+12058594469");
//            var call = CallResource.Create(
//                to: to,
//                from: from,
//                url: new System.Uri("https://handler.twilio.com/twiml/EH7a088917773977e66650a5cffa8f09aa"));
//            Debug.LogError(call.Sid);
//            //StartCoroutine(CreateNewListener());
//        }



//        //private VoiceResponse CreateTwiml()
//        //{
//        //    // TwiML classes can be created as standalone elements
//        //    var gather = new Gather(numDigits: 1, action: new Uri("https://handler.twilio.com/twiml/EH7a088917773977e66650a5cffa8f09aa"), method: HttpMethod.Post)
//        //        .Say("To speak to a real monkey, press 1. Press 2 to record your own monkey howl. Press any other key to start over.");

//        //    // Attributes can be set directly on the object
//        //    gather.Timeout = 100;
//        //    gather.MaxSpeechTime = 200;

//        //    // Arbitrary attributes can be set by calling set/getOption
//        //    var dial = new Dial().SetOption("myAttribute", 200)
//        //                 .SetOption("newAttribute", false);

//        //    var response = new VoiceResponse()
//        //    .Say("Hello Monkey")
//        //    .Play(new Uri("http://demo.twilio.com/hellomonkey/monkey.mp3"))
//        //    .Append(gather)
//        //    .Append(dial);

//        //    var twiml = response.ToString();

//        //    Debug.LogError(response.ToString());

//        //    return response;

//        //}

//        //static List<int> usedPorts = new List<int>();

//        //public IEnumerator CreateNewListener()
//        //{
//        //    if (!HttpListener.IsSupported)
//        //    {
//        //        Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
//        //        yield break;
//        //    }
//        //    // URI prefixes are required,
//        //    // for example "http://contoso.com:8080/index/".


//        //    // Create a listener.
//        //    HttpListener listener = new HttpListener();

//        //    listener.Prefixes.Add("http://localhost:56533/");

//        //    listener.Start();

//        //    //Debug.LogError("listening");
//        //    //// Note: The GetContext method blocks while waiting for a request.
//        //    //HttpListenerContext context = listener.GetContext();
//        //    //HttpListenerRequest request = context.Request;
//        //    //// Obtain a response object.
//        //    //HttpListenerResponse response = context.Response;
//        //    //// Construct a response.
//        //    //string responseString = CreateTwiml().ToString();
//        //    //byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
//        //    //// Get a response stream and write the response to it.
//        //    //response.ContentLength64 = buffer.Length;
//        //    //System.IO.Stream output = response.OutputStream;
//        //    //output.Write(buffer, 0, buffer.Length);
//        //    //// You must close the output stream.
//        //    //output.Close();
//        //    //listener.Stop();
//        //}
//    }
//}


