using Battlehub.RTCommon;
using Battlehub.UIControls;
using Battlehub.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public interface IRTEAppearance
    {
        RTECursor[] CursorSettings
        {
            get;
            set;
        }

        RTEColors Colors
        {
            get;
            set;
        }

        [Obsolete("Use UIScaler instead")]
        CanvasScaler UIBackgroundScaler
        {
            get;
        }

        [Obsolete("Use UIScaler instead")]
        CanvasScaler UIForegroundScaler
        {
            get;
        }

        CanvasScaler UIScaler
        {
            get;
        }

        void ApplyColors(GameObject root);
    }

    [Serializable]
    public struct RTECursor
    {
        public string Name;
        public KnownCursor Type;
        public Texture2D Texture;
    }

    [Serializable]
    public struct RTESelectableColors
    {
        public Color Normal;
        public Color Highlight;
        public Color Pressed;
        public Color Disabled;

        public RTESelectableColors(Color normal, Color highlight, Color pressed, Color disabled)
        {
            Normal = normal;
            Highlight = highlight;
            Pressed = pressed;
            Disabled = disabled;
        }

        public bool EqualTo(RTESelectableColors c)
        {
            return
                c.Normal == Normal &&
                c.Highlight == Highlight &&
                c.Pressed == Pressed &&
                c.Disabled == Disabled;
        }
    }

    [Serializable]
    public struct RTEHierarchyColors
    {
        public Color NormalItem;
        public Color DisabledItem;
        
        public RTEHierarchyColors(Color normal, Color disabled)
        {
            NormalItem = normal;
            DisabledItem = disabled;
        }

        public bool EqualTo(RTEHierarchyColors c)
        {
            return
                c.NormalItem == NormalItem &&
                c.DisabledItem == DisabledItem;
        }
    }

    [Serializable]
    public class RTEColors
    {
        public static readonly Color DefaultPrimary = new Color32(0x38, 0x38, 0x38, 0xFF);
        public static readonly Color DefaultSecondary = new Color32(0x27, 0x27, 0x27, 0xFF);
        public static readonly Color DefaultBorder = new Color32(0x20, 0x20, 0x20, 0xFF);
        public static readonly Color DefaultBorder2 = new Color32(0x1A, 0x1A, 0x1A, 0xFF);
        public static readonly Color DefaultBorder3 = new Color32(0x52, 0x4C, 0x4C, 0xFF);
        public static readonly Color DefaultBorder4 = new Color(0x87, 0x87, 0x87, 0xFF);
        public static readonly Color DefaultAccent = new Color32(0x0, 0x97, 0xFF, 0xC0);
        public static readonly Color DefaultText = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
        public static readonly Color DefaultModalOverlay = new Color32(0x00, 0x00, 0x00, 0x40);
        public static readonly Color DefaultMainMenuBar = new Color32(0x6F, 0x6F, 0x6F, 0xFF);
        public static readonly RTESelectableColors DefaultMainMenuButton = new RTESelectableColors(new Color32(0xff, 0xff, 0xff, 0x00), new Color32(0x0, 0x97, 0xFF, 0x7F), new Color32(0x0, 0x97, 0xFF, 0xFF), new Color32(0, 0, 0, 0));
        public static readonly RTEHierarchyColors DefaultHierarchyColors = new RTEHierarchyColors(Color.white, new Color32(0x93, 0x92, 0x92, 0xFF));
        public static readonly Color DefaultProjectFolderColor = new Color(0x48, 0x48, 0x48, 0xFF);

        public Color Primary;
        public Color Secondary;
        public Color Border;
        public Color Border2;
        public Color Border3;
        public Color Border4;
        public Color Accent;
        public Color Text;
        public Color ModalOverlay;
        public Color MainMenuBar;
        public RTESelectableColors MainMenuButton;
        public RTEHierarchyColors HierarchyColors;
        public Color ProjectFolderColor;
        
        public RTEColors()
        {
            Primary = DefaultPrimary;
            Secondary = DefaultSecondary;
            Border = DefaultBorder;
            Border2 = DefaultBorder2;
            Border3 = DefaultBorder3;
            Border4 = DefaultBorder4;
            Accent = DefaultAccent;
            Text = DefaultText;
            ModalOverlay = DefaultModalOverlay;
            MainMenuBar = DefaultMainMenuBar;
            MainMenuButton = DefaultMainMenuButton;
            HierarchyColors = DefaultHierarchyColors;
            ProjectFolderColor = DefaultProjectFolderColor;
        }

        public bool IsDefault
        {
            get
            {
                return
                    Primary == DefaultPrimary &&
                    Secondary == DefaultSecondary &&
                    Border == DefaultBorder &&
                    Border2 == DefaultBorder2 &&
                    Border3 == DefaultBorder3 &&
                    Border4 == DefaultBorder4 &&
                    Accent == DefaultAccent &&
                    Text == DefaultText &&
                    ModalOverlay == DefaultModalOverlay &&
                    MainMenuBar == DefaultMainMenuBar &&
                    MainMenuButton.EqualTo(DefaultMainMenuButton) &&
                    HierarchyColors.EqualTo(DefaultHierarchyColors) &&
                    ProjectFolderColor == DefaultProjectFolderColor;
            }
        }
    }

    public class RTEAppearance : MonoBehaviour, IRTEAppearance
    {
        [SerializeField]
        private RTECursor[] m_cursorSettings = null;
        public RTECursor[] CursorSettings
        {
            get { return m_cursorSettings; }
            set
            {
                m_cursorSettings = value;
                if (m_editor != null && m_editor.IsOpened)
                {
                    ApplyCursorSettings();
                }
            }
        }

        [SerializeField]
        private CanvasScaler m_uiBackgroundScaler = null;
        [Obsolete("Use UIScaler instead")]
        public CanvasScaler UIBackgroundScaler
        {
            get { return m_uiBackgroundScaler; }
        }

        [SerializeField]
        private CanvasScaler m_uiForegroundScaler = null;
        [Obsolete("Use UIScaler instead")]
        public CanvasScaler UIForegroundScaler
        {
            get { return m_uiForegroundScaler; }
        }

        [SerializeField]
        private CanvasScaler m_uiScaler = null;
        public CanvasScaler UIScaler
        {
            get { return m_uiScaler; }
        }

        [SerializeField]
        private RTEColors m_colors = null;
        public RTEColors Colors
        {
            get { return m_colors; }
            set
            {
                m_colors = value;
                ApplyColors();
            }
        }

        private IRTE m_editor;
        private void Awake()
        {
            m_editor = IOC.Resolve<IRTE>();

            List<RTECursor> cursorSettings;
            if (m_cursorSettings == null)
            {
                cursorSettings = new List<RTECursor>();
            }
            else
            {
                cursorSettings = m_cursorSettings.ToList();
            }

            AddCursorIfRequired(cursorSettings, KnownCursor.DropAllowed, "Drag & Drop Allowed", "RTE_DropAllowed_Cursor");
            AddCursorIfRequired(cursorSettings, KnownCursor.DropNowAllowed, "Drag & Drop Not Allowed", "RTE_DropNotAllowed_Cursor");
            AddCursorIfRequired(cursorSettings, KnownCursor.HResize, "Horizontal Resize", "RTE_HResize_Cursor");
            AddCursorIfRequired(cursorSettings, KnownCursor.VResize, "Vertical Resize", "RTE_VResize_Cursor");
            m_cursorSettings = cursorSettings.ToArray();

            m_editor.IsOpenedChanged += OnIsOpenedChanged;
            if (m_editor.IsOpened)
            {
                ApplyCursorSettings();
                ApplyColors();
            }
        }

        private static void AddCursorIfRequired(List<RTECursor> cursorSettings, KnownCursor cursorType, string name, string texture)
        {
            if (!cursorSettings.Any(c => c.Type == cursorType))
            {
                RTECursor dropAllowedCursor = new RTECursor
                {
                    Name = name,
                    Type = cursorType,
                    Texture = Resources.Load<Texture2D>(texture)
                };
                cursorSettings.Add(dropAllowedCursor);
            }
        }

        private void OnDestroy()
        {
            if (m_editor != null)
            {
                m_editor.IsOpenedChanged -= OnIsOpenedChanged;
            }
        }

        private void OnIsOpenedChanged()
        {
            if (m_editor.IsOpened)
            {
                ApplyCursorSettings();
                ApplyColors();
            }
        }

        private void ApplyCursorSettings()
        {
            for (int i = 0; i < m_cursorSettings.Length; ++i)
            {
                RTECursor cursor = m_cursorSettings[i];
                m_editor.CursorHelper.Map(cursor.Type, cursor.Texture);
            }
        }

        private void ApplyColors()
        {
            ApplyColors(gameObject);
        }

        public void ApplyColors(GameObject root)
        {
            if (Colors.IsDefault)
            {
                return;
            }

            UIStyle[] styles = root.GetComponentsInChildren<UIStyle>(true);
            for(int i = 0; i < styles.Length; ++i)
            {
                UIStyle style = styles[i];
                switch(style.Name)
                {
                    case "PrimaryColor":
                        style.ApplyImageColor(Colors.Primary);
                        break;
                    case "SecondaryColor":
                        style.ApplyImageColor(Colors.Secondary);
                        break;
                    case "BorderColor":
                        style.ApplyImageColor(Colors.Border);
                        break;
                    case "Border2Color":
                        style.ApplyImageColor(Colors.Border2);
                        break;
                    case "Border3Color":
                        style.ApplyImageColor(Colors.Border3);
                        break;
                    case "Border4Color":
                        style.ApplyOutlineColor(Colors.Border4);
                        break;
                    case "AccentColor":
                        style.ApplyImageColor(Colors.Accent);
                        break;
                    case "TextColor":
                        style.ApplyTextColor(Colors.Text);
                        break;
                    case "ModalOverlayColor":
                        style.ApplyImageColor(Colors.ModalOverlay);
                        break;
                    case "MainMenuBarColor":
                        style.ApplyImageColor(Colors.MainMenuBar);
                        break;
                    case "MainMenuButtonColor":
                        style.ApplyMainButtonColor(Colors.MainMenuButton.Normal, Colors.MainMenuButton.Highlight, Colors.MainMenuButton.Pressed);
                        break;
                    case "HierarchyColor":
                        style.ApplyHierarchyColors(Colors.HierarchyColors.NormalItem, Colors.HierarchyColors.DisabledItem);
                        break;
                    case "ProjectFolderColor":
                        style.ApplyImageColor(Colors.ProjectFolderColor);
                        break;
                }
            }
        }
    }

}

