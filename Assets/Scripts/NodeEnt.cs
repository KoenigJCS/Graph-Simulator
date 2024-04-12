using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NodeType
{
    Standard,
    Depot,
    Station,
}

public class NodeEnt : MonoBehaviour
{
    public Color myColor = Color.white;
    public SpriteRenderer innerRing;
    public bool isSelected = false;
    public int myID = -1;
    public int demand = -1;
    public NodeType nodeType = NodeType.Standard;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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

    public void SetType(NodeType newType)
    {
        nodeType=newType;
        switch (newType)
        {
        case NodeType.Standard:
            SetColor(Color.white);
            break;
        case NodeType.Depot:
            SetColor(Color.gray);
            break;
        case NodeType.Station:
            SetColor(Color.green);
            break;
    
        default:
            Debug.LogError("Something Broke");
            break;
        }
    }
}
