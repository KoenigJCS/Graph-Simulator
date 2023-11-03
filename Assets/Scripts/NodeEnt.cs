using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Types;

public class NodeEnt : MonoBehaviour
{
    public Color myColor = Color.white;
    public SpriteRenderer innerRing;
    public List<PathEnt> myPaths;
    public List<Road> myRoads;
    public bool isSelected = false;
    public int nodeID;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddPath(PathEnt newPath)
    {
        myPaths.Add(newPath);
    }

    public void AddRoad(Road newRoad)
    {
        myRoads.Add(newRoad);
    }

    public void UpdateColor(Color color)
    {
        innerRing.color=color;
        myColor=color;
        foreach(PathEnt path in myPaths)
        {
            path.UpdateColor();
        }
    }

    public void SetColor(Color color)
    {
        innerRing.color=color;
        myColor=color;
    }   
    
    public void SetSelected(bool newState)
    {
        isSelected=newState;
        if(isSelected)
            this.GetComponent<SpriteRenderer>().color=Color.cyan;
        else
            this.GetComponent<SpriteRenderer>().color=Color.white;
    }
}
