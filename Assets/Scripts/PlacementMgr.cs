using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlacementMgr : MonoBehaviour
{
    public int placementMode = 0;
    public Transform entContainer;
    int summonObjectType;
    public GameObject node;
    public GameObject path;
    Color placeColor;
    public static PlacementMgr inst;
    public TMPro.TextMeshProUGUI lengthInputText;
    public GameObject inputMenu;
    int lengthCount = 0;
    void Awake()
    {
        inst = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        placeColor=Color.red;
        inputMenu.SetActive(false);
    }
    public GameObject targetedPath = null;
    PathEnt curPath = null;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Escape))
        {
            Cursor.lockState=CursorLockMode.None;
            lengthCount=0;
            inputMenu.SetActive(false);
            lengthInputText.text="Input Length";
            if(curPath!=null)
                curPath.UpdateColor();
            curPath=null;
            if(placementMode==4)
                placementMode=3;
            
        }
        if(Input.GetMouseButtonDown(0) && !IsMouseOverUI())
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint( Input.mousePosition );
            Debug.Log(worldPoint);
            if(placementMode==1)
            {
                int mask = 1 << 6;
                RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero,Mathf.Infinity,mask);
                if(summonObjectType==1)
                {
                    EntMgr.inst.AddNode(Instantiate(node,worldPoint,Quaternion.identity,entContainer).GetComponent<NodeEnt>());
                }
                else if(summonObjectType==2 && hit.collider != null)
                {
                    targetedPath = Instantiate(path,hit.collider.transform.position,Quaternion.identity,entContainer);
                    targetedPath.GetComponent<PathEnt>().aNode=hit.transform.GetComponent<NodeEnt>();
                    EntMgr.inst.AddPath(targetedPath.GetComponent<PathEnt>());
                    hit.transform.GetComponent<NodeEnt>().AddPath(targetedPath.GetComponent<PathEnt>());
                }
            }
            else if (placementMode==2)
            {
                int mask = 1 << 6;
                RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero,Mathf.Infinity,mask);
                if(hit.collider != null)
                {
                    hit.transform.GetComponent<NodeEnt>().UpdateColor(placeColor);
                    // SpriteRenderer sp = hit.transform.GetComponent<SpriteRenderer>();
                    // sp.color=placeColor;
                }
            }
            else if(placementMode==3)
            {
                int mask = 1 << 7;
                RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero,Mathf.Infinity,mask);
                if(hit.collider != null)
                {
                    Cursor.lockState=CursorLockMode.Locked;
                    curPath = hit.transform.GetComponent<PathEnt>();
                    inputMenu.SetActive(true);
                    placementMode=4;
                    curPath.SetColor(Color.cyan);
                }
            }
        }
        if(placementMode==4 && Input.anyKeyDown)
        {
            if(curPath==null)
                placementMode=0;
            if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                curPath.UpdateLength(lengthCount);
                Cursor.lockState=CursorLockMode.None;
                lengthCount=0;
                inputMenu.SetActive(false);
                lengthInputText.text="Input Length";
                if(curPath!=null)
                    curPath.UpdateColor();
                curPath=null;
                if(placementMode==4)
                    placementMode=3;
                
            }
            else
            {
                for(int i = 48; i<58;i++)
                {
                    if(Input.GetKeyDown((KeyCode)(i)))
                    {
                        lengthCount*=10;
                        lengthCount+=(i-48);
                        lengthInputText.text=lengthCount.ToString();
                    }
                }
                for(int i = 256; i<266;i++)
                {
                    if(Input.GetKeyDown((KeyCode)(i)))
                    {
                        lengthCount*=10;
                        lengthCount+=(i-256);
                        lengthInputText.text=lengthCount.ToString();
                    }
                }
                
            }
        }
        if(Input.GetMouseButton(0))
        {
            if(targetedPath!=null)
            {
                Vector2 worldPoint = Camera.main.ScreenToWorldPoint( Input.mousePosition );
                Debug.Log(worldPoint);
                int mask = 1 << 6;
                RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero,Mathf.Infinity,mask);
                if(hit.collider == null)
                {
                    float dist =(targetedPath.transform.position-Camera.main.ScreenToWorldPoint( Input.mousePosition )).magnitude;
                    float xDist =targetedPath.transform.position.x-worldPoint.x;
                    float yDist =targetedPath.transform.position.y-worldPoint.y;
                    targetedPath.transform.localScale= new Vector3(targetedPath.transform.localScale.x,dist,targetedPath.transform.localScale.z);
                    targetedPath.transform.eulerAngles= new Vector3(targetedPath.transform.eulerAngles.x,targetedPath.transform.eulerAngles.y,(Mathf.Atan2(yDist,xDist)*Mathf.Rad2Deg)-90);
                }
                else
                {
                    Vector2 target = hit.transform.position;
                    float dist =(targetedPath.transform.position-hit.transform.position).magnitude;
                    float xDist =targetedPath.transform.position.x-target.x;
                    float yDist =targetedPath.transform.position.y-target.y;
                    targetedPath.transform.localScale= new Vector3(targetedPath.transform.localScale.x,dist,targetedPath.transform.localScale.z);
                    targetedPath.transform.eulerAngles= new Vector3(targetedPath.transform.eulerAngles.x,targetedPath.transform.eulerAngles.y,(Mathf.Atan2(yDist,xDist)*Mathf.Rad2Deg)-90);
                }
            }
        }
        if(Input.GetMouseButtonUp(0))
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint( Input.mousePosition );
            Debug.Log(worldPoint);
            int mask = 1 << 6;
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero,Mathf.Infinity,mask);
            if(hit.collider == null)
            {
                if(targetedPath!=null)
                {
                    targetedPath.GetComponent<PathEnt>().aNode.myPaths.Remove(targetedPath.GetComponent<PathEnt>());
                    Destroy(targetedPath);
                }
            }
            else
            {
                NodeEnt bNode = hit.transform.GetComponent<NodeEnt>();
                if(targetedPath.GetComponent<PathEnt>().aNode==bNode)
                {
                    targetedPath.GetComponent<PathEnt>().aNode.myPaths.Remove(targetedPath.GetComponent<PathEnt>());
                    Destroy(targetedPath);
                }
                else
                {
                    targetedPath.GetComponent<PathEnt>().bNode=bNode;
                    bNode.AddPath(targetedPath.GetComponent<PathEnt>());
                    targetedPath.GetComponent<PathEnt>().UpdateColor();
                }
            }
            targetedPath=null;
        }
    }   

    public void SetColor(int newColor)
    {
        switch (newColor)
        {
            case 1:
                placeColor=Color.red;    
                break;
            case 2:
                placeColor=Color.green;    
                break;
            case 3:
                placeColor=Color.blue;    
                break;
            default:
                break;
        }
    }

    public void SetSummon(int objectType)
    {
        summonObjectType=objectType; 
    }

    public void SetMode(int mode)
    {
        placementMode=mode;
    }

    bool IsMouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
}
