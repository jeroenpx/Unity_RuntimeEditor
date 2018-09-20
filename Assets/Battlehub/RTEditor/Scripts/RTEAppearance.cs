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

        private void Awake()
        {
            RuntimeEditorApplication.IsOpenedChanged += OnIsOpenedChanged;
            if(RuntimeEditorApplication.IsOpened)
            {
                ApplySettings();
            }
        }

        private void OnDestroy()
        {
            RuntimeEditorApplication.IsOpenedChanged -= OnIsOpenedChanged;
        }

        private void OnIsOpenedChanged()
        {
            if(RuntimeEditorApplication.IsOpened)
            {
                ApplySettings();
            }
        }

        private void ApplySettings()
        {
            for (int i = 0; i < CursorSettings.Length; ++i)
            {
                Cursor cursor = CursorSettings[i];
                CursorHelper.Map(cursor.Type, cursor.Texture);
            }
        }

    }

}

