using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.RTBuilder
{
    public class ProBuilderCmdItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField]
        private Graphic m_graphics = null;

        [SerializeField]
        private Color m_normalColor = Color.white;

        [SerializeField]
        private Color m_pointerOverColor = Color.white;

        [SerializeField]
        private Color m_pressedColor = Color.white;

        private bool m_isPointerOver;
        private bool m_isPointerPressed;

        private void Awake()
        {
            UpdateVisualState();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_isPointerOver = true;
            UpdateVisualState();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            m_isPointerOver = false;
            UpdateVisualState();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            m_isPointerPressed = true;
            UpdateVisualState();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            m_isPointerPressed = false;
            UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            if(m_graphics != null)
            {
                if (m_isPointerPressed)
                {
                    m_graphics.color = m_pressedColor;
                }
                else
                {
                    if(m_isPointerOver)
                    {
                        m_graphics.color = m_pointerOverColor;
                    }
                    else
                    {
                        m_graphics.color = m_normalColor;
                    }
                }
            }
            
        }  
    }
}

