using Battlehub.RTCommon;
using Battlehub.RTSaveLoad2.Interface;
using Battlehub.UIControls;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using UnityObject = UnityEngine.Object;
namespace Battlehub.RTEditor
{
    public class ObjectEditor : PropertyEditor<UnityObject>, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private GameObject DragHighlight = null;
        [SerializeField]
        private InputField Input = null;
        [SerializeField]
        private Button BtnSelect = null;
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
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            if(BtnSelect != null)
            {
                BtnSelect.onClick.RemoveListener(OnSelect);
            }

            if(Editor != null)
            {
                Editor.DragDrop.Drop -= OnDrop;
            }
        }

        private void OnSelect()
        {
            SelectObjectDialog objectSelector = null;
            Transform dialogTransform = IOC.Resolve<IWindowManager>().CreateDialog(RuntimeWindowType.SelectObject.ToString(), "Select " + MemberInfoType.Name,
                 (sender, args) =>
                 {
                     if (objectSelector.IsNoneSelected)
                     {
                         SetValue(null);
                         EndEdit();
                         SetInputField(null);
                     }
                     else
                     {
                         SetValue(objectSelector.SelectedObject);
                         EndEdit();
                         SetInputField(objectSelector.SelectedObject);
                     }
                 });
            objectSelector = dialogTransform.GetComponentInChildren<SelectObjectDialog>();
            objectSelector.ObjectType = MemberInfoType;
        }


        private void OnDrop(PointerEventData pointerEventData)
        {
            object dragObject = Editor.DragDrop.DragObjects[0];
            #warning Recover this functionality
            throw new System.NotImplementedException();
            //SetValue(DragDrop.DragObject);
            //EndEdit();
            //SetInputField(DragDrop.DragObject);
            //HideDragHighlight();
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if(!Editor.DragDrop.InProgress)
            {
                return;
            }
            object dragObject = Editor.DragDrop.DragObjects[0];            
            Type type = null;
            if(dragObject is ExposeToEditor)
            {
                type = typeof(GameObject);
            }
            else if(dragObject is AssetItem)
            {
                AssetItem assetItem = (AssetItem)dragObject;
                IProject project = IOC.Resolve<IProject>();
                type = project.ToType(assetItem);
            }

            if (type != null && MemberInfoType.IsAssignableFrom(type))
            {
                Editor.DragDrop.Drop -= OnDrop;
                Editor.DragDrop.Drop += OnDrop;
                ShowDragHighlight();
            }
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            HideDragHighlight();
            Editor.DragDrop.Drop -= OnDrop;
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
