#Introduction

![Screenshot](img/rteditor/overview/overview.png)

This documentation covers most important parts of Runtime Editor. Unlike previous versions of the documentation, 
I tried to concentrate more on examples rather then documenting each property of each script. [Let me know](mailto:Battlehub@outlook) what examples you would like to see.

Start with following sections:

*  [Getting Started with Transform Handles](transform-handles.md#getting-started)
*  [Getting Started with Runtime Editor](runtime-editor.md#getting-started)
*  [Getting Started with Save & Load](save-load.md#getting-started)
*  [Expose To Editor](infrastructure.md#expose-to-editor)
*  [Event methods](infrastructure.md#event-methods)
*  [IOC](infrastructure.md#ioc)
*  [Runtime Selection](infrastructure.md#runtime-selection)
*  [Runtime Undo](infrastructure.md#runtime-undo)

##List of Features

  * Position, Rotation, Scale Handles. 
  * Grid, Box Selection, Scene Gizmo.
  * Transform Handles __mobile__ &  __Google AR Core__ support.
  * Global & Local coordinates, Local & Center pivot point modes, Vertex & Grid snapping.
  * Gizmos for Colliders, Lights and Audio Sources.
  * Scene navigation, Orthographic & Perspective view.
  * Undo & Redo API.
  * Object selection API.
  * Object life-cycle Events.
  * Play & Edit mode.
  * __Configurable Inspector__.
  * Component & Material editors.
  * 16 Built-in property editors.
  * Add Component control.
  * __Multiple Scenes__ and Cameras support.
  * __Dock Panels__ & Windows Management.
  * Dialogs, Message Boxes, Confirmations.
  * Easy to extend with new windows.
  * __Configurable Main & Context menu__.
  * Fast Virtualizing Tree View for Hierarchy and Project windows.
  * __Configurable Save & Load subsystem__ (almost no coding is required).
  * __Easy to use Project API__.
  * __Static Assets, Asset Bundles and Dynamic Assets support__.
  * Load assets on demand.
  * Multiple Projects support.
  
##Upgrade note

Many breaking changes have been made since version 1.3.2u3. Runtime Save Load and some other parts were completely rewritten because they were too tightly coupled, difficult to extend and maintain.
I suggest you to start with new version. For those of you who are stuck in the middle of development and cannot just start from scratch, please [let me know](mailto:Battlehub@outlook) I will try to do my best to help.

  
##About
Hi, I am [Vadym](https://www.facebook.com/vadim.andriyanov). I made a lot of efforts creating Runtime Editor but this was interesting and rewarding experience. First version of Runtime editor was released in august 2016 and was pretty simplistic. Current version is much more sophisticated but in the same time much more flexible and contains a lot of useful features. 
If you have any questions or suggestions send an email to [Battlehub@outlook](mailto:Battlehub@outlook) or join this [support group](https://t.me/battlehub). I hope you will enjoy using Runtime Editor and it will be helpful.