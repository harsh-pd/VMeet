using Fordi.Common;
using Fordi.Core;
using Fordi.Plugins;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Fordi.VMeet.Oculus
{
    [Export(typeof(IPlatformComponent))]
    [ExportMetadata("name", "VMeetOculusModule")]
    public class OculusModule : IPlatformComponent
    {
        public string Version { get { return "v1.0.0"; } }
        public string DepsKey
        {
            get
            {
                var experienceMachine = IOC.Resolve<IExperienceMachine>();
                var experienceType = experienceMachine.CurrentExperience;
                //Debug.LogError("Requesting deps for: " + experienceType.ToString());

                switch (experienceType)
                {
                    case ExperienceType.HOME:
                        return "7723c6a301c4c404d99a27a6bf62a29e";
                    case ExperienceType.MEETING:
                        return "c05fee30e5be7384ba058cac9776385a";
                    case ExperienceType.LOBBY:
                        return "c05fee30e5be7384ba058cac9776385a";
                    default:
                        return null;
                }
            }
        }

        private void TestFunc()
        {
        }


    }
}
