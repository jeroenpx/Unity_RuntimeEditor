using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.UIControls;
using Battlehub.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Battlehub.RTTerrain
{
    public class TerrainSelectionHandlesEditor : MonoBehaviour
    {
        [SerializeField]
        private VirtualizingTreeView m_commandsList = null;

        [SerializeField]
        private BoolEditor m_zTestEditor = null;

        [SerializeField]
        private RangeEditor m_xSpacingEditor = null;

        [SerializeField]
        private RangeEditor m_zSpacingEditor = null;

        private ToolCmd[] m_commands;
        private ITerrainSelectionHandlesTool m_terrainTool;
        private IRuntimeEditor m_editor;

        private bool m_isTerrainHandleSelected = false;

        private void Awake()
        {
            m_editor = IOC.Resolve<IRuntimeEditor>();
                        
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

            m_terrainTool = IOC.Resolve<ITerrainSelectionHandlesTool>();
            m_terrainTool.Selection.SelectionChanged += OnTerrainToolSelectionChanged;
                        
            if (m_xSpacingEditor != null)
            {
                m_xSpacingEditor.Min = 5;
                m_xSpacingEditor.Max = 40;
                m_xSpacingEditor.Init(m_terrainTool, m_terrainTool, Strong.PropertyInfo((ITerrainSelectionHandlesTool x) => x.XSpacing), null, "X Space", null, null, () => m_terrainTool.Refresh(), false);
            }

            if (m_zSpacingEditor != null)
            {
                m_zSpacingEditor.Min = 5;
                m_zSpacingEditor.Max = 40;
                m_zSpacingEditor.Init(m_terrainTool, m_terrainTool, Strong.PropertyInfo((ITerrainSelectionHandlesTool x) => x.ZSpacing), null, "Z Space", null, null, () => m_terrainTool.Refresh(), false);
            }

            if (m_zTestEditor != null)
            {
                m_zTestEditor.Init(m_terrainTool, m_terrainTool, Strong.PropertyInfo((ITerrainSelectionHandlesTool x) => x.EnableZTest), null, "Z Test");
            }
        }

        private void OnDestroy()
        {
            if (m_editor != null)
            {
                m_editor.Selection.SelectionChanged -= OnSelectionChanged;
            }

            if(m_terrainTool != null)
            {
                m_terrainTool.Selection.SelectionChanged -= OnTerrainToolSelectionChanged;
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
        }

        private void Start()
        {
            UpdateFlags();
            m_commands = GetCommands().ToArray();
            m_commandsList.Items = m_commands;
        }

        private void OnEnable()
        {
            if(m_editor != null)
            {
                m_editor.Selection.SelectionChanged += OnSelectionChanged;
            }

            if(m_terrainTool != null)
            {
                m_terrainTool.IsEnabled = true;
            }
            
            UpdateFlags();
            m_commandsList.DataBindVisible();
        }

        private void OnDisable()
        {
            if(m_terrainTool != null)
            {
                m_terrainTool.IsEnabled = false;
            }

            if (m_editor != null)
            {
                m_editor.Selection.SelectionChanged -= OnSelectionChanged;
            }
        }

        private List<ToolCmd> GetCommands()
        {
            return new List<ToolCmd>()
            {
                new ToolCmd("Reset Position", () => m_terrainTool.ResetPosition(), () => m_isTerrainHandleSelected),
                new ToolCmd("Cut Holes", () => m_terrainTool.CutHoles(), () => m_editor.Selection.Length > 0),
                new ToolCmd("Clear Holes", () => m_terrainTool.ClearHoles(), () => m_editor.Selection.Length > 0),
            };
        }

        private void UpdateFlags()
        {
            GameObject[] selected = m_terrainTool.Selection.gameObjects;
            if (selected != null && selected.Length > 0)
            {
                m_isTerrainHandleSelected = selected.Where(go => go.GetComponent<TerrainSelectionHandle>() != null).Any();
            }
            else
            {
                m_isTerrainHandleSelected = false;
            }
        }

        private void OnSelectionChanged(UnityEngine.Object[] unselectedObjects)
        {
            UpdateFlags();
            m_commandsList.DataBindVisible();
        }

        private void OnTerrainToolSelectionChanged(UnityEngine.Object[] unselectedObjects)
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
            if (cmd.Validate())
            {
                cmd.Run();
            }
        }

        private void OnItemBeginDrag(object sender, ItemArgs e)
        {
            m_editor.DragDrop.RaiseBeginDrag(this, e.Items, e.PointerEventData);
        }

        private void OnItemDragEnter(object sender, ItemDropCancelArgs e)
        {
            m_editor.DragDrop.SetCursor(KnownCursor.DropNotAllowed);
            e.Cancel = true;
        }

        private void OnItemDrag(object sender, ItemArgs e)
        {
            m_editor.DragDrop.RaiseDrag(e.PointerEventData);
        }

        private void OnItemDragExit(object sender, EventArgs e)
        {
            m_editor.DragDrop.SetCursor(KnownCursor.DropNotAllowed);
        }

        private void OnItemDrop(object sender, ItemDropArgs e)
        {
            m_editor.DragDrop.RaiseDrop(e.PointerEventData);
        }

        private void OnItemEndDrag(object sender, ItemArgs e)
        {
            m_editor.DragDrop.RaiseDrop(e.PointerEventData);
        }
    }
}


