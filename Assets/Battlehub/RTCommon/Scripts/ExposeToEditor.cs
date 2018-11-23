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
    public enum ExposeToEditorObjectType
    {
        Undefined,
        EditorMode,
        PlayMode
    }


    public delegate void ExposeToEditorChangeEvent<T>(IRTE editor, ExposeToEditor obj, T oldValue, T newValue);
    public delegate void ExposeToEditorEvent(IRTE editor, ExposeToEditor obj);

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
        
        private bool m_applicationQuit;
        [SerializeField]
        [HideInInspector]
        private Collider[] m_colliders;
        public Collider[] Colliders
        {
            get { return m_colliders; }
            set { m_colliders = value; }
        }


        private SpriteRenderer m_spriteRenderer;
        private MeshFilter m_filter;
        private SkinnedMeshRenderer m_skinned;
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

        [SerializeField]
        [HideInInspector]
        private ExposeToEditorObjectType m_objectType;
        public ExposeToEditorObjectType ObjectType
        {
            get { return m_objectType; }
            set
            {
                if (m_objectType != ExposeToEditorObjectType.Undefined && m_objectType != value)
                {
                   // throw new System.InvalidOperationException("ObjectType can not be changed");
                }

                m_objectType = value;
            }
        }

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
                        _MarkAsDestroyedChanged(m_rte, this);
                    }
                }
            }
        }

        private BoundsType m_effectiveBoundsType;
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

        public ExposeToEditor NextSibling()
        {
            if(Parent != null)
            {
                int index = Parent.m_children.IndexOf(this);
                if(index < Parent.m_children.Count - 1)
                {
                    return Parent.m_children[index - 1];
                }
                return null;
            }

            IEnumerable<GameObject> exposedToEditor = m_rte.IsPlaying ?
                FindAll(Editor, ExposeToEditorObjectType.PlayMode) :
                FindAll(Editor, ExposeToEditorObjectType.EditorMode).OrderBy(g => g.transform.GetSiblingIndex()); 

            IEnumerator<GameObject> en = exposedToEditor.GetEnumerator();
            while(en.MoveNext())
            {
                if(en.Current == gameObject)
                {
                    en.MoveNext();
                    return en.Current.GetComponent<ExposeToEditor>();
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
                        _ParentChanged(m_rte, this, oldParent, m_parent);
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
        private IRTE m_rte;
        public IRTE Editor
        {
            get { return m_rte; }
        }

        private void Awake()
        {
            m_rte = IOC.Resolve<IRTE>();

            m_rte.IsOpenedChanged += OnEditorIsOpenedChanged;

            m_objectType = ExposeToEditorObjectType.Undefined;

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
                    _Awaked(m_rte, this);
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

        private void OnEditorIsOpenedChanged()
        {
            if (m_rte.IsOpened)
            {
                TryToAddColliders();
            }
            else
            {
                TryToDestroyColliders();
            }
        }

        private void Start()
        {
            if (BoundsObject == null)
            {
                BoundsObject = gameObject;
            }

            m_effectiveBoundsType = BoundsType;
            m_filter = BoundsObject.GetComponent<MeshFilter>();
            m_skinned = BoundsObject.GetComponent<SkinnedMeshRenderer>();

            if(m_filter == null && m_skinned == null)
            {
                m_spriteRenderer = BoundsObject.GetComponent<SpriteRenderer>();
                //BoundsInWorldSpace = true;
            }
            
            if (m_rte.IsOpened)
            {
                TryToAddColliders();
            }
            else
            {
                TryToDestroyColliders();
                m_colliders = null; //do not move this inside of TryToDestroyColliders;
            }
        
            if (hideFlags != HideFlags.HideAndDontSave)
            {
                if (_Started != null)
                {
                    _Started(m_rte, this);
                }
            }
        }

        private void OnEnable()
        {
            if (hideFlags != HideFlags.HideAndDontSave)
            {
                if (_Enabled != null)
                {
                    _Enabled(m_rte, this);
                }
            }
        }

        private void OnDisable()
        {
            if (hideFlags != HideFlags.HideAndDontSave)
            {
                if (_Disabled != null)
                {
                    _Disabled(m_rte, this);
                }
            }
        }

     
        private void OnDestroy()
        {
            if(m_rte != null)
            {
                m_rte.IsOpenedChanged -= OnEditorIsOpenedChanged;
            }
            
            if (!m_applicationQuit)
            {
                if (hideFlags != HideFlags.HideAndDontSave)
                {
                    if (_Destroying != null)
                    {
                        _Destroying(m_rte, this);
                    }
                }

                if (m_parent != null)
                {
                    ChangeParent(null);
                }

                #if UNITY_EDITOR
               // if(m_saveInPlayMode == null)
                #endif
                {
                    TryToDestroyColliders();

                    if (m_hierarchyItem != null)
                    {
                        Destroy(m_hierarchyItem);
                    }
                }

                if (hideFlags != HideFlags.HideAndDontSave)
                {
                    if (_Destroyed != null)
                    {
                        _Destroyed(m_rte, this);
                    }
                }
            }
        }


        private void OnApplicationQuit()
        {
            m_applicationQuit = true;
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
                            _TransformChanged(m_rte, this);
                        }
                    }
                }
            }
        }

        private void TryToAddColliders()
        {
            if(this == null)
            {
                return;
            }

            if (m_colliders == null || m_colliders.Length == 0)
            {
                List<Collider> colliders = new List<Collider>();
                Rigidbody rigidBody = BoundsObject.GetComponent<Rigidbody>();

                bool isRigidBody = rigidBody != null;
                if (m_effectiveBoundsType == BoundsType.Any)
                {
                    if (m_filter != null)
                    {
                        if (AddColliders && !isRigidBody)
                        {
                            MeshCollider collider = BoundsObject.AddComponent<MeshCollider>();
                            collider.convex = isRigidBody;
                            collider.sharedMesh = m_filter.sharedMesh;
                            colliders.Add(collider);
                        }
                    }
                    else if (m_skinned != null)
                    {
                        if (AddColliders && !isRigidBody)
                        {
                            MeshCollider collider = BoundsObject.AddComponent<MeshCollider>();
                            collider.convex = isRigidBody;
                            collider.sharedMesh = m_skinned.sharedMesh;
                            colliders.Add(collider);
                        }
                    }
                    else if(m_spriteRenderer != null)
                    {
                        if(AddColliders && !isRigidBody)
                        {
                            BoxCollider collider = BoundsObject.AddComponent<BoxCollider>();
                            collider.size = m_spriteRenderer.sprite.bounds.size;
                            colliders.Add(collider);
                        }
                    }
                }
                else if (m_effectiveBoundsType == BoundsType.Mesh)
                {
                    if (m_filter != null)
                    {
                        if (AddColliders && !isRigidBody)
                        {
                            MeshCollider collider = BoundsObject.AddComponent<MeshCollider>();
                            collider.convex = isRigidBody;
                            collider.sharedMesh = m_filter.sharedMesh;
                            colliders.Add(collider);
                        }
                    }
                }
                else if (m_effectiveBoundsType == BoundsType.SkinnedMesh)
                {
                    if (m_skinned != null)
                    {
                        if (AddColliders && !isRigidBody)
                        {
                            MeshCollider collider = BoundsObject.AddComponent<MeshCollider>();
                            collider.convex = isRigidBody;
                            collider.sharedMesh = m_skinned.sharedMesh;
                            colliders.Add(collider);
                        }
                    }
                }
                else if(m_effectiveBoundsType == BoundsType.Sprite)
                {
                    if (m_spriteRenderer != null)
                    {
                        if (AddColliders && !isRigidBody)
                        {
                            BoxCollider collider = BoundsObject.AddComponent<BoxCollider>();
                            collider.size = m_spriteRenderer.sprite.bounds.size;
                            colliders.Add(collider);
                        }
                    }
                }
                else if (m_effectiveBoundsType == BoundsType.Custom)
                {
                    if (AddColliders && !isRigidBody)
                    {
                        Mesh box = RuntimeGraphics.CreateCubeMesh(Color.black, CustomBounds.center, CustomBounds.extents.x * 2, CustomBounds.extents.y * 2, CustomBounds.extents.z * 2);

                        MeshCollider collider = BoundsObject.AddComponent<MeshCollider>();
                        collider.convex = isRigidBody;

                        collider.sharedMesh = box;
                        colliders.Add(collider);
                    }
                }

                m_colliders = colliders.ToArray();
            }
        }

        private void TryToDestroyColliders()
        {
            if (m_colliders != null)
            {
                for (int i = 0; i < m_colliders.Length; ++i)
                {
                    Collider collider = m_colliders[i];
                    if (collider != null)
                    {
                        Destroy(collider);
                    }
                }
                m_colliders = null;
            }
        }

        public void SetName(string name)
        {
            gameObject.name = name;
            if (hideFlags != HideFlags.HideAndDontSave)
            {
                if (_NameChanged != null)
                {
                    _NameChanged(m_rte, this);
                }
            }
        }

        private static bool IsExposedToEditor(IRTE editor, GameObject go, ExposeToEditorObjectType type, bool roots)
        {
            ExposeToEditor exposeToEditor = go.GetComponent<ExposeToEditor>();
            return exposeToEditor != null && (!roots ||
                    exposeToEditor.transform.parent == null ||
                    exposeToEditor.transform.parent.GetComponentsInParent<ExposeToEditor>(true).Length == 0) &&
                !exposeToEditor.MarkAsDestroyed &&
                exposeToEditor.ObjectType == type &&
                exposeToEditor.Editor == editor &&
                exposeToEditor.hideFlags != HideFlags.HideAndDontSave;
        }

        public static IEnumerable<GameObject> FindAll(IRTE editor, ExposeToEditorObjectType type, bool roots = true)
        {
            if(SceneManager.GetActiveScene().isLoaded)
            {
                return FindAllUsingSceneManagement(editor, type, roots);
            }
            List<GameObject> filtered = new List<GameObject>();
            GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
            for (int i = 0; i < objects.Length; ++i)
            {
                GameObject obj = objects[i] as GameObject;
                if (obj == null)
                {
                    continue;
                }

                if (!obj.IsPrefab())
                {
                    filtered.Add(obj);
                }
            }

            return filtered.Where(f => IsExposedToEditor(editor, f, type, roots));
        }

        public static IEnumerable<GameObject> FindAllUsingSceneManagement(IRTE editor, ExposeToEditorObjectType type, bool roots = true)
        {
            List<GameObject> filtered = new List<GameObject>();
            GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < rootGameObjects.Length; ++i)
            {
                ExposeToEditor[] exposedObjects = rootGameObjects[i].GetComponentsInChildren<ExposeToEditor>(true);
                for (int j = 0; j < exposedObjects.Length; ++j)
                {
                    ExposeToEditor obj = exposedObjects[j];
                    if (IsExposedToEditor(editor, obj.gameObject, type, roots))
                    {
                        if (!obj.gameObject.IsPrefab())
                        {
                            filtered.Add(obj.gameObject);
                        }
                    }
                }
            }
            return filtered;
        }
    }
}

