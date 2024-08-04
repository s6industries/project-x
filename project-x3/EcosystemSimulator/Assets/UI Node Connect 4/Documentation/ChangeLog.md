Changelog                         {#changelog}
============

v4.1
- DuplicateNodeButtonItem now duplicates connections if they are connected to selected nodes
- removed unecessary GraphManager parameter from Raycaster.RaycastUIAll method
- now the Connection's ElementColor (current color) is set independetly from the defaultColor (color the elements comes back when unselect or hover exit)
- pointer selection priority of Node and Connection switched to improve usability (Nodes are now selected before Connections)
- Connection constructor made public to facilitate serialization
- adde Connection.NewConnection overload method that can receive a template connection as parameter so the style is copied
- adde Port.ConnectTo overload method that can receive a template connection as parameter so the style is copied
- added "updatePortsIcons" optional parameter to Connection.UpdateLine
- Connection.CopyVariables method made obsolete. Use Clone() instead
- Port inheritance changed from Graphc to MaskableGraphic
- UICLineRenderer.AddUIVertexQuad refactored to improved performance
- added method UICLineRenderer.AddUIVertexTriangle to improved performance
- RaycastPortOfOppositPolarity updated to use PortMatchRule if exists in scene
- PortMatchRule extension added to facilitate the implementation of custom rules for connect ports by dragging
- Serialization system added
    - Serializable Types: SerializableNode, SerializablePort, SerializableConnection and SerializableGraph
    - Serializer helper classes: NodeSerializer, PortSerializer, ConnectionSerializer and GraphSerializer
    - DeserializationTemplates component to instantiate nodes and ports based on its ID
    - SerializationEvents: OnGraphSerialize, OnGraphDeserialize, OnNodeSerialize, OnNodeDeserialize, OnPortSerialize, OnPortDeserialize, OnConnectionSerialize and OnConnectionDeserialize
- added GenerateSID method to utils to generate unique ID for serialization
- added TryGetValue method to utils to get values from Dictionary
- added string SID property to IGraphElement to facilitate serialization
- new scene added: Serialization Sample
- bugfix: - connection line position not updating when added using UICSystemManager.AddConnectionToList 
- bugfix: icons not being updated when the connection was created from script
- bugfix: label from the original connections being transfered to the cloned connection
- bugfix: label flipped down when connected ports are horizontally aligned
- bugfix: ElementColor of Port not changing image color

v4.0.2
- bugfix: line.animation.pointsDistance was changed OnSelection if it was outside of the min and max slider range
- added missing namespace to the CustomScrollRect class
- project guids regenerated to  avoid conflict with version 3

v4.0.1
- bugfix: SetWidth being called OnElementSelected and changing the width of the selected Connections 

v4.0
- Major refactoring

v3.2
- connections can be created in editor mode from the scene view by accessing the UIC3_Manager's inspector
- AddLine and RemoveLine methods removed and DrawConnections method added to the UIC3_Manager, connections are now drawn from the ConnectionsList[n].line using the event E_OnPopulateMesh
- UIC3_Manager.CreateConnection made obsolete
- Nodes Alignment extension added with align vertical/horizontal, center even vertical/horizontal, space even vertical/horizontal
- UIC3_Connection constructor adjusted to facilitate initialization of connections from the inspector
- UIC3_ConnectionPoint.haveSpots changed to HasSpots to fix misspell
- connectionsList removed from UIC3_ConnectionPoint, replaced by the Connections property
- added GetOppositeConnectionPoints method to the UIC3_ConnectionPoint
- added SetControlPointPosition and SetControlPointDistanceAngle methods to manipulate the control point position and direction from script
- OnPopulateMesh event added to the line renderer to handle drawing of connection lines, add or remove listener are done with OnPopulateMeshAddListener and OnPopulateMeshRemoveListener methods
- DrawLine of UIC3_LineRenderer made public
- bugfix: nodelist null on start

v3.1.1
- bugfix: fixed unselecting objects when clicked context menu items

v3.1
- I_UIC3_Hover interface added
- new event types OnPointerHoverEnter and OnPointerHoverExit added to the Event Manager
- added public hoverObject to UIC3 Manager
- implementation of hoverColor for the connections and connection points
- OnPointerDown and FindObjectCloserToPointer refactored to enable pointer hover

v3.0
- upgrade first release
- code refactored and methods moved to the proper classes
- object names changed to better reflect the nodegraph concepts: Entity changed to Node, Node changed to ConnectionPoint
- added drag selection
- added support to the New Input System

v2.2
- UIC_ContextMenu made not static to enable multiple charts
- duplicated block is now instantiated with the same parent of the reference block
- EnableDrag property of Nodes is now implemented and works to allow drag of lines from the node
- DisableClick property of Nodes can now be set
- linewidth now varies with the rectTransform scale, following the screen scale
- bugfix: fixed dropdown values not being updated before OnEnable

v2.1
- Working inside ScrollViews and movable panels
- fix: Corrected lines position when LineRenderer is offset
- new scene added: ScrollView Graphs

v2.0
- UIC components refactored to enable multiple UIC views in the same scene
- new scene added: Multiple Charts
- fix: wrong raycaster found when using more than one canvas
- fix: uILines list reset on enable
- method to find closest node by distance made obsolete, using detect nodes from raycast instead of distance to pointer
- new method to find node using raycast from pointer

v1.5
- working on Canvas World Space
- fix: incorrect enabling of button "duplicate"

v1.4
- added method to get entities connected to an specific node polarity
- line animation feature added: {type: Lerp, Constant Speed; number of points; size; color; draw type: Square, Circle, Triangle; speed}
- NodesAreConnected verification method made public
- fix: circle cap with uv wrongly positioned
- context menu line animation active toggle added
- context menu line animation point count slider added
- Logic Gates scene added

v1.3
- UIC_Manager.AddConnection return UIC_Connection
- working on Canvas screen space Overlay and Camera
- added prefab and script to set nodes to be connected On Start
- added duplicate block button on sample scene
- cleaner Node inspector

v1.2.1
- added uiline count limit
- UIC_Node.ConnectTo return UIC_Connection

v1.2
- UIC_LineRenderer code optimization and refactoring
- using static Manager instance to set parent of dropped entity
- added method that sets the number of points in the line based on its curvature
- Added sprite to UI Line Renderer on scenes to minimize jagged sides
- changed from DistanceToSpline(obsolete) to DistanceToConnection, a general and more precise way to find distance to connections independent of the lineType
- fix: excessive frame drop on spline drawing

v1.0.1
- added property for UIC_LineRenderer.UILines with null check
