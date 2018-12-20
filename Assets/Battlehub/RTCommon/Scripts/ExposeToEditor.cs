using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Battlehub.Utils;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Battlehub.RTCommon
{
    public enum BoundsType
    {
        Any,
        Mesh,
        SkinnedMesh,
        Custom,
        None,
        Sprite,
    }


    public delegate void ExposeToEditorChangeEvent<T>(ExposeToEditor obj, T oldValue, T newValue);
    public delegate void ExposeToEditorEvent(ExposeToEditor obj);

    [System.Serializable]
    public class ExposeToEditorUnityEvent : UnityEvent<ExposeToEditor> { }

    [DisallowMultipleComponent]
    public class ExposeToEditor : MonoBehaviour
    {
        public static event ExposeToEditorEvent _Awaked;
        public static event ExposeToEditorEvent _Destroying;
        public static event ExposeToEditorEvent _Destroyed;
        public static event ExposeToEditorEvent _MarkAsDestroyedChanged;
        public static event ExposeToEditorEvent _NameChanged;
        public static event ExposeToEditorEvent _TransformChanged;
        public static event ExposeToEditorEvent _Started;
        public static event ExposeToEditorEvent _Enabled;
        public static event ExposeToEditorEvent _Disabled;
        public static event ExposeToEditorChangeEvent<ExposeToEditor> _ParentChanged;
        
        [SerializeField]
        [HideInInspector]
        private Collider[] m_colliders;
        public Collider[] Colliders
        {
            get { return m_colliders; }
            set { m_colliders = value; }
        }

        private SpriteRenderer m_spriteRenderer;
        public SpriteRenderer SpriteRenderer
        {
            get { return m_spriteRenderer; }
        }

        private MeshFilter m_filter;
        public MeshFilter MeshFilter
        {
            get { return m_filter; }
        }

        private SkinnedMeshRenderer m_skinned;
        public SkinnedMeshRenderer SkinnedMeshRenderer
        {
            get { return m_skinned; }
        }

        private static readonly Bounds m_none = new Bounds();
        public ExposeToEditorUnityEvent Selected;
        public ExposeToEditorUnityEvent Unselected;
        public GameObject BoundsObject;
        public BoundsType BoundsType;
        public Bounds CustomBounds;

        [HideInInspector]
        public bool CanSelect = true;
        [HideInInspector]
        public bool CanSnap = true;
        public bool AddColliders = true;

  
        private bool m_markAsDestroyed;
        public bool MarkAsDestroyed
        {
            get { return m_markAsDestroyed; }
            set
            {
                if (m_markAsDestroyed != value)
                {
                    m_markAsDestroyed = value;
                    gameObject.SetActive(!m_markAsDestroyed);
                    if (_MarkAsDestroyedChanged != null)
                    {
                        _MarkAsDestroyedChanged(this);
                    }
                }
            }
        }

        private BoundsType m_effectiveBoundsType;
        public BoundsType EffectiveBoundsType
        {
            get { return m_effectiveBoundsType; }
        }
        public Bounds Bounds
        {
            get
            {
                if (m_effectiveBoundsType == BoundsType.Any)
                {
                    if (m_filter != null && m_filter.sharedMesh != null)
                    {
                        return m_filter.sharedMesh.bounds;
                    }
                    else if (m_skinned != null && m_skinned.sharedMesh != null)
                    {
                        return m_skinned.sharedMesh.bounds;
                    }
                    else if(m_spriteRenderer != null)
                    {
                        return m_spriteRenderer.sprite.bounds;
                    }

                    return CustomBounds;
                }
                else if (m_effectiveBoundsType == BoundsType.Mesh)
                {
                    if (m_filter != null && m_filter.sharedMesh != null)
                    {
                        return m_filter.sharedMesh.bounds;
                    }
                    return m_none;
                }
                else if (m_effectiveBoundsType == BoundsType.SkinnedMesh)
                {
                    if (m_skinned != null && m_skinned.sharedMesh != null)
                    {
                        return m_skinned.sharedMesh.bounds;
                    }
                }
                else if(m_effectiveBoundsType == BoundsType.Sprite)
                {
                    if (m_spriteRenderer != null)
                    {
                        return m_spriteRenderer.sprite.bounds;
                    }
                }
                else if (m_effectiveBoundsType == BoundsType.Custom)
                {
                    return CustomBounds;
                }
                return m_none;
            }
        }

        private HierarchyItem m_hierarchyItem;
        private List<ExposeToEditor> m_children = new List<ExposeToEditor>();
        public int ChildCount
        {
            get { return m_children.Count; }
        }

        public int MarkedAsDestroyedChildCount
        {
            get
            {
                return m_children.Where(e => e.MarkAsDestroyed).Count();
            }

        }

        public ExposeToEditor[] GetChildren()
        {
            return m_children.OrderBy(c => c.transform.GetSiblingIndex()).ToArray();
        }

        public ExposeToEditor NextSibling(IEnumerable<ExposeToEditor> objects)
        {
            if (Parent != null)
            {
                int index = Parent.m_children.IndexOf(this);
                if (index < Parent.m_children.Count - 1)
                {
                    return Parent.m_children[index - 1];
                }
                return null;
            }

            IEnumerable<ExposeToEditor> exposedToEditor = objects.OrderBy(o => o.transform.GetSiblingIndex());
            IEnumerator<ExposeToEditor> en = exposedToEditor.GetEnumerator();
            while (en.MoveNext())
            {
                if (en.Current == this)
                {
                    en.MoveNext();
                    return en.Current;
                }
            }
            return null;
        }

        private ExposeToEditor m_parent;
        public ExposeToEditor Parent
        {
            get { return m_parent; }
            set
            {
                if (m_parent != value)
                {
                    ExposeToEditor oldParent = ChangeParent(value);

                    if (_ParentChanged != null)
                    {
                        _ParentChanged(this, oldParent, m_parent);
                    }
                }
            }
        }

        private ExposeToEditor ChangeParent(ExposeToEditor value)
        {
            ExposeToEditor oldParent = m_parent;
            m_parent = value;

            if (oldParent != null)
            {
                oldParent.m_children.Remove(this);
            }

            if (m_parent != null)
            {
                m_parent.m_children.Add(this);
            }

            return oldParent;
        }

        private bool m_initialized;
               
        private void Awake()
        {
            Init();

            m_hierarchyItem = gameObject.GetComponent<HierarchyItem>();
            if (m_hierarchyItem == null)
            {
                m_hierarchyItem = gameObject.AddComponent<HierarchyItem>();
            }

            if (hideFlags != HideFlags.HideAndDontSave)
            {
                if (_Awaked != null)
                {
                    _Awaked(this);
                }
            }
        }

        public void Init()
        {
            if (m_initialized)
            {
                return;
            }
            FindChildren(transform);
            if (BoundsObject == null)
            {
                BoundsObject = gameObject;
            }
            m_initialized = true;
        }

        private void FindChildren(Transform parent)
        {
            foreach (Transform t in parent)
            {
                ExposeToEditor exposeToEditor = t.GetComponent<ExposeToEditor>();
                if (exposeToEditor == null)
                {
                    FindChildren(t);
                }
                else
                {
                    exposeToEditor.m_parent = this;
                    m_children.Add(exposeToEditor);
                }
            }
        }

        private void Start()
        {
          

            m_effectiveBoundsType = BoundsType;
            m_filter = BoundsObject.GetComponent<MeshFilter>();
            m_skinned = BoundsObject.GetComponent<SkinnedMeshRenderer>();

            if(m_filter == null && m_skinned == null)
            {
                m_spriteRenderer = BoundsObject.GetComponent<SpriteRenderer>();
            }
            
            if (hideFlags != HideFlags.HideAndDontSave)
            {
                if (_Started != null)
                {
                    _Started(this);
                }
            }
        }

        private void OnEnable()
        {
            if (hideFlags != HideFlags.HideAndDontSave)
            {
                if (_Enabled != null)
                {
                    _Enabled(this);
                }
            }
        }

        private void OnDisable()
        {
            if (hideFlags != HideFlags.HideAndDontSave)
            {
                if (_Disabled != null)
                {
                    _Disabled(this);
                }
            }
        }

        private void OnDestroy()
        {
            if (!m_isPaused)
            {
                if (hideFlags != HideFlags.HideAndDontSave)
                {
                    if (_Destroying != null)
                    {
                        _Destroying(this);
                    }
                }

                if (m_parent != null)
                {
                    ChangeParent(null);
                }

                if (m_hierarchyItem != null)
                {
                    Destroy(m_hierarchyItem);
                }

                if (hideFlags != HideFlags.HideAndDontSave)
                {
                    if (_Destroyed != null)
                    {
                        _Destroyed(this);
                    }
                }
            }
        }

        private bool m_isPaused;
        private void OnApplicationQuit()
        {
            m_isPaused = true;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (Application.isEditor)
            {
                return;
            }
            m_isPaused = !hasFocus;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            m_isPaused = pauseStatus;
        }

        private void Update()
        {
            if (_TransformChanged != null)
            {
                if (transform.hasChanged)
                {
                    transform.hasChanged = false;

                    if (hideFlags != HideFlags.HideAndDontSave)
                    {
                        if (_TransformChanged != null)
                        {
                            _TransformChanged(this);
                        }
                    }
                }
            }
        }

        public void SetName(string name)
        {
            gameObject.name = name;
            if (hideFlags != HideFlags.HideAndDontSave)
            {
                if (_NameChanged != null)
                {
                    _NameChanged(this);
                }
            }
        }
    }
}

