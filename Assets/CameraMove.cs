using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public float scrollSpeed = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<Camera>().transform.Translate(
                    0,
                    Input.GetAxis("Mouse ScrollWheel") * scrollSpeed,
                    0
                );
    }
}
