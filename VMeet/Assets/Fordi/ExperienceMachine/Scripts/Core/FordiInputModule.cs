using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.EventSystems
{
    /// <summary>
    /// Modified version of OVRInputModule to enable second optional button for click.
    /// </summary>
    public class FordiInputModule : OVRInputModule
    {
        [SerializeField]
        private OVRInput.Button m_additionalClickButton;

        /// <summary>
        /// Get state of button corresponding to gaze pointer
        /// </summary>
        /// <returns></returns>
        override protected PointerEventData.FramePressState GetGazeButtonState()
        {
            var pressed = Input.GetKeyDown(gazeClickKey) || OVRInput.GetDown(joyPadClickButton) || OVRInput.GetDown(m_additionalClickButton, OVRInput.Controller.RTouch);
            var released = Input.GetKeyUp(gazeClickKey) || OVRInput.GetUp(joyPadClickButton) || OVRInput.GetUp(m_additionalClickButton, OVRInput.Controller.RTouch);

#if UNITY_ANDROID && !UNITY_EDITOR
            // On Gear VR the mouse button events correspond to touch pad events. We only use these as gaze pointer clicks
            // on Gear VR because on PC the mouse clicks are used for actual mouse pointer interactions.
            pressed |= Input.GetMouseButtonDown(0);
            released |= Input.GetMouseButtonUp(0);
#endif

            if (pressed && released)
                return PointerEventData.FramePressState.PressedAndReleased;
            if (pressed)
                return PointerEventData.FramePressState.Pressed;
            if (released)
                return PointerEventData.FramePressState.Released;
            return PointerEventData.FramePressState.NotChanged;
        }

        // The following 2 functions are equivalent to PointerInputModule.GetMousePointerEventData but are customized to
        // get data for ray pointers and canvas mouse pointers.

        /// <summary>
        /// State for a pointer controlled by a world space ray. E.g. gaze pointer
        /// </summary>
        /// <returns></returns>
        override protected MouseState GetGazePointerData()
        {
            if (m_Cursor == null)
            {
                m_Cursor = FindObjectOfType<LaserPointer>();
                if (m_Cursor == null)
                    return m_MouseState;
            }
            // Get the OVRRayPointerEventData reference
            OVRPointerEventData leftData;
            GetPointerData(kMouseLeftId, out leftData, true);
            leftData.Reset();

            //Now set the world space ray. This ray is what the user uses to point at UI elements
            leftData.worldSpaceRay = new Ray(rayTransform.position, rayTransform.forward);
            leftData.scrollDelta = GetExtraScrollDelta();

            //Populate some default values
            leftData.button = PointerEventData.InputButton.Left;
            leftData.useDragThreshold = true;
            // Perform raycast to find intersections with world
            eventSystem.RaycastAll(leftData, m_RaycastResultCache);
            var raycast = FindFirstRaycast(m_RaycastResultCache);
            leftData.pointerCurrentRaycast = raycast;
            m_RaycastResultCache.Clear();

            m_Cursor.SetCursorRay(rayTransform);

            OVRRaycaster ovrRaycaster = raycast.module as OVRRaycaster;
            // We're only interested in intersections from OVRRaycasters
            if (ovrRaycaster)
            {
                // The Unity UI system expects event data to have a screen position
                // so even though this raycast came from a world space ray we must get a screen
                // space position for the camera attached to this raycaster for compatability
                leftData.position = ovrRaycaster.GetScreenPosition(raycast);

                // Find the world position and normal the Graphic the ray intersected
                RectTransform graphicRect = raycast.gameObject.GetComponent<RectTransform>();
                if (graphicRect != null)
                {
                    // Set are gaze indicator with this world position and normal
                    Vector3 worldPos = raycast.worldPosition;
                    Vector3 normal = GetRectTransformNormal(graphicRect);
                    if (m_Cursor is LaserPointer laserPointer)
                        laserPointer.SetCursorStartDest(rayTransform.position, worldPos, normal, raycast.sortingLayer);
                    else
                        m_Cursor.SetCursorStartDest(rayTransform.position, worldPos, normal);
                }
            }

            // Now process physical raycast intersections
            OVRPhysicsRaycaster physicsRaycaster = raycast.module as OVRPhysicsRaycaster;
            if (physicsRaycaster)
            {
                Vector3 position = raycast.worldPosition;
                int sortingLayer = 0;

                if (performSphereCastForGazepointer)
                {
                    // Here we cast a sphere into the scene rather than a ray. This gives a more accurate depth
                    // for positioning a circular gaze pointer
                    List<RaycastResult> results = new List<RaycastResult>();
                    physicsRaycaster.Spherecast(leftData, results, m_SpherecastRadius);
                    if (results.Count > 0 && results[0].distance < raycast.distance)
                    {
                        position = results[0].worldPosition;
                        sortingLayer = results[0].sortingLayer;
                    }
                }

                leftData.position = physicsRaycaster.GetScreenPos(raycast.worldPosition);

                if (m_Cursor is LaserPointer laserPointer)
                    laserPointer.SetCursorStartDest(rayTransform.position, position, raycast.worldNormal, sortingLayer);
                else
                    m_Cursor.SetCursorStartDest(rayTransform.position, position, raycast.worldNormal);
            }

            // Stick default data values in right and middle slots for compatability

            // copy the apropriate data into right and middle slots
            OVRPointerEventData rightData;
            GetPointerData(kMouseRightId, out rightData, true);
            CopyFromTo(leftData, rightData);
            rightData.button = PointerEventData.InputButton.Right;

            OVRPointerEventData middleData;
            GetPointerData(kMouseMiddleId, out middleData, true);
            CopyFromTo(leftData, middleData);
            middleData.button = PointerEventData.InputButton.Middle;


            m_MouseState.SetButtonState(PointerEventData.InputButton.Left, GetGazeButtonState(), leftData);
            m_MouseState.SetButtonState(PointerEventData.InputButton.Right, PointerEventData.FramePressState.NotChanged, rightData);
            m_MouseState.SetButtonState(PointerEventData.InputButton.Middle, PointerEventData.FramePressState.NotChanged, middleData);
            return m_MouseState;
        }
    }
}
