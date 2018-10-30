using Battlehub.RTCommon;
using Battlehub.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battlehub.RTEditor
{
    public class SceneView : RuntimeWindow
    {
        protected override void OnActivated()
        {
            base.OnActivated();
            Debug.Log("On SceneView activated");
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            Debug.Log("On SceneView deactivated");
        }

        public override void DragEnter(object[] dragObjects, PointerEventData eventData)
        {
            base.DragEnter(dragObjects, eventData);
            Debug.Log("Drag Enter " + Editor.DragDrop.DragObjects[0] );
            Editor.DragDrop.SetCursor(KnownCursor.DropAllowed);

        }

        public override void DragLeave(PointerEventData eventData)
        {
            base.DragLeave(eventData);
            Debug.Log("Drag Leave");
            Editor.DragDrop.SetCursor(KnownCursor.DropNowAllowed);
        }
    }

}
