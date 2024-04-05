using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class NOptAlgorithm : MonoBehaviour
{
    public bool t1RunFlag = false;
    Queue<System.Action> listOFunctionsToRunInMain;
    Queue<string> listOfStringsToLogInMain;
    Queue<int> listOfScoresToQueueInMain;
    public Thread t1;
    int curRouteCost=int.MaxValue;

    private static int _tracker = 0;
    private static ThreadLocal<System.Random> _random = new ThreadLocal<System.Random>(() => {
        var seed = (int)(System.Environment.TickCount & 0xFFFFFF00 | (byte)(Interlocked.Increment(ref _tracker) % 255));
        var random = new System.Random(seed);
        return random;
    });
    // Start is called before the first frame update
    void Start()
    {
        listOFunctionsToRunInMain = new Queue<System.Action>();
        listOfStringsToLogInMain = new();
        listOfScoresToQueueInMain = new();
    }

    // Update is called once per frame
    void Update()
    {
        while(listOFunctionsToRunInMain.Count>0)
        {
            System.Action functionToRun =  listOFunctionsToRunInMain.Dequeue();
            functionToRun();
        }
        while(listOfStringsToLogInMain.Count>0)
        {
            Debug.Log(listOfStringsToLogInMain.Dequeue());
        }
    }
    
    public static NOptAlgorithm inst;
    void Awake()
    {
        inst = this;
    }

    public void QueueMainThreadFunction(System.Action someFunction)
    {
        if(listOFunctionsToRunInMain.Count < 10000)
            listOFunctionsToRunInMain.Enqueue(someFunction);
    }

    public void QueueMainThreadLog(string logMsg)
    {
        if(listOfStringsToLogInMain.Count < 10000)
            listOfStringsToLogInMain.Enqueue(logMsg);
    }

    public void RunNOpt()
    {
        List<NodeEnt> curRoute = new();
        GenRandomRoute(curRoute);
        lock(EntMgr.inst.algInfoList[0].bestNodeList)
        {
            EntMgr.inst.SetBestNodes(curRoute);
        }
        curRouteCost = EvalRoute(curRoute);
        lock(GraphMgr.inst.scores)
        {
            GraphMgr.inst.scores.Add(curRouteCost);
        }
        bool isImproved = true;
        while(isImproved)
        {
            isImproved=false;
            for(int i = 1;i<curRoute.Count-2;i++)
            {
                for(int j = i+1;j<curRoute.Count;j++)
                {
                    List<NodeEnt> newRoute = new(curRoute);
                    newRoute.Reverse(i,j-i);
                    int newCost = EvalRoute(newRoute);
                    if(newCost<curRouteCost)
                    {
                        curRoute=newRoute;
                        isImproved=true;
                        QueueMainThreadLog("Improvment To"+newCost);
                        curRouteCost=newCost;
                        lock(listOfScoresToQueueInMain)
                        {
                            listOfScoresToQueueInMain.Enqueue(curRouteCost);
                        }
                    }
                }
            }
            lock(EntMgr.inst.algInfoList[0].bestNodeList)
            {
                EntMgr.inst.SetBestNodes(curRoute);
            }
            QueueMainThreadFunction(DisplayUpdate);
        }
        QueueMainThreadLog("Value: "+curRouteCost);
    }

    int EvalRoute(List<NodeEnt> nodes)
    {
        int sum = 0;
        for(int i = 0; i<nodes.Count-1;i++)
        {
            Road tempRoad = new Road(nodes[i],nodes[i+1]);
            int newLength = EntMgr.inst.roadDict[tempRoad];
            if(newLength==-1)
                Debug.LogError("Invalid Path!");
            sum+=newLength;
        }

        Road wrapTempRoad = new Road(nodes[^1],nodes[0]);
        int wrapNewLength = EntMgr.inst.roadDict[wrapTempRoad];
        if(wrapNewLength==-1)
            Debug.LogError("Invalid Path!");
        sum+=wrapNewLength;
        return sum;
    }

    public void DisplayUpdate()
    {
        EntMgr.inst.CreatePathLine();
        while(listOfScoresToQueueInMain.Count>0)
        {
            int newScore = listOfScoresToQueueInMain.Dequeue();
            if(GraphMgr.inst.scores[^1]!=newScore)
                GraphMgr.inst.scores.Add(newScore);
        }
        GraphMgr.inst.UpdateGraph();
    }

    public void ToggleRun()
    {
        //Should make a unified manager at some point but atm this is a patch
        if(t1RunFlag)
        {
            t1RunFlag=false;
            if(t1 != null && t1.IsAlive)
                t1.Abort();
            foreach (var singleAlgInfo in EntMgr.inst.algInfoList)
            {
                singleAlgInfo.Clear();
            }
        }
        else if(!t1RunFlag && EntMgr.inst.nodeList.Count>2)
        {
            t1RunFlag=true;
            if(EntMgr.inst.algSeed!=0)
                UnityEngine.Random.InitState(EntMgr.inst.algSeed);
            EntMgr.inst.SetUpForAlgorithm();
            t1 = new Thread(RunNOpt) {Name = "Thread 1"};
            t1.Start();
            //RunGACHC();
        }
    }

    void GenRandomRoute(List<NodeEnt> fillRoute)
    {
        List<NodeEnt> temp;
        lock(EntMgr.inst.nodeList)
        {
            temp = new(EntMgr.inst.nodeList);
        }
        int nodeLenght=temp.Count;
        fillRoute.Add(temp[0]);
        temp.Remove(temp[0]);
        for(int i = 1; i<nodeLenght;i++)
        {
            int index = _random.Value.Next() % temp.Count;
            fillRoute.Add(temp[index]);
            temp.Remove(temp[index]);
        }
    }
    // Update is
}
