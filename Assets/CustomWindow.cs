using Battlehub.RTCommon;
using Battlehub.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

public class CustomWindow : RuntimeWindow
{
    protected override void AwakeOverride()
    {
        WindowType = RuntimeWindowType.Custom;
        base.AwakeOverride();
    }

    protected override void OnDestroyOverride()
    {
        base.OnDestroyOverride();
    }

    protected override void OnActivated()
    {
        base.OnActivated();
        Debug.Log("On Custom Window Activated");
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();
        Debug.Log("On Custom Window Deactivated");
    }

    public override void DragEnter(object[] dragObjects, PointerEventData eventData)
    {
        base.DragEnter(dragObjects, eventData);
        Editor.DragDrop.SetCursor(KnownCursor.DropAllowed);
    }

    public override void DragLeave(PointerEventData eventData)
    {
        base.DragLeave(eventData);
        Editor.DragDrop.SetCursor(KnownCursor.DropNowAllowed);
    }

    public override void Drop(object[] dragObjects, PointerEventData eventData)
    {
        base.Drop(dragObjects, eventData);
        for(int i = 0; i < dragObjects.Length; ++i)
        {
            Debug.Log(dragObjects[i]);
        }
    }
}

