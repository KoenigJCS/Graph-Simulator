# Graph Simulator CVRP Variant
## Table of Contents
1. Summary
2. Core Components
3. Color Clustering
4. Initialization
5. Threading
6. Vehicle Apportionment
7. Evaluation
8. Notes
9. Key Code Chunks

## Summary
This is a visualization tool meant to make it easier to work on and implement algorithms for node based problems such as the TSP (traveling salesman problem) and the CVRP. There are a large number of predesigned nodal problems saved that can be loaded into the scene. Then an evaluation heuristic can be selected to attempt to “solve” the nodes in the visualiser, the user can modify the parameters and location of the nodes while they are in the visualizer to change the details of the simulation. Then as the evaluation runs in processes solutions are sent to be visualized and graphed for the user to see. See diagram below.
![Diagram of Program Flow](/READMEIMGS/GS_RDME_IMG1.PNG)

## Core Components
This program has the following components with the basic functionality
* Entity Manager
  + Serves as the data container for much of the visualization and handles all global manipulation of the nodes and edges between them. 
* Converters (Multiple)
  + Multiple parsers used to interpret raw data files into useful information for the visualiser. Current supported formats are TSP (.txt) + (.xml) VRP (.vrp) and EVRP (.evrp)
* Graph Manager + Line Graph Renderer
  + Display Score Over Time
* Camera Controller + Selection Manager + Placement Manager
  + Allows the user to pan around and scroll in and out (RTS Style Movement), as well as actively manipulate the visualization, such as node color.
* Algorithms (Multiple)
  + Variety of algorithms that take in the current state of the visualizer and attempt to product solutions based on the current state of the field.
 
## Color Clustering
The program allows for the 2 opt algorithm to be run in separate threads based on color clustering. The user can color nodes using the selection tools (see the compass with three lines icon on the image below). The coloring looks like this:
![Color Clustering Example](/READMEIMGS/GS_RDME_IMG2.PNG)

## Initialization
For initializing the ecvrp click the gears in the corner and load one of the evrps at the bottom, the structure goes (Type - n[nodecount] - k[vehiclecount]). Then all of the nodes should appear on the screen
When done alter the scene or color to your heart's content, once you want to run click 2Opt Algorithm

## Threading
Even if there is only one color the program runs multithreaded in order to allow real time viewing and updating while the program runs. For each color another thread is made to deal with its specific sub problem. In the ent manager each thread is assigned to a struct called algorithm info, which is separated in order to avoid locking issues, but also allows for outside data to be passed into the specific threads if needed. Each thread runs occasionally queuing update display functions to the main thread so that the new scores can be processed. Locking is minimized as most of the data is entirely separate. Locking only occurs when data needs to be read to update the best path list.

## Vehicle Apportionment
Vehicles are divided using an algorithm similar to the US Senate seat division, where the total demand of each color is summed, and then the minimum number of vehicles needed to fulfill that demand is given to them (NOTE: For certain user made allocations of colors this can lead to more vehicles being needed than exist, the program simply gives out more vehicles but will log an error to inform the user). If there are still vehicles left over then they are divided out proportionately based on weight. See Key Code Chunks for code.

## Evaluation
Once this algorithm is running, 2 opt progresses as normal, however only "valid" (meaning that no vehicle is over capacity) routes are accepted as successful improvement, once a successful improvement is found the best track is updated and improvement is marked down. If all possible reversals are iterated through without an improvement the algorithm ends, otherwise it restarts and continues the search. The exception to this is if the initial string never finds a valid route, in which case the program simply generates a new random starting seed and tries again. 

## Notes
1. Certain problems have extremely tight solving parameters and can be very difficult to manually cluster
2. If the set of valid paths is extremely small it may take a large amount of time for the program to find them
3. A few of the .vrp and .cvrp files don't parse correctly as they are non-standard
4. Hill Climbing doesn't currently work 

## Key Code Chunks
### Color Clustering + Vehicle Apportionment
```
public int ColorSplit(ref List<List<NodeEnt>> colorSepatatedNodeList, ref List<int> vehicleSplit)
{
    //~~~
    foreach(NodeEnt singleNode in nodeList)
    {
        if(!colors.Contains(singleNode.myColor) && singleNode.nodeType != NodeType.Depot)
            colors.Add(singleNode.myColor);
    }
    for(int i = 0; i < colors.Count; i++)
    {
        colorSepatatedNodeList.Add(new List<NodeEnt>());
        colorDemand.Add(0);
    }
    foreach(NodeEnt singleNode in nodeList)
    {
        if(singleNode.nodeType == NodeType.Depot)
            continue;

        int index = colors.IndexOf(singleNode.myColor);
        colorSepatatedNodeList[index].Add(singleNode);
        colorDemand[index]+=singleNode.demand;
    }
    int totalVehicles = EVRPConverter.inst.evrpData.vehicles;
    for(int i = 0;i<totalVehicles;i++)
    {
        vehicleSplit.Add(0);
    }
    int porportionalAmmount = colorDemand.Sum()/totalVehicles;
    for(int i = 0;i<colors.Count;i++)
    {
        int minCars = (int)Mathf.Ceil(colorDemand[i] / (float)(EVRPConverter.inst.evrpData.capacity+1));
        minCars = minCars==0?1:minCars;
        vehicleSplit[i]+=minCars;
        totalVehicles-=minCars;
    }
    if(totalVehicles<0)
        Debug.LogError("Impossible Color Grouping");
    while(totalVehicles>0)
    {
        int temp = colorDemand.IndexOf(colorDemand.Max());
        colorDemand[temp] -= porportionalAmmount;
        vehicleSplit[temp]++;
        totalVehicles--;
    }
    return colors.Count;
}
```
### Threading
```
t1RunFlag=true;
if(EntMgr.inst.algSeed!=0)
    UnityEngine.Random.InitState(EntMgr.inst.algSeed);
int colorCount = EntMgr.inst.ColorSplit(ref colorSepatatedNodeList, ref vehicleSplit);
EntMgr.inst.MultiLineInitalization(colorCount);
EntMgr.inst.SetUpForAlgorithm();
threads.Clear();
for(int i = 0;i < colorCount; i++)
{
    int temp = i; //This is done to prevent some weird race condition strangeness
    threads.Add(new Thread(() => RunNOptColoring(temp)) {Name = "Thread "+temp});
}
for(int i = 0;i < colorCount; i++)
{
    threads[i].Start();
}
```
### Evaluation
```
int EvalRoute(ref List<NodeEnt> nodes, out bool isValid)
{
    //~~~
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
    //~~~
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
}
```
