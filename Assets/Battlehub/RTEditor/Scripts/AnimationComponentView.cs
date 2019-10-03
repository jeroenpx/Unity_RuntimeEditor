using Battlehub.UIControls;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class AnimationComponentView : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI m_label = null;

        [SerializeField]
        private Button m_addPropertyButton = null;

        private AnimationPropertyItem m_item;
        public AnimationPropertyItem Item
        {
            get { return m_item; }
            set
            {
                m_item = value;

                if (m_item != null)
                {
                    if (m_label != null)
                    {
                        if (m_item.Parent == null)
                        {
                            m_label.name = m_item.ComponentName;
                        }
                        else
                        {
                            m_label.name = m_item.PropertyName;
                        }
                    }

                    if(m_addPropertyButton != null)
                    {
                        m_addPropertyButton.gameObject.SetActive(m_item.Parent != null);
                    }
                }
                else
                {
                    if (m_label != null)
                    {
                        m_label.gameObject.SetActive(false);
                    }

                    if (m_addPropertyButton != null)
                    {
                        m_addPropertyButton.gameObject.SetActive(false);
                    }
                }
            }
        }


        public AnimationPropertiesView View
        {
            get;
            set;
        }

        private void Awake()
        {
            UnityEventHelper.AddListener(m_addPropertyButton, button => button.onClick, OnAddPropertyButtonClick);
        }

        private void OnDestroy()
        {
            UnityEventHelper.RemoveListener(m_addPropertyButton, button => button.onClick, OnAddPropertyButtonClick);
        }

        private void OnAddPropertyButtonClick()
        {
            View.AddProperty(Item);
        }
    }
}

