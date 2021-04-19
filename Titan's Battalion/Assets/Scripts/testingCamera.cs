using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testingCamera : MonoBehaviour
{
    public Camera cam1, cam2;
    // Start is called before the first frame update
    void Start()
    {
        cam2.enabled = false;
        cam1.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            cam1.enabled = !cam1.enabled;
            cam2.enabled = !cam2.enabled;
        }
    }
}
