using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentLED : MonoBehaviour
{
    public GameObject visual;
    // Start is called before the first frame update
    void Start()
    {
        TurnOff();
        
        // Get the Renderer component from the new cube
        // var ledRenderer = visual.GetComponent<MeshRenderer>();

        // ledRenderer.material.color = Color.red;

       // Call SetColor using the shader property name "_Color" and setting the color to red
    //    ledRenderer.material.SetColor("_Color", Color.red);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TurnOn() {
        visual.GetComponent<MeshRenderer>().material.color = Color.red;

    }

    public void TurnOff() {
        visual.GetComponent<MeshRenderer>().material.color = Color.black;

    }
}
