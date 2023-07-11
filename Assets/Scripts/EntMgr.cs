using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntMgr : MonoBehaviour
{
    public List<NodeEnt> nodeList;
    public List<PathEnt> pathList;
    public static EntMgr inst;
    void Awake()
    {
        inst = this;
    }
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
        pathList.Add(newPath);
    }

    public void AddNode(NodeEnt newNode)
    {
        nodeList.Add(newNode);
    }
}
