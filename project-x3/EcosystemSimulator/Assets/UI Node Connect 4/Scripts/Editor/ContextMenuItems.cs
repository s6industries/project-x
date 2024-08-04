using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MeadowGames.UINodeConnect4.EditorScript
{
    public class ContextMenuItem : MonoBehaviour
    {
        const string UIC_PATH = "GameObject/UI Node Connect 4/";

        [MenuItem(UIC_PATH + "Create GraphManager", false, 10)]
        static void CreateGraphManager(MenuCommand menuCommand)
        {
            UICSystemManager uicSystemManager = FindObjectOfType<UICSystemManager>();
            if (!uicSystemManager)
            {
                GameObject uicSystemManagerGO = new GameObject("UIC4 System Manager");
                uicSystemManager = uicSystemManagerGO.AddComponent<UICSystemManager>();
                Undo.RegisterCreatedObjectUndo(uicSystemManagerGO, "Create " + uicSystemManagerGO.name);
            }

            InputManager inputManager = FindObjectOfType<InputManager>();
            if (!inputManager)
            {
                uicSystemManager.gameObject.AddComponent<InputManager_LegacyInputSystem>();
                Undo.RegisterCompleteObjectUndo(uicSystemManager.gameObject, "Add UIC input system " + uicSystemManager.gameObject);
            }

            // Create a custom game object
            GameObject graphManagerGO = Instantiate(Resources.Load("Editor GraphManager Template")) as GameObject;
            graphManagerGO.name = "Canvas - UIC4 GraphManager";

            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(graphManagerGO, menuCommand.context as GameObject);

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(graphManagerGO, "Create " + graphManagerGO.name);
            Selection.activeObject = graphManagerGO;

            var eventSystem = FindObjectOfType<EventSystem>();
            if (eventSystem == null)
            {
                var eventSystemGO = new GameObject("EventSystem");
                eventSystem = eventSystemGO.AddComponent<EventSystem>();
                eventSystemGO.AddComponent<StandaloneInputModule>();

                Undo.RegisterCreatedObjectUndo(eventSystemGO, "Create " + eventSystemGO.name);
            }
        }

        [MenuItem(UIC_PATH + "Create Node", false, 10)]
        static void CreateNode(MenuCommand menuCommand)
        {
            // Create a custom game object
            GameObject nodeGO = Instantiate(Resources.Load("Editor Node Template")) as GameObject;
            nodeGO.name = "Node";
            Node node = nodeGO.GetComponent<Node>();

            GraphManager parentGraphManager = Selection.activeTransform.GetComponentInParent<GraphManager>();
            node.graphManager = parentGraphManager;
            foreach (Port port in node.ports)
            {
                port.graphManager = parentGraphManager;
            }

            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(nodeGO, menuCommand.context as GameObject);

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(nodeGO, "Create " + nodeGO.name);
            Selection.activeObject = nodeGO;
        }
        [MenuItem(UIC_PATH + "Create Node", true)]
        static bool ValidateCreateNode(MenuCommand menuCommand)
        {
            return Selection.activeTransform != null && Selection.activeTransform.GetComponentInParent<GraphManager>() != null;
        }

        const string PORTIN = "Port In";
        [MenuItem(UIC_PATH + "Port/Create " + PORTIN, false, 10)]
        static void CreatePortIn(MenuCommand menuCommand)
        {
            // Create a custom game object
            GameObject portGO = Instantiate(Resources.Load("Editor " + PORTIN + " Template")) as GameObject;
            portGO.name = PORTIN;

            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(portGO, menuCommand.context as GameObject);

            Node parentNode = portGO.GetComponentInParent<Node>();

            Port port = portGO.GetComponent<Port>();
            port.node = parentNode;
            port.graphManager = parentNode.graphManager;
            port.SetLocalPositionX(-145, false);
            port.SetLocalPositionY(0.5f, true);

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(portGO, "Create " + portGO.name);
            Selection.activeObject = portGO;
        }
        [MenuItem(UIC_PATH + "Port/Create " + PORTIN, true)]
        static bool ValidateCreatePortIn(MenuCommand menuCommand)
        {
            return Selection.activeTransform != null && Selection.activeTransform.GetComponentInParent<Node>() != null;
        }

        const string PORTOUT = "Port Out";
        [MenuItem(UIC_PATH + "Port/Create " + PORTOUT, false, 10)]
        static void CreatePortOut(MenuCommand menuCommand)
        {
            // Create a custom game object
            GameObject portGO = Instantiate(Resources.Load("Editor " + PORTOUT + " Template")) as GameObject;
            portGO.name = PORTOUT;

            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(portGO, menuCommand.context as GameObject);

            Node parentNode = portGO.GetComponentInParent<Node>();

            Port port = portGO.GetComponent<Port>();
            port.node = parentNode;
            port.SetLocalPositionX(145, false);
            port.SetLocalPositionY(0.5f, true);

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(portGO, "Create " + portGO.name);
            Selection.activeObject = portGO;
        }
        [MenuItem(UIC_PATH + "Port/Create " + PORTOUT, true)]
        static bool ValidateCreatePortOut(MenuCommand menuCommand)
        {
            return Selection.activeTransform != null && Selection.activeTransform.GetComponentInParent<Node>() != null;
        }

        const string PORTALL = "Port All";
        [MenuItem(UIC_PATH + "Port/Create " + PORTALL, false, 10)]
        static void CreatePortAll(MenuCommand menuCommand)
        {
            // Create a custom game object
            GameObject portGO = Instantiate(Resources.Load("Editor " + PORTALL + " Template")) as GameObject;
            portGO.name = PORTALL;

            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(portGO, menuCommand.context as GameObject);

            Node parentNode = portGO.GetComponentInParent<Node>();

            Port port = portGO.GetComponent<Port>();
            port.node = parentNode;
            port.SetLocalPositionX(0.5f, true);
            port.SetLocalPositionY(-115, false);

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(portGO, "Create " + portGO.name);
            Selection.activeObject = portGO;
        }
        [MenuItem(UIC_PATH + "Port/Create " + PORTALL, true)]
        static bool ValidateCreatePortAll(MenuCommand menuCommand)
        {
            return Selection.activeTransform != null && Selection.activeTransform.GetComponentInParent<Node>() != null;
        }
    }
}