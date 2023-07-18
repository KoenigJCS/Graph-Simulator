using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphMgr : MonoBehaviour
{
    public GameObject graphPanel;
    public Camera cam;
    public LineRenderer lineRender;
    public TMPro.TextMeshProUGUI xText;
    static float MAX_X = 610;
    public TMPro.TextMeshProUGUI yText;
    static float MAX_Y = 420;
    float maxScore;
    float minScore;
    public List<int> scores;
    public static GraphMgr inst;
    Vector3 startPos;
    void Awake() 
    {
       inst = this; 
    }
    // Start is called before the first frame update
    void Start()
    {
        lineRender.useWorldSpace = false;
        graphPanel.SetActive(false);
        lineRender.gameObject.SetActive(false);
        startPos=lineRender.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnGUI()
    {
        lineRender.transform.localScale= new Vector3(cam.orthographicSize/725,cam.orthographicSize/725,1);
        lineRender.transform.localPosition=startPos*(cam.orthographicSize/725);
        lineRender.startWidth=20*(cam.orthographicSize/725);
        lineRender.endWidth=20*(cam.orthographicSize/725);
    }

    public void StartGraph()
    {
        graphPanel.SetActive(true);
        lineRender.gameObject.SetActive(true);
        lineRender.positionCount=0;
    }
    public void UpdateGraph()
    {
        if(graphPanel.activeInHierarchy && scores.Count>0)
        {
            lineRender.positionCount=scores.Count;
            //Note that I'm assuming the first score will always be the highest
            maxScore=scores[0];
            minScore=scores[scores.Count-1];
            for(int i = 0; i<scores.Count;i++)
            {
                lineRender.SetPosition(i,new Vector3(((float)i/(float)(scores.Count-1))*MAX_X,((float)(scores[i]-minScore)/(maxScore-minScore))*MAX_Y,5));
            }
            string yValues = "",xValues="";
            for(int i = 8; i>=0;i--)
            {
                yValues+=(int)(minScore+((maxScore-minScore)/(9f-i)));
                yValues+="\n\n";
            }
            for(int i = 1; i<=10;i++)
            {
                xValues+=(int)((float)scores.Count/10)*i;
                xValues+="    ";
            }

            yText.text=yValues;
            xText.text=xValues;
        }
    }
}
