using UnityEngine;

namespace Battlehub.RTEditor
{
    public class OptionsEditorFloat : OptionsEditor<float>
    {
        protected override void SetInputField(float value)
        {
            m_input.value = Mathf.RoundToInt(value);
        }

        protected override void OnValueChanged(int index)
        {
            SetValue(index);
            EndEdit();
        }
    }
}
