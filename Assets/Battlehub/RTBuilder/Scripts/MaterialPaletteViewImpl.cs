using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.UIControls;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTBuilder
{
    public class MaterialPaletteViewImpl : MonoBehaviour
    {
        private const string DataFolder = "RTBuilderData/";
        private const string PaletteFile = "DefaultMaterialPalette";
        private const string MaterialFile = "Material";

        private IProBuilderTool m_proBuilderTool;
        private IMaterialPaletteManager m_paletteManager;
        private MaterialPaletteView m_view;
        public MaterialPaletteView View
        {
            get { return m_view; }
        }

        private IRTE m_editor;
        public IRTE Editor
        {
            get { return m_editor; }
        }

        private IWindowManager m_wm;
        private Transform m_probuilderWindow;

        protected virtual IEnumerator Start()
        {
            m_editor = IOC.Resolve<IRTE>();
            m_view = GetComponent<MaterialPaletteView>();
            m_wm = IOC.Resolve<IWindowManager>();
            m_wm.WindowCreated += OnWindowCreated;
            m_wm.WindowDestroyed += OnWindowDestroyed;


            m_proBuilderTool = IOC.Resolve<IProBuilderTool>();
            m_paletteManager = IOC.Resolve<IMaterialPaletteManager>();

            m_view.TreeView.ItemDataBinding += OnItemDataBinding;
            m_view.TreeView.ItemDrop += OnItemDrop;
            m_view.TreeView.SelectionChanged += OnSelectionChanged;

            m_view.TreeView.CanEdit = false;
            m_view.TreeView.CanReorder = true;
            m_view.TreeView.CanReparent = false;
            m_view.TreeView.CanSelectAll = false;
            m_view.TreeView.CanUnselectAll = true;
            m_view.TreeView.CanRemove = false;

            if (m_paletteManager != null)
            {
                m_paletteManager.PaletteChanged += OnPaletteChanged;
                m_paletteManager.MaterialAdded += OnMaterialAdded;
                m_paletteManager.MaterialCreated += OnMaterialCreated;
                m_paletteManager.MaterialRemoved += OnMaterialRemoved;

                yield return new WaitUntil(() => m_paletteManager.IsReady);
                yield return new WaitWhile(() => m_editor.IsBusy);

                if (m_view.CreateMaterialButton != null)
                {
                    m_view.CreateMaterialButton.onClick.AddListener(CreateMaterial);
                }

                m_view.TreeView.Items = m_paletteManager.Palette.Materials;

            }
        }

        protected virtual void OnDestroy()
        {
            if(m_wm != null)
            {
                m_wm.WindowCreated -= OnWindowCreated;
                m_wm.WindowDestroyed -= OnWindowDestroyed;
            }
            if (m_view.TreeView != null)
            {
                m_view.TreeView.ItemDataBinding -= OnItemDataBinding;
                m_view.TreeView.ItemDrop -= OnItemDrop;
                m_view.TreeView.SelectionChanged -= OnSelectionChanged;
            }

            if (m_view.CreateMaterialButton != null)
            {
                m_view.CreateMaterialButton.onClick.RemoveListener(CreateMaterial);
            }

            if (m_paletteManager != null)
            {
                m_paletteManager.MaterialAdded -= OnMaterialAdded;
                m_paletteManager.MaterialCreated -= OnMaterialCreated;
                m_paletteManager.MaterialRemoved -= OnMaterialRemoved;
                m_paletteManager.PaletteChanged -= OnPaletteChanged;
            }
        }

        private void OnItemDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            Material material = (Material)e.Item;

            MaterialPaletteItem paletteItem = e.ItemPresenter.GetComponent<MaterialPaletteItem>();
            paletteItem.Material = material;

            int index = m_paletteManager.Palette.Materials.IndexOf(material);
            if (index > 10)
            {
                paletteItem.Text = "Apply";
            }
            else
            {
                paletteItem.Text = "Alt + " + (m_paletteManager.Palette.Materials.IndexOf(material) + 1) % 10;
            }
        }

        private void OnItemDrop(object sender, ItemDropArgs args)
        {
            m_view.TreeView.ItemDropStdHandler<Material>(args,
                (item) => null,
                (item, parent) => { },
                (item, parent) => m_paletteManager.Palette.Materials.IndexOf(item),
                (item, parent) => m_paletteManager.Palette.Materials.Remove(item),
                (item, parent, i) => m_paletteManager.Palette.Materials.Insert(i, item),
                (item, parent) => m_paletteManager.Palette.Materials.Add(item));

            for (int i = 0; i < m_paletteManager.Palette.Materials.Count; ++i)
            {
                m_view.TreeView.DataBindItem(m_paletteManager.Palette.Materials[i]);
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedArgs e)
        {
            Material material = (Material)e.NewItem;
            if (material != null)
            {                
                m_view.Texture = material.mainTexture;
                m_view.TexturePicker.gameObject.SetActive(true);
                m_view.TreeView.ScrollIntoView(material);
            }
            else
            {
                m_view.TexturePicker.gameObject.SetActive(false);
                m_view.Texture = null;
            }
        }

        public virtual bool CanDrop()
        {
            return false;
        }

        public virtual void CompleteDragDrop()
        {

        }

        public void ApplyMaterial(Material material)
        {
            if (m_proBuilderTool != null)
            {
                m_proBuilderTool.ApplyMaterial(material);
            }
        }

        public void CreateMaterial()
        {
            m_paletteManager.CreateMaterial();
        }

        public void RemoveMaterial(Material material)
        {
            m_view.TreeView.RemoveChild(null, material);
            m_paletteManager.RemoveMaterial(material);
        }

        public void SelectMaterial(Material material)
        {
            m_view.TreeView.SelectedItem = material;
        }

        public void SelectFacesByMaterial(Material material)
        {
            if (m_proBuilderTool != null)
            {
                m_proBuilderTool.SelectFaces(material);
            }
        }

        public void UnselectFacesByMaterial(Material material)
        {
            if(m_proBuilderTool != null)
            {
                m_proBuilderTool.UnselectFaces(material);
            }    
        }

        private void OnPaletteChanged(MaterialPalette palette)
        {
            m_view.TreeView.Items = palette.Materials;
            m_view.TreeView.SelectedItem = palette.Materials.FirstOrDefault();
        }

        private void OnMaterialCreated(Material material)
        {
            m_view.TreeView.Add(material);
            m_view.TreeView.SelectedItem = material;
            m_view.TreeView.ScrollIntoView(material);
        }

        private void OnMaterialAdded(Material material)
        {
            m_view.TreeView.Add(material);
        }

        private void OnMaterialRemoved(Material material)
        {
            for (int i = 0; i < m_paletteManager.Palette.Materials.Count; ++i)
            {
                m_view.TreeView.DataBindItem(m_paletteManager.Palette.Materials[i]);
            }
        }

        private void OnWindowCreated(Transform obj)
        {
            if (obj == m_wm.GetWindow("ProBuilder"))
            {
                m_probuilderWindow = obj;
                m_proBuilderTool = IOC.Resolve<IProBuilderTool>();
                m_paletteManager = IOC.Resolve<IMaterialPaletteManager>();
                m_paletteManager.PaletteChanged += OnPaletteChanged;
                m_paletteManager.MaterialAdded += OnMaterialAdded;
                m_paletteManager.MaterialCreated += OnMaterialCreated;
                m_paletteManager.MaterialRemoved += OnMaterialRemoved;

                m_coPopulateTreeView = CoPopulateTreeView();
                StartCoroutine(m_coPopulateTreeView);
            }
        }

        private IEnumerator m_coPopulateTreeView;
        private IEnumerator CoPopulateTreeView()
        {
            yield return new WaitUntil(() => m_paletteManager.IsReady);
            yield return new WaitWhile(() => m_editor.IsBusy);

            if (m_view.CreateMaterialButton != null)
            {
                m_view.CreateMaterialButton.onClick.AddListener(CreateMaterial);
            }

            m_view.TreeView.Items = m_paletteManager.Palette.Materials;
        }

        private void OnWindowDestroyed(Transform obj)
        {
            if(obj == m_probuilderWindow)
            {
                m_probuilderWindow = null;
                m_proBuilderTool = null;
                if(m_paletteManager != null)
                {
                    if (m_coPopulateTreeView != null)
                    {
                        StopCoroutine(m_coPopulateTreeView);
                        m_coPopulateTreeView = null;
                    }

                    m_paletteManager.PaletteChanged -= OnPaletteChanged;
                    m_paletteManager.MaterialAdded -= OnMaterialAdded;
                    m_paletteManager.MaterialCreated -= OnMaterialCreated;
                    m_paletteManager.MaterialRemoved -= OnMaterialRemoved;

                    if (m_view.CreateMaterialButton != null)
                    {
                        m_view.CreateMaterialButton.onClick.RemoveListener(CreateMaterial);
                    }

                    m_view.TreeView.Items = null;
                }
                m_paletteManager = null;
            }   
        }

        protected virtual void Update()
        {
            if (m_editor.ActiveWindow == null || m_editor.ActiveWindow != this && m_editor.ActiveWindow.WindowType != RuntimeWindowType.Scene)
            {
                return;
            }

            if (m_proBuilderTool == null || !m_proBuilderTool.HasSelection)
            {
                return;
            }

            IInput input = m_editor.Input;
            if (input.GetKeyDown(KeyCode.Delete))
            {
                if (m_view.TreeView.SelectedItem != null)
                {
                    foreach (Material material in m_view.TreeView.SelectedItems)
                    {
                        m_paletteManager.RemoveMaterial(material);
                    }
                }

                m_view.TreeView.RemoveSelectedItems();
            }
        }
    }

}
