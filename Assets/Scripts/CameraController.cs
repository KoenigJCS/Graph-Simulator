using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float cameraSpeed = 1f;
    public float scrollSpeed = 10f;
    //public TextMesh scaleText;
    public GameObject gameObj;
    public Camera cam;
    public Vector3 position;
    // Start is called before the first frame update
    void Start()
    {
        position=gameObj.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.A))
        {
            position.x-=cameraSpeed * Time.deltaTime * cam.orthographicSize;
        }
        if(Input.GetKey(KeyCode.W))
        {
            position.y+=cameraSpeed* Time.deltaTime * cam.orthographicSize;
        }
        if(Input.GetKey(KeyCode.D))
        {
            position.x+=cameraSpeed* Time.deltaTime * cam.orthographicSize;
        }
        if(Input.GetKey(KeyCode.S))
        {
            position.y-=cameraSpeed* Time.deltaTime * cam.orthographicSize;
        }
        if(Input.GetKey(KeyCode.UpArrow))
        {
            cam.orthographicSize+=scrollSpeed * Time.deltaTime;
        }
        if(Input.GetKey(KeyCode.DownArrow))
        {
            cam.orthographicSize-=scrollSpeed * Time.deltaTime;
        }
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize,1,10000);
        gameObj.transform.position=position;
    }
}
