# Runtime Editor Docs

Welcome to documentation of <a href="http://u3d.as/v9j" target="_blank"><strong>Runtime Editor</strong></a> the set of scripts and prefabs which help you to implement scene editor, game level editor or build your own modeling application. If you are new to this documentation you could proceed to [introduction](general.md#introduction) page to get an overview what Runtime Editor   
and this documentation has to offer.
	
!!! note
		
	If you cannot find something in the documentation or have any questions, send an email to [Battlehub@outlook.com](mailto:Battlehub@outlook.com) or ask directly in this [support group](https://t.me/battlehub). 
	

The documentation is organized as following:

##[General](general.md)
*   [Introduction](general.md#introduction)
*   [List of Features](general.md#list-of-features)
*   [About](general.md#about)

## __Getting Started__
*  [Getting Started with Transform Handles](transform-handles.md#getting-started)
*  [Getting Started with Runtime Editor](runtime-editor.md#getting-started)
*  [Getting Started with Save & Load](save-load.md#getting-started)
*  [Expose To Editor](infrastructure.md#expose-to-editor)
*  [Event methods](infrastructure.md#event-methods)
*  [IOC](infrastructure.md#ioc)
*  [Runtime Selection](infrastructure.md#runtime-selection)
*  [Runtime Undo](infrastructure.md#runtime-undo)

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

## [Common Infrastructure](infrastructure.md)
*  [Overview](infrastructure.md#overview)
*  [Expose To Editor](infrastructure.md#expose-to-editor)
*  [Event methods](infrastructure.md#event-methods)
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
*  [Getting Started](runtime-editor.md#getting-started)
*  [Runtime Editor](runtime-editor.md#runtime-editor)
*  [Main & Context Menu](runtime-editor.md#main-context-menu)
*  [RTEDeps](runtime-editor.md#rtedeps)
*  [Window Manager](runtime-editor.md#window-manager)
*  [How To: How to add custom window to Window Manager](runtime-editor.md#how-to-add-custom-window-to-window-manager)
*  [Inspector View](runtime-editor.md#inspector-view)
*  [How To: Configure Editors](runtime-editor.md#how-to-configure-editors)
*  [How To: Select Component Properties](runtime-editor.md#how-to-select-component-properties)

## [Save Load](save-load.md)
*  [Overview](save-load.md#overview)
*  [Getting Started](save-load.md#getting-started)
*  [Persistent Classes](save-load.md#persistent-classes)
*  [How To: Create Custom Persistent Class](save-load.md#how-to-create-custom-persistent-class)
*  [Asset Library](save-load.md#/asset-library)
*  [How To: Create Asset Library](save-load.md#how-to-create-asset-library)
*  [Project Item & Asset Item](save-load.md#project-item-asset-item)
*  [Project](save-load.md#project)

## [Rendering](rendering.md)
*  [IGL](rendering.md#igl)
*  [GLRenderer](rendering.md#glrenderer)
*  [GLCamera](rendering#glcamera)
*  [Runtime Graphics Layer](rendering#runtime-graphics-layer)

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





