using Battlehub.RTEditor;
using UnityEngine;

namespace Battlehub.UIControls
{
    public partial class UIStyle
    {
        public void ApplyTimelineControlBackgroundColor(Color background)
        {
            TimelineControl timelineControl = GetComponent<TimelineControl>();
            if(timelineControl != null)
            {
                timelineControl.BackgroundColor = background;
            }
        }

        public void ApplyHierarchyColors(Color enabledItem, Color disabledItem)
        {
            HierarchyView hierarchy = GetComponent<HierarchyView>();
            if (hierarchy != null)
            {
                hierarchy.EnabledItemColor = enabledItem;
                hierarchy.DisabledItemColor = disabledItem;
            }
        }

        public void ApplyToolCmdItemColor(Color normalColor, Color pointerOverColor, Color pressedColor)
        {
            ToolCmdItem cmdItem = GetComponent<ToolCmdItem>();
            if(cmdItem != null)
            {
                cmdItem.NormalColor = normalColor;
                cmdItem.PointerOverColor = pointerOverColor;
                cmdItem.PressedColor = pressedColor;
            }
        }
    }
}