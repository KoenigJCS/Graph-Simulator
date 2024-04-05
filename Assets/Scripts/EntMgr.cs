using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Unity.VisualScripting;
using UnityEditor.PackageManager;

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

    public override int GetHashCode()
    {
        return base.GetHashCode();
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
    public new TValue this [TKey key]
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

    public override readonly bool Equals(object obj)
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

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

public struct AlgorithmInfo
{
    public int index;
    public List<NodeEnt> bestNodeList;
    public readonly void Clear()
    {
        bestNodeList.Clear();
    }
    public void Start()
    {
        bestNodeList = new List<NodeEnt>();
    }

    public AlgorithmInfo(int newIndex)
    {
        index=newIndex;
        bestNodeList = new List<NodeEnt>();
    }

    // public void SetBestNodes(List<NodeEnt> newNodeList)
    // {
    //     bestNodeList = newNodeList;
    // }

    public AlgorithmInfo(int newIndex, List<NodeEnt> newNodeList)
    {
        index=newIndex;
        bestNodeList=newNodeList;
    }
}
public class EntMgr : MonoBehaviour
{
    public List<NodeEnt> nodeList;
    public static EntMgr inst;
    public Transform entContainer;
    [SerializeField]
    public Map<Road,int> roadDict;
    public Transform lineRendContainer;
    public GameObject lineRendPrefab;
    public List<LineRenderer> multiLineRendererList;
    public List<AlgorithmInfo> algInfoList;
    public static readonly Color white = new(1,1,1,1);
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
        nodeList = new List<NodeEnt>();
        algInfoList = new List<AlgorithmInfo>
        {
            new AlgorithmInfo(0)
        };
        algInfoList[0].Start();
        roadDict = new Map<Road, int>
        {
            DefaultValue = -1
        };
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

    public void AddRoadToDict(Road newRoad, int length)
    {
        roadDict.TryAdd(newRoad,length);
    }
    float curScale = .25f;
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

    public void CleanUpAll()
    {
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



    public void MakeNodeObj(Vector3 newPosition, int color = 0, int newID = -1)
    {
        NodeEnt newNode = Instantiate(PlacementMgr.inst.node,newPosition,Quaternion.identity,PlacementMgr.inst.entContainer).GetComponent<NodeEnt>();
        newNode.transform.localScale=new Vector3(curScale,curScale,1);
        newNode.myID=newID;
        if(color==1)
            newNode.innerRing.GetComponent<SpriteRenderer>().color=Color.grey;
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
        algInfoList[0] = new(algInfoList[0].index,newNodeList);
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
        AddAllPaths();
    }

    public GameObject InstantiatePath(KeyValuePair<Road,int> singleRoad)
    {
        GameObject newPath = Instantiate(PlacementMgr.inst.path,singleRoad.Key.startNode.transform.position,Quaternion.identity,entContainer);
        PathEnt temp = newPath.GetComponent<PathEnt>();

        temp.aNode=singleRoad.Key.startNode;
        temp.bNode=singleRoad.Key.endNode;
        temp.UpdateLength(singleRoad.Value);

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
            DeployPaths();
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
