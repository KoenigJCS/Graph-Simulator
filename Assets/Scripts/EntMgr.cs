using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
public class Map<TKey,TValue> : Dictionary<TKey,TValue>
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
    public List<Road> roadList;
    public List<Road> routeList;
    public static EntMgr inst;
    public Transform entContainer;
    public int MAXALGORITM = 40000;
    public Map<Road,int> roadDict;
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

    public void DeployPaths()
    {
        foreach(var singleRoad in roadDict)
        {
            InstantiatePath(singleRoad);
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
        float dist =(newPath.transform.position-end.transform.position).magnitude;
        float xDist =newPath.transform.position.x-endPosition.x;
        float yDist =newPath.transform.position.y-endPosition.y;
        newPath.transform.localScale= new Vector3(newPath.transform.localScale.x,dist,newPath.transform.localScale.z);
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
        if(!algoritmOnline)
            HillClimbAlgoritm();
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
    public void HillClimbAlgoritm()
    {
        //SETUP PHASE
        GraphMgr.inst.StartGraph();
        GraphMgr.inst.scores.Clear();
        CleanUpPaths();
        AddAllPaths();
        List<NodeEnt> templist = new List<NodeEnt>(nodeList);
        
        for(int i=0;i<nodeList.Count;i++)
        {
            int index1 = Random.Range(0,templist.Count);
            bestNodeList.Add(templist[index1]);
            templist.RemoveAt(index1);
        }
        
        for (int i=0;i<bestNodeList.Count-1;i++)
        {
            Road tempRoad = new Road(bestNodeList[i],bestNodeList[i+1]);
            int newLength = roadDict[tempRoad];
            if(newLength==-1)
                newLength = roadDict[tempRoad.Swap()];
            if(newLength==-1)
                Debug.LogError("No Valid Road Found");
            else
                bestRouteList.Add(new Route(tempRoad,newLength));
        }

        Road lastRoad = new Road(bestNodeList[bestNodeList.Count-1],bestNodeList[0]);
        int lastLength = roadDict[lastRoad];
        if(lastLength==-1)
            lastLength = roadDict[lastRoad.Swap()];
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
    void ProcessHillClimb()
    {
        int bestLength = GetRouteListLength(bestRouteList);
        for(int i = 0; i<MAXALGORITM;i++)
        {
            //swap
            List<Route> newRouteList = new List<Route>();
            int index1 = Random.Range(0,bestNodeList.Count);
            int index2 = Random.Range(0,bestNodeList.Count);
            NodeEnt temp = bestNodeList[index1];
            bestNodeList[index1]=bestNodeList[index2];
            bestNodeList[index2]=temp;

            for (int o=0;o<bestNodeList.Count-1;o++)
            {
                Road tempRoad = new Road(bestNodeList[o],bestNodeList[o+1]);
                int tempLength = roadDict[tempRoad];
                if(tempLength==-1)
                    tempLength = roadDict[tempRoad.Swap()];
                if(tempLength==-1)
                    Debug.LogError("No Valid Road Found");
                else
                    newRouteList.Add(new Route(tempRoad,tempLength));
            }

            Road lastRoad = new Road(bestNodeList[bestNodeList.Count-1],bestNodeList[0]);
            int lastLength = roadDict[lastRoad];
            if(lastLength==-1)
                lastLength = roadDict[lastRoad.Swap()];
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
                bestRouteList=newRouteList;
                Debug.Log(bestLength);
                if(bestLength!=GraphMgr.inst.scores[GraphMgr.inst.scores.Count-1])
                    GraphMgr.inst.scores.Add(bestLength);
            }
        }
        if(newLines)
        {
            CleanUpPaths();
            foreach(Route singleRoute in bestRouteList)
            {
                InstantiatePath(singleRoute).GetComponent<PathEnt>().SetColor(Color.yellow);
            }
            GraphMgr.inst.UpdateGraph();
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
}
