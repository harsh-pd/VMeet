using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Fordi.Common;
using Fordi.Core;

namespace Fordi.UI.MenuControl
{
    public class ColorInterfaceArgs
    {
        public string Title;
        public bool Blocked;
        public bool Persist;
        public ColorGroup ColorGroup;
        public AudioClip GuideClip;
        public ColorGroup Preset1, CustomPreset;
    }

    public class ColorInfo
    {
        public string Title;
        public ColorGroup ColorGroup;
        public UnityAction<MenuClickArgs> OnItemClick;
        public ToggleGroup ToggleGroup;
    }

    public enum ColorType
    {
        PRIMARY,
        SECONDARY,
        TERTIARY
    }

    public class ColorInterface : MenuScreen
    {
        [SerializeField]
        private ColorPicker m_colorPickerPrefab;
        [SerializeField]
        private ColorPreset m_colorPresetPrefab;
        [SerializeField]
        private GameObject m_nextButton;
        [SerializeField]
        private ToggleGroup m_toggleGroup;
        [SerializeField]
        private RawImage m_rawPreview;

        private ColorPreset m_customPreset = null;
        private ColorPreset m_givenPreset = null;

        private IMenuSelection m_menuSelection = null;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_menuSelection = IOC.Resolve<IMenuSelection>();
        }

        public void OpenColorInterface(ColorInterfaceArgs args)
        {
            ColorGroup primaryColorGroup, secondaryColorGroup, tertiaryColorGroup;
            primaryColorGroup = new ColorGroup
            {
                Resources = new ColorResource[args.ColorGroup.Resources.Length]
            };
            secondaryColorGroup = new ColorGroup
            {
                Resources = new ColorResource[args.ColorGroup.Resources.Length]
            };
            tertiaryColorGroup = new ColorGroup
            {
                Resources = new ColorResource[args.ColorGroup.Resources.Length]
            };

            for (int i = 0; i < primaryColorGroup.Resources.Length; i++)
            {
                primaryColorGroup.Resources[i] = (ColorResource)args.ColorGroup.Resources[i].Clone();
                secondaryColorGroup.Resources[i] = (ColorResource)args.ColorGroup.Resources[i].Clone();
                tertiaryColorGroup.Resources[i] = (ColorResource)args.ColorGroup.Resources[i].Clone();

                primaryColorGroup.Resources[i].ShortDescription = primaryColorGroup.Resources[i].PrimaryDescription;
                secondaryColorGroup.Resources[i].ShortDescription = primaryColorGroup.Resources[i].SecondaryDescription;
                tertiaryColorGroup.Resources[i].ShortDescription = primaryColorGroup.Resources[i].TertiaryDescription;
            }


            Clear();

            if (m_title != null)
                m_title.text = args.Title;

            Blocked = args.Blocked;
            Persist = args.Persist;
            gameObject.SetActive(true);

            if (m_uiEngine == null)
                m_uiEngine = IOC.Resolve<IUIEngine>();

            if (m_okButton != null)
                m_okButton.onClick.AddListener(() => m_uiEngine.CloseLastScreen());
            if (m_closeButton != null)
                m_closeButton.onClick.AddListener(() => m_uiEngine.CloseLastScreen());

            ColorPicker colorPicker = Instantiate(m_colorPickerPrefab, m_contentRoot);
            colorPicker.Colors = new ColorInfo
            {
                ColorGroup = primaryColorGroup,
                Title = "Primary Colors",
                OnItemClick = (clickArgs) => OnColorClick(clickArgs, ColorType.PRIMARY)
            };

            colorPicker = Instantiate(m_colorPickerPrefab, m_contentRoot);
            colorPicker.Colors = new ColorInfo
            {
                ColorGroup = secondaryColorGroup,
                Title = "Secondary Colors",
                OnItemClick = (clickArgs) => OnColorClick(clickArgs, ColorType.SECONDARY)
            };

            colorPicker = Instantiate(m_colorPickerPrefab, m_contentRoot);
            colorPicker.Colors = new ColorInfo
            {
                ColorGroup = tertiaryColorGroup,
                Title = "Tertiary Colors",
                OnItemClick = (clickArgs) => OnColorClick(clickArgs, ColorType.TERTIARY)
            };

            if (args.Preset1.Resources.Length > 3)
                args.Preset1.Resources = args.Preset1.Resources.SubArray(0, 3);

            foreach (var item in args.Preset1.Resources)
                item.ShortDescription = item.PrimaryDescription;

            m_givenPreset = Instantiate(m_colorPresetPrefab, m_contentRoot);
            m_givenPreset.Colors = new ColorInfo
            {
                ColorGroup = args.Preset1,
                Title = "Preset 1",
                ToggleGroup = m_toggleGroup
            };

            if (args.CustomPreset != null && args.CustomPreset.Resources != null && args.CustomPreset.Resources.Length > 2)
            {
                m_customPreset = Instantiate(m_colorPresetPrefab, m_contentRoot);
                m_customPreset.Colors = new ColorInfo
                {
                    ColorGroup = args.CustomPreset,
                    Title = "Custom",
                    ToggleGroup = m_toggleGroup
                };

                if (m_customPreset.Toggle != null)
                    m_customPreset.Toggle.onValueChanged.AddListener((val) => UpdatePreview());
            }

            if (m_givenPreset.Toggle != null)
                m_givenPreset.Toggle.onValueChanged.AddListener((val) => UpdatePreview());

           

            m_nextButton.transform.SetParent(m_contentRoot);
            m_nextButton.SetActive(true);
            UpdatePreview();
        }

        public void OnColorClick(MenuClickArgs args, ColorType colorType)
        {
            var colorResource = (ColorResource)args.Data;
            ColorResource[] existingResources = null;

            if (m_customPreset == null)
            {
                m_customPreset = Instantiate(m_colorPresetPrefab, m_contentRoot);
                existingResources = new ColorResource[] { colorResource, colorResource, colorResource };
                if (m_customPreset.Toggle != null)
                    m_customPreset.Toggle.onValueChanged.AddListener((val) => UpdatePreview());
            }
            else
            {
                existingResources = m_customPreset.Colors.ColorGroup.Resources;
                switch (colorType)
                {
                    case ColorType.PRIMARY:
                        existingResources = new ColorResource[] { colorResource, existingResources[1], existingResources[2] };
                        break;
                    case ColorType.SECONDARY:
                        existingResources = new ColorResource[] { existingResources[0], colorResource, existingResources[2] };
                        break;
                    case ColorType.TERTIARY:
                        existingResources = new ColorResource[] { existingResources[0], existingResources[1], colorResource};
                        break;
                }
            }

            m_customPreset.Colors = new ColorInfo
            {
                ColorGroup = new ColorGroup
                {
                    Resources = existingResources,
                    ResourceType = ResourceType.COLOR,
                    Preview = null
                },
                Title = "Custom",
                ToggleGroup = m_toggleGroup
            };

            m_customPreset.Select();

            MenuClickArgs modifiedArgs = new MenuClickArgs(args.Path, args.Name, args.Command, MenuCommandType.SAVE_PRESET, existingResources);
            m_experienceMachine.ExecuteMenuCommand(modifiedArgs);

            m_nextButton.transform.SetAsLastSibling();
            Invoke("RebuildLayout", Time.deltaTime);
            UpdatePreview();
        }

        private void RebuildLayout()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)m_contentRoot.transform);
            Canvas.ForceUpdateCanvases();
        }

        public void ClickNext()
        {
            ColorGroup colorGroup = null;
            if (m_givenPreset.Selected)
                colorGroup = m_givenPreset.Colors.ColorGroup;
            if (m_customPreset != null && m_customPreset.Selected)
            {
                colorGroup = m_customPreset.Colors.ColorGroup;
                m_menuSelection.MandalaResource.CustomPreset = m_customPreset.Colors.ColorGroup.Resources;
            }
            if (colorGroup != null)
                m_experienceMachine.ExecuteMenuCommand(new MenuClickArgs("", "", "", MenuCommandType.SELECTION, colorGroup));
        }

        private void UpdatePreview()
        {
            if (m_menuSelection.MandalaResource == null || m_menuSelection.MandalaResource.Mandala == null)
                return;

            List<Material> materials = new List<Material>();

            var mandala = m_menuSelection.MandalaResource.Mandala;

            if (mandala != null)
            {
                Renderer[] renderers = mandala.GetComponentsInChildren<Renderer>();
                foreach (var item in renderers)
                    materials.AddRange(item.sharedMaterials);
            }

            ColorResource[] selectedColors = null;
            if (m_givenPreset.Selected)
                selectedColors = m_givenPreset.Colors.ColorGroup.Resources;
            else
                selectedColors = m_customPreset.Colors.ColorGroup.Resources;

            ColorResource[] mainColorResources = (ColorResource[]) m_experienceMachine.GetExperience(ExperienceType.MANDALA).GetResource(ResourceType.COLOR, MandalaExperience.MainColor);
            ColorResource[] supportColorResources = (ColorResource[])m_experienceMachine.GetExperience(ExperienceType.MANDALA).GetResource(ResourceType.COLOR, MandalaExperience.SupportColor);

            int[] colorResourceIndices = new int[3];

            for (int i = 0; i < 3; i++)
            {
                colorResourceIndices[i] = Array.FindIndex(mainColorResources, item => item.Name == selectedColors[i].Name);
                if (colorResourceIndices[i] == -1)
                    return;
            }

            foreach (var item in materials)
            {
                if (item.name.Length > 1)
                {
                    string prefix = item.name.Substring(0, 2);
                    switch (prefix)
                    {
                        case MandalaExperience.PrimaryMain:
                            item.color = mainColorResources[colorResourceIndices[0]].Color;
                            item.SetColor("_EmissionColor", item.color);
                            break;
                        case MandalaExperience.SecondaryMain:
                            item.color = mainColorResources[colorResourceIndices[1]].Color;
                            item.SetColor("_EmissionColor", item.color);
                            break;
                        case MandalaExperience.TertiaryMain:
                            item.color = mainColorResources[colorResourceIndices[2]].Color;
                            item.SetColor("_EmissionColor", item.color);
                            break;
                        case MandalaExperience.PrimarySupport:
                            item.color = supportColorResources[colorResourceIndices[0]].Color;
                            item.SetColor("_EmissionColor", item.color);
                            break;
                        case MandalaExperience.SecondarySupport:
                            item.color = supportColorResources[colorResourceIndices[1]].Color;
                            item.SetColor("_EmissionColor", item.color);
                            break;
                        case MandalaExperience.TertiarySupport:
                            item.color = supportColorResources[colorResourceIndices[2]].Color;
                            item.SetColor("_EmissionColor", item.color);
                            break;
                    }
                }
            }


            RuntimePreviewGenerator.PreviewDirection = new Vector3(0, 0, -1);
            RuntimePreviewGenerator.BackgroundColor = new Color32(0, 0, 0, 0);
            Texture2D tex = RuntimePreviewGenerator.GenerateModelPreview(m_menuSelection.MandalaResource.Mandala.transform, 800, 800);
            m_rawPreview.texture = tex;
        }
    }
}
