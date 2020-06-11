using Fordi.AssetManagement;
using Fordi.Common;
using Fordi.Core;
using Fordi.Plugins;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

//[Export(typeof(IFordiComponent))]
//[ExportMetadata("name", "VMeet Oculus Module")]
//public class OculusModule : IFordiComponent
//{
//    public string Version { get { return "v1.0.0"; } }
//    public string DepsKey {
//        get
//        {
//            var experienceMachine = IOC.Resolve<IExperienceMachine>();
//            var experienceType = experienceMachine.CurrentExperience;

//            Debug.LogError("Deps requested for: " + experienceType.ToString());

//            switch (experienceType)
//            {
//                case ExperienceType.HOME:
//                    return "c05fee30e5be7384ba058cac9776385a";
//                case ExperienceType.MEETING:
//                    return "c05fee30e5be7384ba058cac9776385a";
//                case ExperienceType.LOBBY:
//                    return "c05fee30e5be7384ba058cac9776385a";
//                default:
//                    return null;
//            }
//        }
//    }


//}
