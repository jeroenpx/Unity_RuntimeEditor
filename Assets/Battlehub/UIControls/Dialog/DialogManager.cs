using UnityEngine;
using Battlehub.UIControls.DockPanels;
using System.Collections.Generic;

namespace Battlehub.UIControls.Dialogs
{
    public class DialogManager : MonoBehaviour
    {
        [SerializeField]
        private DockPanelsRoot m_dockPanels = null;

        [SerializeField]
        private Dialog m_dialogPrefab = null;

        private Stack<Dialog> m_dialogStack = new Stack<Dialog>();

        public event DialogAction DialogDestroyed;

        public Dialog ShowDialog(Sprite icon, string header, Transform content,
             DialogAction<DialogCancelArgs> okAction = null, string okText = "OK",
             DialogAction<DialogCancelArgs> cancelAction = null, string cancelText = "Cancel",
             float minWidth = 150,
             float minHeight = 150,
             float preferredWidth = 700, 
             float preferredHeight = 400, 
             bool canResize = true)
        {
            Dialog dialog = ShowDialog(icon, header, string.Empty, okAction, okText, cancelAction, cancelText, minWidth, minHeight, preferredWidth, preferredHeight, canResize);
            dialog.Content = content;
            return dialog;
        }

        public Dialog ShowDialog(Sprite icon, string header, string content,
            DialogAction<DialogCancelArgs> okAction = null, string okText = "OK",
            DialogAction<DialogCancelArgs> cancelAction = null, string cancelText = "Cancel",
            float minWidth = 350, 
            float minHeight = 150,
            float preferredWidth = 350,
            float preferredHeight = 150,
            bool canResize = false)
        {
            if(m_dialogStack.Count > 0)
            {
                Dialog previousDialog = m_dialogStack.Peek();
                previousDialog.Hide();
            }

            Dialog dialog = Instantiate(m_dialogPrefab);
            dialog.name = "Dialog " + header;
            dialog.Icon = icon;
            dialog.HeaderText = header;
            dialog.ContentText = content;
            dialog.OkAction = okAction;
            dialog.OkText = okText;
            if(cancelAction != null)
            {
                dialog.CancelAction = cancelAction;
                dialog.CancelText = cancelText;
            }
            else
            {
                dialog.IsCancelVisible = false;
            }
            
            dialog.Closed += OnDestroyed;
            m_dockPanels.AddModalRegion(dialog.HeaderRoot, dialog.transform, minWidth, minHeight, new Rect(0, 0, preferredWidth, preferredHeight), true, canResize);
            m_dialogStack.Push(dialog);

            return dialog;      
        }

        private void OnDestroyed(Dialog sender)
        {
            sender.Closed -= OnDestroyed;
            if (m_dialogStack.Contains(sender))
            {
                while(m_dialogStack.Count > 0)
                {
                    Dialog dialog = m_dialogStack.Pop();
                    
                    if (sender == dialog)
                    {
                        if (DialogDestroyed != null)
                        {
                            DialogDestroyed(dialog);
                        }
                        dialog.Close();
                        if (m_dialogStack.Count > 0)
                        {
                            Dialog previousDialog = m_dialogStack.Peek();
                            previousDialog.Show();
                        }
                        break;
                    }

                    dialog.Close();
                }
            }
        }
    }
}

