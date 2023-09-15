using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Unity.VisualScripting;

[System.Serializable]
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
[System.Serializable]
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
			throw new System.Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are System.Serializable."));

		for(int i = 0; i < keys.Count; i++)
			this.Add(keys[i], values[i]);
    }
}

[System.Serializable]
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
[System.Serializable]
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

public struct AlgorithmInfo
{
    public int index;
    public List<NodeEnt> bestNodeList;
    public List<Route> bestRouteList;
    public readonly void Clear()
    {
        bestNodeList.Clear();
        bestRouteList.Clear();
    }
    public void Start()
    {
        bestNodeList = new List<NodeEnt>();
        bestRouteList  = new List<Route>();
    }

    public AlgorithmInfo(int newIndex)
    {
        index=newIndex;
        bestNodeList = new List<NodeEnt>();
        bestRouteList  = new List<Route>();
    }

    public void SetBestNodes(List<NodeEnt> newNodeList)
    {
        bestNodeList = newNodeList;
    }

    public AlgorithmInfo(int newIndex, List<NodeEnt> newNodeList, List<Route> newRouteList)
    {
        index=newIndex;
        bestNodeList=newNodeList;
        bestRouteList=newRouteList;
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
    public Transform lineRendContainer;
    public GameObject lineRendPrefab;
    public List<LineRenderer> multiLineRendererList;
    public List<AlgorithmInfo> algInfoList;
    //public LineRenderer multiLineRendererList[0];
    public int algSeed = 0;
    void Awake()
    {
        inst = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        multiLineRendererList.Clear();
        multiLineRendererList.Add(Instantiate(lineRendPrefab,Vector3.zero,Quaternion.identity,lineRendContainer).GetComponent<LineRenderer>());
        pathList = new List<PathEnt>();
        nodeList = new List<NodeEnt>();
        algInfoList = new List<AlgorithmInfo>();
        algInfoList.Add(new AlgorithmInfo(0));
        algInfoList[0].Start();
        roadDict = new Map<Road, int>(){};
        roadDict.DefaultValue = -1;
        GraphMgr.inst.StartGraph();
        listOFunctionsToRunInMain = new Queue<System.Action>();
    }

    // Update is called once per frame
    void Update()
    {
        while(listOFunctionsToRunInMain.Count>0)
        {
            System.Action functionToRun =  listOFunctionsToRunInMain.Dequeue();
            functionToRun();
        }
    }
    Queue<System.Action> listOFunctionsToRunInMain;
    public void QueueMainThreadFunction(System.Action someFunction)
    {
        listOFunctionsToRunInMain.Enqueue(someFunction);
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

    public void CleanUpAll()
    {
        CleanUpPaths();
        for(int i=0;i<nodeList.Count;i++)
        {
            Destroy(nodeList[i].gameObject);
        }
        nodeList.Clear();
        roadDict.Clear();
        foreach (var singleLineRend in multiLineRendererList)
        {
            singleLineRend.positionCount=0;
        }
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
        multiLineRendererList[0].startWidth=.02f*size;
        multiLineRendererList[0].endWidth=.02f*size;
        
    }

    public void CreatePathLine()
    {
        multiLineRendererList[0].positionCount=algInfoList[0].bestNodeList.Count;
        for(int i = 0; i<algInfoList[0].bestNodeList.Count;i++)
        {
            multiLineRendererList[0].SetPosition(i,algInfoList[0].bestNodeList[i].transform.position + new Vector3(0,0,2));
        }
    }

    public void SetBestNodes(List<NodeEnt> newNodeList)
    {
        algInfoList[0] = new(algInfoList[0].index,newNodeList,algInfoList[0].bestRouteList);
    }

    public void SetUpForAlgorithm()
    {
        roadDict.Clear();
        multiLineRendererList[0].positionCount=0;
        //SETUP PHASE
        if(algSeed!=0)
                UnityEngine.Random.InitState(algSeed);
        GraphMgr.inst.StartGraph();
        GraphMgr.inst.scores.Clear();
        CleanUpPaths();
        AddAllPaths();
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

    //private volatile bool workInProgress = false;
    public Thread t1;
    public Thread t2;
 
    public volatile bool t1RunFlag = false;
    //private volatile bool cancelFlag = false;
    private static int _tracker = 0;
    private static ThreadLocal<System.Random> _random = new ThreadLocal<System.Random>(() => {
        var seed = (int)(System.Environment.TickCount & 0xFFFFFF00 | (byte)(Interlocked.Increment(ref _tracker) % 255));
        var random = new System.Random(seed);
        return random;
    });

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
        if(!t1RunFlag && nodeList.Count>2 && SelectionMgr.inst.selectedNodes.Count==0)
        {
            HillClimbAlgoritm(nodeList);
            t1 = new Thread(ProcessHillClimb) {Name = "Thread 1"};
            t1.Start();
            t1RunFlag=true;
        }
        else if(!t1RunFlag && SelectionMgr.inst.selectedNodes.Count>2)
        {
            HillClimbAlgoritm(SelectionMgr.inst.selectedNodes);
            t1 = new Thread(ProcessHillClimb) {Name = "Thread 1"};
            t1.Start();
            t1RunFlag=true;
        }
        else
        {
            t1RunFlag=false;
            t1.Abort();
            foreach (var singleAlgInfo in algInfoList)
            {
                singleAlgInfo.Clear();
            }
        }
    }
    public void HillClimbAlgoritm(List<NodeEnt> listToClimb)
    {
        SetUpForAlgorithm();
        
        List<NodeEnt> templist = new(listToClimb);
        
        for(int i=0;i<listToClimb.Count;i++)
        {
            int index1 = Random.Range(0,templist.Count);
            algInfoList[0].bestNodeList.Add(templist[index1]);
            templist.RemoveAt(index1);
        }
        
        for (int i=0;i<algInfoList[0].bestNodeList.Count-1;i++)
        {
            Road tempRoad = new(algInfoList[0].bestNodeList[i],algInfoList[0].bestNodeList[i+1]);
            int newLength = roadDict[tempRoad];
            if(newLength==-1)
                Debug.LogError("No Valid Road Found");
            else
                algInfoList[0].bestRouteList.Add(new Route(tempRoad,newLength));
        }

        Road lastRoad = new(algInfoList[0].bestNodeList[^1],algInfoList[0].bestNodeList[0]);
        int lastLength = roadDict[lastRoad];
        if(lastLength==-1)
            Debug.LogError("No Valid Road Found");
        else
            algInfoList[0].bestRouteList.Add(new Route(lastRoad,lastLength));
   
        // foreach(Route singleRoute in algInfoList[0].bestRouteList)
        // {
        //     InstantiatePath(singleRoute).GetComponent<PathEnt>().SetColor(Color.yellow);
        // }
        int bestLength = GetRouteListLength(algInfoList[0].bestRouteList);
        GraphMgr.inst.scores.Add(bestLength);
        Debug.Log("Started With: "+bestLength);
        //Begin Improvement
        t1RunFlag=true;
    }
    bool newLines = false;
    
    
    void ProcessHillClimb()
    {
        int bestLength = GetRouteListLength(algInfoList[0].bestRouteList);
        //int calculatedMAX = (MAXALGORITM/algInfoList[0].bestRouteList.Count)+1;
        while(t1RunFlag)
        {
            //swap
            List<Route> newRouteList = new();
            List<NodeEnt> newNodeList = new(algInfoList[0].bestNodeList);
            int index1 = _random.Value.Next() % newNodeList.Count;
            int index2 = _random.Value.Next() % newNodeList.Count;
            (newNodeList[index2], newNodeList[index1]) = (newNodeList[index1], newNodeList[index2]);
            for (int o=0;o<newNodeList.Count-1;o++)
            {
                Road tempRoad = new Road(newNodeList[o],newNodeList[o+1]);
                int tempLength = roadDict[tempRoad];
                if(tempLength==-1)
                    tempLength = roadDict[tempRoad.Swap()];
                if(tempLength==-1)
                    Debug.LogError("No Valid Road Found");
                else
                    newRouteList.Add(new Route(tempRoad,tempLength));
            }

            Road lastRoad = new(newNodeList[^1],newNodeList[0]);
            int lastLength = roadDict[lastRoad];
            if(lastLength==-1)
                Debug.LogError("No Valid Road Found");
            else
                newRouteList.Add(new Route(lastRoad,lastLength));
            
            int newLength = GetRouteListLength(newRouteList);
            if(newLength<=bestLength)
            {
                newLines=true;
                bestLength=newLength;
                algInfoList[0] = new AlgorithmInfo(0,newNodeList,newRouteList);
                //Debug.Log(bestLength);
                if(bestLength!=GraphMgr.inst.scores[^1])
                {
                    GraphMgr.inst.scores.Add(bestLength);
                    QueueMainThreadFunction(UpdateGraphWrapper);
                    CleanUpPaths();
                    QueueMainThreadFunction(CreatePathLine);
                    newLines=false;
                }
            }
        }
        
    }

    private void UpdateGraphWrapper()
    {
        GraphMgr.inst.UpdateGraph();
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
        algSeed=0;
        char[] seedAr = newSeed.ToCharArray();
        for(int i=0;i<seedAr.Length;i++)
            algSeed+= seedAr[i];
    }
}
