using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class InputDialog : MonoBehaviour
    {
        [SerializeField]
        private InputField m_inputField;

        public string Text
        {
            get { return m_inputField.text; }
            private set
            {
                m_inputField.text = value;
            }
        }

        private void Start()
        {
            m_inputField.Select();
        }

    }

}

