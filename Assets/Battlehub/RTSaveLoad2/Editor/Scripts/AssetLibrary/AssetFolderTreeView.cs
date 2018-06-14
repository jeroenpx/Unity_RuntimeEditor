using Battlehub.RTCommon.EditorTreeView;
using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battlehub.RTSaveLoad2
{
    internal class AssetFolderTreeView : TreeViewWithTreeModel<AssetFolderInfo>
    {
        private const float kRowHeights = 20f;
        private const float kIconWidth = 18f;
        public bool showControls = true;

        private static Texture2D[] s_icons =
        {
            EditorGUIUtility.FindTexture ("Folder Icon")
        };

        private enum Columns
        {
            Name,
            ExposeToEditor,
        }

        public AssetFolderTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, TreeModel<AssetFolderInfo> model) : base(state, multiColumnHeader, model)
        {
            rowHeight = kRowHeights;
            columnIndexForTreeFoldouts = 0;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            customFoldoutYOffset = (kRowHeights - EditorGUIUtility.singleLineHeight) * 0.5f; // center foldout in the row since we also center content. See RowGUI
            extraSpaceBeforeIconAndLabel = kIconWidth;

            Reload();
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (TreeViewItem<AssetFolderInfo>)args.item;

            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, (Columns)args.GetColumn(i), ref args);
            }
        }

        private void CellGUI(Rect cellRect, TreeViewItem<AssetFolderInfo> item, Columns column, ref RowGUIArgs args)
        {
            // Center cell rect vertically (makes it easier to place controls, icons etc in the cells)
            CenterRectUsingSingleLineHeight(ref cellRect);

            switch (column)
            {
          
                case Columns.Name:
                    {
                        // Do toggle
                        Rect iconRect = cellRect;
                        iconRect.x += GetContentIndent(item);
                        iconRect.width = kIconWidth;
                        if (iconRect.xMax < cellRect.xMax)
                        {
                            GUI.DrawTexture(iconRect, s_icons[0], ScaleMode.ScaleToFit);
                        }

                        // Default icon and label
                        args.rowRect = cellRect;
                        base.RowGUI(args);
                    }
                    break; 
                case Columns.ExposeToEditor:
                    {
                        item.data.IsEnabled = EditorGUI.Toggle(cellRect, item.data.IsEnabled);
                    }
                    break;
            }
        }

        protected override bool CanRename(TreeViewItem item)
        {
            // Only allow rename if we can show the rename overlay with a certain width (label might be clipped by other columns)
            Rect renameRect = GetRenameRect(treeViewRect, 0, item);
            return renameRect.width > 30;
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            // Set the backend name and reload the tree to reflect the new model
            if (args.acceptedRename)
            {
                var element = treeModel.Find(args.itemID);
                element.name = args.newName;
                Reload();
            }
        }

        protected override Rect GetRenameRect(Rect rowRect, int row, TreeViewItem item)
        {
            Rect cellRect = GetCellRectForTreeFoldouts(rowRect);
            CenterRectUsingSingleLineHeight(ref cellRect);
            return base.GetRenameRect(cellRect, row, item);
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return true;
        }

        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState(float treeViewWidth)
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Folder", ""),
                    contextMenuText = "Folder",
                    headerTextAlignment = TextAlignment.Left,
                    canSort = false,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 200,
                    minWidth = 60,
                    autoResize = true,
                    allowToggleVisibility = false
                },
            
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Is Enabled"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 70,
                    minWidth = 70,
                    autoResize = false,
                    allowToggleVisibility = false
                },
               
            };

            Assert.AreEqual(columns.Length, Enum.GetValues(typeof(Columns)).Length, "Number of columns should match number of enum values: You probably forgot to update one of them.");

            var state = new MultiColumnHeaderState(columns);
            return state;
        }


    }
}

