using Battlehub.RTCommon;
using Battlehub.RTSaveLoad2.Interface;
using Battlehub.UIControls;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class ProjectsDialog : MonoBehaviour
    {
        [SerializeField]
        private InputDialog m_inputDialogPrefab = null;

        [SerializeField]
        private VirtualizingTreeView m_treeView = null;
        
        [SerializeField]
        private Sprite ProjectIcon = null;

        private PopupWindow m_parentPopup;

        private IProject m_project;

        [SerializeField]
        private Button m_btnNew = null;

        [SerializeField]
        private Button m_btnDelete = null;

        public ProjectInfo SelectedProject
        {
            get { return m_treeView.SelectedItem as ProjectInfo; }
        }

        private void Start()
        {
            m_parentPopup = GetComponentInParent<PopupWindow>();
            if (m_parentPopup != null)
            {
                m_parentPopup.OK.AddListener(OnOK);
            }


            if (m_treeView == null)
            {
                Debug.LogError("m_builtInTreeView == null");
                return;
            }

            m_treeView.ItemDataBinding += OnItemDataBinding;
            m_treeView.ItemDoubleClick += OnItemDoubleClick;
            m_treeView.CanDrag = false;
            m_treeView.CanEdit = false;
            m_treeView.CanUnselectAll = false;

            m_project = IOC.Resolve<IProject>();
           
            IRTE editor = IOC.Resolve<IRTE>();

            m_parentPopup.IsContentLoaded = false;
            editor.IsBusy = true;

            m_project.GetProjects((error, projectInfo) =>
            {
                if(error.HasError)
                {
                    PopupWindow.Show("Unable to get projects", error.ToString(), "OK");
                    return;
                }

                m_parentPopup.IsContentLoaded = true;
                editor.IsBusy = false;

                m_treeView.Items = projectInfo.OrderBy(p => p.Name).ToArray();
                if(projectInfo != null && projectInfo.Length > 0)
                {
                    m_treeView.SelectedIndex = 0;
                }

                if (m_btnNew != null)
                {
                    m_btnNew.onClick.AddListener(OnCreateProjectClick);
                }

                if (m_btnDelete != null)
                {
                    m_btnDelete.onClick.AddListener(OnDestroyProjectClick);
                }
            });
        }

        private void OnDestroy()
        {
            if (m_parentPopup != null)
            {
                m_parentPopup.OK.RemoveListener(OnOK);
            }

            if (m_treeView != null)
            {
                m_treeView.ItemDataBinding -= OnItemDataBinding;
                m_treeView.ItemDoubleClick -= OnItemDoubleClick;
            }

            if (m_btnNew != null)
            {
                m_btnNew.onClick.RemoveListener(OnCreateProjectClick);
            }

            if (m_btnDelete != null)
            {
                m_btnDelete.onClick.RemoveListener(OnDestroyProjectClick);
            }
        }

        private void OnItemDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            ProjectInfo item = e.Item as ProjectInfo;
            if (item != null)
            {
                Text text = e.ItemPresenter.GetComponentInChildren<Text>(true);
                text.text = item.Name;

                Image image = e.ItemPresenter.GetComponentInChildren<Image>(true);
                image.sprite = ProjectIcon;
                image.gameObject.SetActive(true);

                e.HasChildren = false;
            }
        }

        private void OnItemDoubleClick(object sender, ItemArgs e)
        {
            m_parentPopup.Close(true);
        }

        private void OnOK(PopupWindowArgs args)
        {
            if (m_treeView.SelectedItem == null)
            {
                args.Cancel = true;
                return;
            }
        }

        private void OnCreateProjectClick()
        {
            InputDialog input = Instantiate(m_inputDialogPrefab);
            input.transform.position = Vector3.zero;

            PopupWindow.Show("Create Project", input.transform, "Create",
                args =>
                {
                    string projectName = input.Text;
                    if(string.IsNullOrEmpty(projectName))
                    {
                        args.Cancel = true;
                        return;
                    }

                    if (m_treeView.Items != null && m_treeView.Items.OfType<ProjectInfo>().Any(p => p.Name == projectName))
                    {
                        PopupWindow.Show("Unable to create project", "Project with the same name already exists", "OK");
                        args.Cancel = true;
                        return;
                    }

                    m_project.CreateProject(projectName, (error, newProjectInfo) =>
                    {
                        if(error.HasError)
                        {
                            PopupWindow.Show("Unable to create project", error.ErrorText, "OK");
                            args.Cancel = true;
                            return;
                        }

                        ProjectInfo[] projectInfo = m_treeView.Items.OfType<ProjectInfo>().Union(new[] { newProjectInfo }).OrderBy(p => p.Name).ToArray();
                        m_treeView.Insert(Array.IndexOf(projectInfo, newProjectInfo), newProjectInfo);
                        m_treeView.SelectedItem = newProjectInfo;
                    });
                },
                "Cancel");
        }

        private void OnDestroyProjectClick()
        {
            ProjectInfo selectedProject = (ProjectInfo)m_treeView.SelectedItem;
            PopupWindow.Show("Delete Project", "Delete " + selectedProject.Name  + " project?", "Delete", args =>
            {
                ProjectInfo[] projectInfo = m_treeView.Items.OfType<ProjectInfo>().ToArray();
                int index = Array.IndexOf(projectInfo, selectedProject);
                m_project.DeleteProject(selectedProject.Name, (error, deletedProject) =>
                {
                    if(error.HasError)
                    {
                        PopupWindow.Show("Unable to delete project", error.ErrorText, "OK");
                        args.Cancel = true;
                        return;
                    }

                    m_treeView.RemoveChild(null, selectedProject, projectInfo.Length == 1);

                    if((projectInfo.Length - 1) == index)
                    {
                        m_treeView.SelectedIndex = (index - 1);
                    }
                    else
                    {
                        m_treeView.SelectedIndex = index;
                    }
                    
                });
            },
            "Cancel");

        }
    }
}
