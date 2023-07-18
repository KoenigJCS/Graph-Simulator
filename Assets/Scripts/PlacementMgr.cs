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
    public GameObject inputMenu;
    public GameObject configMenu;
    int lengthCount = 0;
    public bool pathRender = false;
    public TMPro.TextMeshProUGUI lengthInputText;
    void Awake()
    {
        inst = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        placeColor=Color.red;
        inputMenu.SetActive(false);
        configMenu.SetActive(false);
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
                else if(summonObjectType==2 && hit.collider != null && pathRender)
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
            if(placementMode==5 && !EntMgr.inst.algoritmOnline)
            {
                Vector2 worldPoint = Camera.main.ScreenToWorldPoint( Input.mousePosition );
                int mask = 1 << 6;
                RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero,Mathf.Infinity,mask);
                if(hit.collider != null)
                {
                    NodeEnt deletedNode = hit.transform.GetComponent<NodeEnt>();
                    EntMgr.inst.nodeList.Remove(deletedNode);
                    foreach (NodeEnt singleNode in EntMgr.inst.nodeList)
                    {
                        Road tempRoad = new Road(deletedNode,singleNode);
                        if(EntMgr.inst.roadDict.ContainsKey(tempRoad))
                            EntMgr.inst.roadDict.Remove(tempRoad);
                        else if(EntMgr.inst.roadDict.ContainsKey(tempRoad.Swap()))
                            EntMgr.inst.roadDict.Remove(tempRoad.Swap());
                    }
                    Destroy(deletedNode.gameObject);
                }
            }
            if(targetedPath!=null)
            {
                Vector2 worldPoint = Camera.main.ScreenToWorldPoint( Input.mousePosition );
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
            int mask = 1 << 6;
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero,Mathf.Infinity,mask);
            if(hit.collider == null)
            {
                if(targetedPath!=null)
                {
                    targetedPath.GetComponent<PathEnt>().aNode.myPaths.Remove(targetedPath.GetComponent<PathEnt>());
                    EntMgr.inst.pathList.Remove(targetedPath.GetComponent<PathEnt>());
                    Destroy(targetedPath);
                }
            }
            else if(targetedPath!=null)
            {
                NodeEnt bNode = hit.transform.GetComponent<NodeEnt>();
                PathEnt tempPath = targetedPath.GetComponent<PathEnt>();
                tempPath.bNode=bNode;
                Road tempRoad = new Road(tempPath.aNode,tempPath.bNode);
                //Route tempRoute = new Route(tempPath.aNode,tempPath.bNode,tempPath.length);
                if(EntMgr.inst.roadDict.ContainsKey(tempRoad) || EntMgr.inst.roadDict.ContainsKey(tempRoad.Swap()))
                {
                    targetedPath.GetComponent<PathEnt>().aNode.myPaths.Remove(targetedPath.GetComponent<PathEnt>());
                    EntMgr.inst.pathList.Remove(targetedPath.GetComponent<PathEnt>());
                    Destroy(targetedPath);
                }
                else
                {
                    EntMgr.inst.AddRoadToDict(tempRoad,tempPath.length);
                    tempPath.UpdateLength(Mathf.RoundToInt((tempPath.aNode.transform.position-bNode.transform.position).magnitude));
                    bNode.AddPath(tempPath);
                    tempPath.UpdateColor();
                }
            }
            targetedPath=null;
        }
    }   

    public void SetColor(int newColor)
    {
        placementMode=2;
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
        placementMode=1;
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

    public void ToggleMenu()
    {
        configMenu.SetActive(!configMenu.activeSelf);
    }

    public void AddAllPaths()
    {

    }

    public void SetPathRender( bool state)
    {
        pathRender=state;
        if(!pathRender)
            EntMgr.inst.CleanUpPaths();
        else
            EntMgr.inst.DeployPaths();
        
    }
}
