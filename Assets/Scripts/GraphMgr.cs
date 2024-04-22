using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphMgr : MonoBehaviour
{
    public GameObject graphPanel;
    public Camera cam;
    public LineGraph lineGraph;
    static float MAX_X = 165f;
    public TMPro.TextMeshProUGUI yText;
    public TMPro.TextMeshProUGUI bestScore;
    static float MAX_Y = 100f;
    float maxScore;
    float minScore;
    public List<int> scores;
    public static GraphMgr inst;
    Vector3 startPos;
    Vector3 startScale;
    void Awake() 
    {
       inst = this; 
    }
    // Start is called before the first frame update
    void Start()
    {
        //lineRender.useWorldSpace = false;
        graphPanel.SetActive(false);
        // lineRender.gameObject.SetActive(false);
        // startPos=lineRender.transform.localPosition;
        // startScale=lineRender.transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnGUI()
    {
        // lineRender.transform.localScale= startScale*(cam.orthographicSize/7.25f);
        // lineRender.transform.localPosition=startPos*(cam.orthographicSize/7.25f);
        // lineRender.startWidth=.2f*(cam.orthographicSize/7.25f);
        // lineRender.endWidth=.2f*(cam.orthographicSize/7.25f);
    }

    public void StartGraph()
    {
        graphPanel.SetActive(true);
        lineGraph.SetAllDirty();
    }
    public void UpdateGraph()
    {
        if(graphPanel.activeInHierarchy && scores.Count>0)
        {
            lineGraph.dataPoints.Clear();
            lineGraph.SetAllDirty();
            //lineRender.positionCount=scores.Count;
            //Note that I'm assuming the first score will always be the highest
            maxScore=scores[0];
            minScore=scores[^1];
            for(int i = 0; i<scores.Count;i++)
            {
                lineGraph.dataPoints.Add(new Vector2(i / (float)(scores.Count-1)*MAX_X,(float)(scores[i]-minScore)/(maxScore-minScore)*MAX_Y));
                //lineRender.SetPosition(i,new Vector3(((float)i/(float)(scores.Count-1))*MAX_X,((float)(scores[i]-minScore)/(maxScore-minScore))*MAX_Y,5));
            }
            string yValues = "";
            float detla = (maxScore-minScore)/(8f);
            for(int i = 8; i>=0;i--)
            {
                yValues+=(int)(minScore+detla*i);
                yValues+="\n\n";
            }
            bestScore.text="Best Score: "+scores[scores.Count-1];
            yText.text=yValues;
            lineGraph.SetAllDirty();
        }
    }
}
