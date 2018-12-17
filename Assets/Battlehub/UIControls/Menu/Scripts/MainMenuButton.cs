using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.UIControls.MenuControl
{
    public class MainMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField]
        private Menu m_menu = null;

        [SerializeField]
        private Image m_selection = null;

        [SerializeField]
        private Color m_pointerOverColor = new Color32(0x00, 0x97, 0xFF, 0x7F);

        [SerializeField]
        private Color m_focusedColor = new Color32(0x00, 0x97, 0xFF, 0xFF);

        [SerializeField]
        private Color m_normalColor = new Color32(0xFF, 0xFF, 0xFF, 0x00);

        private bool m_isPointerOver;

        private void Awake()
        {
            if(m_selection == null)
            {
                m_selection = GetComponent<Image>();
            }

            m_menu.Opened += OnOpened;
            m_menu.Closed += OnClosed;
        }

        private void OnDestroy()
        {
            m_menu.Opened -= OnOpened;
            m_menu.Closed -= OnClosed;
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            m_menu.Open();
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            m_isPointerOver = true;
            if(m_menu.gameObject.activeSelf)
            {
                m_selection.color = m_focusedColor;
            }
            else
            {
                m_selection.color = m_pointerOverColor;
            }
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            m_isPointerOver = false;
            if (m_menu.gameObject.activeSelf)
            {
                m_selection.color = m_focusedColor;
            }
            else
            {
                m_selection.color = m_normalColor;
            }
        }

        private void OnClosed(object sender, System.EventArgs e)
        {
            if(m_isPointerOver)
            {
                m_selection.color = m_pointerOverColor;
            }
            else
            {
                m_selection.color = m_normalColor;
            }
        }

        private void OnOpened(object sender, System.EventArgs e)
        {
            m_selection.color = m_focusedColor;
        }

    }

}

