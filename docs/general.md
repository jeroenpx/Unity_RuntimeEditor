#Introduction


Runtime Save&Load menu allows you to configure Save&Load subsystem. There
are following menu items:
1) Build Resource Map creates or updates mapping between objects (prefabs,
resources, special scene objects) and unique identifiers. These identifiers are
required to make Save&Load subsystem work correctly. Project’s Resource
map is saved to Battlehub_ResourceMap prefab located in RTSaveLoad
/ResourceMaps/Resources/ folder. Create Resource Map menu item also
creates or updates resource maps for each asset bundle in project. Resource
maps for asset bundles saved outside of Resources folder. Name of resource
map for asset bundle have following format:
ResourceMap_<bundle name>_<guid> where <bundle name> is name of
asset bundle and <guid> is string representation of arbitrary System.Guid
2) Build Type Model will precompile TypeModel wich will be used for
serialization at runtime. Result of execution of this command will be located in
Assets/Battlehub/Deps/RTTypeModel.dll
Fig.1.2 Runtime Save&Load menu
3) Build All will build ResourceMap first, then it will build TypeModel
1.3 Runtime Transform Handles Menu
If you need something lightweight instead of Runtime Editor, you could use
runtime handles package. Runtime Transform Handles have separate menu.
There are three menu items and one submenu:
1. Create menu item creates simple selection controller, box selection component
and 3 handles (positon, rotation, scale)
2. Enable Editing makes game object or prefab visible to selection controller
3. Disable Editing makes game object or invisible to selection controller
4. Demo->Create Editor creates demo editor
5. Demo->Expose Prefab makes prefab visible to demo editor
6. Demo->Hide Prefab makes prefab invisible to demo editor
Fig.1.3 Runtime Handles menu
2. Getting Started
Whole process is shown in this video: https://vimeo.com/192127888
There are several easy streps to get started:
1. Create editor using Tools->Runtime Editor->Create
Fig. 2.1 Create Runtime Editor
2. Select scene objects you want to make available to editor and click
Tools->Runtime Editor->Expose To Editor or just attach ExposeToEditor
script to selected game objects.
Fig. 2.2 Expose “Cube” to editor
Fig. 2.3 ExposeToEditor script
3. Select resources or prefabs you want to expose to editor and click
Tools->Runtime Editor->Expose To Editor
Fig. 2.3 Expose Texture to editor
Fig. 2.4 Expose Prefabs to editor
4. Create Resource Map and Type Model using
Tools->Runtime SaveLoad->Build All menu item
Fig. 2.5 Creating Resource Map
Fig. 2
5. Build & Run File->Build & Run
Fig.2.7 Open editor button
6. Open Editor
Fig.2.8 Opened editor with available objects and resources
3. Transform Handles
There are three transform handles included in this package: Position, Rotation and
Scale. They behaves almost identical to their equivalents in unity editor. Transform
Handles, Scene Gizmo, Grid, rendering classes and all required shaders can be found
in Assets/Battlehub/RTHandles folder.
Position Handle, Rotation Handle and Scale Handle scripts allows you to
choose Raycasting Camera, Selection Margin (in screen space coordinates), Target
objects, Grid Size, and key which will switch position gizmo to “Snapping mode”
Scene Gizmo script let you to choose Scene Camera, Pivot Point (to rotate
Scene Camera around), size of Gizmo.
Scene Gizmo could raise following events:
- Orientation Changing;
- Orientation Changed;
- Projection Changed;
Note: Scene gizmo always aligned to the top right corner of the screen
3.1 Getting Started With Transform Handles
Create editor using Tools->Runtime Editor->Create
1. Click Tools->Runtime Handles->Create menu item
Fig.3.1 Create RuntimeTransform handles
2. Create several GameObjects and select them
Fig.3.2 Several GameObjects selected
3. Click Tools->Runtime Handles->Ena



#Features
#About