using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct Road
{
    public NodeEnt startNode;
    public NodeEnt endNode;
    public Road(NodeEnt newStartNode, NodeEnt newEndNode)
    {
        this.startNode = newStartNode;
        this.endNode = newEndNode;
    }
    public static bool operator==(Road lhsRoute, Road rhsRoute)
    {
        return (lhsRoute.startNode==rhsRoute.startNode && lhsRoute.endNode==rhsRoute.endNode) 
            || (lhsRoute.startNode==rhsRoute.endNode && lhsRoute.endNode==rhsRoute.startNode);
    }
    public static bool operator!=(Road lhsRoute, Road rhsRoute)
    {
        return !((lhsRoute.startNode==rhsRoute.startNode && lhsRoute.endNode==rhsRoute.endNode) 
              || (lhsRoute.startNode==rhsRoute.endNode && lhsRoute.endNode==rhsRoute.startNode));
    }
    public override bool Equals(object obj)
    {
        //
        // See the full list of guidelines at
        //   http://go.microsoft.com/fwlink/?LinkID=85237
        // and also the guidance for operator== at
        //   http://go.microsoft.com/fwlink/?LinkId=85238
        //
        
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        Road otherRoad = (Road) obj;
        // TODO: write your implementation of Equals() here
        return (this.startNode==otherRoad.startNode && this.endNode==otherRoad.endNode) 
            || (this.startNode==otherRoad.endNode && this.endNode==otherRoad.startNode);
    }

    public Road Swap()
    {
        Road swappedRoad = new Road(this.endNode,this.startNode);
        return swappedRoad;
    }
}
[Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
	[SerializeField]
	private List<TKey> keys = new List<TKey>();
	
	[SerializeField]
	private List<TValue> values = new List<TValue>();
	
	// save the dictionary to lists
	public void OnBeforeSerialize()
	{
		keys.Clear();
		values.Clear();
		foreach(KeyValuePair<TKey, TValue> pair in this)
		{
			keys.Add(pair.Key);
			values.Add(pair.Value);
		}
	}
	
	// load dictionary from lists
	public void OnAfterDeserialize()
	{
		this.Clear();

		if(keys.Count != values.Count)
			throw new System.Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));

		for(int i = 0; i < keys.Count; i++)
			this.Add(keys[i], values[i]);
    }
}

[Serializable]
public class Map<TKey,TValue> : SerializableDictionary<TKey,TValue>
{
    public TValue this [TKey key]
    {
        get
        {
            if (base.ContainsKey(key) || !isDefaultValueSet)
            {
                return base[key];
            }
            else
            {
                return DefaultValue;
            }
        }
        set { base[key] = value; }
    }

    private bool isDefaultValueSet = false;
    private TValue _DefaultValue;
    public TValue DefaultValue
    {
        get { return _DefaultValue; }
        set { _DefaultValue = value; isDefaultValueSet = true; }
    }
}
[Serializable]
public struct Route
{
    public NodeEnt startNode;
    public NodeEnt endNode;
    public int length;
    public Route(NodeEnt newStartNode, NodeEnt newEndNode, int newLength)
    {
        this.startNode = newStartNode;
        this.endNode = newEndNode;
        this.length = newLength;
    }
    public Route(Road newRoad, int newLength)
    {
        this.startNode = newRoad.startNode;
        this.endNode = newRoad.endNode;
        this.length = newLength;
    }
    public override string ToString() 
    {
        return "NodeA - " + length + " - NodeB";
    }
    public static bool operator==(Route lhsRoute, Route rhsRoute)
    {
        return (lhsRoute.startNode==rhsRoute.startNode && lhsRoute.endNode==rhsRoute.endNode) 
            || (lhsRoute.startNode==rhsRoute.endNode && lhsRoute.endNode==rhsRoute.startNode);
    }
    public static bool operator!=(Route lhsRoute, Route rhsRoute)
    {
        return !((lhsRoute.startNode==rhsRoute.startNode && lhsRoute.endNode==rhsRoute.endNode) 
              || (lhsRoute.startNode==rhsRoute.endNode && lhsRoute.endNode==rhsRoute.startNode));
    }

    public override bool Equals(object obj)
    {
        //
        // See the full list of guidelines at
        //   http://go.microsoft.com/fwlink/?LinkID=85237
        // and also the guidance for operator== at
        //   http://go.microsoft.com/fwlink/?LinkId=85238
        //
        
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        Route otherRoute = (Route) obj;
        // TODO: write your implementation of Equals() here
        return (this.startNode==otherRoute.startNode && this.endNode==otherRoute.endNode) 
            || (this.startNode==otherRoute.endNode && this.endNode==otherRoute.startNode);
    }
}

public class EntMgr : MonoBehaviour
{
    public List<NodeEnt> nodeList;
    public List<PathEnt> pathList;
    public static EntMgr inst;
    public Transform entContainer;
    public int MAXALGORITM = 40000;
    [SerializeField]
    public Map<Road,int> roadDict;
    public LineRenderer pathLineRenderer;
    void Awake()
    {
        inst = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        pathList = new List<PathEnt>();
        nodeList = new List<NodeEnt>();
        roadDict = new Map<Road, int>(){};
        roadDict.DefaultValue = -1;
        GraphMgr.inst.StartGraph();
    }

    // Update is called once per frame
    void Update()
    {
        if(algoritmOnline)
        {
            ProcessHillClimb();
        }
    }

    public void AddPath(PathEnt newPath)
    {
        pathList.Add(newPath);
    }

    public void AddRoadToDict(Road newRoad, int length)
    {
        roadDict.TryAdd(newRoad,length);
    }
    float curScale = 1f;
    public void ChangeNodeScale(float newScale)
    {
        curScale=newScale;
        foreach(NodeEnt singleNode in nodeList)
        {
            singleNode.transform.localScale=new Vector3(newScale,newScale,1);
        }
    }

    public void AddNode(NodeEnt newNode)
    {
        nodeList.Add(newNode);
    }

    public void CleanUpPaths()
    {
        foreach (PathEnt path in pathList)
        {
            path.aNode.myPaths.Remove(path);
            path.bNode.myPaths.Remove(path);
            Destroy(path.gameObject);
        }
        pathList.Clear();
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        Debug.Log("The current: "+GetRouteListLength(bestRouteList));
    }

    public void CleanUpAll()
    {
        CleanUpPaths();
        for(int i=0;i<nodeList.Count;i++)
        {
            Destroy(nodeList[i].gameObject);
        }
        nodeList.Clear();
        roadDict.Clear();
        pathLineRenderer.positionCount=0;
    }

    public void MakeNodeObj(Vector3 newPosition)
    {
        NodeEnt newNode = Instantiate(PlacementMgr.inst.node,newPosition,Quaternion.identity,PlacementMgr.inst.entContainer).GetComponent<NodeEnt>();
        newNode.transform.localScale=new Vector3(curScale,curScale,1);
        AddNode(newNode);
    }
    public void DeployPaths()
    {
        foreach(var singleRoad in roadDict)
        {
            InstantiatePath(singleRoad);
        }
    }
    
    public void UpdateLineSize(float size)
    {
        pathLineRenderer.startWidth=.2f*size;
        pathLineRenderer.endWidth=.2f*size;
        
    }

    public void CreatePathLine(List<NodeEnt> nodeList)
    {
        pathLineRenderer.positionCount=nodeList.Count;
        for(int i = 0; i<nodeList.Count;i++)
        {
            pathLineRenderer.SetPosition(i,nodeList[i].transform.position + new Vector3(0,0,2));
        }
    }

    public GameObject InstantiatePath(Route pathRoute)
    {
        GameObject newPath = Instantiate(PlacementMgr.inst.path,pathRoute.startNode.transform.position,Quaternion.identity,entContainer);
        PathEnt temp = newPath.GetComponent<PathEnt>();

        temp.aNode=pathRoute.startNode;
        temp.bNode=pathRoute.endNode;
        temp.UpdateLength(pathRoute.length);

        AddPath(temp);
        pathRoute.startNode.AddPath(temp);
        pathRoute.endNode.AddPath(temp);
        GameObject end = pathRoute.endNode.gameObject;
        Vector2 endPosition = end.transform.position;
        //float dist =(newPath.transform.position-end.transform.position).magnitude;
        float xDist =newPath.transform.position.x-endPosition.x;
        float yDist =newPath.transform.position.y-endPosition.y;
        newPath.transform.localScale= new Vector3(newPath.transform.localScale.x,pathRoute.length,newPath.transform.localScale.z);
        //newPath.transform.localScale= new Vector3(newPath.transform.localScale.x,dist,newPath.transform.localScale.z);
        newPath.transform.eulerAngles= new Vector3(newPath.transform.eulerAngles.x,newPath.transform.eulerAngles.y,(Mathf.Atan2(yDist,xDist)*Mathf.Rad2Deg)-90);
        temp.UpdateColor();
        return newPath;
    }

    public GameObject InstantiatePath(KeyValuePair<Road,int> singleRoad)
    {
        GameObject newPath = Instantiate(PlacementMgr.inst.path,singleRoad.Key.startNode.transform.position,Quaternion.identity,entContainer);
        PathEnt temp = newPath.GetComponent<PathEnt>();

        temp.aNode=singleRoad.Key.startNode;
        temp.bNode=singleRoad.Key.endNode;
        temp.UpdateLength(singleRoad.Value);

        AddPath(temp);
        singleRoad.Key.startNode.AddPath(temp);
        singleRoad.Key.endNode.AddPath(temp);
        GameObject end = singleRoad.Key.endNode.gameObject;
        Vector2 endPosition = end.transform.position;
        float dist =(newPath.transform.position-end.transform.position).magnitude;
        float xDist =newPath.transform.position.x-endPosition.x;
        float yDist =newPath.transform.position.y-endPosition.y;
        newPath.transform.localScale= new Vector3(newPath.transform.localScale.x,dist,newPath.transform.localScale.z);
        newPath.transform.eulerAngles= new Vector3(newPath.transform.eulerAngles.x,newPath.transform.eulerAngles.y,(Mathf.Atan2(yDist,xDist)*Mathf.Rad2Deg)-90);
        temp.UpdateColor();
        return newPath;
    }

    

    public void AddAllPaths()
    {
        foreach(NodeEnt node in nodeList)
        {
            foreach (NodeEnt otherNode in nodeList)
            {
                if(node!=otherNode)
                {
                    Road tempRoad = new Road(node,otherNode);
                    if(!roadDict.ContainsKey(tempRoad) && !roadDict.ContainsKey(tempRoad.Swap()))
                        roadDict.Add(tempRoad,Mathf.RoundToInt((node.transform.position-otherNode.transform.position).magnitude));
                }
            }
        }
        if(PlacementMgr.inst.pathRender)
        {
            CleanUpPaths();
            DeployPaths();
        }
    }
    public void ToggleHillClimb()
    {
        if(!algoritmOnline && nodeList.Count>2 && SelectionMgr.inst.selectedNodes.Count==0)
            HillClimbAlgoritm(nodeList);
        else if(!algoritmOnline && SelectionMgr.inst.selectedNodes.Count>2)
            HillClimbAlgoritm(SelectionMgr.inst.selectedNodes);
        else
        {
            algoritmOnline=false;
            bestNodeList = new List<NodeEnt>();
            bestRouteList = new List<Route>();
        }
    }

    public bool algoritmOnline = false;
    List<NodeEnt> bestNodeList = new List<NodeEnt>();
    List<Route> bestRouteList = new List<Route>();
    public void HillClimbAlgoritm(List<NodeEnt> listToClimb)
    {
        roadDict.Clear();
        pathLineRenderer.positionCount=0;
        //SETUP PHASE
        if(seed!=0)
                UnityEngine.Random.InitState(seed);
        GraphMgr.inst.StartGraph();
        GraphMgr.inst.scores.Clear();
        CleanUpPaths();
        AddAllPaths();
        List<NodeEnt> templist = new List<NodeEnt>(listToClimb);
        
        for(int i=0;i<listToClimb.Count;i++)
        {
            int index1 = UnityEngine.Random.Range(0,templist.Count);
            bestNodeList.Add(templist[index1]);
            templist.RemoveAt(index1);
        }
        
        for (int i=0;i<bestNodeList.Count-1;i++)
        {
            Road tempRoad = new Road(bestNodeList[i],bestNodeList[i+1]);
            int newLength = roadDict[tempRoad];
            if(newLength==-1)
                Debug.LogError("No Valid Road Found");
            else
                bestRouteList.Add(new Route(tempRoad,newLength));
        }

        Road lastRoad = new Road(bestNodeList[bestNodeList.Count-1],bestNodeList[0]);
        int lastLength = roadDict[lastRoad];
        if(lastLength==-1)
            Debug.LogError("No Valid Road Found");
        else
            bestRouteList.Add(new Route(lastRoad,lastLength));
   
        // foreach(Route singleRoute in bestRouteList)
        // {
        //     InstantiatePath(singleRoute).GetComponent<PathEnt>().SetColor(Color.yellow);
        // }
        int bestLength = GetRouteListLength(bestRouteList);
        GraphMgr.inst.scores.Add(bestLength);
        Debug.Log("Started With: "+bestLength);
        //Begin Improvement
        algoritmOnline=true;
    }
    bool newLines = false;
    int seed = 0;
    void ProcessHillClimb()
    {
        int bestLength = GetRouteListLength(bestRouteList);
        int calculatedMAX = (MAXALGORITM/bestRouteList.Count)+1;
        for(int i = 0; i<calculatedMAX;i++)
        {
            //swap
            List<Route> newRouteList = new List<Route>();
            List<NodeEnt> newNodeList = new List<NodeEnt>(bestNodeList);
            int index1 = UnityEngine.Random.Range(0,newNodeList.Count);
            int index2 = UnityEngine.Random.Range(0,newNodeList.Count);
            NodeEnt temp = newNodeList[index1];
            newNodeList[index1]=newNodeList[index2];
            newNodeList[index2]=temp;

            for (int o=0;o<newNodeList.Count-1;o++)
            {
                Road tempRoad = new Road(newNodeList[o],newNodeList[o+1]);
                int tempLength = roadDict[tempRoad];
                if(tempLength==-1)
                    Debug.LogError("No Valid Road Found");
                else
                    newRouteList.Add(new Route(tempRoad,tempLength));
            }

            Road lastRoad = new Road(newNodeList[newNodeList.Count-1],newNodeList[0]);
            int lastLength = roadDict[lastRoad];
            if(lastLength==-1)
                Debug.LogError("No Valid Road Found");
            else
                newRouteList.Add(new Route(lastRoad,lastLength));

            /*

            This is a optimization but it needs more work :(

            
            // Road tempRoad1,tempRoad2;
            // int newLength1,newLength2;
            // if(index1==bestNodeList.Count-1)
            // {
            //     tempRoad1 = new Road(bestNodeList[index1],bestNodeList[0]);
            //     tempRoad2 = new Road(bestNodeList[index1-1],bestNodeList[index1]);
            // }
            // else if(index1==0)
            // {
            //     tempRoad1 = new Road(bestNodeList[0],bestNodeList[1]);
            //     tempRoad2 = new Road(bestNodeList[bestNodeList.Count-1],bestNodeList[0]);
            // }
            // else
            // {
            //     tempRoad1 = new Road(bestNodeList[index1],bestNodeList[index1+1]);
            //     tempRoad2 = new Road(bestNodeList[index1-1],bestNodeList[index1]);
            // }

            // newLength1 = roadDict[tempRoad1];
            // if(newLength1==-1)
            //     newLength1 = roadDict[tempRoad1.Swap()];
            // newLength2 = roadDict[tempRoad2];
            // if(newLength2==-1)
            //     newLength2 = roadDict[tempRoad1.Swap()];
            // if(newLength1==-1 || newLength2==-1)
            //     Debug.LogError("No Valid Road Found");
            // else
            // {
            //     if(index1==0)
            //     {
            //         newRouteList[0] = new Route(tempRoad1,newLength1);
            //         newRouteList[newRouteList.Count-1] = new Route(tempRoad2,newLength2);
            //     }
            //     else
            //     {
            //         newRouteList[index1] = new Route(tempRoad1,newLength1);
            //         newRouteList[index1-1] = new Route(tempRoad2,newLength2);
            //     }
            // }

            // if(index2==bestNodeList.Count-1)
            // {
            //     tempRoad1 = new Road(bestNodeList[index2],bestNodeList[0]);
            //     tempRoad2 = new Road(bestNodeList[index2-1],bestNodeList[index2]);
            // }
            // else if(index2==0)
            // {
            //     tempRoad1 = new Road(bestNodeList[0],bestNodeList[1]);
            //     tempRoad2 = new Road(bestNodeList[bestNodeList.Count-1],bestNodeList[0]);
            // }
            // else
            // {
            //     tempRoad1 = new Road(bestNodeList[index2],bestNodeList[index2+1]);
            //     tempRoad2 = new Road(bestNodeList[index2-1],bestNodeList[index2]);
            // }

            // newLength1 = roadDict[tempRoad1];
            // if(newLength1==-1)
            //     newLength1 = roadDict[tempRoad1.Swap()];
            // newLength2 = roadDict[tempRoad2];
            // if(newLength2==-1)
            //     newLength2 = roadDict[tempRoad1.Swap()];
            // if(newLength1==-1 || newLength2==-1)
            //     Debug.LogError("No Valid Road Found");
            // else
            // {
            //     if(index2==0)
            //     {
            //         newRouteList[0] = new Route(tempRoad1,newLength1);
            //         newRouteList[newRouteList.Count-1] = new Route(tempRoad2,newLength2);
            //     }
            //     else
            //     {
            //         newRouteList[index2] = new Route(tempRoad1,newLength1);
            //         newRouteList[index2-1] = new Route(tempRoad2,newLength2);
            //     }
            // }

            */
            
            int newLength = GetRouteListLength(newRouteList);
            if(newLength<=bestLength)
            {
                newLines=true;
                bestLength=newLength;
                bestNodeList=newNodeList;
                bestRouteList=newRouteList;
                //Debug.Log(bestLength);
                if(bestLength!=GraphMgr.inst.scores[GraphMgr.inst.scores.Count-1])
                {
                    GraphMgr.inst.scores.Add(bestLength);
                    GraphMgr.inst.UpdateGraph();
                }
            }
        }
        if(newLines)
        {
            CleanUpPaths();
            CreatePathLine(bestNodeList);
            newLines=false;
        }
    }

    public int GetRouteListLength(List<Route> routeList)
    {
        int sum = 0;
        foreach(Route singleRoute in routeList)
        {
            sum+=singleRoute.length;
        }
        return sum;
    }

    public void SetSeed(string newSeed)
    {
        seed=0;
        char[] seedAr = newSeed.ToCharArray();
        for(int i=0;i<seedAr.Length;i++)
            seed+=(int)seedAr[i];
    }
}
