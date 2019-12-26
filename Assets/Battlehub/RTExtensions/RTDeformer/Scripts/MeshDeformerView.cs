using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.RTHandles;
using Battlehub.Spline3;
using Battlehub.UIControls;
using Battlehub.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.MeshDeformer3
{
    [DefaultExecutionOrder(-1)]
    public class MeshDeformerView : RuntimeWindow
    {
        [SerializeField]
        private Toggle m_toggleObject = null;
        [SerializeField]
        private Toggle m_toggleControlPoints = null;

        [SerializeField]
        private VirtualizingTreeView m_commandsList = null;
        private ToolCmd[] m_commands;
        private bool m_isMeshDeformerSelected;
        private bool m_isDragging;
        private IMeshDeformerTool m_tool;

        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.Custom;
            base.AwakeOverride();

            m_tool = IOC.Resolve<IMeshDeformerTool>();
            m_tool.ModeChanged += OnModeChanged;
            m_tool.SelectionChanged += OnSelectionChanged;

            Editor.Selection.SelectionChanged += OnEditorSelectionChanged;
                        
            m_commandsList.ItemClick += OnItemClick;
            m_commandsList.ItemDataBinding += OnItemDataBinding;
            m_commandsList.ItemExpanding += OnItemExpanding;
            m_commandsList.ItemBeginDrag += OnItemBeginDrag;
            m_commandsList.ItemDrop += OnItemDrop;
            m_commandsList.ItemDragEnter += OnItemDragEnter;
            m_commandsList.ItemDragExit += OnItemDragExit;
            m_commandsList.ItemEndDrag += OnItemEndDrag;

            m_commandsList.CanEdit = false;
            m_commandsList.CanReorder = false;
            m_commandsList.CanReparent = false;
            m_commandsList.CanSelectAll = false;
            m_commandsList.CanUnselectAll = true;
            m_commandsList.CanRemove = false;

            UnityEventHelper.AddListener(m_toggleObject, o => o.onValueChanged, OnObjectMode);
            UnityEventHelper.AddListener(m_toggleControlPoints, o => o.onValueChanged, OnControlPointMode);
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();

            if(m_tool != null)
            {
                m_tool.ModeChanged -= OnModeChanged;
                m_tool.SelectionChanged -= OnSelectionChanged;
                m_tool.Mode = MeshDeformerToolMode.Object;
            }

            if (m_commandsList != null)
            {
                m_commandsList.ItemClick -= OnItemClick;
                m_commandsList.ItemDataBinding -= OnItemDataBinding;
                m_commandsList.ItemExpanding -= OnItemExpanding;
                m_commandsList.ItemBeginDrag -= OnItemBeginDrag;
                m_commandsList.ItemDrop -= OnItemDrop;
                m_commandsList.ItemDragEnter -= OnItemDragEnter;
                m_commandsList.ItemDragExit -= OnItemDragExit;
                m_commandsList.ItemEndDrag -= OnItemEndDrag;
            }

            UnityEventHelper.RemoveListener(m_toggleObject, o => o.onValueChanged, OnObjectMode);
            UnityEventHelper.RemoveListener(m_toggleControlPoints, o => o.onValueChanged, OnControlPointMode);
        }

        protected virtual void Start()
        {
            UpdateFlags();
            m_commands = GetCommands().ToArray();
            m_commandsList.Items = m_commands;
            m_commandsList.Expand(m_commands[0]);
        }

        protected virtual void LateUpdate()
        {
            RuntimeWindow window = Editor.ActiveWindow;
            if (window == null || window.WindowType != RuntimeWindowType.Scene || !window.IsPointerOver)
            {
                return;
            }

            IInput input = Editor.Input;

            bool remove = input.GetKeyDown(KeyCode.Delete);
            if (m_isMeshDeformerSelected && remove && m_tool.CanRemove())
            {
                Remove();
            }
 
            if (Editor.Tools.ActiveTool == null && input.GetPointerDown(0))
            {
                if (Editor.ActiveWindow != null )
                {
                    m_tool.SelectControlPoint(window.Camera, input.GetPointerXY(0));
                }
            }
            else
            {
                bool extend = input.GetKey(KeyCode.LeftControl) && !m_isDragging || input.GetKeyDown(KeyCode.LeftControl);
                m_isDragging = m_tool.DragControlPoint(extend);
            }
        }

        private void OnObjectMode(bool value)
        {
            if(value)
            {
                m_tool.Mode = MeshDeformerToolMode.Object;
            }
        }

        private void OnControlPointMode(bool value)
        {
            if(value)
            {
                m_tool.Mode = MeshDeformerToolMode.ControlPoint;
            }
        }


        private void OnSelectionChanged()
        {
            UpdateFlags();
            m_commands = GetCommands().ToArray();
            m_commandsList.Items = m_commands;
            m_commandsList.Expand(m_commands[0]);
        }

        private void OnModeChanged()
        {
            if(m_tool.Mode == MeshDeformerToolMode.Object)
            {
                m_toggleObject.isOn = true;
            }
            else if(m_tool.Mode == MeshDeformerToolMode.ControlPoint)
            {
                m_toggleControlPoints.isOn = true;
            }

            UpdateFlags();
            m_commands = GetCommands().ToArray();
            m_commandsList.Items = m_commands;
            m_commandsList.Expand(m_commands[0]);
        }        

        private List<ToolCmd> GetCommands()
        {
            ILocalization lc = IOC.Resolve<ILocalization>();

            List<ToolCmd> commands = new List<ToolCmd>();
            if(m_tool.Mode == MeshDeformerToolMode.Object)
            {
                ToolCmd deformCmd = new ToolCmd(lc.GetString("ID_RTDeformer_View_Deform", "Deform"), () => DeformAxis(Axis.Z), CanDeform);
                deformCmd.Children = new List<ToolCmd>
                {
                    new ToolCmd(lc.GetString("ID_RTDeformer_View_DeformX", "Deform X"), () => DeformAxis(Axis.X), CanDeform) { Parent = deformCmd },
                    new ToolCmd(lc.GetString("ID_RTDeformer_View_DeformY", "Deform Y"), () => DeformAxis(Axis.Y), CanDeform) { Parent = deformCmd },
                };
                commands.Add(deformCmd);
            }
            else if(m_tool.Mode == MeshDeformerToolMode.ControlPoint)
            {
                commands.Add(new ToolCmd(lc.GetString("ID_RTDeformer_View_Append", "Append"), Append, CanAppend));
                commands.Add(new ToolCmd(lc.GetString("ID_RTDeformer_View_Remove", "Remove"), Remove, () => m_isMeshDeformerSelected));
            }

            return commands;
        }

        private bool CanDeform()
        {
            return m_tool.CanDeform();
        }

        private void DeformAxis(Axis axis)
        {
            m_tool.DeformAxis(axis);
            m_tool.Mode = MeshDeformerToolMode.ControlPoint;
        }

        private bool CanAppend()
        {
            if(!m_isMeshDeformerSelected)
            {
                return false;
            }
            return m_tool.CanAppend();
        }

        private void Append()
        {
            m_tool.Append();
        }

        private void Remove()
        {
            m_tool.Remove();
        }

        private void UpdateFlags()
        {
            GameObject[] selected = Editor.Selection.gameObjects;
            if (selected != null && selected.Length > 0)
            {
                m_isMeshDeformerSelected = selected.Where(go =>
                {
                    ControlPointPicker picker = go.GetComponentInParent<ControlPointPicker>();
                    return picker != null && picker.Selection != null && picker.Selection.GetSpline() is Deformer && picker.Selection.Index >= 0;
                }).Any();
            }
            else
            {
                m_isMeshDeformerSelected = false;
            }
        }

        private void OnEditorSelectionChanged(UnityEngine.Object[] unselectedObjects)
        {
            UpdateFlags();
            m_commandsList.DataBindVisible();
        }

        private void OnItemDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            TextMeshProUGUI text = e.ItemPresenter.GetComponentInChildren<TextMeshProUGUI>();
            ToolCmd cmd = (ToolCmd)e.Item;
            text.text = cmd.Text;

            bool isValid = cmd.Validate();
            Color color = text.color;
            color.a = isValid ? 1 : 0.5f;
            text.color = color;
          
            e.CanDrag = cmd.CanDrag;
            e.HasChildren = cmd.HasChildren;
        }

        private void OnItemExpanding(object sender, VirtualizingItemExpandingArgs e)
        {
            ToolCmd cmd = (ToolCmd)e.Item;
            e.Children = cmd.Children;
        }

        private void OnItemClick(object sender, ItemArgs e)
        {
            ToolCmd cmd = (ToolCmd)e.Items[0];
            if(cmd.Validate())
            {
                cmd.Run();
            }
        }

        private void OnItemBeginDrag(object sender, ItemArgs e)
        {
            Editor.DragDrop.RaiseBeginDrag(this, e.Items, e.PointerEventData);
        }

        private void OnItemDragEnter(object sender, ItemDropCancelArgs e)
        {
            Editor.DragDrop.SetCursor(KnownCursor.DropNotAllowed);
            e.Cancel = true;
        }

        private void OnItemDrag(object sender, ItemArgs e)
        {
            Editor.DragDrop.RaiseDrag(e.PointerEventData);
        }

        private void OnItemDragExit(object sender, EventArgs e)
        {
            Editor.DragDrop.SetCursor(KnownCursor.DropNotAllowed);
        }

        private void OnItemDrop(object sender, ItemDropArgs e)
        {
            Editor.DragDrop.RaiseDrop(e.PointerEventData);
        }

        private void OnItemEndDrag(object sender, ItemArgs e)
        {
            Editor.DragDrop.RaiseDrop(e.PointerEventData);
        }
    }
}


