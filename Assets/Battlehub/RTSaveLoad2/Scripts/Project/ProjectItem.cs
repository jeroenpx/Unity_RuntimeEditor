using ProtoBuf;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System;

namespace Battlehub.RTSaveLoad2
{
    [ProtoContract]
    public class AssetLibraryReferenceInfo
    {
        [ProtoMember(1)]
        public string AssetLibrary;

        [ProtoMember(2)]
        public int Ordinal;
    }

    [ProtoContract]
    public class ProjectInfo
    {
        [ProtoMember(1)]
        public int IdentitiyCounter = 1;

        [ProtoMember(2)]
        public AssetLibraryReferenceInfo[] References;
    }

    [ProtoContract]
    public class ProjectItem
    {
        [ProtoMember(1)]
        public long ItemID;

        public string Name;
        public string Ext;

        public ProjectItem Parent;
        public List<ProjectItem> Children;

        public string NameExt
        {
            get { return Name + Ext; }
        }

        public virtual bool IsFolder
        {
            get { return true; }
        }

        public static bool IsValidName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return true;
            }
            return Path.GetInvalidFileNameChars().All(c => !name.Contains(c));
        }

        public void AddChild(ProjectItem item)
        {
            if (Children == null)
            {
                Children = new List<ProjectItem>();
            }

            if (item.Parent != null)
            {
                item.Parent.RemoveChild(item);
            }
            Children.Add(item);
            item.Parent = this;
        }

        public void RemoveChild(ProjectItem item)
        {
            if (Children == null)
            {
                return;
            }
            Children.Remove(item);
            item.Parent = null;
        }

        public int GetSiblingIndex()
        {
            return Parent.Children.IndexOf(this);
        }

        public void SetSiblingIndex(int index)
        {
            Parent.Children.Remove(this);
            Parent.Children.Insert(index, this);
        }

        public ProjectItem Get(string path)
        {
            path = path.Trim('/');
            string[] pathParts = path.Split('/');

            ProjectItem item = this;
            for (int i = 1; i < pathParts.Length; ++i)
            {
                string pathPart = pathParts[i];
                if (item.Children == null)
                {
                    return item;
                }

                if (i == pathParts.Length - 1)
                {
                    item = item.Children.Where(child => child.NameExt == pathPart).FirstOrDefault();
                }
                else
                {
                    item = item.Children.Where(child => child.Name == pathPart).FirstOrDefault();
                }

                if (item == null)
                {
                    break;
                }
            }
            return item;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            ProjectItem parent = this;
            while (parent != null)
            {
                sb.Insert(0, parent.Name);
                sb.Insert(0, "/");
                parent = parent.Parent;
            }

            string ext = Ext;
            if (string.IsNullOrEmpty(ext))
            {
                return sb.ToString();
            }
            return string.Format("{0}.{1}", sb.ToString(), Ext);
        }
    }

    [ProtoContract]
    public class AssetItem : ProjectItem
    {
        public event EventHandler PreviewDataChanged;

        [ProtoMember(1)]
        public byte[] m_previewData;
        public byte[] PreviewData
        {
            get { return m_previewData; }
            set
            {
                if(m_previewData != value)
                {
                    m_previewData = value;
                    if (PreviewDataChanged != null)
                    {
                        PreviewDataChanged(this, EventArgs.Empty);
                    }
                }
                
            }
        }

        [ProtoMember(2)]
        public Guid TypeGuid;

        public override bool IsFolder
        {
            get { return false; }
        }

    }

}

