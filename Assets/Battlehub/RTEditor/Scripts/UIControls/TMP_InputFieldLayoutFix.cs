﻿using TMPro;
using UnityEngine;

namespace Battlehub.RTEditor
{
    /// <summary>
    /// This script is required to fix wrong input field text alignment (starting from unity 2019.1f)
    /// </summary>
    public class TMP_InputFieldLayoutFix : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField m_inputField;

#if UNITY_2019_1_OR_NEWER
        private void Start()
        {
            m_inputField.textComponent.havePropertiesChanged = true;
        }
#endif
    }
}
