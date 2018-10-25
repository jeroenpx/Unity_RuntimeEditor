using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTHandles
{
    public class RuntimeSelectionInput : RuntimeSelectionInputBase
    {
#if UNITY_EDITOR
        private KeyCode EditorModifierKey = KeyCode.LeftShift;
#else
        private KeyCode RuntimeModifierKey = KeyCode.LeftControl;
#endif
        protected KeyCode ModifierKey
        {
            get
            {
#if UNITY_EDITOR
                return EditorModifierKey;
#else
                return RuntimeModifierKey;
#endif
            }
        }

        private KeyCode MultiselectKey = KeyCode.LeftControl;
        private KeyCode MultiselectKey2 = KeyCode.RightControl;
        private KeyCode RangeSelectKey = KeyCode.LeftShift;

        protected virtual bool RangeSelectAction()
        {
            return m_component.Editor.Input.GetKey(RangeSelectKey);
        }

        protected virtual bool MultiselectAction()
        {
            IInput input = m_component.Editor.Input;
            return input.GetKey(MultiselectKey) || input.GetKey(MultiselectKey2) || input.GetKey(RangeSelectKey);
        }

        protected override void OnSelectGO()
        {
            m_component.SelectGO(RangeSelectAction(), MultiselectAction());
        }
       
    }

}

