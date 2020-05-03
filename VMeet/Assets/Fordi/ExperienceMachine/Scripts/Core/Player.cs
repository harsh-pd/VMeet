using Oculus.Platform;
using Oculus.Platform.Models;
using OculusSampleFramework;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using VRExperience.Common;
using VRExperience.ObjectControl;
using VRExperience.UI;
using VRExperience.UI.MenuControl;

namespace VRExperience.Core
{
    public interface IPlayer
    {
        OVRPlayerController PlayerController { get; }
        void RequestHaltMovement(bool val);
        Transform PlayerCanvas { get; }
        OVRCameraRig CameraRig { get; }
        Transform RightHand { get; }
        Transform LeftHand { get; }
        int PlayerViewId { get; set; }
        int AvatarViewId { get; set; }
        void PrepareForSpawn();
        void UpdateAdditionalRotation(float angle);
        float RootRotation { get; }
        void StartTooltipRoutine(List<VRButtonGroup> buttonGroups);
        void ApplyTooltipSettings();
        void Grab(DistanceGrabbable grabbable, OVRInput.Controller controller);
        void ToogleGrabGuide(OVRInput.Controller controller, bool val);
        bool GuideOn { get; }
        void DoWaypointTeleport(Transform anchor);
        void FadeOut();
    }

    public class ToolTip
    {
        private VRButton VRButton;
        private Condition Condition;
        public string Tip;


        public ToolTip(string tip, VRButton vRButton, Condition condition)
        {
            Tip = tip;
            VRButton = vRButton;
            Condition = condition;
        }

        private bool m_lastActionResult = false;

        public bool CheckCondition()
        {
            if (Condition == null || Condition.target == null || string.IsNullOrEmpty(Condition.methodName))
            {
                return FordiInput.LastPressedButton == VRButton.Button && FordiInput.LastPressedController == VRButton.Controller;
            }
            return Condition.Invoke() && FordiInput.LastPressedButton == VRButton.Button && FordiInput.LastPressedController == VRButton.Controller;
        }
    }

    [Serializable]
    public class Condition : SerializableCallback<bool> { }

    public enum ControllerState
    {
        BOTH_ACTIVE,
        ONE_ACTIVE,
        NONE_ACTIVE
    }

    public class Player : MonoBehaviour, IPlayer
    {
        [SerializeField]
        private ExperienceMachine m_experienceMachinePrefab;
        [SerializeField]
        private OVRPlayerController m_playerController;
        [SerializeField]
        private Transform m_playerCanvas;
        [SerializeField]
        private FordiTeleport m_teleport;
        [SerializeField]
        private OvrAvatar m_avatar;
        [SerializeField]
        PhotonView m_playerPhotonView;
        [SerializeField]
        PhotonView m_avatarPhotonView;


        [SerializeField]
        private FordiGrabber m_leftGrabber, m_rightGrabber;

        [SerializeField]
        private AudioClip m_welldoneClip;

        [SerializeField]
        private List<UIJoint> m_controllerTips;
       
        [SerializeField]
        private List<ButtonTip> m_leftControllerShortTips, m_rightControllerShortTips;

        [SerializeField]
        private GameObject m_leftGrabGuidePrefab, m_rightGrabGuidePrefab;

        [SerializeField]
        private OVRScreenFade m_fadeScript;

        public int PlayerViewId { get { return m_playerPhotonView.ViewID; } set { m_playerPhotonView.ViewID = value; } }
        public int AvatarViewId { get { return m_avatarPhotonView.ViewID; } set { m_avatarPhotonView.ViewID = value; } }

        private GameObject m_leftGrabGuide, m_rightGrabGuide;

        private List<ButtonTip> m_shortTooltips = new List<ButtonTip>();
        private OvrAvatarTouchController m_leftController, m_rightController;
        private List<UIJoint> m_tooltips = new List<UIJoint>();

        public float RootRotation { get; private set; }

        public OVRPlayerController PlayerController { get { return m_playerController; } }
        public Transform PlayerCanvas { get { return m_playerCanvas; } }

        [SerializeField]
        private OVRCameraRig m_cameraRig;

        public OVRCameraRig CameraRig { get { return m_cameraRig; } }

        [SerializeField]
        private Transform m_leftHandAnchor, m_rightHandAnchor;
        public Transform LeftHand { get { return m_leftHandAnchor; } }
        public Transform RightHand { get { return m_rightHandAnchor; } }

        public bool GuideOn { get { return m_tooltips.Count > 0; } }

        private bool m_shortTipOn = false;
        private bool m_initialized = false;

        private ISettings m_settings;
        private IVRMenu m_vrMenu;
        private IAudio m_audio;

        [SerializeField]
        private List<ButtonTip> m_movementButtonTips = new List<ButtonTip>();

        private void Awake()
        {
            m_settings = IOC.Resolve<ISettings>();
            m_vrMenu = IOC.Resolve<IVRMenu>();
            m_audio = IOC.Resolve<IAudio>();
            FordiGrabber.OnObjectDelete += OnObjectDelete;

            Oculus.Platform.Core.Initialize();
            Users.GetLoggedInUser().OnComplete(GetLoggedInUserCallback);
            Request.RunCallbacks();

            StartCoroutine(ConigureHandColor());
        }

        private IEnumerator ConigureHandColor()
        {
            var hands = m_avatar.GetComponentsInChildren<OvrAvatarHand>();
            while(hands.Length < 2)
            {
                yield return new WaitForSeconds(.2f);
                hands = m_avatar.GetComponentsInChildren<OvrAvatarHand>();
            }

            foreach (var item in hands)
            {
                var renderer = item.transform.GetChild(0).GetComponent<Renderer>();
                if (renderer != null)
                    renderer.material.SetColor("_BaseColor", new Color32(217, 117, 117, 255));
            }
        }


        private void GetLoggedInUserCallback(Message<User> message)
        {
            if (!message.IsError)
            {
                m_avatar.oculusUserID = message.Data.ID.ToString();
                m_experienceMachinePrefab?.SetupPersonalisedAvatar(m_avatar.oculusUserID);
                Debug.Log(message.Data.ID);
                Debug.Log(message.Data.OculusID);
            }
            else
            {
                Debug.LogError("Failed to fetch avatar: " + message.GetError().Message);
            }
        }



        private void OnDestroy()
        {
            FordiGrabber.OnObjectDelete -= OnObjectDelete;
        }

        private IEnumerator Start()
        {
            OvrAvatarTouchController[] touchControllers = null;

            do
            {
                yield return null;
                touchControllers = FindObjectsOfType<OvrAvatarTouchController>();
            }
            while (touchControllers == null || touchControllers.Length < 2);

            if (touchControllers[0].isLeftHand)
            {
                m_leftController = touchControllers[0];
                m_rightController = touchControllers[1];
            }
            else
            {
                m_leftController = touchControllers[1];
                m_rightController = touchControllers[0];
            }

            yield return null;

            m_initialized = true;
            //foreach (var item in m_controllerTips)
            //{
            //    ShowTooltip(new VRButton(item.Button, item.Controller));
            //    yield return new WaitUntil(() => OVRInput.GetDown(item.Button, item.Controller));
            //    DeactivateTooltip();
            //}
        }

        private void OnObjectDelete(object sender, EventArgs e)
        {
            m_objectDeleted = true;
        }

        public void ToggleShortTooltips(bool val)
        {
            m_shortTipOn = val;
            

            if (!val || m_shortTooltips.Count > 0)
                foreach (var item in m_shortTooltips)
                    item.Toggle(val);
            
            if (m_shortTooltips.Count == 0)
            {
                foreach (var item in m_leftControllerShortTips)
                {
                    ButtonTip buttonTip = Instantiate(item, m_leftController.transform);
                    m_shortTooltips.Add(buttonTip);
                    if (item.ButtonController.Button == m_teleport.TeleportButton || item.ButtonController.Button == OVRInput.Button.Down || item.ButtonController.Button == m_teleport.WaypointTeleportButton)
                        m_movementButtonTips.Add(buttonTip);
                }
                foreach (var item in m_rightControllerShortTips)
                {
                    ButtonTip buttonTip = Instantiate(item, m_rightController.transform);
                    m_shortTooltips.Add(buttonTip);
                    if (item.ButtonController.Button == m_teleport.TeleportButton || item.ButtonController.Button == OVRInput.Button.Down || item.ButtonController.Button == m_teleport.WaypointTeleportButton)
                        m_movementButtonTips.Add(buttonTip);
                }
            }

            if (m_shortTooltips.Count > 0 && m_avatar != null)
                m_avatar.ShowControllers(val);
        }

        private void DeactivateTooltip()
        {
            foreach (var item in m_tooltips)
                Destroy(item.gameObject);
            m_tooltips.Clear();
        }

        private UIJoint ShowTooltip(VRButton button, ToolTip tip, AudioClip guide)
        {
            UIJoint joint = GetJointPrefab(button);

            if (joint == null)
            {
                Debug.LogError("ShowTooltip: joint null " + button.Button.ToString() + " " + button.Controller.ToString());
                return null;
            }

            UIJoint clone = null;

            if (joint.Controller == OVRInput.Controller.LTouch)
                clone = Instantiate(joint, m_leftController.transform);
            else
                clone = Instantiate(joint, m_rightController.transform);

            clone.Tooltip = tip;

            m_tooltips.Add(clone);

            if (guide != null)
            {
                AudioArgs audioArgs = new AudioArgs(guide, AudioType.VO)
                {
                    FadeTime = 0,
                    Done = null
                };
                m_audio.Play(audioArgs);
            }

            return clone;
        }

        private ControllerState m_lastControllerState = ControllerState.NONE_ACTIVE;

        private void Update()
        {
            bool movement = false;
            m_playerController.GetHaltUpdateMovement(ref movement);
            if (!movement)
            {
                if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickRight, OVRInput.Controller.RTouch))
                {
                    RootRotation += 45;
                    PrepareForSpawn();
                }
                else if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickLeft, OVRInput.Controller.RTouch))
                {
                    RootRotation -= 45;
                    PrepareForSpawn();
                }
            }

            if (!m_initialized || ( !m_settings.SelectedPreferences.ShowTooltip && ExperienceMachine.AppMode == AppMode.APPLICATION))
                return;
            
            var activeController = OVRInput.GetActiveController();
            if (activeController == OVRInput.Controller.Touch && m_lastControllerState != ControllerState.BOTH_ACTIVE)
            {
                m_lastControllerState = ControllerState.BOTH_ACTIVE;
                ToggleShortTooltips(true);
            }
            else if (activeController != OVRInput.Controller.Touch && m_lastControllerState == ControllerState.BOTH_ACTIVE)
            {
                if (activeController == OVRInput.Controller.None)
                    m_lastControllerState = ControllerState.NONE_ACTIVE;
                else
                    m_lastControllerState = ControllerState.ONE_ACTIVE;
                ToggleShortTooltips(false);
            }

            //if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch))
            //    ToggleShortTooltips(!m_shortTipOn);

            for (int i = m_tooltips.Count - 1; i >= 0; i--)
            {
                if (m_tooltips[i].Tooltip.CheckCondition())
                {
                    UIJoint joint = m_tooltips[i];
                    m_tooltips.Remove(joint);
                    Destroy(joint.gameObject);
                }
            }

           

            //if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickDown, OVRInput.Controller.All))
            //{
            //    DeactivateTooltip();
            //}

            //if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickLeft, OVRInput.Controller.All) || OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickRight, OVRInput.Controller.All))
            //{
            //    DeactivateTooltip();
            //}
        }

        public void RequestHaltMovement(bool val)
        {
            if (m_vrMenu.IsOpen || m_rightGrabber.grabbedObject != null || m_leftGrabber.grabbedObject != null)
            {
                foreach (var item in m_movementButtonTips)
                    item.gameObject.SetActive(false);
            }

            
            foreach (var item in m_movementButtonTips)
            {
                if (item.ButtonController == new ButtonController(OVRInput.Button.Two, OVRInput.Controller.RTouch))
                {
                    if (m_rightGrabber.grabbedObject != null)
                    {
                        item.gameObject.SetActive(true);
                        item.Text = "B: Delete";
                    }
                }

                if (item.ButtonController == new ButtonController(OVRInput.Button.Two, OVRInput.Controller.LTouch))
                {
                    if (m_leftGrabber.grabbedObject != null)
                    {
                        item.gameObject.SetActive(true);
                        item.Text = "Y: Delete";
                    }
                }
            }

            if (!val && (m_vrMenu.IsOpen || FordiGrabber.GrabCount > 0))
            {
                return;
            }
            
            m_playerController.SetHaltUpdateMovement(val);
            m_teleport.canTeleport = !val;

            if (!val && m_settings.SelectedPreferences.ShowTooltip)
            {
                foreach (var item in m_movementButtonTips)
                {
                    item.gameObject.SetActive(true);
                    item.OnReset();
                }
            }
        }

        public void PrepareForSpawn()
        {
            PlayerCanvas.transform.localPosition = PlayerController.transform.localPosition;
            PlayerCanvas.transform.localRotation = Quaternion.identity;
            PlayerCanvas.transform.Rotate(new Vector3(0, RootRotation, 0));
        }

        public void UpdateAdditionalRotation(float angle)
        {
            RootRotation += angle;
            PrepareForSpawn();
        }

        private IEnumerator m_tooltipCoroutine = null;

        private UIJoint GetJointPrefab(VRButton vrButton)
        {
            foreach (var item in m_controllerTips)
            {
                if (vrButton.Button == OVRInput.Button.Down || vrButton.Button == OVRInput.Button.Up || vrButton.Button == OVRInput.Button.Left || vrButton.Button == OVRInput.Button.Right)
                {
                    if (item.Button == OVRInput.Button.Down || item.Button == OVRInput.Button.Up || item.Button == OVRInput.Button.Left || item.Button == OVRInput.Button.Right)
                    {
                        if (item.Controller == vrButton.Controller)
                        {
                            //Debug.LogError(vrButton.Button.ToString() + " " + vrButton.Controller.ToString());
                            return item;
                        }
                    }
                }
                else if (item.Button == vrButton.Button && item.Controller == vrButton.Controller)
                {
                    //Debug.LogError(vrButton.Button.ToString() + " " + vrButton.Controller.ToString());
                    return item;
                }
            }
            return null;
        }

        public void StartTooltipRoutine(List<VRButtonGroup> buttonGroups)
        {
            if (buttonGroups.Count == 0)
                return;

            if (m_tooltipCoroutine != null)
                StopCoroutine(m_tooltipCoroutine);
            m_tooltipCoroutine = CoTooltipRoutine(buttonGroups);
            StartCoroutine(m_tooltipCoroutine);
        }

        private void ReorderTooltips()
        {
            m_tooltips.Sort((x, y) => (int)(1000*(y.UIHandle.position.z - x.UIHandle.position.z)));

            for (int i = 0; i < m_tooltips.Count; i++)
            {
                m_tooltips[i].UIHandle.SetSiblingIndex(i);
                m_tooltips[i].UIHandle.name = i + "";
            }
        }

        private IEnumerator CoTooltipRoutine(List<VRButtonGroup> buttonGroups)
        {
            do
            {
                yield return null;
            }
            while (m_leftController == null || m_rightController == null);

            foreach (var item in buttonGroups)
            {
                DeactivateTooltip();
                if (item.TooltipOperation == TooltipOperation.AND)
                {
                    foreach (var vrButton in item.VRButtons)
                    {
                        var joint = ShowTooltip(vrButton, new ToolTip(vrButton.Tip, vrButton, item.Condition), vrButton.GuideClip);
                    }
                    yield return null;
                    yield return null;
                    ReorderTooltips();
                }
                else if (item.VRButtons.Length > 0)
                {
                    Func<bool> action = () =>
                    {
                        foreach (var vrButton in item.VRButtons)
                        {
                            if (OVRInput.GetDown(vrButton.Button, vrButton.Controller))
                                return true;
                        }
                        return false;
                    };

                    ShowTooltip(item.VRButtons[0], new ToolTip(item.CommonTip, item.VRButtons[0], item.Condition), item.VRButtons[0].GuideClip);
                }

                yield return new WaitUntil(() => m_tooltips.Count == 0);
            }

            if (m_welldoneClip != null)
            {
                AudioArgs audioArgs = new AudioArgs(m_welldoneClip, AudioType.VO)
                {
                    FadeTime = 0,
                    Done = null
                };
                m_audio.Play(audioArgs);
            }


            //m_avatar.ShowControllers(false);
        }

        #region SETTING_CHANGES
        public void ApplyTooltipSettings()
        {
            if (m_settings == null)
                m_settings = IOC.Resolve<ISettings>();
            if (m_settings.SelectedPreferences.ShowTooltip && !m_shortTipOn && OVRInput.GetActiveController() == OVRInput.Controller.Touch)
                ToggleShortTooltips(true);
            else if (!m_settings.SelectedPreferences.ShowTooltip && m_shortTipOn)
                ToggleShortTooltips(false);
        }
        #endregion

        public void Grab(DistanceGrabbable grabbable, OVRInput.Controller controller)
        {
            if (controller == OVRInput.Controller.LTouch)
                m_leftGrabber.ForceGrab(grabbable);
            if (controller == OVRInput.Controller.RTouch)
                m_rightGrabber.ForceGrab(grabbable);
            m_objectLoaded = true;
            m_vrMenu.Close();
        }

        #region GUIDE_CONDITIONS
        private bool m_objectLoaded = false;
        public bool ObjectLoaded()
        {
            var val = m_objectLoaded;
            if (m_objectLoaded)
                m_objectLoaded = false;
            return val;
        }

        private bool m_objectDeleted = false;
        public bool ObjectDeleted()
        {
            var val = m_objectDeleted;
            if (m_objectDeleted)
                m_objectDeleted = false;
            return val;
        }
        #endregion

        public void ToogleGrabGuide(OVRInput.Controller controller, bool val)
        {
            if (controller == OVRInput.Controller.LTouch)
            {
                if (m_leftGrabGuide == null)
                    m_leftGrabGuide = Instantiate(m_leftGrabGuidePrefab, m_leftHandAnchor);
                m_leftGrabGuide.SetActive(val);
            }

            if (controller == OVRInput.Controller.RTouch)
            {
                if (m_rightGrabGuide == null)
                    m_rightGrabGuide = Instantiate(m_rightGrabGuidePrefab, m_rightHandAnchor);
                m_rightGrabGuide.SetActive(val);
            }
        }

        public void DoWaypointTeleport(Transform anchor)
        {
            m_teleport.WaypointTeleport(anchor);
        }

        public void FadeOut()
        {
            m_fadeScript.FadeOut();
        }
    }
}