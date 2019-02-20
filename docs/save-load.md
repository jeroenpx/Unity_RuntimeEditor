#Save Load Docs
##Overview

The __Runtime Save & Load__ (RTSL) subsystem is needed for saving and managing scenes, assets and projects at runtime and consists of three main parts:
 
 * [Persistent Classes](#persistent-classes) - this part allows you to choose what to serialize and generate persistent classes for serialization. 
 * [Asset Libraries](#asset-library) - this part allows you to create and manage assets, as well as provide information to the RTSL to identify these assets.
 * [Project](#project) - this part provides api to interact with RTSL.

 
!!! note

       RTSL use [protobuf.net](https://github.com/mgravell/protobuf-net) for serialization.
	   
![Screenshot](img/save-load/get-started/demo.png)
	   
##Getting Started

After importing RTSL you will see the configuration window:

![Screenshot](img/save-load/get-started/config-window.png)

After clicking "Build All", several folders will be created under __/Battlehub/RTSL_Data__

![Screenshot](img/save-load/get-started/folders.png)

  * __Scripts__ for [serializable persistent classes](#persistent-classes).
  * __Custom Implementation__ for user defined persistent classes.
  * __Mappings__ for mappings between types that must be stored and serializable persistent types. 
  * __Libraries__ for [asset libraries and shader profiles](#asset-library).
  
!!! note
	
	__RTSLTypeModel.dll__ contains [protobuf-net](https://github.com/mgravell/protobuf-net) type model. Due to [unity scripting restrictions](https://docs.unity3d.com/Manual/ScriptingRestrictions.html), runtime type model need to be pre-complied before
    using at runtime.
	
1. Create new scene
2. Drag and Drop __Assets/Battlehub/RTDemoGame/Prefabs/Game.prefab__ to hierarchy.

     ![Screenshot](img/save-load/get-started/game-prefab.png)
     <br/><br/>
	 
3. Click Tools->Runtime SaveLoad->Libraries->__Collect Scene Dependencies__

	![Screenshot](img/save-load/get-started/collect-scene-dependencies.png)
     <br/><br/>

4. Create Game Object, then add Assets/Battlehub/RTSL/Interface/__Project__ and Assets/Battlehub/RTSL/Interface/__RTSLIgnore__ components. 

	![Screenshot](img/save-load/get-started/project.png)
     <br/><br/>
	 
5. Create __RTSLTest__ script.
6. Hit play.
7. Save scene using 'M' key.
8. Load scene using 'L' key.

``` C#

using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;

using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;


public class RTSLTest : MonoBehaviour
{
	IProject m_project;

	IEnumerator Start()
	{
		m_project = IOC.Resolve<IProject>();

		yield return m_project.OpenProject("My Project");
		yield return m_project.CreateFolder("Scenes/Demo");           
	}

	IEnumerator SaveScene()
	{
		ProjectAsyncOperation ao = m_project.Save("Scenes/Demo/Scene", SceneManager.GetActiveScene());
		yield return ao;

		if(ao.Error.HasError)
		{
			Debug.LogError(ao.Error.ToString());
		}
	}

	IEnumerator LoadScene()
	{
		ProjectAsyncOperation ao = m_project.Load<Scene>("Scenes/Demo/Scene");
		yield return ao;

		if (ao.Error.HasError)
		{
			Debug.LogError(ao.Error.ToString());
		}
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.M))
		{
			StartCoroutine(SaveScene());
		}

		if (Input.GetKeyDown(KeyCode.L))
		{   
			if (m_project.Exist<Scene>("Scenes/Demo/Scene"))
			{
				StartCoroutine(LoadScene());
			}
		}
	}
}

```


Saved scene can be found in [__PersistentDataPath__](https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html)__/My Project/Assets/Scenes/Demo folder__.

!!! note

    Only few persistent classes enabled and could be saved by default. Use [Persistent Classes Editor Window](#persistent-classes) to enable more.
	
!!! note

    Demo scene can be found in __Assets/Battlehub/RTSL/Demo__ folder.
	 
	 

##Persistent Classes

__Persistent Class__ is a class derived from __Persistent Surrogate__ and having the  __[ProtoContract]__ attribute. The main purpose of the persistent classes is to give you full control over what and how to save, without having to use reflection and without writing a lot of code.

To open persistent classes editor, click Tools->Runtime SaveLoad->__Persistent Classes->Edit__.

![Screenshot](img/save-load/persistent-classes/open-persistent-classes-editor.png)

Here is the persistent classes editor window:

![Screenshot](img/save-load/persistent-classes/editor.png)

This window allow you:
 
 * Search for types you want to save.
 * Select properties you want to save.
 * Generate C# code of persistent classes.
 
After clicking the __"Create Persistent Classes"__ button the following will occur:

  1. Persistent classes will be created in 
__Assets/Battlehub/RTSL_Data/Scripts/PersistentClasses__.

    ![Screenshot](img/save-load/persistent-classes/persistent-classes-folder.png)
	<br/><br/>
	
  2. The editor window state will be saved in
__Assets/Battlehub/RTSL_Data/Mappings/Editor__.
 
    ![Screenshot](img/save-load/persistent-classes/mappings-folder.png)
	<br/><br/>

  3. Custom Implementation will be created in __.../RTSL_Data/Scripts/CustomImplementation__.

	![Screenshot](img/save-load/persistent-classes/custom-implementation-folder.png)
	<br/><br/>
	

!!! note

    To ensure forward compatibility of saved files, never delete the __ClassMappingStorage__ and __SurrogatesMappingsStorage__ prefabs !!!
	
##How To: Create Custom Persistent Class

In some cases, it is not possible to get all the data to be saved using public properties and fields. For example, data can only be obtained using the methods.
If you have to write your own code to save and load data, follow these steps:

  1. Open Persistent Classes Editor.
  2. Find and select required type.
  3. Select __"Custom Implementation"__

	![Screenshot](img/save-load/persistent-classes/create-custom-implementation.png)
	<br/><br/>
	
  4. Click __Create Persistent Classes__ button.
  5. Click __Edit Custom Implementation__ button.

    ![Screenshot](img/save-load/persistent-classes/edit-custom-implementation.png)
	<br/><br/>
  
  6. You should see following:
  
``` C#
#if !RTSL_MAINTENANCE
using Battlehub.RTSL;
namespace UnityEngine.Battlehub.SL2
{
    [CustomImplementation]
    public partial class PersistentJoint
    {
        /*
        public override void ReadFrom(object obj)
        {
            base.ReadFrom(obj);
        }

        public override object WriteTo(object obj)
        {
            return base.WriteTo(obj);
        }

        public override void GetDeps(GetDepsContext context)
        {
            base.GetDeps(context);
        }

        public override void GetDepsFrom(object obj, GetDepsFromContext context)
        {
            base.GetDepsFrom(obj, context);
        }
        */
    }
}
#endif

```

There are four methods that can be implemented:

   * `void ReadFrom(object obj)` - invoked before serialization. Read the data from obj and save it to the fields of the persistent object.
   * `object WriteTo(object obj)` - invoked after deserialization. Write stored data to obj.
   * `void GetDeps(GetDepsContext context)` - return identifiers of dependencies.
   * `void GetDepsFrom(object obj, GetDepsFromContext context)` - get dependencies from obj.

Implementation of PersistentJoint may look like this:

``` C#

#if !RTSL_MAINTENANCE
using Battlehub.RTSL;
namespace UnityEngine.Battlehub.SL2
{
    [CustomImplementation]
    public partial class PersistentJoint
    {
        [ProtoBuf.ProtoMember(1)]
        private long m_connectedBody;

        [ProtoBuf.ProtoMember(2)]
        private PersistentVector3 m_anchor;
		
		//..... more fields

        public override void ReadFrom(object obj)
        {
            base.ReadFrom(obj);

            Joint joint = (Joint)obj;
            if(joint == null)
            {
                return;
            }

            m_connectedBody = ToID(joint.connectedBody);
            m_anchor = joint.anchor;
			
			//... read other fields
        }

        public override object WriteTo(object obj)
        {
            obj = base.WriteTo(obj);

            Joint joint = (Joint)obj;
            if (joint == null)
            {
                return obj;
            }

            joint.connectedBody = FromID<Rigidbody>(m_connectedBody);
            joint.anchor = m_anchor;
			
			//... write other fields

            return joint;
        }

        public override void GetDeps(GetDepsContext context)
        {
            base.GetDeps(context);

            AddDep(m_connectedBody, context);
			
			//... get other dependencies
        }

        public override void GetDepsFrom(object obj, GetDepsFromContext context)
        {
            base.GetDepsFrom(obj, context);

            Joint joint = (Joint)obj;
            if (joint == null)
            {
                return;
            }

            AddDep(joint.connectedBody, context);
			
			//... get other dependencies
        }
    }
}
#endif


```

!!! note

    Click __Tools-> Runtime SaveLoad-> Build All__ each time you have finished making changes to persistent classes and are ready to build the application.
	

##Asset Library

Game objects in any scene refer assets such as materials, meshes, textures, etc.
Identifiers of these assets obtained using GetInstanceID() method do not remain constant and can change.    
In contrast, __Asset libraries__ are used to store unique asset identifiers that never change.

There are two special asset libraries:

  * Built-in asset library - contains Unity build-in assets.
  * Scene asset library - contains assets referenced by scene Game Objects.

The former library created automatically by clicking __Tools->Runtime SaveLoad->Libraries->Update Built-In Asset Library__   
The latter created by opening scene and clicking __Tools->Runtime SaveLoad->Libraries->Collect Scene Dependencies__

![Screenshot](img/save-load/asset-libraries/special-asset-libraries.png)

!!! note

    Scene asset libraries are referenced using the scene name. This means that scenes must have unique names.
	
!!! note

    Click __Tools->Runtime SaveLoad->Libraries->Collect Scene Dependencies__ each time you have finished making changes to scene and are ready to run the application.

##How To: Create Asset Library

If the built-in asset library and scene dependency library are not sufficient,
 and the new resources must be [Resource.Loaded](https://docs.unity3d.com/ScriptReference/Resources.Load.html) at runtime, you can create 
a new asset library by clicking   
__Create-> Runtime Asset Library__ in the context menu.

![Screenshot](img/save-load/asset-libraries/create-asset-library.png)

!!! note

    An asset library must be created inside the __Resources__ folder.
	
Use drag & drop to add assets to asset library
	
![Screenshot](img/save-load/asset-libraries/populate-asset-library.png)

!!! note

	One asset library can contain no more than __65535__ assets.
	
If you change one or more prefabs, you will be asked to synchronize the assets library. Do it by clicking __Synchronize__ button.

![Screenshot](img/save-load/asset-libraries/synchronize-asset-library.png)

##Project Item & Asset Item

__Project Items__ are used to create tree structures representing the project tree.

``` C#
    [ProtoContract]
    [ProtoInclude(1, typeof(AssetItem))]
    public class ProjectItem
    {
        [ProtoMember(2)]
        public long ItemID;

        public string Name;
        public string Ext;

        public ProjectItem Parent;
        public List<ProjectItem> Children;
		
	    // ...

```

__Asset Items__ are meta representations of assets in a project. They are stored in *.rtmeta files.

``` C#

    [ProtoContract]
    public class AssetItem : ProjectItem
    {
        public event EventHandler PreviewDataChanged;

        [ProtoMember(1)]
        public Guid TypeGuid;

        [ProtoMember(2)]
        public PrefabPart[] Parts;

        [ProtoMember(3)]
        public long[] Dependencies;

        private Preview m_preview;
        public Preview Preview
        {
            get { return m_preview; }
            set
            {
                if (m_preview != value)
                {
                    m_preview = value;
                    if (PreviewDataChanged != null)
                    {
                        PreviewDataChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        public override bool IsFolder
        {
            get { return false; }
        }

    }

```

##Project

__IProject__ is the main interface of the RTSL library. Here is how to access it:

``` C#

using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;

void Awake()
{
	IProject project = IOC.Resolve<IProject>();
}

```
<br/>
Open project:
``` C#

using System.Collections;
using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;

IEnumerator Start()
{
	IProject project = IOC.Resolve<IProject>();
	yield return m_project.OpenProject("My Project");
}

```

<br/>
Close project:
``` C#
m_project.CloseProject();
```

<br/>
Delete project:
``` C#
yield return m_project.DeleteProject("My Project");
```

<br/>
Create folder:
``` C#
yield return m_project.CreateFolder("My Scenes");   
```

<br/>
Delete folder:
``` C#
yield return m_project.DeleteFolder("My Scenes");
```

<br/>
Save scene:
``` C#

using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;

using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;

IEnumerator Start()
{
	//...
	
	ProjectAsyncOperation ao = m_project.Save("My Scenes/Scene 1", SceneManager.GetActiveScene());
	yield return ao;
            
	if (ao.Error.HasError)
	{
		Debug.LogError(ao.Error.ToString());
	}
}

```
<br/>
Load scene:
``` C#

using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;

using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;

IEnumerator Start()
{
	//...
	
	ProjectAsyncOperation ao = m_project.Load<Scene>("My Scenes/Scene 1");
	yield return ao;
            
	if (ao.Error.HasError)
	{
		Debug.LogError(ao.Error.ToString());
	}
}

```

<br/>
Delete scene:
``` C#
yield return m_project.Delete<Scene>("My Scenes/Scene 1");
```


<br/>
Find objects of type:
``` C#
foreach(string scene in m_project.Find<Scene>("Scene 1"))
{
	Debug.Log(scene);
}

```

<br/>
Create Prefab:
``` C# 
GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Capsule);
yield return m_project.Save("Capsule", primitive);
Destroy(primitive);
```

<br/>
Load and instantiate Prefab:
``` C# 
ProjectAsyncOperation<Object[]> ao = m_project.Load<GameObject>("Capsule");
yield return ao;

if(!ao.Error.HasError)
{
	Instantiate(ao.Result[0]);
}
```

<br/>
Import all assets from asset bundle:
``` C#

//get names of asset bundles from Assets/StreamingAssets folder.
ProjectAsyncOperation<string[]> ao = m_project.GetAssetBundles();
yield return ao;

//load ImportItems from first asset bundle
ProjectAsyncOperation<ProjectItem> loadAo = m_project.LoadImportItems(ao.Result[0], false);
yield return loadAo;


if (!loadAo.Error.HasError)
{
	//create previews here...
	//then unload asset bundle assets
	m_project.UnloadImportItems(loadAo.Result);

	//import all
	yield return m_project.Import(loadAo.Result.Flatten(true).OfType<ImportItem>().ToArray());
}

//log all asset items in project
foreach(string path in m_project.Find<object>(string.Empty, true))
{
	Debug.Log(path);
}
```




