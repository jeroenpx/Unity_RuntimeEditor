using Battlehub.RTCommon;
using Battlehub.UIControls;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using UnityObject = UnityEngine.Object;
namespace Battlehub.RTEditor
{
    public class ObjectEditor : PropertyEditor<UnityObject>, IPointerEnterHandler, IPointerExitHandler
    {
        private bool m_isPointerOver;

        [SerializeField]
        private GameObject DragHighlight;
        [SerializeField]
        private InputField Input;
        [SerializeField]
        private Button BtnSelect;
        [SerializeField]
        private SelectObjectDialog ObjectSelectorPrefab;

        private SelectObjectDialog m_objectSelector;

        protected override void SetInputField(UnityObject value)
        {
            if (value != null)
            {
                Input.text = string.Format("{1} ({0})", MemberInfoType.Name, value.name);
            }
            else
            {
                Input.text = string.Format("None ({0})", MemberInfoType.Name);
            }
        }

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            BtnSelect.onClick.AddListener(OnSelect);
           // DragDrop.BeginDrag += OnGlobalBeginDrag;
            //DragDrop.Drop += OnGlobalDrop;
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            if(BtnSelect != null)
            {
                BtnSelect.onClick.RemoveListener(OnSelect);
            }
           // DragDrop.BeginDrag -= OnGlobalBeginDrag;
           // DragDrop.Drop -= OnGlobalDrop;
        }

        private void OnSelect()
        {
            m_objectSelector = Instantiate(ObjectSelectorPrefab);
            m_objectSelector.transform.position = Vector3.zero;
            m_objectSelector.ObjectType = MemberInfoType;

            PopupWindow.Show("Select " + MemberInfoType.Name, m_objectSelector.transform, "Select",
                args =>
                {
                    if(m_objectSelector.IsNoneSelected)
                    {
                        SetValue(null);
                        EndEdit();
                        SetInputField(null);
                    }
                    else
                    {
                        SetValue(m_objectSelector.SelectedObject);
                        EndEdit();
                        SetInputField(m_objectSelector.SelectedObject);
                    }
                },
                "Cancel");
        }

#warning Commented Out. TODO: Re-implement using DragDropTarget
        //private void OnGlobalBeginDrag()
        //{
        //    if(m_isPointerOver)
        //    {
        //        if (DragDrop.DragObject != null && MemberInfoType.IsAssignableFrom(DragDrop.DragObject.GetType()))
        //        {
        //            ShowDragHighlight();
        //        }
        //    }
        //}

        //private void OnGlobalDrop()
        //{
        //    if (m_isPointerOver)
        //    {
        //        if (DragDrop.DragObject != null && MemberInfoType.IsAssignableFrom(DragDrop.DragObject.GetType()))
        //        {
        //            SetValue(DragDrop.DragObject);
        //            EndEdit();
        //            SetInputField(DragDrop.DragObject);
        //            HideDragHighlight();
        //        }
        //    }
        //}

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            m_isPointerOver = true;
            if (DragDrop.DragObject != null && MemberInfoType.IsAssignableFrom(DragDrop.DragObject.GetType()))
            {
                ShowDragHighlight();
            }
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            m_isPointerOver = false;
            if(DragDrop.DragObject != null)
            {
                HideDragHighlight();
            }
        }

        private void ShowDragHighlight()
        {
            if(DragHighlight != null)
            {
                DragHighlight.SetActive(true);
            }
        }

        private void HideDragHighlight()
        {
            if(DragHighlight != null)
            {
                DragHighlight.SetActive(false);
            }
        }
    }
}
