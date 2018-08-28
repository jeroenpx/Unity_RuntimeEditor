using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class ProjectItemView : MonoBehaviour
    {
        public Sprite Preview
        {
            get { return m_imgPreview.sprite; }
            set { m_imgPreview.sprite = value; }
        }

        [SerializeField]
        private Image m_imgPreview;

        private void Awake()
        {        
        }
    }
}

