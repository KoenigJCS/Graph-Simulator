using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeEnt : MonoBehaviour
{
    public Color myColor = Color.white;
    public List<PathEnt> myPaths;
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

    public void UpdateColor(Color color)
    {
        this.GetComponent<SpriteRenderer>().color=color;
        myColor=color;
        foreach(PathEnt path in myPaths)
        {
            path.UpdateColor();
        }
    }
}
