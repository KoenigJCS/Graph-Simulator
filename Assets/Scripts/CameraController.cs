using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float cameraSpeed = 1f;
    public float scrollSpeed = .1f;
    //public TextMesh scaleText;
    public GameObject gameObj;
    public Camera cam;
    public Vector3 position;
    public TMPro.TextMeshProUGUI scaleText;
    public float startScale;
    public static CameraController inst;
    void Awake() {
        inst=this;
    }
    // Start is called before the first frame update
    void Start()
    {
        position=gameObj.transform.position;
        startScale=cam.orthographicSize;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            cameraSpeed*=3;
            scrollSpeed*=3;
        }
        if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            cameraSpeed/=3;
            scrollSpeed/=3;
        }
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
            cam.orthographicSize+=scrollSpeed * Time.deltaTime * cam.orthographicSize;
            EntMgr.inst.ChangeNodeScale(.25f*cam.orthographicSize/startScale);
            EntMgr.inst.UpdateLineSize(cam.orthographicSize/startScale);
            scaleText.text=((float)((int)(cam.orthographicSize/1.81f))/4).ToString()+"x";
        }
        if(Input.GetKey(KeyCode.DownArrow))
        {
            cam.orthographicSize-=scrollSpeed * Time.deltaTime * cam.orthographicSize;
            EntMgr.inst.ChangeNodeScale(.25f*cam.orthographicSize/startScale);
            EntMgr.inst.UpdateLineSize(cam.orthographicSize/startScale);
            scaleText.text=((float)((int)(cam.orthographicSize/1.81f))/4).ToString()+"x";
        }
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize,1,100000);
        gameObj.transform.position=position;
        
    }

    public void ResizeScreen(float newSize)
    {
        cam.orthographicSize=(newSize/17.5f)*startScale;
        EntMgr.inst.ChangeNodeScale(.25f*cam.orthographicSize/startScale);
        EntMgr.inst.UpdateLineSize(cam.orthographicSize/startScale);
        scaleText.text=((float)((int)(cam.orthographicSize/1.81f))/4).ToString()+"x";
    }

    public void SetLocation(Vector3 pos)
    {
        position=pos + new Vector3(0,0,-10);
    }
}
