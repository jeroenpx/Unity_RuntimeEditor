# Runtime Editor Docs

Welcome to documentation of <a href="http://u3d.as/v9j" target="_blank"><strong>Runtime Editor</strong></a> the set of scripts and prefabs which help you to implement scene editor, game level editor or build your own modeling application. If you are new to this documentation you could proceed to [introduction](general.md#introduction) page to get an overview what Runtime Editor   
and this documentation has to offer.
	
!!! note
	
	If you can't find something in documentation or have any questions I encourage you to send mail to [Battlehub@outlook](mailto:Battlehub@outlook) or ask it directly in this [support group](https://t.me/battlehub). 
	
!!! note
	
	Documentation is under construction
	


   
The documentation is organized as following:

##[General](general.md)
*   [Introduction](general.md#introduction)
*   [List of Features](general.md#list-of-features)
*   [About](general.md#about)

## [Getting Started](get-started.md)
*  [Minimal setup](get-started.md#minimal-setup)
*  [How to create and use Runtime Editor](get-started.md#how-to-create-and-use-runtime-editor)

## [Transform Handles](transform-handles.md)
*  [Overview](transform-handles.md#overview)
*  [Getting Started](transform-handles.md#getting-started)
*  [Base Handle](transform-handles.md#base-handle)
*  [Position Handle](transform-handles.md#position_handle)
*  [Rotation Handle](transform-handles.md#rotation-handle)
*  [Scale Handle](transform-handles.md#scale-handle)  
*  [Locking Axes](transform-handles.md#lock-axes)
*  [Scene Gizmo](transform-handles#scene-gizmo)
*  [Selection Gizmo](transform-handles#selection-gizmo)
*  [Grid](transform-handles.md#grid)
*  [Box Selection](transform-handles.md#box-selection)
*  [Runtime Handles Component](transform-handles.md#runtime-handles-component)
*  [Runtime Selection Component](transform-handles.md#runtime-selection-component)
*  [Runtime Scene Component](transform-handles.md#runtime-scene-component)

## [Gizmos](gizmos.md)
*  [Overview](gizmos.md#overview)
*  [Getting Started With Gizmos](gizmos.md#getting-started)
*  [Box Gizmo](gizmos.md#box-gizmo)
*  [Sphere Gizmo](gizmos.md#sphere-gizmo)
*  [Capsule Gizmo](gizmos.md#capsule-gizmo)
*  [Cone Gizmo](gizmos.md#cone-gizmo)
*  [Box Collider Gizmo](gizmos.md#box-collider-gizmo)
*  [Capsule Collider Gizmo](gizmos.md#capsule-collider-gizmo)
*  [Point Light Gizmo](gizmos.md#point-light-gizmo.md)
*  [Spot Light Gizmo](gizmos.md#spot-light-gizmo.md)
*  [Directional Light Gizmo](gizmos.md#directional-light-gizmo)
*  [Audio Source Gizmo](gizmos.md#audio-source-gizmo.md)
*  [Audio Reverb Zone Gimzo](gizmos.md#audio-reverb-zone-gizmo)
*  [Skinned Mesh Renderer Gizmo](gizmos.md#skinned-mesh-renderer-gizmo)

## [Rendering](rendering.md)
*  [IGL](rendering.md#igl)
*  [GLRenderer](rendering.md#glrenderer)
*  [GLCamera](rendering#glcamera)
*  [Runtime Graphics Layer](rendering#runtime-graphics-layer)

## [Common Infrastructure](infrastructure.md)
*  [Overview](infrastructure.md#overview)
*  [Expose To Editor](infrastructure.md#expose-to-editor)
*  [IOC](infrastructure.md#ioc)
*  [Input](infrastructure.md#input)
*  [IRTE interface](infrastructure.md#irte-interface)
*  [RTE Component](infrastructure.md#rte-component)
*  [Runtime Tools](infrastructure.md#runtime-tools)
*  [Runtime Objects](infrastructure.md#runtime-objects)
*  [Runtime Selection](infrastructure.md#runtime-selection)
*  [Runtime Undo](infrastructure.md#runtime-undo)
*  [Drag And Drop](infrastructure.md#drag-and-drop)
*  [Runtime Window](infrastructure.md#runtime-window)

## [Runtime Editor](runtime-editor.md)
*  [Overview](runtime-editor.md#overview)
*  [Runtime Editor](runtime-editor.md#runtime-editor)
*  [RTEDeps](runtime-editor.md#rtedeps)
*  [Window Manager](runtime-editor.md#window-manager)
*  [How To: How to add custom window to Window Manager](runtime-editor.md#how-to-add-custom-window-to-window-manager)
*  [Inspector View](runtime-editor.md#inspector-view)
*  [Game Object Editor](runtime-editor.md#game-object-editor)
*  [Material Editor](runtime-editor.md#material-editor)
*  [Component Editor](runtime-editor.md#component-editor)
*  [Property Editor](runtime-editor.md#property-editor)
*  [How To: Configure Editors](runtime-editor.md#how-to-configure-editors)
*  [How To: Select Component Properties](runtime-editor.md#how-to-select-component-properties)
*  [How To: Create Component Editor](runtime-editor.md#how-to-create-component-edtitor)
*  [Hierarchy View](runtime-editor.md#hierarchy-view)
*  [Project View](runtime-editor.md#project-view)
*  [Console View](runtime-editor.md#console-view)
*  [Scene View](runtime-editor.md#scene-view)
*  [Game View](runtime-editor.md#game-view)
*  [Dialogs](runtime-editor.md#dialogs)


## [Save Load 2](save-load.md)
*  [Overview](save-load.md#overview)
*  [Asset Library](save-load.md#/asset-library)
*  [How To: Create Asset Library](save-load.md#how-to-create-asset-library)
*  [Persistent Classes](save-load.md#persistent-classes)
*  [Project Item](save-load.md#project-item)
*  [Asset Item](save-load.md#asset-item)
*  [Project](save-load.md#project)


## [Dock Panel](dock-panels.md)
*  [Overview](dock-panels.md#overview)
*  [Getting Started](dock-panels.md#getting-started)
*  [Dock Panel](dock-panels.md#dock-panel)
*  [Tab](dock-panels.md#tab)
*  [Tab Preview](dock-panels.md#tab-preview)
*  [Region](dock-panels#region)
*  [Dialog Manager](dock-panels#dialog-manager)

## [Virtualizing TreeView](vtv.md)
*  [Overview](vtv.md#overview)
*  [Getting Started](vtv#getting-started)
*  [Virtualizing Scroll Rect](vtv#virtualizing-scroll-rect)
*  [Virtualizing Items Control](vtv#virtualizing-items-control)
*  [Virtualizing Drop Marker](vtv#virtualizing-drop-marker)
*  [Virtualizing Tree View](vtv#virtualizing-tree-view)
*  [Virtualizing Tree View Item](vtv#virtualizing-tree-view-item)

## [Menu Control](menu-control.md)
*  [Overview](menu-control.md#overview)
*  [Getting Started](menu-control.md#getting-started)
*  [Menu Item](menu-control.md#menu-item)
*  [Menu](menu-control.md#menu)
*  [Main Menu Button](menu-control.md#main-menu-button)
*  [Context Menu Trigger](menu-control.md#context-menu-trigger)





