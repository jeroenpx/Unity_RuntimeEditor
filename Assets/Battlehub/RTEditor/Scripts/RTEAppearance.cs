using Battlehub.RTCommon;
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
    }

    [Serializable]
    public struct RTECursor
    {
        public string Name;
        public KnownCursor Type;
        public Texture2D Texture;
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
    }

}

