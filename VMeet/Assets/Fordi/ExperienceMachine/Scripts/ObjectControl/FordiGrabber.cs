/************************************************************************************

Adapted from DistanceGrabber.cs

Copyright (c) Facebook Technologies, LLC and its affiliates. All rights reserved.  

See SampleFramework license.txt for license terms.  Unless required by applicable law 
or agreed to in writing, the sample code is provided “AS IS” WITHOUT WARRANTIES OR 
CONDITIONS OF ANY KIND, either express or implied.  See the license for specific 
language governing permissions and limitations under the license.

************************************************************************************/

using OculusSampleFramework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Fordi.Common;
using Fordi.Core;
using Fordi.UI.MenuControl;
using System;
using Fordi.UI;
#if UNITY_EDITOR
using UnityEngine.SceneManagement;
#endif

namespace Fordi.ObjectControl
{
    /// <summary>
    /// Allows grabbing and throwing of objects with the DistanceGrabbable component on them.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class FordiGrabber : OVRGrabber
    {
        // Radius of sphere used in spherecast from hand along forward ray to find target object.
        [SerializeField]
        public Color m_focusColor;

        // Radius of sphere used in spherecast from hand along forward ray to find target object.
        [SerializeField]
        float m_spherecastRadius = .15f;

        // Distance below which no-snap objects won't be teleported, but will instead be left
        // where they are in relation to the hand.
        [SerializeField]
        float m_noSnapThreshhold = 0.05f;

        [SerializeField]
        bool m_useSpherecast;
        public bool UseSpherecast
        {
            get { return m_useSpherecast; }
            set
            {
                m_useSpherecast = value;
                GrabVolumeEnable(!m_useSpherecast);
            }
        }

        // Public to allow changing in demo.
        [SerializeField]
        public bool m_preventGrabThroughWalls;

        [SerializeField]
        float m_objectPullVelocity = 10.0f;
        float m_objectPullMaxRotationRate = 360.0f; // max rotation rate in degrees per second

        bool m_movingObjectToHand = false;

        // Objects can be distance grabbed up to this distance from the hand.
        [SerializeField]
        float m_maxGrabDistance = 2;

        // Only allow grabbing objects in this layer.
        [SerializeField]
        int m_grabObjectsInLayer;
        [SerializeField]
        int m_obstructionLayer;

        FordiGrabber m_otherHand;

        protected DistanceGrabbable m_target;

        private IUIEngine m_vrMenu;
        private IExperienceMachine m_experienceMachine;

        private bool m_grabIntention = false;

        public static int m_grabCount = 0;
        public static int GrabCount { get { return m_grabCount; } }

        public static EventHandler OnObjectDelete { get; set; }

        // Tracked separately from m_target, because we support child colliders of a DistanceGrabbable.
        // MTF TODO: verify this still works!
        protected Collider m_targetCollider;

        protected override void Start()
        {
            m_grabCount = 0;
            base.Start();

            m_vrMenu = IOC.Resolve<IUIEngine>();
            m_experienceMachine = IOC.Resolve<IExperienceMachine>();
            // Set up our max grab distance to be based on the player's max grab distance.
            // Adding a liberal margin of error here, because users can move away some from the 
            // OVRPlayerController, and also players have arms.
            // Note that there's no major downside to making this value too high, as objects
            // outside the player's grabbable trigger volume will not be eligible targets regardless.
            SphereCollider sc = m_experienceMachine.Player.PlayerController.GetComponentInChildren<SphereCollider>();
            m_maxGrabDistance = sc.radius + 3.0f;

            if (m_parentHeldObject == false)
            {
                Debug.LogError("Non m_parentHeldObject incompatible with FordiGrabber. Setting to true.");
                m_parentHeldObject = true;
            }

            FordiGrabber[] grabbers = FindObjectsOfType<FordiGrabber>();
            for (int i = 0; i < grabbers.Length; ++i)
            {
                if (grabbers[i] != this) m_otherHand = grabbers[i];
            }
            Debug.Assert(m_otherHand != null);

#if UNITY_EDITOR
            OVRPlugin.SendEvent("distance_grabber", (SceneManager.GetActiveScene().name == "DistanceGrab").ToString(), "sample_framework");
#endif
        }

        private void Update()
        {

            Debug.DrawRay(transform.position, transform.forward, Color.red, 0.1f);

            DistanceGrabbable target;
            Collider targetColl;
            FindTarget(out target, out targetColl);

            if (target != m_target)
            {
                if (m_target != null)
                {
                    m_target.Targeted = m_otherHand.m_target == m_target;
                }
                if (m_target != null)
                    m_target.ClearColor();
                if (target != null)
                    target.SetColor(m_focusColor);
                m_target = target;
                m_targetCollider = targetColl;
                if (m_target != null)
                {
                    m_target.Targeted = true;
                }
            }
        }

        private void LateUpdate()
        {
            if (m_grabbedObj != null && OVRInput.GetDown(OVRInput.Button.Two, m_controller))
            {
                var grabbedItem = m_grabbedObj.gameObject;
                ForceRelease(m_grabbedObj);
                Destroy(grabbedItem);
                OnObjectDelete?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void FixedUpdateOverride()
        {
            base.FixedUpdateOverride();
            MoveGrabbedObject();
        }

        protected override void GrabBegin()
        {
            DistanceGrabbable closestGrabbable = m_target;
            Collider closestGrabbableCollider = m_targetCollider;

            GrabVolumeEnable(false);

            if (closestGrabbable != null)
            {
                if (closestGrabbable.isGrabbed)
                {
                    ((FordiGrabber)closestGrabbable.grabbedBy).OffhandGrabbed(closestGrabbable);
                }

                m_grabbedObj = closestGrabbable;
                m_grabbedObj.GrabBegin(this, closestGrabbableCollider);

                m_movingObjectToHand = true;
                m_lastPos = transform.position;
                m_lastRot = transform.rotation;

                // If it's within a certain distance respect the no-snap.
                Vector3 closestPointOnBounds = closestGrabbableCollider.ClosestPointOnBounds(m_gripTransform.position);
                if (!m_grabbedObj.snapPosition && !m_grabbedObj.snapOrientation && m_noSnapThreshhold > 0.0f && (closestPointOnBounds - m_gripTransform.position).magnitude < m_noSnapThreshhold)
                {
                    Vector3 relPos = m_grabbedObj.transform.position - transform.position;
                    m_movingObjectToHand = false;
                    relPos = Quaternion.Inverse(transform.rotation) * relPos;
                    m_grabbedObjectPosOff = relPos;
                    Quaternion relOri = Quaternion.Inverse(transform.rotation) * m_grabbedObj.transform.rotation;
                    m_grabbedObjectRotOff = relOri;
                }
                else
                {
                    // Set up offsets for grabbed object desired position relative to hand.
                    m_grabbedObjectPosOff = m_gripTransform.localPosition;
                    if (m_grabbedObj.snapOffset)
                    {
                        Vector3 snapOffset = m_grabbedObj.snapOffset.position;
                        if (m_controller == OVRInput.Controller.LTouch) snapOffset.x = -snapOffset.x;
                        m_grabbedObjectPosOff += snapOffset;
                    }

                    m_grabbedObjectRotOff = m_gripTransform.localRotation;
                    if (m_grabbedObj.snapOffset)
                    {
                        m_grabbedObjectRotOff = m_grabbedObj.snapOffset.rotation * m_grabbedObjectRotOff;
                    }
                }

                if (m_parentHeldObject)
                {
                    m_grabbedObj.transform.rotation = Quaternion.Euler(new Vector3(0, ((IVRPlayer)m_experienceMachine.Player).CameraRig.transform.rotation.eulerAngles.y, 0));
                    m_grabbedObj.transform.parent = transform;
                }

                FordiInput.Block(OVRInput.Button.One, OVRInput.Controller.LTouch);
                FordiInput.Block(OVRInput.Button.Two, OVRInput.Controller.LTouch);
                FordiInput.Block(OVRInput.Button.One, OVRInput.Controller.RTouch);
                FordiInput.Block(OVRInput.Button.Two, OVRInput.Controller.RTouch);
                m_grabCount++;
                m_experienceMachine.Player.RequestHaltMovement(true);
                if (m_vrMenu == null)
                    m_vrMenu = IOC.Resolve<IUIEngine>();
                m_vrMenu.DeactivateUI();
            }
        }

        protected override void MoveGrabbedObject(Vector3 pos, Quaternion rot, bool forceTeleport = false)
        {
            if (m_grabbedObj == null)
            {
                return;
            }

            Rigidbody grabbedRigidbody = m_grabbedObj.grabbedRigidbody;
            Vector3 grabbablePosition = pos + rot * m_grabbedObjectPosOff;
            Quaternion grabbableRotation = rot * m_grabbedObjectRotOff;

            if (m_movingObjectToHand)
            {
                float travel = m_objectPullVelocity * Time.deltaTime;
                Vector3 dir = grabbablePosition - m_grabbedObj.transform.position;
                if (travel * travel * 1.1f > dir.sqrMagnitude)
                {
                    m_movingObjectToHand = false;
                }
                else
                {
                    dir.Normalize();
                    grabbablePosition = m_grabbedObj.transform.position + dir * travel;
                    grabbableRotation = Quaternion.RotateTowards(m_grabbedObj.transform.rotation, grabbableRotation, m_objectPullMaxRotationRate * Time.deltaTime);
                }
            }

            grabbedRigidbody.transform.position = grabbablePosition;
            grabbedRigidbody.transform.rotation = grabbableRotation;

            //grabbedRigidbody.MovePosition(grabbablePosition);
            //grabbedRigidbody.MoveRotation(grabbableRotation);
        }

        static private DistanceGrabbable HitInfoToGrabbable(RaycastHit hitInfo)
        {
            if (hitInfo.collider != null)
            {
                GameObject go = hitInfo.collider.gameObject;
                return go.GetComponent<DistanceGrabbable>() ?? go.GetComponentInParent<DistanceGrabbable>();
            }
            return null;
        }

        protected bool FindTarget(out DistanceGrabbable dgOut, out Collider collOut)
        {
            dgOut = null;
            collOut = null;
            float closestMagSq = float.MaxValue;

            // First test for objects within the grab volume, if we're using those.
            // (Some usage of FordiGrabber will not use grab volumes, and will only 
            // use spherecasts, and that's supported.)
            //foreach (OVRGrabbable cg in m_grabCandidates.Keys)
            //{
            //    DistanceGrabbable grabbable = cg as DistanceGrabbable;
            //    bool canGrab = grabbable != null && grabbable.InRange && !(grabbable.isGrabbed && !grabbable.allowOffhandGrab);
            //    if (!canGrab)
            //    {
            //        continue;
            //    }

            //    for (int j = 0; j < grabbable.grabPoints.Length; ++j)
            //    {
            //        Collider grabbableCollider = grabbable.grabPoints[j];
            //        // Store the closest grabbable
            //        Vector3 closestPointOnBounds = grabbableCollider.ClosestPointOnBounds(m_gripTransform.position);
            //        float grabbableMagSq = (m_gripTransform.position - closestPointOnBounds).sqrMagnitude;
            //        if (grabbableMagSq < closestMagSq)
            //        {
            //            bool accept = true;
            //            if (m_preventGrabThroughWalls)
            //            {
            //                // NOTE: if this raycast fails, ideally we'd try other rays near the edges of the object, especially for large objects.
            //                // NOTE 2: todo optimization: sort the objects before performing any raycasts.
            //                Ray ray = new Ray();
            //                ray.direction = grabbable.transform.position - m_gripTransform.position;
            //                ray.origin = m_gripTransform.position;
            //                RaycastHit obstructionHitInfo;
            //                Debug.DrawRay(ray.origin, ray.direction, Color.red, 0.1f);

            //                if (Physics.Raycast(ray, out obstructionHitInfo, m_maxGrabDistance, 1 << m_obstructionLayer))
            //                {
            //                    float distToObject = (grabbableCollider.ClosestPointOnBounds(m_gripTransform.position) - m_gripTransform.position).magnitude;
            //                    if (distToObject > obstructionHitInfo.distance * 1.1)
            //                    {
            //                        accept = false;
            //                    }
            //                }
            //            }
            //            if (accept)
            //            {
            //                closestMagSq = grabbableMagSq;
            //                dgOut = grabbable;
            //                collOut = grabbableCollider;
            //            }
            //        }
            //    }
            //}

            if (dgOut == null)
            {
                return FindTargetWithRaycast(out dgOut, out collOut);
            }

            if (dgOut == null && m_useSpherecast)
            {
                return FindTargetWithSpherecast(out dgOut, out collOut);
            }
            return dgOut != null;
        }

        protected bool FindTargetWithSpherecast(out DistanceGrabbable dgOut, out Collider collOut)
        {
            dgOut = null;
            collOut = null;
            Ray ray = new Ray(m_gripTransform.position, m_gripTransform.forward);
            RaycastHit hitInfo;

            // If no objects in grab volume, raycast.
            // Potential optimization: 
            // In DistanceGrabbable.RefreshCrosshairs, we could move the object between collision layers.
            // If it's in range, it would move into the layer FordiGrabber.m_grabObjectsInLayer,
            // and if out of range, into another layer so it's ignored by FordiGrabber's SphereCast.
            // However, we're limiting the SphereCast by m_maxGrabDistance, so the optimization doesn't seem
            // essential.

            m_maxGrabDistance = ((IVRPlayer)m_experienceMachine.Player).CameraRig.leftEyeCamera.farClipPlane - ((IVRPlayer)m_experienceMachine.Player).CameraRig.leftEyeCamera.nearClipPlane;

            if (Physics.SphereCast(ray, m_spherecastRadius, out hitInfo, m_maxGrabDistance, 1 << m_grabObjectsInLayer))
            {
                DistanceGrabbable grabbable = null;
                Collider hitCollider = null;
                if (hitInfo.collider != null)
                {
                    grabbable = hitInfo.collider.gameObject.GetComponentInParent<DistanceGrabbable>();
                    hitCollider = grabbable == null ? null : hitInfo.collider;
                    if (grabbable)
                    {
                        dgOut = grabbable;
                        collOut = hitCollider;
                    }
                }

                if (grabbable != null && m_preventGrabThroughWalls)
                {
                    // Found a valid hit. Now test to see if it's blocked by collision.
                    RaycastHit obstructionHitInfo;
                    ray.direction = hitInfo.point - m_gripTransform.position;

                    dgOut = grabbable;
                    collOut = hitCollider;
                    if (Physics.Raycast(ray, out obstructionHitInfo, 1 << m_obstructionLayer))
                    {
                        DistanceGrabbable obstruction = null;
                        if (hitInfo.collider != null)
                        {
                            obstruction = obstructionHitInfo.collider.gameObject.GetComponentInParent<DistanceGrabbable>();
                        }
                        if (obstruction != grabbable && obstructionHitInfo.distance < hitInfo.distance)
                        {
                            dgOut = null;
                            collOut = null;
                        }
                    }
                }
            }
            return dgOut != null;
        }

        protected bool FindTargetWithRaycast(out DistanceGrabbable dgOut, out Collider collOut)
        {
            dgOut = null;
            collOut = null;
            Ray ray = new Ray(m_gripTransform.position, m_gripTransform.forward);
            RaycastHit hitInfo;

            // If no objects in grab volume, raycast.
            // Potential optimization: 
            // In DistanceGrabbable.RefreshCrosshairs, we could move the object between collision layers.
            // If it's in range, it would move into the layer FordiGrabber.m_grabObjectsInLayer,
            // and if out of range, into another layer so it's ignored by FordiGrabber's SphereCast.
            // However, we're limiting the SphereCast by m_maxGrabDistance, so the optimization doesn't seem
            // essential.

            m_maxGrabDistance = ((IVRPlayer)m_experienceMachine.Player).CameraRig.leftEyeCamera.farClipPlane - ((IVRPlayer)m_experienceMachine.Player).CameraRig.leftEyeCamera.nearClipPlane;

            if (Physics.Raycast(ray, out hitInfo, m_maxGrabDistance, 1 << m_grabObjectsInLayer))
            {
                DistanceGrabbable grabbable = null;
                Collider hitCollider = null;
                if (hitInfo.collider != null)
                {
                    grabbable = hitInfo.collider.gameObject.GetComponentInParent<DistanceGrabbable>();
                    hitCollider = grabbable == null ? null : hitInfo.collider;
                    if (grabbable)
                    {
                        dgOut = grabbable;
                        collOut = hitCollider;
                    }
                }

                if (grabbable != null && m_preventGrabThroughWalls)
                {
                    // Found a valid hit. Now test to see if it's blocked by collision.
                    RaycastHit obstructionHitInfo;
                    ray.direction = hitInfo.point - m_gripTransform.position;

                    dgOut = grabbable;
                    collOut = hitCollider;
                    if (Physics.Raycast(ray, out obstructionHitInfo, 1 << m_obstructionLayer))
                    {
                        DistanceGrabbable obstruction = null;
                        if (hitInfo.collider != null)
                        {
                            obstruction = obstructionHitInfo.collider.gameObject.GetComponentInParent<DistanceGrabbable>();
                        }
                        if (obstruction != grabbable && obstructionHitInfo.distance < hitInfo.distance)
                        {
                            dgOut = null;
                            collOut = null;
                        }
                    }
                }
            }
            return dgOut != null;
        }


        protected override void GrabVolumeEnable(bool enabled)
        {
            if (m_useSpherecast) enabled = false;
            base.GrabVolumeEnable(enabled);
        }

        // Just here to allow calling of a protected member function.
        protected override void OffhandGrabbed(OVRGrabbable grabbable)
        {
            base.OffhandGrabbed(grabbable);
        }

        protected override void CheckForGrabOrRelease(float prevFlex)
        {
            if (m_grabIntention && OVRInput.GetDown(OVRInput.Button.One, m_controller))
            {
                ((IVRPlayer)m_experienceMachine.Player).ToogleGrabGuide(m_controller, false);
                GrabBegin();
            }

            if ((m_prevFlex >= grabBegin) && (prevFlex < grabBegin))
            {
                if (m_grabbedObj == null && m_target != null)
                {
                    m_grabIntention = true;
                    ((IVRPlayer)m_experienceMachine.Player).ToogleGrabGuide(m_controller, true);
                }
            }
            else if ((m_prevFlex <= grabEnd) && (prevFlex > grabEnd))
            {
                m_grabIntention = false;
                ((IVRPlayer)m_experienceMachine.Player).ToogleGrabGuide(m_controller, false);

                GrabEnd();
            }

            if (m_target == null)
            {
                m_grabIntention = false;
                ((IVRPlayer)m_experienceMachine.Player).ToogleGrabGuide(m_controller, false);
            }
        }

        public void ForceGrab(DistanceGrabbable target)
        {
            float closestMagSq = float.MaxValue;

            DistanceGrabbable grabbable = target;
            bool canGrab = grabbable != null && !(grabbable.isGrabbed && !grabbable.allowOffhandGrab);
            if (!canGrab)
            {
                Debug.LogError("Grabbable.InRange: " + grabbable.InRange);
                return;
            }

            for (int j = 0; j < grabbable.grabPoints.Length; ++j)
            {
                Collider grabbableCollider = grabbable.grabPoints[j];
                // Store the closest grabbable
                Vector3 closestPointOnBounds = grabbableCollider.ClosestPointOnBounds(m_gripTransform.position);
                float grabbableMagSq = (m_gripTransform.position - closestPointOnBounds).sqrMagnitude;
                if (grabbableMagSq < closestMagSq)
                {
                    bool accept = true;
                    if (m_preventGrabThroughWalls)
                    {
                        // NOTE: if this raycast fails, ideally we'd try other rays near the edges of the object, especially for large objects.
                        // NOTE 2: todo optimization: sort the objects before performing any raycasts.
                        Ray ray = new Ray();
                        ray.direction = grabbable.transform.position - m_gripTransform.position;
                        ray.origin = m_gripTransform.position;
                        RaycastHit obstructionHitInfo;
                        Debug.DrawRay(ray.origin, ray.direction, Color.red, 0.1f);

                        if (Physics.Raycast(ray, out obstructionHitInfo, m_maxGrabDistance, 1 << m_obstructionLayer))
                        {
                            float distToObject = (grabbableCollider.ClosestPointOnBounds(m_gripTransform.position) - m_gripTransform.position).magnitude;
                            if (distToObject > obstructionHitInfo.distance * 1.1)
                            {
                                accept = false;
                            }
                        }
                    }
                    if (accept)
                    {
                        closestMagSq = grabbableMagSq;
                        m_targetCollider = grabbableCollider;
                    }
                }
            }


            if (target != m_target)
            {
                if (m_target != null)
                {
                    m_target.Targeted = m_otherHand.m_target == m_target;
                }
                if (m_target != null)
                    m_target.ClearColor();
                if (target != null)
                    target.SetColor(m_focusColor);
                m_target = target;

                if (m_target != null)
                {
                    m_target.Targeted = true;
                }

                GrabBegin();
            }
        }

        protected override void GrabEnd()
        {
            base.GrabEnd();
        }

        private void MoveGrabbedObject()
        {
            if (m_grabbedObj == null)
                return;

            Rigidbody grabbedRigidbody = m_grabbedObj.grabbedRigidbody;
            Vector3 grabbablePosition = transform.position + transform.rotation * m_grabbedObjectPosOff;
            //Quaternion grabbableRotation = transform.rotation * m_grabbedObjectRotOff;

            Vector2 primaryAxis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, m_controller);
            if ((Mathf.Abs(primaryAxis.y) > Mathf.Abs(primaryAxis.x)))
            {
                float travel = primaryAxis.y * Time.deltaTime * 10;
                Vector3 dir = grabbablePosition - m_grabbedObj.transform.position;
                if (primaryAxis.y > 0)
                {
                    grabbablePosition = m_grabbedObj.transform.position + transform.forward * primaryAxis.y * Time.deltaTime * 10;
                    //if (grabbablePosition.y < 0)
                    //    grabbablePosition = new Vector3(grabbablePosition.x, 0, grabbablePosition.z);
                }
                else if (travel * travel * 1.5f <= dir.sqrMagnitude)
                    grabbablePosition = m_grabbedObj.transform.position + Vector3.Normalize(m_grabbedObj.transform.position - grabbablePosition) * primaryAxis.y * Time.deltaTime * 10;
                m_grabbedObj.transform.position = grabbablePosition;
            }
            else
            {
                m_grabbedObj.transform.RotateAround(m_grabbedObj.transform.position, m_grabbedObj.transform.up, primaryAxis.x * Time.deltaTime * 700);
            }


        }

        protected override void GrabbableRelease(Vector3 linearVelocity, Vector3 angularVelocity)
        {
            base.GrabbableRelease(linearVelocity, angularVelocity);
            FordiInput.Unblock(OVRInput.Button.One, OVRInput.Controller.LTouch);
            FordiInput.Unblock(OVRInput.Button.Two, OVRInput.Controller.LTouch);
            FordiInput.Unblock(OVRInput.Button.One, OVRInput.Controller.RTouch);
            FordiInput.Unblock(OVRInput.Button.Two, OVRInput.Controller.RTouch);
            if (m_grabCount > 0)
                m_grabCount--;
            m_experienceMachine.Player.RequestHaltMovement(false);
            m_vrMenu.ShowUI();
        }
    }
}
