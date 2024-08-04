using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeadowGames.UINodeConnect4.UICSerialization
{
    [ExecuteInEditMode]
    public class DeserializationTemplates : MonoBehaviour
    {
        public Templates<Node> nodeTemplates = new Templates<Node>();
        public Templates<Port> portInTemplates = new Templates<Port>();
        public Templates<Port> portOutTemplates = new Templates<Port>();
        public Templates<Port> portAllTemplates = new Templates<Port>();

        public GraphManager graphManager;
        public bool generateTemplate = false;
        void Update()
        {
            if (generateTemplate)
            {
                generateTemplate = false;
                GenerateTemplatesFromGraphManager(graphManager);
            }
        }

        T InstantiateAndAddTemplate<T>(ref Templates<T> templates, string key, T value) where T : Object, IGraphElement
        {
            if (templates.FindInTemplatesOnly(key) == null)
            {
                T newValue = Instantiate(value, Vector3.zero, Quaternion.identity, transform) as T;
                templates.templates.Add(new Templates<T>.TemplateItem(key, newValue));

                return newValue;
            }

            return default;
        }

        public void GenerateTemplatesFromGraphManager(GraphManager graphManager)
        {
            foreach (Node node in graphManager.localNodes)
            {
                Node instantiatedNode = InstantiateAndAddTemplate<Node>(ref nodeTemplates, node.ID, node);

                foreach (Port port in node.ports)
                {
                    if (port.Polarity == Port.PolarityType._in)
                    {
                        InstantiateAndAddTemplate<Port>(ref portInTemplates, port.ID, port);
                    }
                    else if (port.Polarity == Port.PolarityType._out)
                    {
                        InstantiateAndAddTemplate<Port>(ref portOutTemplates, port.ID, port);
                    }
                    else if (port.Polarity == Port.PolarityType._all)
                    {
                        InstantiateAndAddTemplate<Port>(ref portAllTemplates, port.ID, port);
                    }
                }

                if (instantiatedNode)
                {
                    for (int i = instantiatedNode.ports.Count - 1; i >= 0; i--)
                    {
                        instantiatedNode.ports[i].Remove();
                    }
                }
            }
        }

        [System.Serializable]
        public struct Templates<T>
        {
            [System.Serializable]
            public struct TemplateItem
            {
                public string key;
                public T template;

                public TemplateItem(string key, T template)
                {
                    this.key = key;
                    this.template = template;
                }
            }

            public List<TemplateItem> templates;
            public T defaultTemplate;

            public T Find(string key)
            {
                foreach (TemplateItem template in templates)
                {
                    if (template.key == key)
                    {
                        return template.template;
                    }
                }
                return defaultTemplate;
            }

            public T FindInTemplatesOnly(string key)
            {
                foreach (TemplateItem template in templates)
                {
                    if (template.key == key)
                    {
                        return template.template;
                    }
                }
                return default;
            }
        }
    }
}