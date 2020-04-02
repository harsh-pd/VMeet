using OculusSampleFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VRExperience.Common;
using VRExperience.Core;
using VRExperience.ObjectControl;

namespace VRExperience.UI.MenuControl
{
    public class ObjectItem : MenuItem
    {
        [SerializeField]
        private RawImage m_rawImage;
        [SerializeField]
        private ObjectHolder m_objectHolder;

        private const string MaskPrefix = "Masked/";

        private IPlayer m_player;

        public RawImage RawImage { get { return m_rawImage; } }

        public GameObject Object { get; private set; }

        protected override void UpdateOverride()
        {
            base.UpdateOverride();
            if (pointerHovering && (FordiInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch)))
            {
                var prop = Instantiate(((ObjectResource)Item.Data).ObjectPrefab, m_player.RightHand);
                prop.transform.position = Object.transform.position;
                var grabbable = prop.GetComponent<DistanceGrabbable>();
                if (grabbable != null)
                    m_player.Grab(grabbable, OVRInput.Controller.RTouch);
            }
        }

        protected override void DataBind()
        {
            if (m_item != null)
            {
                m_text.text = m_item.Text;
            }
            else
            {
                m_icon.sprite = null;
                m_icon.gameObject.SetActive(false);
                m_text.text = string.Empty;
            }

            m_item.Validate = new MenuItemValidationEvent();

            if (m_experienceMachine == null)
                m_experienceMachine = IOC.Resolve<IExperienceMachine>();
            if (m_appTheme == null)
                m_appTheme = IOC.Resolve<IAppTheme>();

            m_item.Validate.AddListener(m_experienceMachine.CanExecuteMenuCommand);
            m_item.Validate.AddListener((args) => args.IsValid = m_item.IsValid);

            var validationResult = IsValid();
            if (validationResult.IsVisible)
            {
                if (m_item.IsValid)
                {
                    m_text.color = overrideColor? overriddenHighlight : m_appTheme.SelectedTheme.buttonNormalTextColor;
                }
                else
                {
                    m_text.color = m_appTheme.SelectedTheme.buttonDisabledTextColor;
                }

                if (m_image != null)
                {
                    if (m_item.IsValid)
                    {
                        m_image.color = m_appTheme.SelectedTheme.buttonNormalTextColor;
                    }
                    else
                    {
                        m_image.color = m_appTheme.SelectedTheme.buttonDisabledTextColor;
                    }
                }

                if (m_item.Data is ObjectResource objectResource)
                {
                    m_item.Action = new MenuItemEvent();
                    m_item.Action.AddListener(m_experienceMachine.ExecuteMenuCommand);
                    //((Button)selectable).onClick.AddListener(() => m_item.Action.Invoke(new MenuClickArgs(m_item.Path, m_item.Text, m_item.Command, m_item.CommandType, m_item.Data)));
                    Object = Instantiate(objectResource.ObjectUIPrefab, m_objectHolder.transform);
                    Mask();
                }
            }

            gameObject.SetActive(validationResult.IsVisible);
            selectable.interactable = validationResult.IsValid;
            if (m_allowTextScroll)
                StartCoroutine(InitializeTextScroll());
        }

        private void Mask()
        {
            var renderers = Object.GetComponentsInChildren<Renderer>();
            foreach (var item in renderers)
            {
                foreach (var material in item.materials)
                    material.shader = Shader.Find(MaskPrefix + material.shader.name);
            }
        }

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_player = IOC.Resolve<IPlayer>();
        }
    }
}