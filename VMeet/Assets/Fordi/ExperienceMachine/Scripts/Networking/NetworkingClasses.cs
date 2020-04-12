using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fordi.Networking
{
    public interface ISyncHelper
    {
        void PauseSync();
        void ResumeSync();
    }
}
