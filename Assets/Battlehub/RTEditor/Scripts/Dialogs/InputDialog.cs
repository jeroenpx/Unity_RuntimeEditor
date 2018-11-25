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
            get;
            private set;
        }

    }

}

