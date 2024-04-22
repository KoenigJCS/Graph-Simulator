using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class NOptAlgorithm : MonoBehaviour
{
    public bool t1RunFlag = false;
    Queue<System.Action> listOFunctionsToRunInMain;
    Queue<string> listOfStringsToLogInMain;
    Queue<int> listOfScoresToQueueInMain;
    public List<Thread> threads;
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
        threads = new();
        for(int i=0; i<6;i++)
        {
            curScores.Add(0);
        }
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

    List<int> vehicleSplit = new();

    public void RunNOptColoring(int index = -1)
    {
        List<NodeEnt> curRoute = new();
        List<NodeEnt> lastValidRoute = new();
        List<NodeEnt> colorNodeList;
        if(index==-1)   
        {
            Debug.LogError("ColoringError!");
            return;
        }

        colorNodeList = new(colorSepatatedNodeList[index]);
        GenRandomRoute(curRoute, ref colorNodeList, vehicleSplit[index]);

        lock(EntMgr.inst.algInfoList[0].bestNodeList)
        {
            EntMgr.inst.SetBestNodes(curRoute, index);
        }
                bool lastisValid = false;
        curRouteCost = EvalRoute(curRoute, out lastisValid);
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
                for(int j = i+1;j<curRoute.Count-1;j++)
                {
                    List<NodeEnt> newRoute = new(curRoute);
                    newRoute.Reverse(i,j-i+1);
                    int newCost = EvalRoute(newRoute, out lastisValid);
                    
                    if(newCost>=curRouteCost)
                        continue;
                    
                    curRoute=newRoute;
                    isImproved=true;
                    //QueueMainThreadLog("Improvment To"+newCost);
                    if(lastisValid)
                    {
                        curRouteCost=newCost;
                        lastValidRoute=newRoute;
                        lock(listOfScoresToQueueInMain)
                        {
                            listOfScoresToQueueInMain.Enqueue(curRouteCost);
                        }
                    }
                }
            }
            lock(EntMgr.inst.algInfoList[index].bestNodeList)
            {
                EntMgr.inst.SetBestNodes(lastValidRoute, index);
            }
            QueueMainThreadFunction(() => DisplayUpdate(index));
        }
        QueueMainThreadLog("Value: "+curRouteCost);
    }

    public void RunNOpt()
    {
        List<NodeEnt> curRoute = new();
        List<NodeEnt> lastValidRoute = new();
        if(SelectionMgr.inst.selectedNodes.Count>2)
            GenRandomRoute(curRoute, ref SelectionMgr.inst.selectedNodes, EVRPConverter.inst.evrpData.vehicles);
        else
            GenRandomRoute(curRoute, ref EntMgr.inst.nodeList, EVRPConverter.inst.evrpData.vehicles);

        lock(EntMgr.inst.algInfoList[0].bestNodeList)
        {
            EntMgr.inst.SetBestNodes(curRoute);
        }
        bool lastisValid = false;
        curRouteCost = EvalRoute(curRoute, out lastisValid);
        lock(GraphMgr.inst.scores)
        {
            GraphMgr.inst.scores.Add(curRouteCost);
        }
        bool isImproved = true;

        while(isImproved)
        {
            isImproved=false;
            for(int i = 1;i<curRoute.Count-1;i++)
            {
                for(int j = i+1;j<curRoute.Count;j++)
                {
                    List<NodeEnt> newRoute = new(curRoute);
                    newRoute.Reverse(i,j-i+1);
                    int newCost = EvalRoute(newRoute, out lastisValid);
                    
                    if(newCost>=curRouteCost)
                        continue;

                    curRoute=newRoute;
                    isImproved=true;
                    if(lastisValid)
                    {
                        curRouteCost=newCost;
                        lastValidRoute=newRoute;
                        lock(listOfScoresToQueueInMain)
                        {
                            listOfScoresToQueueInMain.Enqueue(curRouteCost);
                        }
                    }
                    
                }
            }
            lock(EntMgr.inst.algInfoList[0].bestNodeList)
            {
                EntMgr.inst.SetBestNodes(lastValidRoute);
            }
            QueueMainThreadFunction(() =>DisplayUpdate());
        }
        QueueMainThreadLog("Value: "+curRouteCost);
    }

    int EvalRoute(List<NodeEnt> nodes, out bool isValid)
    {
        List<int> vehicleSums = new();
        List<int> vehicleCapacities = new();
        int vehicles = EVRPConverter.inst.evrpData.vehicles;
        for(int i =0;i<vehicles;i++)
        {
            vehicleSums.Add(0);
            vehicleCapacities.Add(EVRPConverter.inst.evrpData.capacity);
        }
        int curVehicle = 0;
        for(int i = 0; i<nodes.Count-1;i++)
        {
            if(nodes[i].nodeType==NodeType.Depot && i!=0)
            {
                curVehicle++;
                vehicleSums.Add(0);
                vehicleCapacities.Add(EVRPConverter.inst.evrpData.capacity);
            }
            Road tempRoad = new Road(nodes[i],nodes[i+1]);
            int newLength = EntMgr.inst.roadDict[tempRoad];
            if(nodes[i]==nodes[i+1])
            {
                newLength=0;
            }
            else if(newLength==-1)
            {
                Debug.LogError("Invalid Path!");
                newLength=99999;
            }
            vehicleCapacities[curVehicle]-=nodes[i+1].demand;
            vehicleSums[curVehicle]+=newLength;
        }

        Road wrapTempRoad = new Road(nodes[^1],nodes[0]);
        int wrapNewLength = EntMgr.inst.roadDict[wrapTempRoad];
        if(nodes[^1]==nodes[0])
        {
            wrapNewLength=0;
        }
        else if(wrapNewLength==-1)
        {
            Debug.LogError("Invalid Path!");
            wrapNewLength=99999;
        }
        vehicleCapacities[vehicles-1]-=nodes[0].demand;
        vehicleSums[vehicles-1]+=wrapNewLength;
        bool isOverCapacity = false;
        for(int i = 0; i<vehicles;i++)
        {
            if(vehicleCapacities[i]<0)
                isOverCapacity=true;
        }
        isValid= !isOverCapacity;
        int sum = 0;
        foreach (int item in vehicleSums)
        {
            sum+=item;
        }     
        return sum;
        // return vehicleSums.Max();
    }

    List<int> curScores = new();

    public void DisplayUpdate(int index = -1)
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

    List<List<NodeEnt>> colorSepatatedNodeList;

    public void ToggleRun()
    {
        //Should make a unified manager at some point but atm this is a patch
        if(t1RunFlag)
        {
            t1RunFlag=false;
            foreach (Thread thread in threads)
            {
                if(thread != null && thread.IsAlive)
                    thread.Abort();
            }
            foreach (var singleAlgInfo in EntMgr.inst.algInfoList)
            {
                singleAlgInfo.Clear();
            }
        }
        else if(!t1RunFlag && EntMgr.inst.nodeList.Count>2 &&!isColoring)
        {
            t1RunFlag=true;
            if(EntMgr.inst.algSeed!=0)
                UnityEngine.Random.InitState(EntMgr.inst.algSeed);
            EntMgr.inst.SetUpForAlgorithm();
            threads.Clear();
            threads.Add(new Thread(RunNOpt) {Name = "Thread 0"});
            threads[0].Start();
        }
        else if(!t1RunFlag && EntMgr.inst.nodeList.Count>2 &&isColoring)
        {
            t1RunFlag=true;
            if(EntMgr.inst.algSeed!=0)
                UnityEngine.Random.InitState(EntMgr.inst.algSeed);
            EntMgr.inst.SetUpForAlgorithm();
            int colorcount = EntMgr.inst.ColorSplit(ref colorSepatatedNodeList);
            threads.Clear();
            for(int i = 0;i < colorcount; i++)
            {
                threads.Add(new Thread(() => RunNOptColoring(i)) {Name = "Thread "+i});
                threads[i].Start();
            }
        }
    }

    void GenRandomRoute(List<NodeEnt> fillRoute, ref List<NodeEnt> nodeSource, int vehicles)
    {
        if(vehicles==0)
        {
            Debug.LogError("Vehicle Count Error!");
            return;
        }

        List<NodeEnt> temp;
        lock(nodeSource)
        {
            temp = new(nodeSource);
        }
        int nodeLenght=temp.Count;
        int start = 1;
        if(temp[0].nodeType==NodeType.Depot)
        {
            fillRoute.Add(temp[0]);
            temp.Remove(temp[0]);
        }
        else
        {
            fillRoute.Add(EntMgr.inst.nodeList[0]);
            start--;
        }

        int nextWarehouse = vehicles;
        int warehouseCount = 1;
        for(int i = start; i<nodeLenght;i++)
        {
            int index = _random.Value.Next() % temp.Count;
            if(i==warehouseCount*nodeLenght/nextWarehouse)
            {
                fillRoute.Add(fillRoute[0]);
                warehouseCount++;
            }
            fillRoute.Add(temp[index]);
            temp.Remove(temp[index]);
        }
    }
    public bool isColoring = true;
    public void SetColoring(bool n_isColoring)
    {
        isColoring = n_isColoring;
    }
    // Update is
}
