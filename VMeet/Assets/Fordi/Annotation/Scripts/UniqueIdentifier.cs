using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fordi.Annotation
{
    public class UniqueIdentifier : MonoBehaviour
    {
        public int playerId = 0;
        public Trail currentDefaultTrail;
        public Transform rightHand, leftHand;
        public OVRInput.Controller selectedController = OVRInput.Controller.RTouch;
        public Transform pen;
    }
}
