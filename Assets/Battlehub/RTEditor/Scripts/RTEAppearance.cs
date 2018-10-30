using Battlehub.RTCommon;
using Battlehub.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Battlehub.RTEditor
{
    public interface IRTEAppearance
    {

    }

    public class RTEAppearance : MonoBehaviour, IRTEAppearance
    {
        [Serializable]
        public struct Cursor
        {
            public string Name;
            public KnownCursor Type;
            public Texture2D Texture;
        }

        [SerializeField]
        private Cursor[] CursorSettings;       
        private IRTE m_editor;

        private void Awake()
        {
            m_editor = IOC.Resolve<IRTE>();

            List<Cursor> cursorSettings;
            if (CursorSettings == null)
            {
                cursorSettings = new List<Cursor>();
            }
            else
            {
                cursorSettings = CursorSettings.ToList();
            }
            
            AddCursorIfRequired(cursorSettings, KnownCursor.DropAllowed, "Drag & Drop Allowed", "RTE_DropAllowed_Cursor");
            AddCursorIfRequired(cursorSettings, KnownCursor.DropNowAllowed, "Drag & Drop Not Allowed", "RTE_DropNotAllowed_Cursor");
            AddCursorIfRequired(cursorSettings, KnownCursor.HResize, "Horizontal Resize", "RTE_HResize_Cursor");
            AddCursorIfRequired(cursorSettings, KnownCursor.VResize, "Vertical Resize", "RTE_VResize_Cursor");
            CursorSettings = cursorSettings.ToArray();

            m_editor.IsOpenedChanged += OnIsOpenedChanged;
            if (m_editor.IsOpened)
            {
                ApplySettings();
            }
        }

        private static void AddCursorIfRequired(List<Cursor> cursorSettings, KnownCursor cursorType, string name, string texture)
        {
            if (!cursorSettings.Any(c => c.Type == cursorType))
            {
                Cursor dropAllowedCursor = new Cursor
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
            if(m_editor != null)
            {
                m_editor.IsOpenedChanged -= OnIsOpenedChanged;
            }
        }

        private void OnIsOpenedChanged()
        {
            if(m_editor.IsOpened)
            {
                ApplySettings();
            }
        }

        private void ApplySettings()
        {
            for (int i = 0; i < CursorSettings.Length; ++i)
            {
                Cursor cursor = CursorSettings[i];
                m_editor.CursorHelper.Map(cursor.Type, cursor.Texture);
            }
        }

    }

}

