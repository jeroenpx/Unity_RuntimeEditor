using Battlehub.RTCommon;
using Battlehub.Utils;
using System;

using UnityEngine;

namespace Battlehub.RTEditor
{
    public class RTEAppearance : MonoBehaviour
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
            m_editor = RTE.Get;

            m_editor.IsOpenedChanged += OnIsOpenedChanged;
            if(m_editor.IsOpened)
            {
                ApplySettings();
            }
        }

        private void OnDestroy()
        {
            m_editor.IsOpenedChanged -= OnIsOpenedChanged;
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

