using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityObject = UnityEngine.Object;
using System.IO;
using Battlehub.RTSaveLoad2.Internal;

namespace Battlehub.RTSaveLoad2
{
    public class PersistentTemplateInfo
    {
        public string Usings;
        public string Body;
        public HashSet<string> FieldNames;
        public string Path;
    }

    public class PersistentClassMapperGUI
    {
        private int m_uniqueId;
        private const string k_SessionStatePrefix = "PersistentClassMapperGUI";

        public static readonly HashSet<Type> HideMustHaveTypes = new HashSet<Type>
        {
            typeof(UnityObject),
            typeof(Transform),
            typeof(GameObject),
            typeof(Vector3),
            typeof(Quaternion)
        };

        public event Action<Type> TypeLocked;
        public event Action<Type> TypeUnlocked;

        private Type m_baseType;
        private Func<Type, string, bool> m_groupFilter;
        private string[] m_groupNames;
        private string m_groupLabel;
        private int m_selectedGroupIndex;
        private string m_filterText = string.Empty;
        private Vector2 m_scrollViewPosition;
        private Type[] m_types;
        private Dictionary<Type, int> m_typeToIndex;
        private string[] m_customImplementationOptions;

        private bool IsAllSelected
        {
            get { return m_mappings.Count(m => m.IsEnabled) == m_mappings.Length; }
        }

        private bool IsNoneSelected
        {
            get { return m_mappings.Count(m => m.IsEnabled) == 0; }
        }


        [Serializable]
        public class ClassMapperGUIState
        {
            public ClassMappingInfoState[] State;
            public string FilterText;
            public int GroupIndex;
        }
        [Serializable]
        public class ClassMappingInfoState
        {
            public bool IsExpanded;
            public bool IsPlatformsExpanded;
            public bool[] IsParentExpanded;
            public string TypeName;
        }

        //private int m_selectedCount;
        private int[] m_filteredTypeIndices;
        private class ClassMappingInfo
        {
            public bool IsAlwaysEnabled;
            public ObsoleteAttribute ObsoleteAttribute;
            public int PersistentPropertyTag;
            public int PersistentSubclassTag;

            private bool m_isEnabled;
            public bool IsEnabled
            {
                get { return m_isEnabled || IsAlwaysEnabled; }
                set { m_isEnabled = value; }
            }

            public bool IsExpanded;
            public bool[] IsParentExpanded;
            public int ExpandedCounter;
            public PersistentPropertyMapping[] PropertyMappings;
            public PersistentSubclass[] Subclasses;
            public bool[] IsPropertyMappingEnabled;
            
            public string[][] PropertyMappingNames; //per property
            public GUIContent[][] PropertyMappingsDisplayNames;
            public bool[][] PropertyIsObsolete;
            public Type[][] PropertyMappingTypes;
            public string[][] PropertyMappingTypeNames; //per property
            public string[][] PropertyMappingNamespaces;
            public string[][] PropertyMappingAssemblyNames;
            public int[] PropertyMappingSelection;

            public bool IsSupportedPlaftormsSectionExpanded;
            public HashSet<RuntimePlatform> UnsupportedPlatforms;

            public bool HasCustomImplementation;
            public bool CreateCustomImplementation;
            public bool HasTemplate;
            public bool UseTemplate;
        }


        private Dictionary<Type, PersistentTemplateInfo> m_templates;
        private Dictionary<Type, int> m_dependencyTypes;
        private ClassMappingInfo[] m_mappings;
        private string m_mappingStoragePath;
        private string m_mappingTemplateStoragePath;
        
        private Dictionary<string, UnityObject> m_typeToScriptObject;
        private CodeGen m_codeGen;

        private GUIStyle m_deprecatedPopupStyle;
        private GUIStyle m_deprecatedFoldoutStyle;

        public PersistentClassMapperGUI(
            int uniqueId,
            CodeGen codeGen,
            string mappingStorage,
            string mappingTemplateStorage,
            FilePathStorage filePathStorage,
            Type baseType, 
            Type[] types, 
            string[] groupNames,
            string groupLabel, 
            Func<Type, string, bool> groupFilter)
        {
            m_uniqueId = uniqueId;
            m_mappingStoragePath = mappingStorage;
            m_mappingTemplateStoragePath = mappingTemplateStorage;

            m_typeToScriptObject = new Dictionary<string, UnityObject>();
            if(filePathStorage != null)
            {
                FilePathRecord[] records = filePathStorage.PathRecords;
                if(records != null)
                {
                    for(int i = 0; i < records.Length; ++i)
                    {
                        FilePathRecord record = records[i];
                        if(!m_typeToScriptObject.ContainsKey(record.PeristentTypeName))
                        {
                            m_typeToScriptObject.Add(record.PeristentTypeName, record.File);
                        }
                    }
                }
            }

            m_codeGen = codeGen;
            m_baseType = baseType;
            m_types = types;
            m_groupNames = groupNames;
            m_groupLabel = groupLabel;
            m_groupFilter = groupFilter;
            m_templates = new Dictionary<Type, PersistentTemplateInfo>();

            Dictionary<string, Type> typeNameToType = m_types.ToDictionary(t => t.FullName);
            Type[] templateTypes = Assembly.GetAssembly(GetType()).GetTypes().Where(t => t.GetCustomAttributes(typeof(PersistentTemplateAttribute), false).Length != 0).ToArray();
            for(int i = 0; i < templateTypes.Length; ++i)
            {
                Type templateType = templateTypes[i];
                PersistentTemplateAttribute templateAttrib = (PersistentTemplateAttribute)templateType.GetCustomAttributes(typeof(PersistentTemplateAttribute), false)[0];

                Type mappedType;
                if(typeNameToType.TryGetValue(templateAttrib.ForType, out mappedType))
                {
                    MonoScript monoScript = MonoScript.FromScriptableObject(ScriptableObject.CreateInstance(templateType));
                    string contents = monoScript.text;
                    string usings = contents.ToString();
                    if(m_codeGen.TryGetTemplateBody(contents, out contents) && m_codeGen.TryGetTemplateUsings(usings, out usings))
                    {
                        PersistentTemplateInfo templateInfo = new PersistentTemplateInfo
                        {
                            Body = contents,
                            FieldNames = new HashSet<string>(),
                            Usings = usings,
                            Path = AssetDatabase.GetAssetPath(monoScript),
                        };

                        if (templateAttrib.FieldNames != null)
                        {
                            for (int n = 0; n < templateAttrib.FieldNames.Length; ++n)
                            {
                                string fieldName = templateAttrib.FieldNames[n];
                                if(!templateInfo.FieldNames.Contains(fieldName))
                                {
                                    templateInfo.FieldNames.Add(fieldName);
                                }
                            }
                        }
   
                        if (m_templates.ContainsKey(mappedType))
                        {
                            Debug.LogWarning("m_templates dictionary already contains " + mappedType.FullName);
                        }
                        else
                        {
                            m_templates.Add(mappedType, templateInfo);
                        }
                    }
                    else
                    {
                        string path = AssetDatabase.GetAssetPath(monoScript);
                        Debug.LogWarningFormat("Template {0} has invalid format", path);
                    }
                } 
            }
            
            m_deprecatedPopupStyle = new GUIStyle(EditorStyles.popup);
            m_deprecatedPopupStyle.normal.textColor = Color.red;
            m_deprecatedPopupStyle.focused.textColor = Color.red;
            m_deprecatedFoldoutStyle = new GUIStyle(EditorStyles.foldout);
            m_deprecatedFoldoutStyle.normal.textColor = Color.red;
            m_deprecatedFoldoutStyle.focused.textColor = Color.red;

            m_customImplementationOptions = new[] { "Create new", "Create from template" };
        }

        public void OnGUI()
        {
            if (m_mappings == null)
            {
                Initialize();
                LoadMappings();
            }

            EditorGUILayout.Separator();
            EditorGUI.BeginChangeCheck();

            m_selectedGroupIndex = EditorGUILayout.Popup(m_groupLabel, m_selectedGroupIndex, m_groupNames);
            m_filterText = EditorGUILayout.TextField("Type Filter:", m_filterText);

            if (EditorGUI.EndChangeCheck())
            {
                ApplyFilter();
            }

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();
            //EditorGUI.BeginDisabledGroup(m_enableAll);
            if (IsAllSelected)
            {
                EditorGUILayout.Toggle(true, GUILayout.MaxWidth(20));
                EditorGUILayout.LabelField("Select All Types", GUILayout.MaxWidth(230));
            }
            else if (IsNoneSelected)
            {
                EditorGUILayout.Toggle(false,  GUILayout.MaxWidth(20));
                EditorGUILayout.LabelField("Select All Types", GUILayout.MaxWidth(230));
                
            }
            else
            {
                EditorGUILayout.Toggle(false, "ToggleMixed", GUILayout.MaxWidth(20));
                EditorGUILayout.LabelField("Select All Types", GUILayout.MaxWidth(230));
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (IsAllSelected)
                {
                    UnselectAll();
                }
                else
                {
                    SelectAll();
                }
            }
            //EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator();
            
            m_scrollViewPosition = EditorGUILayout.BeginScrollView(m_scrollViewPosition);

            EditorGUILayout.BeginVertical();
            {
                for (int i = 0; i < m_filteredTypeIndices.Length; ++i)
                {
                    int typeIndex = m_filteredTypeIndices[i];
                    DrawTypeEditor(typeIndex, typeIndex);
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();  
        }

        private void ApplyFilter()
        {
            List<int> filteredTypeIndices = new List<int>();
            for (int i = 0; i < m_types.Length; ++i)
            {
                Type type = m_types[i];
                if (m_codeGen.TypeName(type).ToLower().Contains(m_filterText.ToLower()) && (m_selectedGroupIndex == 0 || m_selectedGroupIndex >= 0 && m_selectedGroupIndex < m_groupNames.Length && m_groupFilter(type, m_groupNames[m_selectedGroupIndex])))
                {
                    filteredTypeIndices.Add(i);
                }
            }
            m_filteredTypeIndices = filteredTypeIndices.ToArray();
        }

        public void OnDisable()
        {
            if(m_mappings == null)
            {
                return;
            }
            ClassMappingInfoState[] sessionState = new ClassMappingInfoState[m_mappings.Length];
            for(int i = 0; i < m_mappings.Length; ++i)
            {
                ClassMappingInfo mapping = m_mappings[i];
                if(mapping != null)
                {
                    ClassMappingInfoState state = new ClassMappingInfoState();
                    state.IsExpanded = mapping.IsExpanded;
                    state.IsPlatformsExpanded = mapping.IsSupportedPlaftormsSectionExpanded;
                    state.IsParentExpanded = mapping.IsParentExpanded;
                    state.TypeName = m_types[i].AssemblyQualifiedName;
                    sessionState[i] = state;
                }
            }

            string jsonState = JsonUtility.ToJson(
                new ClassMapperGUIState
                {
                    State = sessionState,
                    FilterText = m_filterText,
                    GroupIndex = m_selectedGroupIndex
                });
            SessionState.SetString(k_SessionStatePrefix + m_uniqueId, jsonState);
        }

        private void Initialize()
        {
            m_mappings = new ClassMappingInfo[m_types.Length];
            for (int i = 0; i < m_types.Length; ++i)
            {
                m_mappings[i] = new ClassMappingInfo();
                m_mappings[i].ObsoleteAttribute = m_types[i].GetCustomAttributes(false).OfType<ObsoleteAttribute>().FirstOrDefault();
                m_mappings[i].HasTemplate = m_templates.ContainsKey(m_types[i]);
                m_mappings[i].UseTemplate = true;

                Type persistentType = m_codeGen.GetPersistentType(PersistentClassMapping.ToPersistentFullName(m_types[i].Namespace, m_types[i].Name));
                m_mappings[i].HasCustomImplementation = persistentType != null && persistentType.GetCustomAttributes(typeof(CustomImplementationAttribute), false).Length > 0;
            }

            m_dependencyTypes = new Dictionary<Type, int>();
            m_typeToIndex = new Dictionary<Type, int>();
            m_filteredTypeIndices = new int[m_types.Length];
            for (int i = 0; i < m_filteredTypeIndices.Length; ++i)
            {
                m_filteredTypeIndices[i] = i;
                m_typeToIndex.Add(m_types[i], i);
                m_mappings[i].IsParentExpanded = new bool[GetAncestorsCount(m_types[i])];
            }


            var jsonState = SessionState.GetString(k_SessionStatePrefix + m_uniqueId, "");
            if (!string.IsNullOrEmpty(jsonState))
            {
                ClassMapperGUIState guiState = JsonUtility.FromJson<ClassMapperGUIState>(jsonState);
                if(guiState.State != null)
                {
                    Dictionary<string, ClassMappingInfoState> typeToState = guiState.State.ToDictionary(s => s.TypeName);
                    for (int i = 0; i < m_types.Length; ++i)
                    {
                        string type = m_types[i].AssemblyQualifiedName;
                        ClassMappingInfoState state;
                        if (typeToState.TryGetValue(type, out state))
                        {
                            ClassMappingInfo mapping = m_mappings[i];
                            mapping.IsExpanded = state.IsExpanded;
                            ExpandType(i);
                            mapping.IsSupportedPlaftormsSectionExpanded = state.IsPlatformsExpanded;
                            mapping.IsParentExpanded = state.IsParentExpanded;
                        }
                    }
                }
                m_filterText = guiState.FilterText;
                m_selectedGroupIndex = guiState.GroupIndex;
                ApplyFilter();
            }
        }

        private void SelectAll()
        {
            for (int i = 0; i < m_mappings.Length; ++i)
            {
                m_mappings[i].IsEnabled = true;
                TryExpandType(i);
            }
            //m_selectedCount = m_mappings.Length;
        }

        private void UnselectAll()
        {
            for (int i = 0; i < m_mappings.Length; ++i)
            {
                m_mappings[i].IsEnabled = false;
                TryExpandType(i);
            }
            //m_selectedCount = 0;
        }

        private bool m_tryingToLockType = false;
        public void TryLockType(Type mappedType)
        {
            if(m_tryingToLockType)
            {
                return;
            }
            if (m_mappings == null)
            {
                Initialize();
                LoadMappings();
            }

            if(mappedType.IsArray)
            {
                mappedType = mappedType.GetElementType();
            }

            if (!m_dependencyTypes.ContainsKey(mappedType))
            {
                m_dependencyTypes.Add(mappedType, 0);
                LockType(mappedType);

                if(m_typeToIndex.ContainsKey(mappedType))
                {
                    ExpandType(m_typeToIndex[mappedType]);
                }

                m_tryingToLockType = true;
                if (TypeLocked != null)
                {
                    TypeLocked(mappedType);
                }
                m_tryingToLockType = false;
            }
            m_dependencyTypes[mappedType]++;
        }

        private bool m_tryingToUnlockType = false;
        public void TryUnlockType(Type mappedType)
        {
            if(m_tryingToUnlockType)
            {
                return;
            }
            if (m_mappings == null)
            {
                Initialize();
                LoadMappings();
            }

            if (mappedType.IsArray)
            {
                mappedType = mappedType.GetElementType();
            }


            if (m_dependencyTypes.ContainsKey(mappedType))
            {
                m_dependencyTypes[mappedType]--;
                if (m_dependencyTypes[mappedType] <= 0)
                {
                    UnlockType(mappedType);
                    m_dependencyTypes.Remove(mappedType);
                    m_tryingToUnlockType = true;
                    if (TypeUnlocked != null)
                    {
                        TypeUnlocked(mappedType);
                    }
                    m_tryingToUnlockType = false;
                }
            }
        }

        private void LockType(Type type)
        {
            int index;
            if (m_typeToIndex.TryGetValue(type, out index))
            {
                m_mappings[index].IsAlwaysEnabled = true;
                m_mappings[index].IsEnabled = true;
            }
        }

        private void UnlockType(Type type)
        {
            int index;
            if (m_typeToIndex.TryGetValue(type, out index))
            {
                m_mappings[index].IsAlwaysEnabled = false;
            }
        }

        public void Reset()
        {
            for (int typeIndex = 0; typeIndex < m_mappings.Length; ++typeIndex)
            {
                UnselectAllProperties(typeIndex);
            }
            UnselectAll();
            LoadMappings();
        }

        public void ClearDependencies()
        {
            foreach (Type type in m_dependencyTypes.Keys)
            {
                UnlockType(type);
            }
            m_dependencyTypes.Clear();
        }

        private void LoadMappings()
        {
            PersistentClassMapping[] mappings = GetMappings();
            for (int i = 0; i < mappings.Length; ++i)
            {
                PersistentClassMapping classMapping = mappings[i];
                Type type = Type.GetType(classMapping.MappedAssemblyQualifiedName);
                int typeIndex;
                if (type != null && m_typeToIndex.TryGetValue(type, out typeIndex))
                {
                    PersistentPropertyMapping[] pMappings = classMapping.PropertyMappings;
                    PersistentSubclass[] subclasses = classMapping.Subclasses;
                    m_mappings[typeIndex].PropertyMappings = pMappings;
                    m_mappings[typeIndex].Subclasses = subclasses;
                    m_mappings[typeIndex].IsEnabled = classMapping.IsEnabled;
                    m_mappings[typeIndex].PersistentPropertyTag = classMapping.PersistentPropertyTag;
                    m_mappings[typeIndex].PersistentSubclassTag = classMapping.PersistentSubclassTag;
                    m_mappings[typeIndex].CreateCustomImplementation = classMapping.CreateCustomImplementation;
                    m_mappings[typeIndex].UseTemplate = classMapping.UseTemplate;
                    
                    ExpandType(typeIndex);

                    for (int propIndex = 0; propIndex < m_mappings[typeIndex].PropertyMappings.Length; ++propIndex)
                    {
                        if(m_mappings[typeIndex].IsPropertyMappingEnabled[propIndex])
                        {
                            int selection = m_mappings[typeIndex].PropertyMappingSelection[propIndex];
                            if(selection > -1)
                            {
                                Type mappedType = m_mappings[typeIndex].PropertyMappingTypes[propIndex][selection];
                                TryLockType(mappedType);
                            }
                        }
                    }
                }
            }

            ExpandType(0);
        }

        public Dictionary<Type, PersistentTemplateInfo> GetTemplates()
        {
            return m_templates;
        }

        public PersistentClassMapping[] GetMappings()
        {
            GameObject storageGO = (GameObject)AssetDatabase.LoadAssetAtPath(m_mappingStoragePath, typeof(GameObject));
            if (storageGO == null)
            {
                storageGO = (GameObject)AssetDatabase.LoadAssetAtPath(m_mappingTemplateStoragePath, typeof(GameObject));
            }

            if (storageGO != null)
            {
                PersistentClassMapping[] mappings = storageGO.GetComponentsInChildren<PersistentClassMapping>(true);
                return mappings;
            }
            return new PersistentClassMapping[0];
        }

        public void SaveMappings()
        {
            GameObject storageGO = (GameObject)AssetDatabase.LoadAssetAtPath(m_mappingStoragePath, typeof(GameObject));
            if (storageGO == null)
            {
                storageGO = (GameObject)AssetDatabase.LoadAssetAtPath(m_mappingTemplateStoragePath, typeof(GameObject));
            }

            Dictionary<string, PersistentClassMapping> existingMappings;
            if (storageGO != null)
            {
                storageGO = UnityObject.Instantiate(storageGO);
                existingMappings = storageGO.GetComponentsInChildren<PersistentClassMapping>(true).ToDictionary(m => m.name);
            }
            else
            {
                storageGO = new GameObject();
                existingMappings = new Dictionary<string, PersistentClassMapping>();
            }

            Dictionary<int, Dictionary<string, PersistentSubclass>> typeIndexToSubclasses = new Dictionary<int, Dictionary<string, PersistentSubclass>>();
            for (int typeIndex = 0; typeIndex < m_mappings.Length; ++typeIndex)
            {
                ClassMappingInfo mapping = m_mappings[typeIndex];
                Dictionary<string, PersistentSubclass> subclassDictionary;
                if (mapping.Subclasses == null)
                {
                    subclassDictionary = new Dictionary<string, PersistentSubclass>();
                }
                else
                {
                    for (int i = 0; i < mapping.Subclasses.Length; ++i)
                    {
                        PersistentSubclass subclass = mapping.Subclasses[i];
                        subclass.IsEnabled = false;
                    }

                    subclassDictionary = mapping.Subclasses.ToDictionary(s => s.FullTypeName);
                }

                typeIndexToSubclasses.Add(typeIndex, subclassDictionary);
            }

            for (int typeIndex = 0; typeIndex < m_mappings.Length; ++typeIndex)
            {
                ClassMappingInfo mapping = m_mappings[typeIndex];
                Type type = m_types[typeIndex];
                if(HideMustHaveTypes.Contains(type))
                {
                    mapping.IsEnabled = true;
                }

                if (!mapping.IsEnabled)
                {
                    continue;
                }

                Type baseType = GetEnabledBaseType(typeIndex);
                if (baseType == null)
                {
                    continue;
                }

                int baseTypeIndex;
                if (m_typeToIndex.TryGetValue(baseType, out baseTypeIndex))
                {
                    ClassMappingInfo baseClassMapping = m_mappings[baseTypeIndex];
                    string ns = PersistentClassMapping.ToPersistentNamespace(m_types[typeIndex].Namespace);
                    string typeName = PersistentClassMapping.ToPersistentName(m_types[typeIndex].Name);
                    string fullTypeName = string.Format("{0}.{1}", ns, typeName);

                    Dictionary<string, PersistentSubclass> subclassDictionary = typeIndexToSubclasses[baseTypeIndex];
                    if (!subclassDictionary.ContainsKey(fullTypeName))
                    {
                        PersistentSubclass subclass = new PersistentSubclass();
                        subclass.IsEnabled = true;
                        subclass.Namespace = PersistentClassMapping.ToPersistentNamespace(type.Namespace);
                        subclass.TypeName = PersistentClassMapping.ToPersistentName(m_codeGen.TypeName(type));
                        baseClassMapping.PersistentSubclassTag++;
                        subclass.PersistentTag = baseClassMapping.PersistentSubclassTag;

                        subclassDictionary.Add(fullTypeName, subclass);
                    }
                    else
                    {
                        PersistentSubclass subclass = subclassDictionary[fullTypeName];
                        subclass.IsEnabled = true;
                    }
                }
            }

            PersistentClassMapping[] savedMappings = new PersistentClassMapping[m_mappings.Length];
            for (int typeIndex = 0; typeIndex < m_mappings.Length; ++typeIndex)
            {
                if(m_types[typeIndex].BaseType == null)
                {
                    continue;
                }

                PersistentClassMapping classMapping;
                if (!existingMappings.TryGetValue(m_types[typeIndex].FullName, out classMapping))
                {
                    GameObject typeStorageGO = new GameObject();
                    typeStorageGO.transform.SetParent(storageGO.transform, false);
                
                    typeStorageGO.name = m_types[typeIndex].FullName;
                    classMapping = typeStorageGO.AddComponent<PersistentClassMapping>();
                    classMapping.PersistentTypeGUID = Guid.NewGuid().ToString();
                    classMapping.MappedTypeGUID = Guid.NewGuid().ToString();
                }

                savedMappings[typeIndex] = classMapping;

                PersistentTemplateInfo templateInfo;
                if (!classMapping.CreateCustomImplementation || !classMapping.UseTemplate || !m_templates.TryGetValue(m_types[typeIndex], out templateInfo))
                {
                    templateInfo = null;
                }


                List<PersistentPropertyMapping> selectedPropertyMappings = new List<PersistentPropertyMapping>();
                PersistentPropertyMapping[] propertyMappings = m_mappings[typeIndex].PropertyMappings;
                if(propertyMappings != null)
                {
                    int[] propertyMappingsSelection = m_mappings[typeIndex].PropertyMappingSelection;
                    for (int propIndex = 0; propIndex < propertyMappings.Length; ++propIndex)
                    {
                        PersistentPropertyMapping propertyMapping = propertyMappings[propIndex];
                        propertyMapping.IsEnabled = m_mappings[typeIndex].IsPropertyMappingEnabled[propIndex];

                        bool hasPropertyInTemplate = templateInfo != null && templateInfo.FieldNames.Contains(propertyMapping.PersistentName);
                        if (propertyMappingsSelection[propIndex] >= 0 && !hasPropertyInTemplate)
                        {
                            propertyMapping.MappedName = m_mappings[typeIndex].PropertyMappingNames[propIndex][propertyMappingsSelection[propIndex]];
                            propertyMapping.MappedTypeName = m_mappings[typeIndex].PropertyMappingTypeNames[propIndex][propertyMappingsSelection[propIndex]];
                            propertyMapping.MappedNamespace = m_mappings[typeIndex].PropertyMappingNamespaces[propIndex][propertyMappingsSelection[propIndex]];
                            propertyMapping.MappedAssemblyName = m_mappings[typeIndex].PropertyMappingAssemblyNames[propIndex][propertyMappingsSelection[propIndex]];
                            if (propertyMapping.PersistentTag == 0)
                            {
                                m_mappings[typeIndex].PersistentPropertyTag++;
                                propertyMapping.PersistentTag = m_mappings[typeIndex].PersistentPropertyTag;
                            }

                            selectedPropertyMappings.Add(propertyMapping);
                        }
                    }
                }
               

                m_mappings[typeIndex].PropertyMappings = selectedPropertyMappings.ToArray();
                ExpandType(typeIndex);

                classMapping.IsEnabled = m_mappings[typeIndex].IsEnabled;
                classMapping.PersistentPropertyTag = m_mappings[typeIndex].PersistentPropertyTag;
                classMapping.PersistentSubclassTag = m_mappings[typeIndex].PersistentSubclassTag;
                classMapping.PropertyMappings = selectedPropertyMappings.ToArray();
                if (typeIndexToSubclasses.ContainsKey(typeIndex))
                {
                    classMapping.Subclasses = typeIndexToSubclasses[typeIndex].Values.ToArray();
                }
                classMapping.MappedAssemblyName = m_types[typeIndex].Assembly.FullName.Split(',')[0];
                classMapping.MappedNamespace = m_types[typeIndex].Namespace;
                classMapping.MappedTypeName = m_codeGen.TypeName(m_types[typeIndex]);
                    
                classMapping.PersistentNamespace = PersistentClassMapping.ToPersistentNamespace(classMapping.MappedNamespace);
                classMapping.PersistentTypeName = PersistentClassMapping.ToPersistentName(m_codeGen.TypeName(m_types[typeIndex]));

                Type baseType = GetEnabledBaseType(typeIndex);
                if (baseType == null || baseType == typeof(object))
                {
                    classMapping.PersistentBaseNamespace = typeof(PersistentSurrogate).Namespace;
                    classMapping.PersistentBaseTypeName = typeof(PersistentSurrogate).Name;
                }
                else
                {
                    classMapping.PersistentBaseNamespace = PersistentClassMapping.ToPersistentNamespace(baseType.Namespace);
                    classMapping.PersistentBaseTypeName = PersistentClassMapping.ToPersistentName(m_codeGen.TypeName(baseType));
                }

                classMapping.CreateCustomImplementation = m_mappings[typeIndex].CreateCustomImplementation;
                classMapping.UseTemplate = m_mappings[typeIndex].UseTemplate;
            }

            EditorUtility.SetDirty(storageGO);
            PrefabUtility.CreatePrefab(m_mappingStoragePath, storageGO);
            UnityObject.DestroyImmediate(storageGO);
        }

        private Type GetEnabledBaseType(int typeIndex)
        {
            Type baseType = null;
            Type type = m_types[typeIndex];
            while (true)
            {
                type = type.BaseType;
                if (type == m_baseType)
                {
                    baseType = type;
                    break;
                }

                if (type == null)
                {
                    break;
                }

                int baseIndex;
                if (m_typeToIndex.TryGetValue(type, out baseIndex))
                {
                    if (m_mappings[baseIndex].IsEnabled)
                    {
                        baseType = type;
                        break;
                    }
                }
            }

            return baseType;
        }

        private int GetAncestorsCount(Type type)
        {
            int count = 0;
            while (type != null && type.BaseType != m_baseType)
            {
                count++;
                type = type.BaseType;
            }
            return count;
        }

        private GUIContent m_guiContent = new GUIContent();
        private void DrawTypeEditor(int rootTypeIndex, int typeIndex, int indent = 1)
        {
            Type type = m_types[typeIndex];
            if (type == m_baseType || HideMustHaveTypes.Contains(type))
            {
                return;
            }


            GUIStyle foldOutStyle;
            if(m_mappings[typeIndex].ObsoleteAttribute != null)
            {
                foldOutStyle = m_deprecatedFoldoutStyle;
                m_guiContent.tooltip = m_mappings[typeIndex].ObsoleteAttribute.Message;
                m_guiContent.text = type.Name + " [Deprecated]";
            }
            else
            {
                foldOutStyle = EditorStyles.foldout;
                m_guiContent.tooltip = null;
                m_guiContent.text = type.Name;
            }

            
            bool isExpandedChanged;
            bool isExpanded;
            bool isSelectionChanged;

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            {
                GUILayout.Space(5 + 18 * (indent - 1));
                EditorGUI.BeginChangeCheck();

                EditorGUI.BeginDisabledGroup(m_mappings[typeIndex].IsAlwaysEnabled);
                m_mappings[typeIndex].IsEnabled = EditorGUILayout.Toggle(m_mappings[typeIndex].IsEnabled, GUILayout.MaxWidth(15));
                EditorGUI.EndDisabledGroup();

                isSelectionChanged = EditorGUI.EndChangeCheck();

                EditorGUI.BeginChangeCheck();
                if (indent == 1)
                {
                    m_mappings[typeIndex].IsExpanded = EditorGUILayout.Foldout(m_mappings[typeIndex].IsExpanded, m_guiContent, true, foldOutStyle);
                    isExpanded = m_mappings[typeIndex].IsExpanded;
                }
                else
                {
                    m_mappings[rootTypeIndex].IsParentExpanded[indent - 2] = EditorGUILayout.Foldout(m_mappings[rootTypeIndex].IsParentExpanded[indent - 2], m_guiContent, true, foldOutStyle);
                    isExpanded = m_mappings[rootTypeIndex].IsParentExpanded[indent - 2];
                }
                isExpandedChanged = EditorGUI.EndChangeCheck();
            }
            EditorGUILayout.EndHorizontal();
       

            if (isExpandedChanged || isSelectionChanged)
            {
                if (isExpandedChanged)
                {
                    m_mappings[typeIndex].ExpandedCounter = isExpanded ?
                        m_mappings[typeIndex].ExpandedCounter + 1 :
                        m_mappings[typeIndex].ExpandedCounter - 1;
                }

                if (isSelectionChanged)
                {
                    if (m_mappings[typeIndex].IsEnabled)
                    {
                        //m_selectedCount++;
                    }
                    else
                    {
                        //m_selectedCount--;
                    }
                }

                TryExpandType(typeIndex);
            }

            if (isExpanded)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(5 + 18 * indent);
                    EditorGUILayout.BeginVertical();
                    m_mappings[typeIndex].IsSupportedPlaftormsSectionExpanded = EditorGUILayout.Foldout(m_mappings[typeIndex].IsSupportedPlaftormsSectionExpanded, "Supported Platforms");
                    if (m_mappings[typeIndex].IsSupportedPlaftormsSectionExpanded)
                    {
                        string[] platformNames = Enum.GetNames(typeof(RuntimePlatform));
                        RuntimePlatform[] platforms = (RuntimePlatform[])Enum.GetValues(typeof(RuntimePlatform));

                        for (int i = 0; i < platformNames.Length; ++i)
                        {
                            EditorGUI.BeginChangeCheck();
                            bool platformChecked = EditorGUILayout.Toggle(platformNames[i], m_mappings[typeIndex].UnsupportedPlatforms == null || !m_mappings[typeIndex].UnsupportedPlatforms.Contains(platforms[i]));
                            if (EditorGUI.EndChangeCheck())
                            {
                                if (m_mappings[typeIndex].UnsupportedPlatforms == null)
                                {
                                    m_mappings[typeIndex].UnsupportedPlatforms = new HashSet<RuntimePlatform>();
                                }
                                if (platformChecked)
                                {
                                    m_mappings[typeIndex].UnsupportedPlatforms.Remove(platforms[i]);
                                }
                                else
                                {
                                    m_mappings[typeIndex].UnsupportedPlatforms.Add(platforms[i]);
                                }
                            }

                        }
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginVertical();
                {
                    ClassMappingInfo mapping = m_mappings[typeIndex];
                    if (!mapping.HasCustomImplementation)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Space(5 + 18 * indent);

                            mapping.CreateCustomImplementation = EditorGUILayout.Toggle(mapping.CreateCustomImplementation, GUILayout.MaxWidth(20));
                            EditorGUILayout.LabelField("Custom Implementation", GUILayout.MaxWidth(230));

                            if (mapping.CreateCustomImplementation && mapping.HasTemplate)
                            {
                                mapping.UseTemplate = EditorGUILayout.Popup(mapping.UseTemplate ? 1 : 0, m_customImplementationOptions, GUILayout.MaxWidth(230)) == 1;
                            }

                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Space(5 + 18 * indent);
                            
                            if (GUILayout.Button("Edit Custom Implementation", GUILayout.Width(215)))
                            {
                                Type t = m_types[typeIndex];
                                string fullTypeName = PersistentClassMapping.ToPersistentFullName(t.Namespace, t.Name);
                                UnityObject scriptFile;
                                if(m_typeToScriptObject.TryGetValue(fullTypeName, out scriptFile))
                                {
                                    AssetDatabase.OpenAsset(scriptFile);
                                }
                                else
                                {
                                    if(EditorApplication.isCompiling)
                                    {
                                        EditorUtility.DisplayDialog("Unable to open file for editing", "Try again when script compilation will be finished", "OK");
                                    }
                                    else
                                    {
                                        EditorUtility.DisplayDialog("Unable to open file for editing", "Open file manually using project window", "OK");
                                    }
                                }
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
             
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(5 + 18 * indent);

                        int selectedPropertiesCount = m_mappings[typeIndex].IsPropertyMappingEnabled.Count(enabled => enabled);
                        bool isAllPropertiesSelected = selectedPropertiesCount == m_mappings[typeIndex].IsPropertyMappingEnabled.Length;
                        if (isAllPropertiesSelected)
                        {
                            EditorGUILayout.Toggle(true, GUILayout.MaxWidth(20));
                            EditorGUILayout.LabelField("Select All Properties", GUILayout.MaxWidth(230));
                        }
                        else if (selectedPropertiesCount == 0)
                        {
                            EditorGUILayout.Toggle(false, GUILayout.MaxWidth(20));
                            EditorGUILayout.LabelField("Select All Properties", GUILayout.MaxWidth(230));
                        }
                        else
                        {
                            EditorGUILayout.Toggle(false, "ToggleMixed", GUILayout.MaxWidth(20));
                            EditorGUILayout.LabelField("Select All Properties", GUILayout.MaxWidth(230));
                        }

                        if (EditorGUI.EndChangeCheck())
                        {
                            if (isAllPropertiesSelected)
                            {
                                UnselectAllProperties(typeIndex);
                            }
                            else
                            {
                                SelectAllProperties(typeIndex);
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    PersistentTemplateInfo templateInfo;
                    if (!m_mappings[typeIndex].CreateCustomImplementation || !m_mappings[typeIndex].UseTemplate || !m_templates.TryGetValue(m_types[typeIndex], out templateInfo))
                    {
                        templateInfo = null; 
                    }

                    for (int propIndex = 0; propIndex < m_mappings[typeIndex].PropertyMappings.Length; ++propIndex)
                    {
                        PersistentPropertyMapping pMapping = m_mappings[typeIndex].PropertyMappings[propIndex];
                        bool hasPropertyInTemplate = templateInfo != null && templateInfo.FieldNames.Contains(pMapping.PersistentName);
                        if (hasPropertyInTemplate)
                        {
                            continue;
                        }

                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Space(5 + 18 * indent);

                            EditorGUI.BeginChangeCheck();
                            m_mappings[typeIndex].IsPropertyMappingEnabled[propIndex] = EditorGUILayout.Toggle(m_mappings[typeIndex].IsPropertyMappingEnabled[propIndex], GUILayout.MaxWidth(20));
                            if(EditorGUI.EndChangeCheck())
                            {
                                if (m_mappings[typeIndex].IsPropertyMappingEnabled[propIndex])
                                {
                                    int propSelection = m_mappings[typeIndex].PropertyMappingSelection[propIndex];
                                    if(propSelection >= 0)
                                    {
                                        Type mappedType = m_mappings[typeIndex].PropertyMappingTypes[propIndex][propSelection];
                                        TryLockType(mappedType);
                                    }
                                }
                                else
                                {
                                    int propSelection = m_mappings[typeIndex].PropertyMappingSelection[propIndex];
                                    if (propSelection >= 0)
                                    {
                                        Type mappedType = m_mappings[typeIndex].PropertyMappingTypes[propIndex][propSelection];
                                        TryUnlockType(mappedType);
                                    }
                                }
                            }

                            m_guiContent.text = pMapping.PersistentName;
                            m_guiContent.tooltip = pMapping.PersistentTypeName;
                            EditorGUILayout.LabelField(m_guiContent, GUILayout.MaxWidth(230));

                            int selectedIndex = m_mappings[typeIndex].PropertyMappingSelection[propIndex];
                            
                            int newPropertyIndex = selectedIndex >= 0 && m_mappings[typeIndex].PropertyIsObsolete[propIndex][selectedIndex] ?
                                EditorGUILayout.Popup(selectedIndex, m_mappings[typeIndex].PropertyMappingsDisplayNames[propIndex], m_deprecatedPopupStyle) :
                                EditorGUILayout.Popup(selectedIndex, m_mappings[typeIndex].PropertyMappingsDisplayNames[propIndex]);
                            m_mappings[typeIndex].PropertyMappingSelection[propIndex] = newPropertyIndex;

                            if(selectedIndex != newPropertyIndex)
                            {
                                if(selectedIndex >= 0)
                                {
                                    Type mappedType = m_mappings[typeIndex].PropertyMappingTypes[propIndex][selectedIndex];
                                    TryUnlockType(mappedType);
                                }

                                if(newPropertyIndex >= 0)
                                {
                                    Type mappedType = m_mappings[typeIndex].PropertyMappingTypes[propIndex][newPropertyIndex];
                                    TryLockType(mappedType);
                                }
                            }

                            EditorGUI.BeginChangeCheck();
                            GUILayout.Button("X", GUILayout.Width(20));
                            if (EditorGUI.EndChangeCheck())
                            {
                                if (newPropertyIndex >= 0)
                                {
                                    Type mappedType = m_mappings[typeIndex].PropertyMappingTypes[propIndex][newPropertyIndex];
                                    TryUnlockType(mappedType);
                                }
                                m_mappings[typeIndex].PropertyMappingSelection[propIndex] = -1;
                            }

                            EditorGUILayout.LabelField("Slot: " + pMapping.PersistentTag, GUILayout.Width(60));
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    if (type.BaseType != m_baseType)
                    {
                        int parentIndex;
                        if (m_typeToIndex.TryGetValue(type.BaseType, out parentIndex))
                        {
                            DrawTypeEditor(rootTypeIndex, parentIndex, indent + 1);
                        }
                    }
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.Separator();
            }
        }

        private void SelectAllProperties(int typeIndex)
        {
            if (m_mappings[typeIndex].IsPropertyMappingEnabled == null)
            {
                return;
            }
            for (int propIndex = 0; propIndex < m_mappings[typeIndex].IsPropertyMappingEnabled.Length; ++propIndex)
            {
                if (!m_mappings[typeIndex].IsPropertyMappingEnabled[propIndex])
                {
                    m_mappings[typeIndex].IsPropertyMappingEnabled[propIndex] = true;

                    int selectedIndex = m_mappings[typeIndex].PropertyMappingSelection[propIndex];
                    if(selectedIndex >= 0)
                    {
                        Type mappedType = m_mappings[typeIndex].PropertyMappingTypes[propIndex][selectedIndex];
                        TryLockType(mappedType);
                    }
                }
            }
        }

        private void UnselectAllProperties(int typeIndex)
        {
            if(m_mappings[typeIndex].IsPropertyMappingEnabled == null)
            {
                return;
            }
            for (int propIndex = 0; propIndex < m_mappings[typeIndex].IsPropertyMappingEnabled.Length; ++propIndex)
            {
                if (m_mappings[typeIndex].IsPropertyMappingEnabled[propIndex])
                {
                    m_mappings[typeIndex].IsPropertyMappingEnabled[propIndex] = false;

                    int selectedIndex = m_mappings[typeIndex].PropertyMappingSelection[propIndex];
                    if (selectedIndex >= 0)
                    {
                        Type mappedType = m_mappings[typeIndex].PropertyMappingTypes[propIndex][selectedIndex];
                        TryUnlockType(mappedType);
                    }
                }
            }
        }

        private void TryExpandType(int typeIndex)
        {
            if (m_mappings[typeIndex].PropertyMappings != null)
            {
                return;
            }
            if (m_mappings[typeIndex].ExpandedCounter > 0 || m_mappings[typeIndex].IsEnabled)
            {
                ExpandType(typeIndex);
            }
        }

        private void ExpandType(int typeIndex)
        {
            Type type = m_types[typeIndex];

            List<PersistentPropertyMapping> pMappings = new List<PersistentPropertyMapping>();
            List<bool> pMappingsEnabled = new List<bool>();

            PersistentPropertyMapping[] fieldMappings = m_mappings[typeIndex].PropertyMappings != null ?
                m_mappings[typeIndex].PropertyMappings.Where(p => !p.IsProperty).ToArray() :
                new PersistentPropertyMapping[0];

            HashSet<string> fieldMappingsHs = new HashSet<string>();
            IEnumerable<string> fmapKeys = fieldMappings.Select(fMap => fMap.PersistentFullTypeName + " " + fMap.PersistentName);
            foreach (string key in fmapKeys)
            {
                if (!fieldMappingsHs.Contains(key))
                {
                    fieldMappingsHs.Add(key);
                }
            }

            PersistentPropertyMapping[] propertyMappings = m_mappings[typeIndex].PropertyMappings != null ?
                m_mappings[typeIndex].PropertyMappings.Where(p => p.IsProperty).ToArray() :
                new PersistentPropertyMapping[0];

            HashSet<string> propertyMappingsHs = new HashSet<string>();
            IEnumerable<string> pmapKeys = propertyMappings.Select(pMap => pMap.PersistentFullTypeName + " " + pMap.PersistentName);
            foreach (string key in pmapKeys)
            {
                if (!propertyMappingsHs.Contains(key))
                {
                    propertyMappingsHs.Add(key);
                }
            }

            FieldInfo[] fields = m_codeGen.GetFields(type);
            HashSet<string> fieldHs = new HashSet<string>(fields.Select(fInfo => fInfo.FieldType.FullName + " " + fInfo.Name));

            PropertyInfo[] properties = m_codeGen.GetProperties(type);
            HashSet<string> propertyHs = new HashSet<string>(properties.Select(pInfo => pInfo.PropertyType.FullName + " " + pInfo.Name));

            for (int i = 0; i < fieldMappings.Length; ++i)
            {
                PersistentPropertyMapping mapping = fieldMappings[i];
                string key = mapping.MappedFullTypeName + " " + mapping.MappedName;
                if (!fieldHs.Contains(key))
                {
                    mapping.MappedName = null;
                    mapping.MappedTypeName = null;
                    mapping.MappedNamespace = null;
                    mapping.MappedAssemblyName = null;

                    pMappingsEnabled.Add(false);
                }
                else
                {
                    pMappingsEnabled.Add(mapping.IsEnabled);
                }

                pMappings.Add(mapping);
            }


            for (int f = 0; f < fields.Length; ++f)
            {
                FieldInfo fInfo = fields[f];

                string key = string.Format("{0}.{1}",
                    PersistentClassMapping.ToPersistentNamespace(fInfo.FieldType.Namespace),
                    m_codeGen.TypeName(fInfo.FieldType)) + " " + fInfo.Name;

                if (fieldMappingsHs.Contains(key))
                {
                    continue;
                }

                PersistentPropertyMapping pMapping = new PersistentPropertyMapping();
                pMapping.PersistentName = fInfo.Name;
                pMapping.PersistentTypeName = m_codeGen.TypeName(fInfo.FieldType);
                pMapping.PersistentNamespace = PersistentClassMapping.ToPersistentNamespace(fInfo.FieldType.Namespace);

                pMapping.MappedName = fInfo.Name;
                pMapping.MappedTypeName = m_codeGen.TypeName(fInfo.FieldType);
                pMapping.MappedNamespace = fInfo.FieldType.Namespace;
                pMapping.MappedAssemblyName = fInfo.FieldType.Assembly.FullName.Split(',')[0];
                pMapping.IsProperty = false;

                pMapping.UseSurrogate = m_codeGen.GetSurrogateType(fInfo.FieldType) != null;
                pMapping.HasDependenciesOrIsDependencyItself = m_codeGen.HasDependencies(fInfo.FieldType);

                pMappingsEnabled.Add(false);
                pMappings.Add(pMapping);
            }

            for (int i = 0; i < propertyMappings.Length; ++i)
            {
                PersistentPropertyMapping mapping = propertyMappings[i];
                string key = mapping.MappedFullTypeName + " " + mapping.MappedName;
                if (!propertyHs.Contains(key))
                {
                    mapping.MappedName = null;
                    mapping.MappedTypeName = null;
                    mapping.MappedNamespace = null;
                    mapping.MappedAssemblyName = null;

                    pMappingsEnabled.Add(false);
                }
                else
                {
                    pMappingsEnabled.Add(mapping.IsEnabled);
                }

                pMappings.Add(mapping);
            }


            for (int p = 0; p < properties.Length; ++p)
            {
                PropertyInfo pInfo = properties[p];

                string key = string.Format("{0}.{1}",
                    PersistentClassMapping.ToPersistentNamespace(pInfo.PropertyType.Namespace),
                    m_codeGen.TypeName(pInfo.PropertyType)) + " " + pInfo.Name;

                if (propertyMappingsHs.Contains(key))
                {
                    continue;
                }

                PersistentPropertyMapping pMapping = new PersistentPropertyMapping();

                pMapping.PersistentName = pInfo.Name;       //property name of mapping
                pMapping.PersistentTypeName = m_codeGen.TypeName(pInfo.PropertyType);
                pMapping.PersistentNamespace = PersistentClassMapping.ToPersistentNamespace(pInfo.PropertyType.Namespace);

                pMapping.MappedName = pInfo.Name;           //property name of unity type
                pMapping.MappedTypeName = m_codeGen.TypeName(pInfo.PropertyType);
                pMapping.MappedNamespace = pInfo.PropertyType.Namespace;
                pMapping.MappedAssemblyName = pInfo.PropertyType.Assembly.FullName.Split(',')[0];
                pMapping.IsProperty = true;

                pMapping.UseSurrogate = m_codeGen.GetSurrogateType(pInfo.PropertyType) != null;
                pMapping.HasDependenciesOrIsDependencyItself = m_codeGen.HasDependencies(pInfo.PropertyType);

                pMappingsEnabled.Add(false);
                pMappings.Add(pMapping);
            }

            
            m_mappings[typeIndex].PropertyMappings = pMappings.ToArray();
            m_mappings[typeIndex].IsPropertyMappingEnabled = pMappingsEnabled.ToArray();
            m_mappings[typeIndex].PropertyMappingNames = new string[pMappings.Count][];
            m_mappings[typeIndex].PropertyMappingsDisplayNames = new GUIContent[pMappings.Count][];
            m_mappings[typeIndex].PropertyIsObsolete = new bool[pMappings.Count][];
            m_mappings[typeIndex].PropertyMappingTypes = new Type[pMappings.Count][];
            m_mappings[typeIndex].PropertyMappingTypeNames = new string[pMappings.Count][];
            m_mappings[typeIndex].PropertyMappingNamespaces = new string[pMappings.Count][];
            m_mappings[typeIndex].PropertyMappingAssemblyNames = new string[pMappings.Count][];
            m_mappings[typeIndex].PropertyMappingSelection = new int[pMappings.Count];


            string[][] mappedKeys = new string[pMappings.Count][];

            for (int propIndex = 0; propIndex < pMappings.Count; ++propIndex)
            {
                PersistentPropertyMapping pMapping = pMappings[propIndex];
              
                var propertyInfo = GetSuitableFields(fields, PersistentClassMapping.ToMappedNamespace(pMapping.PersistentNamespace) + "." + pMapping.PersistentTypeName)
                    .Select(f => new {
                        Name = f.Name,
                        ObsoleteAttribute = f.GetCustomAttributes(false).OfType<ObsoleteAttribute>().FirstOrDefault(),
                        Type = f.FieldType,
                        TypeName = m_codeGen.TypeName(f.FieldType),
                        Namespace = f.FieldType.Namespace,
                        Assembly = f.FieldType.Assembly.FullName.Split(',')[0] })
                    .Union(GetSuitableProperties(properties, PersistentClassMapping.ToMappedNamespace(pMapping.PersistentNamespace) + "." + pMapping.PersistentTypeName)
                    .Select(p => new {
                        Name = p.Name,
                        ObsoleteAttribute = p.GetCustomAttributes(false).OfType<ObsoleteAttribute>().FirstOrDefault(),
                        Type = p.PropertyType,
                        TypeName = m_codeGen.TypeName(p.PropertyType),
                        Namespace = p.PropertyType.Namespace,
                        Assembly = p.PropertyType.Assembly.FullName.Split(',')[0] }))
                    .OrderBy(p => p.Name)
                    .ToArray();

                m_mappings[typeIndex].PropertyMappingNames[propIndex] = propertyInfo.Select(p => p.Name).ToArray();
                m_mappings[typeIndex].PropertyIsObsolete[propIndex] = propertyInfo.Select(p => p.ObsoleteAttribute != null).ToArray();
                m_mappings[typeIndex].PropertyMappingsDisplayNames[propIndex] = propertyInfo.Select(p => new GUIContent(p.ObsoleteAttribute != null ? p.Name + " [Deprecated]" : p.Name, p.ObsoleteAttribute != null ? p.ObsoleteAttribute.Message : "")).ToArray();
                m_mappings[typeIndex].PropertyMappingTypeNames[propIndex] = propertyInfo.Select(p => p.TypeName).ToArray();
                m_mappings[typeIndex].PropertyMappingTypes[propIndex] = propertyInfo.Select(p => p.Type).ToArray();
                m_mappings[typeIndex].PropertyMappingNamespaces[propIndex] = propertyInfo.Select(p => p.Namespace).ToArray();
                m_mappings[typeIndex].PropertyMappingAssemblyNames[propIndex] = propertyInfo.Select(p => p.Assembly).ToArray();
                mappedKeys[propIndex] = propertyInfo.Select(m => m.Namespace + "." + m.TypeName + " " + m.Name).ToArray();
            }

            for (int propIndex = 0; propIndex < m_mappings[typeIndex].PropertyMappingSelection.Length; ++propIndex)
            {
                PersistentPropertyMapping mapping = m_mappings[typeIndex].PropertyMappings[propIndex];

                m_mappings[typeIndex].PropertyMappingSelection[propIndex] = Array.IndexOf(mappedKeys[propIndex], mapping.MappedFullTypeName + " " + mapping.MappedName);
            }
        }

        private IEnumerable<PropertyInfo> GetSuitableProperties(PropertyInfo[] properties, string persistentType)
        {
            return properties.Where(pInfo => pInfo.PropertyType.FullName == persistentType);
        }

        private IEnumerable<FieldInfo> GetSuitableFields(FieldInfo[] fields, string persistentType)
        {
            return fields.Where(fInfo => fInfo.FieldType.FullName == persistentType);
        }
    }

    public class PersistentClassMapperWindow : EditorWindow
    {
        [MenuItem("Tools/Runtime SaveLoad2/Persistent Classes")]
        public static void ShowMenuItem()
        {
            ShowWindow();
        }

        public static void ShowWindow()
        {
            PersistentClassMapperWindow prevWindow = GetWindow<PersistentClassMapperWindow>();
            if (prevWindow != null)
            {
                prevWindow.Close();
            }

            PersistentClassMapperWindow window = CreateInstance<PersistentClassMapperWindow>();
            window.titleContent = new GUIContent("Persistent Classes");
            window.Show();
            window.position = new Rect(20, 40, 1280, 768);
        }

        private Type[] m_mostImportantUOTypes =
        {
            typeof(UnityObject),
            typeof(GameObject),
            typeof(MeshRenderer),
            typeof(MeshFilter),
            typeof(SkinnedMeshRenderer),
            typeof(Mesh),
            typeof(Material),
            typeof(Rigidbody),
            typeof(BoxCollider),
            typeof(SphereCollider),
            typeof(CapsuleCollider),
            typeof(MeshCollider),
            typeof(Camera),
            typeof(AudioClip),
            typeof(AudioSource),
            typeof(Light),
        };

        private Type[] m_mostImportantSurrogateTypes =
        {
            typeof(object),
            typeof(Vector2),
            typeof(Vector3),
            typeof(Vector4),
            typeof(Vector2Int),
            typeof(Vector3Int),
            typeof(Color),
            typeof(Color32),
            typeof(Matrix4x4),
        };

        public const string SaveLoadRoot = @"/" + BHPath.Root + @"/RTSaveLoad2";
        public const string EditorPrefabsPath = SaveLoadRoot + "/Editor/Prefabs";

        public const string FilePathStoragePath = "Assets" + EditorPrefabsPath + @"/FilePathStorage.prefab";
        public const string ClassMappingsStoragePath = "Assets" + EditorPrefabsPath + @"/ClassMappingsStorage.prefab";
        public const string ClassMappingsTemplatePath = "Assets" + EditorPrefabsPath + @"/ClassMappingsTemplate.prefab";
        public const string SurrogatesMappingsStoragePath = "Assets" + EditorPrefabsPath + @"/SurrogatesMappingsStorage.prefab";
        public const string SurrogatesMappingsTemplatePath = "Assets" + EditorPrefabsPath + @"/SurrogatesMappingsTemplate.prefab";

        public const string ScriptsAutoFolder = "Scripts_Auto";
        public const string ScriptsFolder = "Scripts";
        public const string PersistentClassesFolder = "PersistentClasses";
        public const string PersistentCustomImplementationClasessFolder = "MyPersistentClasses";

        private Type[] m_uoTypes;
        private PersistentClassMapperGUI m_uoMapperGUI;
        private PersistentClassMapperGUI m_surrogatesMapperGUI;
        private FilePathStorage m_filePathStorage;
        private CodeGen m_codeGen = new CodeGen();

        private void GetUOAssembliesAndTypes(out Assembly[] assemblies, out Type[] types)
        {
            assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.Contains("UnityEngine")).OrderBy(a => a.FullName).ToArray();

            List<Type> allUOTypes = new List<Type>();
            List<Assembly> assembliesList = new List<Assembly>() { null };

            for (int i = 0; i < assemblies.Length; ++i)
            {
                Assembly assembly = assemblies[i];
                Type[] uoTypes = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(UnityObject))).ToArray();
                if (uoTypes.Length > 0)
                {
                    assembliesList.Add(assembly);
                    allUOTypes.AddRange(uoTypes);
                }
            }

            for (int i = 0; i < m_mostImportantUOTypes.Length; ++i)
            {
                allUOTypes.Remove(m_mostImportantUOTypes[i]);
            }

            types = m_mostImportantUOTypes.Union(allUOTypes.OrderBy(t => m_codeGen.TypeName(t))).ToArray();
            assemblies = assembliesList.ToArray();
        }

        private void GetTypesRecursive(Type type, HashSet<Type> typesHS)
        {
            PropertyInfo[] properties = m_codeGen.GetAllProperties(type);
            FieldInfo[] fields = m_codeGen.GetFields(type);

            for (int p = 0; p < properties.Length; ++p)
            {
                PropertyInfo pInfo = properties[p];
                if (!typesHS.Contains(pInfo.PropertyType))
                {
                    Type surrogateType = m_codeGen.GetSurrogateType(pInfo.PropertyType);
                    if(surrogateType != null && !typesHS.Contains(surrogateType))
                    {
                        typesHS.Add(surrogateType);
                        GetTypesRecursive(surrogateType, typesHS);
                    }
                }
            }

            for (int f = 0; f < fields.Length; ++f)
            {
                FieldInfo fInfo = fields[f];
                if (!typesHS.Contains(fInfo.FieldType))
                {
                    Type surrogateType = m_codeGen.GetSurrogateType(fInfo.FieldType);
                    if (surrogateType != null && !typesHS.Contains(surrogateType))
                    {
                        typesHS.Add(surrogateType);
                        GetTypesRecursive(surrogateType, typesHS);
                    }
                }
            }
        }

        private void GetSurrogateAssembliesAndTypes(Type[] uoTypes, out Dictionary<string, HashSet<Type>> declaredIn, out Type[] types)
        {
            HashSet<Type> allTypesHS = new HashSet<Type>();
            declaredIn = new Dictionary<string, HashSet<Type>>();
            
            for(int typeIndex = 0; typeIndex < uoTypes.Length; ++typeIndex)
            {
                Type uoType = uoTypes[typeIndex];

                HashSet<Type> typesHs = new HashSet<Type>();
                GetTypesRecursive(uoType, typesHs);
                declaredIn.Add(uoType.Name, typesHs);

                foreach (Type type in typesHs)
                {
                    if(!allTypesHS.Contains(type))
                    {
                        allTypesHS.Add(type);
                    }
                }
            }

            for (int i = 0; i < m_mostImportantSurrogateTypes.Length; ++i)
            {
                allTypesHS.Remove(m_mostImportantSurrogateTypes[i]);
            }

            types = m_mostImportantSurrogateTypes.Union(allTypesHS.OrderBy(t => m_codeGen.TypeName(t))).ToArray();
        }

        private void OnGUI()
        {
           
            if(m_uoMapperGUI == null)
            {
                if (m_filePathStorage == null)
                {
                    GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(FilePathStoragePath); 
                    if(go != null)
                    {
                        m_filePathStorage = go.GetComponent<FilePathStorage>();
                    }   
                }

                Assembly[] assemblies;
                GetUOAssembliesAndTypes(out assemblies, out m_uoTypes);
                m_uoMapperGUI = new PersistentClassMapperGUI(GetInstanceID(),
                    m_codeGen, 
                    ClassMappingsStoragePath, 
                    ClassMappingsStoragePath,
                    m_filePathStorage,
                    typeof(object), 
                    m_uoTypes, 
                    assemblies.Select(a => a == null ? "All" : a.GetName().Name).ToArray(),
                    "Assembly",
                    (type, groupName) => type.Assembly.GetName().Name == groupName);
                m_uoMapperGUI.TypeLocked += OnUOTypeLocked;
                m_uoMapperGUI.TypeUnlocked += OnUOTypeUnlocked;
            }

            if(m_surrogatesMapperGUI == null)
            {
                Type[] types;
                Dictionary<string, HashSet<Type>> declaredIn;
                GetSurrogateAssembliesAndTypes(m_uoTypes, out declaredIn, out types);
                m_surrogatesMapperGUI = new PersistentClassMapperGUI(GetInstanceID() + 1,
                    m_codeGen, 
                    SurrogatesMappingsStoragePath, 
                    SurrogatesMappingsTemplatePath,
                    m_filePathStorage,
                    typeof(object),
                    types, 
                    new[] { "All" }.Union(declaredIn.Where(t => t.Value.Count > 0).Select(t => t.Key)).ToArray(), 
                    "Declaring Type",
                    (type, groupName) => declaredIn[groupName].Contains(type));

                m_surrogatesMapperGUI.TypeLocked += OnSurrogateTypeLocked;
                m_surrogatesMapperGUI.TypeUnlocked += OnSurrogateTypeUnlocked;
            }

            EditorGUILayout.Separator();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(GUILayout.MinWidth(position.width / 2));
      
            m_uoMapperGUI.OnGUI();

            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical(GUILayout.MinWidth(position.width / 2));
 
            m_surrogatesMapperGUI.OnGUI();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            GUILayout.Button("Undo & Reload", GUILayout.Height(37));
            if (EditorGUI.EndChangeCheck())
            {
                m_uoMapperGUI.ClearDependencies();
                m_surrogatesMapperGUI.ClearDependencies();
                m_uoMapperGUI.Reset();
                m_surrogatesMapperGUI.Reset();
            }
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.HelpBox("Please note that most of the data are stored and restored using public properties which may cause undesired side effects. For example accessing renderer.material or meshfilter.mesh will instantiate new objects.", MessageType.Info);
            GUILayout.Button("Create Persistent Objects", GUILayout.Height(37));


            if (EditorGUI.EndChangeCheck())
            {
                m_uoMapperGUI.SaveMappings();
                m_surrogatesMapperGUI.SaveMappings();

                Dictionary<string, string> persistentFileTypeToPath = new Dictionary<string, string>();

                PersistentClassMapping[] uoMappings = m_uoMapperGUI.GetMappings();
                Dictionary<Type, PersistentTemplateInfo> uoTemplates = m_uoMapperGUI.GetTemplates();
                PersistentClassMapping[] surrogateMappings = m_surrogatesMapperGUI.GetMappings();
                Dictionary<Type, PersistentTemplateInfo> surrogateTemplates = m_surrogatesMapperGUI.GetTemplates();

                string scriptsAutoPath = Application.dataPath + SaveLoadRoot + "/" + ScriptsAutoFolder;
                if (Directory.Exists(scriptsAutoPath))
                {
                    Directory.Delete(scriptsAutoPath, true);
                }

                Directory.CreateDirectory(scriptsAutoPath);
                string persistentClassesPath = scriptsAutoPath + "/" + PersistentClassesFolder;
                if (!Directory.Exists(persistentClassesPath))
                {
                    Directory.CreateDirectory(persistentClassesPath);
                }

                string scriptsPath = Application.dataPath + SaveLoadRoot + "/" + ScriptsFolder;
                if (!Directory.Exists(scriptsPath))
                {
                    Directory.CreateDirectory(scriptsPath);
                }

                string myPersistentClassesPath = scriptsPath + "/" + PersistentCustomImplementationClasessFolder;
                if (!Directory.Exists(myPersistentClassesPath))
                {
                    Directory.CreateDirectory(myPersistentClassesPath);
                }

                HashSet<string> hideMustHaveTypes = new HashSet<string>(PersistentClassMapperGUI.HideMustHaveTypes.Select(t => t.FullName));
                CodeGen codeGen = new CodeGen();
                for (int i = 0; i < uoMappings.Length; ++i)
                {
                    PersistentClassMapping mapping = uoMappings[i];
                    if (mapping != null)
                    {
                        if (!mapping.IsEnabled)
                        {
                            continue;
                        }

                        if (hideMustHaveTypes.Contains(mapping.MappedFullTypeName))
                        {
                            continue;
                        }

                        if (mapping.CreateCustomImplementation)
                        {
                            if(HasCustomImplementation(codeGen, mapping))
                            {
                                persistentFileTypeToPath.Add(mapping.PersistentFullTypeName, null);
                            }
                            else
                            {
                                persistentFileTypeToPath.Add(mapping.PersistentFullTypeName, GetCSFilePath(myPersistentClassesPath, mapping));
                            }
                        }
                        CreateCSFiles(persistentClassesPath, myPersistentClassesPath, codeGen, mapping, uoTemplates);
                        
                    }
                }

                for (int i = 0; i < surrogateMappings.Length; ++i)
                {
                    PersistentClassMapping mapping = surrogateMappings[i];
                    if(mapping != null)
                    {
                        if (!mapping.IsEnabled)
                        {
                            continue;
                        }

                        if (hideMustHaveTypes.Contains(mapping.MappedFullTypeName))
                        {
                            continue;
                        }

                        if (mapping.CreateCustomImplementation)
                        {
                            if (HasCustomImplementation(codeGen, mapping))
                            {
                                persistentFileTypeToPath.Add(mapping.PersistentFullTypeName, null);
                            }
                            else
                            {
                                persistentFileTypeToPath.Add(mapping.PersistentFullTypeName, GetCSFilePath(myPersistentClassesPath, mapping));
                            }
                        }

                        CreateCSFiles(persistentClassesPath, myPersistentClassesPath, codeGen, mapping, surrogateTemplates);
                    }
                }

                string typeModelCreatorCode = codeGen.CreateTypeModelCreator(uoMappings.Union(surrogateMappings).ToArray());
                File.WriteAllText(scriptsAutoPath + "/TypeModelCreator.cs", typeModelCreatorCode);

                string typeMapCode = codeGen.CreateTypeMap(uoMappings.Union(surrogateMappings).ToArray());
                File.WriteAllText(scriptsAutoPath + "/TypeMap.cs", typeMapCode);

                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

                GameObject storageGO = (GameObject)AssetDatabase.LoadAssetAtPath(FilePathStoragePath, typeof(GameObject));
                if(storageGO == null)
                {
                    storageGO = new GameObject();
                }

                FilePathStorage filePathStorage = storageGO.GetComponent<FilePathStorage>();
                if(filePathStorage == null)
                {
                    filePathStorage = storageGO.AddComponent<FilePathStorage>();
                }

                Dictionary<string, FilePathRecord> typeNameToExistingRecord = filePathStorage.PathRecords != null ?
                    filePathStorage.PathRecords.ToDictionary(r => r.PeristentTypeName) :
                    new Dictionary<string, FilePathRecord>();

                List<FilePathRecord> records = new List<FilePathRecord>();
                foreach(string fullTypeName in persistentFileTypeToPath.Keys)
                {
                    string filePath = persistentFileTypeToPath[fullTypeName];
                    if(filePath != null)
                    {
                        int indexOfAssets = filePath.IndexOf("Assets");

                        filePath = filePath.Substring(indexOfAssets);

                        UnityObject asset = AssetDatabase.LoadAssetAtPath<UnityObject>(filePath);

                        FilePathRecord record = new FilePathRecord
                        {
                            File = asset,
                            PeristentTypeName = fullTypeName
                        };
                        records.Add(record);
                    }
                    else
                    {
                        FilePathRecord record;
                        if(typeNameToExistingRecord.TryGetValue(fullTypeName, out record))
                        {
                            if(record.File == null)
                            {
                                Debug.LogWarningFormat("FilePathRecord for type {0} is broken", fullTypeName);
                            }
                           
                            records.Add(record);
                        }
                        else
                        {
                            Debug.LogWarningFormat("FilePathRecord for type {0} does not exist", fullTypeName);
                        }
                    }
                }

                DestroyImmediate(storageGO, true);
                storageGO = new GameObject();
                filePathStorage = storageGO.AddComponent<FilePathStorage>();
                filePathStorage.PathRecords = records.ToArray();
                PrefabUtility.CreatePrefab(FilePathStoragePath, storageGO);
                DestroyImmediate(storageGO);
            }
      
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator();
        }

        private static bool HasCustomImplementation(CodeGen codeGen, PersistentClassMapping mapping)
        {
            Type persistentType = codeGen.GetPersistentType(mapping.PersistentFullTypeName);
            return persistentType != null && persistentType.GetCustomAttributes(typeof(CustomImplementationAttribute), false).Length > 0;
        }

        private static void CreateCSFiles(string persistentClassesPath, string myPersistentClassesPath, CodeGen codeGen, PersistentClassMapping mapping, Dictionary<Type, PersistentTemplateInfo> templates)
        {
            string code = codeGen.CreatePersistentClass(mapping);
            CreateCSFile(persistentClassesPath, mapping, code);

            if(mapping.CreateCustomImplementation)
            {
                if (!HasCustomImplementation(codeGen, mapping))
                {
                    PersistentTemplateInfo template;
                    Type mappedType = Type.GetType(mapping.MappedAssemblyQualifiedName);
                    if (mappedType == null || !templates.TryGetValue(mappedType, out template))
                    {
                        template = null;
                    }
                    string customCode = codeGen.CreatePersistentClassCustomImplementation(mapping.PersistentNamespace, mapping.PersistentTypeName, template);
                    CreateCSFile(myPersistentClassesPath, mapping, customCode);
                }
            }
        }

    
        private void OnDisable()
        {
            if(m_uoMapperGUI != null)
            {
                m_uoMapperGUI.OnDisable();
            }

            if(m_surrogatesMapperGUI != null)
            {
                m_surrogatesMapperGUI.OnDisable();
            }
        }

        private void OnSurrogateTypeLocked(Type obj)
        {
            m_uoMapperGUI.TryLockType(obj);
        }

        private void OnSurrogateTypeUnlocked(Type obj)
        {
            m_uoMapperGUI.TryUnlockType(obj);
        }

        private void OnUOTypeLocked(Type obj)
        {
            m_surrogatesMapperGUI.TryLockType(obj);
        }

        private void OnUOTypeUnlocked(Type obj)
        {
            m_surrogatesMapperGUI.TryUnlockType(obj);
        }

        private static void CreateCSFile(string persistentClassesPath, PersistentClassMapping mapping, string code)
        {
            File.WriteAllText(GetCSFilePath(persistentClassesPath, mapping), code);
        }

        private static string GetCSFilePath(string persistentClassesPath, PersistentClassMapping mapping)
        {
            string path = persistentClassesPath + "/" + mapping.PersistentFullTypeName.Replace(".", "_") + ".cs";
            return path;
        }

   
    }
}
