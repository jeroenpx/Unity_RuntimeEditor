using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.UIControls
{
    public partial class UIStyle : MonoBehaviour
    {
        public string Name;

        public void ApplyImageColor(Color color)
        {
            Image image = GetComponent<Image>();
            if(image != null)
            {
                image.color = color;
            }
        }

        public void ApplyOutlineColor(Color color)
        {
            Outline outline = GetComponent<Outline>();
            if(outline != null)
            {
                outline.effectColor = color;
            }
        }

        public void ApplyTextColor(Color color)
        {
            TextMeshProUGUI text = GetComponent<TextMeshProUGUI>();
            if(text != null)
            {
                text.color = color;
            }
            else
            {
                Text uitext = GetComponent<Text>();
                if(uitext != null)
                {
                    uitext.color = color;
                }
            }
        }
    }
}
