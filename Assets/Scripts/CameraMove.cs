using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public float scrollSpeed = 0.5f;
    public float zoomSpeed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        GetComponent<Camera>().transform.Translate(
                    0,
                    scroll * scrollSpeed,
                    0
                );
        if (Input.GetKey(KeyCode.LeftControl) && scroll != 0)
        {
            GetComponent<Camera>().orthographicSize += scroll * zoomSpeed;
        }
    }
}
