using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using UnityEngine.Assertions.Must;
using Unity.VisualScripting;
using System.Runtime.CompilerServices;

public struct Options
{
    public int seed;
    public int popSize;
    public int chromLength;
    public int maxgens;
    public float px;
    public float pm;
}

public class GeneticAlgorithm : MonoBehaviour
{
    public ComputeShader computeShader;
    public const int ARRSIZE = 48;
    public List<NodeEnt> initalNodeList;
    public static int MAXPOP = 2000;
    public static int MAX_CHROMLENGTHS = 100;
    public static int MAX_VERTICIES = 100;
    public Population parent,child;
    public Options options;
    public bool t1RunFlag = false;
    Queue<System.Action> listOFunctionsToRunInMain;
    public Thread t1;
    // Start is called before the first frame update
    void Start()
    {
        options.seed = 0;
        options.popSize = 25;
        //Numb of cities
        options.chromLength = 10;
        options.maxgens = 200;
        options.px = 0.95f;
        options.pm = 0.01f;

        //initalNodeList = new List<NodeEnt>();
        listOFunctionsToRunInMain = new Queue<System.Action>();
    }

    public static GeneticAlgorithm inst;
    void Awake()
    {
        inst = this;
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

    public void RunGACHC()
    {
        for(int i = 1;t1RunFlag;i++)
        {
            parent.CHCGeneration(child);
            child.Statistics();
            child.Report(i);
            //Debug.Log(child.ToString());

            EntMgr.inst.SetBestNodes((int[])child.members[0].me.nodeIDs.Clone());
            QueueMainThreadFunction(DisplayUpdate);

            (child, parent) = (parent, child);
        }
    }

    public void DisplayUpdate()
    {
        EntMgr.inst.CreatePathLine();
        if(GraphMgr.inst.scores[^1]!=child.members[0].me.fitness)
            GraphMgr.inst.scores.Add((int)child.members[0].me.fitness);
        GraphMgr.inst.UpdateGraph();
    }

    public void ToggleRun()
    {
        //Should make a unified manager at some point but atm this is a patch
        if(EntMgr.inst.t1RunFlag)
        {
            EntMgr.inst.t1RunFlag=false;
            if(EntMgr.inst.t1 != null && EntMgr.inst.t1.IsAlive)
                EntMgr.inst.t1.Abort();
            foreach (var singleAlgInfo in EntMgr.inst.algInfoList)
            {
                singleAlgInfo.Clear();
            }
        }

        if(!t1RunFlag && EntMgr.inst.nodeList.Count>2 && SelectionMgr.inst.selectedNodes.Count==0)
        {
            t1RunFlag=true;
            initalNodeList = EntMgr.inst.nodeList;
            options.chromLength = initalNodeList.Count;
            if(EntMgr.inst.algSeed!=0)
                UnityEngine.Random.InitState(EntMgr.inst.algSeed);

            Init();
            t1 = new Thread(RunGACHC) {Name = "Thread 1"};
            t1.Start();
            //RunGACHC();
        }
        else if(!t1RunFlag && SelectionMgr.inst.selectedNodes.Count>2)
        {
            // t1 = new Thread(RunGACHC) {Name = "Thread 1"};
            // t1.Start();
            t1RunFlag=true;
            initalNodeList = SelectionMgr.inst.selectedNodes;
            options.chromLength = initalNodeList.Count;
            if(EntMgr.inst.algSeed!=0)
                UnityEngine.Random.InitState(EntMgr.inst.algSeed);
            Init();
            t1 = new Thread(RunGACHC) {Name = "Thread 1"};
            t1.Start();
            //RunGACHC();
        }
        else
        {
            t1RunFlag=false;
            if(t1 != null && t1.IsAlive)
                t1.Abort();
            foreach (var singleAlgInfo in EntMgr.inst.algInfoList)
            {
                singleAlgInfo.Clear();
            }
            
        }
        
    }

    public void Init()
    {
        EntMgr.inst.SetUpForAlgorithm();

        parent = new Population(options);
        child = new Population(options);
        parent.Init();
        parent.Statistics();
        parent.Report(0);        
        //Debug.Log(parent.ToString());
        GraphMgr.inst.scores.Add((int)parent.min);
    }

    public void QueueMainThreadFunction(System.Action someFunction)
    {
        listOFunctionsToRunInMain.Enqueue(someFunction);
    }
}

public class Population
{
    public class FitnessComparer : IComparer<Individual>
    {
        public int Compare(Individual x, Individual y)
        {
            return (int)(x.me.fitness-y.me.fitness);
        }
    }
    Options myOptions;
    public double min = 1;
    double avg, max, sumFitness = 1;
    public List<Individual> members;

    public Population(Options newOptions)
    {
        myOptions=newOptions;
        if(myOptions.popSize * 2 > GeneticAlgorithm.MAXPOP)
            Debug.Log("Invalid Population");

        members = new List<Individual>
        {
            Capacity = GeneticAlgorithm.MAXPOP
        };
        for (int i = 0; i< myOptions.popSize * 2; i++)
        {
            members.Add(new Individual(myOptions.chromLength));
            members[i].Init(GeneticAlgorithm.inst.initalNodeList);
        }
    }

    public void SortMembers()
    {
        members.Sort(new FitnessComparer());
    }

    public override string ToString()
    {
        string s = "";
        for(int i = 0;i<myOptions.popSize;i++)
        {
            s+=members[i].ToString()+'\n';
        }
        return s;
    }

    public void CHCGeneration(Population child)
    {
        int pi1,pi2,ci1,ci2;
        Individual p1,p2,c1,c2;

        for(int i =0;i<myOptions.popSize-1;i+=2)
        {
            pi1 = PorportionalSelectorIndex();
            pi2 = PorportionalSelectorIndex();
            
            ci1 = myOptions.popSize + i;
            ci2 = myOptions.popSize + i + 1;
            
            p1 = members[pi1];
            p2 = members[pi2];
            c1 = members[ci1];
            c2 = members[ci2];

            PMXandSwap(p1, p2, c1, c2);
        }
        Halev(child);
    }

    public void Halev(Population child)
    {
        for(int i = 0; i<members.Count;i++)
        {
            members[i].Evaluate();
        }
        //This should sort it from largest[0] to smallest[myOptions.popSize*2]
        SortMembers();
        //members.RemoveRange(myOptions.popSize,myOptions.popSize);
        child.members = new List<Individual>(members);
    }

    public void PMXandSwap(Individual p1,Individual p2,Individual c1,Individual c2)
    {
        c1.me.chromosomes = (Vector2[])p1.me.chromosomes.Clone();
        c2.me.chromosomes = (Vector2[])p2.me.chromosomes.Clone();

        if(Flip(myOptions.px))
        {
            PMX(p1,p2,c1);
            PMX(p1,p2,c2);
        }
        
        if(Flip(myOptions.pm))
            c1.Swap();
        if(Flip(myOptions.pm))
            c2.Swap();
    }

    public void PMX(Individual p1, Individual p2, Individual c1)
    {
        int i1 = _random.Value.Next() % myOptions.chromLength;
        int i2 = _random.Value.Next() % myOptions.chromLength;
        int maxI = i1 > i2 ? i1 : i2;
        int minI = i1 <= i2 ? i1 : i2;
        int movedCounter = -1;
        Vector2[] listOfMoved = new Vector2[GeneticAlgorithm.ARRSIZE];
        for(int i = minI;i<=maxI;i++)
        {
            c1.me.chromosomes[i] = p1.me.chromosomes[i];
            listOfMoved[++movedCounter] = p1.me.chromosomes[i];
        }
        int openIndex = 0;
        for(int i =0;i<myOptions.chromLength&&movedCounter<myOptions.chromLength;i++)
        {
            for(int j = 0;j<movedCounter;j++)
            {
                if(listOfMoved[j] == p2.me.chromosomes[i])
                    continue;
            }
                
            if(openIndex >= minI)
            {
                openIndex=maxI+1;
            }
            else
            {
                c1.me.chromosomes[openIndex] = p2.me.chromosomes[i];
                listOfMoved[++movedCounter] = p1.me.chromosomes[i];
                openIndex++;
            }
        }
    }
    public void Init()
    {
        Evaluate();
    }

    public bool Flip(float prob)
    {
        return prob>=_random.Value.Next() % 2;
    }

    private static int _tracker = 0;
    private static ThreadLocal<System.Random> _random = new ThreadLocal<System.Random>(() => {
        var seed = (int)(System.Environment.TickCount & 0xFFFFFF00 | (byte)(Interlocked.Increment(ref _tracker) % 255));
        var random = new System.Random(seed);
        return random;
    });

    public int PorportionalSelectorIndex()
    {
        int i = -1;
        double sum = 0;
        double limit = _random.Value.Next() % 100/100f * sumFitness;
        do
        {
            i++;
            sum+=members[i].me.fitness;
        } while (sumFitness<limit && i < myOptions.popSize-1);
        return i;
    }

    public void Evaluate()
    {
        int datasize = 0;
        datasize+=sizeof(float);
        datasize+=sizeof(int);
        datasize+=sizeof(float) * 2 * GeneticAlgorithm.ARRSIZE;
        datasize+=sizeof(int) * GeneticAlgorithm.ARRSIZE;
        
        ComputeBuffer evalBuffer = new ComputeBuffer(members.Count,datasize);
        
        Individual.IndividualVal[] mydata = new Individual.IndividualVal[members.Count];

        for(int i = 0;i<members.Count;i++)
        {
            mydata[i]=members[i].me;
        }

        evalBuffer.SetData(mydata);

        evalBuffer.Set
        
        evalBuffer.GetData(mydata);
    }
    public void Statistics()
    {
        sumFitness=0;
        min = members[0].me.fitness;
        max = members[0].me.fitness;
        for(int i = 0; i< myOptions.popSize; i++)
        {
            sumFitness+= members[i].me.fitness;
            if(min > members[i].me.fitness)
                min = members[i].me.fitness;
            if(max < members[i].me.fitness)
                max = members[i].me.fitness;
        }
        avg = sumFitness/myOptions.popSize;
    }

    public void Report(int gen)
    {
	    Debug.Log("Gen: "+ gen + "\tMin: "+(int)min+"\tAvg: "+(int)avg+"\tMax: "+(int)max);
    }
}

public class Individual
{
    public struct IndividualVal
    {
        public float fitness;
        public int chromosomeLength;
        public Vector2[] chromosomes;
        public int[] nodeIDs;

    }
    public IndividualVal me = new();
    public Individual(int newChromeLength)
    {
        me.chromosomeLength = newChromeLength;
    }
    public void Init(List<NodeEnt> nodeEnts)
    {
        me.chromosomes = new Vector2[GeneticAlgorithm.ARRSIZE];
        me.fitness=-1;
        me.chromosomeLength=GeneticAlgorithm.ARRSIZE;
        List<NodeEnt> templist = new(nodeEnts);
        for(int i=0;i<nodeEnts.Count;i++)
        {
            int index1 = _random.Value.Next() % templist.Count;
            me.chromosomes[i] = new Vector2(templist[index1].transform.position.x,templist[index1].transform.position.y);
            me.nodeIDs[i] = templist[index1].nodeID;
            templist.RemoveAt(index1);
        }
    }

    private static int _tracker = 0;
    private static ThreadLocal<System.Random> _random = new ThreadLocal<System.Random>(() => {
        var seed = (int)(System.Environment.TickCount & 0xFFFFFF00 | (byte)(Interlocked.Increment(ref _tracker) % 255));
        var random = new System.Random(seed);
        return random;
    });

    public override string ToString()
    {
        string s = "";
        for(int i = 0;i<me.chromosomeLength;i++)
        {
            s+=me.chromosomes[i].ToString()+'\t';
        }
        return s;
    }

    public void Swap()
    {
        int index1 = _random.Value.Next() % me.chromosomes.Length;
        int index2 = _random.Value.Next() % me.chromosomes.Length;
        (me.chromosomes[index2], me.chromosomes[index1]) = (me.chromosomes[index1], me.chromosomes[index2]);
    }

    public void Evaluate()
    {
        //Don't Eval Here
    }

    
}
