using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathEnt : MonoBehaviour
{
    public int length = -1;
    public NodeEnt aNode;
    public NodeEnt bNode; 
    public SpriteRenderer aPart;
    public SpriteRenderer bPart;
    public TMPro.TextMeshPro lengthText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetColor(Color newColor)
    {
        aPart.color=newColor;
        bPart.color=newColor;
    }

    public void UpdateColor()
    {
        aPart.color=aNode.myColor;
        bPart.color=bNode.myColor;
    }

    public void UpdateLength(int newLength)
    {
        if(lengthText.GetComponent<Transform>().eulerAngles.z<180 && lengthText.GetComponent<Transform>().eulerAngles.z>0)
        {
            Vector3 rotate = new Vector3(0,0,180);
            Transform tempT = lengthText.GetComponent<Transform>();
            tempT.Rotate(rotate,Space.World);
        }
        length=newLength;
        lengthText.text=newLength.ToString();
    }
}
