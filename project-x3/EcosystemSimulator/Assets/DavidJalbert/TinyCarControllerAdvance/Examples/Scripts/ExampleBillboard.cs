using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleBillboard : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        if (Camera.main == null) return;

        transform.rotation = Camera.main.transform.rotation;
    }
}
