using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GACompute : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    

    /*
    public class Individual
{
    public int fitness = -1;
    public int chromosomeLength = 0;
    public List<NodeEnt> chromosomes;
    public Individual(int newChromeLength)
    {
        chromosomeLength = newChromeLength;
    }
    public void Init(List<NodeEnt> nodeEnts)
    {
        chromosomes = new List<NodeEnt>();
        List<NodeEnt> templist = new List<NodeEnt>(nodeEnts);
        for(int i=0;i<nodeEnts.Count;i++)
        {
            int index1 = _random.Value.Next() % templist.Count;
            chromosomes.Add(templist[index1]);
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
        for(int i = 0;i<chromosomeLength;i++)
        {
            s+=chromosomes[i].ToString()+'\t';
        }
        return s;
    }

    public void Swap()
    {
        int index1 = _random.Value.Next() % chromosomes.Count;
        int index2 = _random.Value.Next() % chromosomes.Count;
        NodeEnt temp = chromosomes[index1];
        chromosomes[index1]=chromosomes[index2];
        chromosomes[index2]=temp;
    }

    public void Evaluate()
    {
        int sum = 0;
        for(int i = 0; i<chromosomes.Count-1;i++)
        {
            Road tempRoad = new Road(chromosomes[i],chromosomes[i+1]);
            int newLength = EntMgr.inst.roadDict[tempRoad];
            if(newLength==-1)
                Debug.LogError("Invalid Path!");
            sum+=newLength;
        }

        Road wrapTempRoad = new Road(chromosomes[chromosomes.Count-1],chromosomes[0]);
        int wrapNewLength = EntMgr.inst.roadDict[wrapTempRoad];
        if(wrapNewLength==-1)
            Debug.LogError("Invalid Path!");
        sum+=wrapNewLength;

        fitness=sum;
    }

    
}
*/
}
