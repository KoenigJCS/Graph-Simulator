using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Xml.Linq;
using UnityEditor;

namespace Xml2CSharp
{
    [XmlRoot(ElementName="edge")]
    public class Edge {
        [XmlAttribute(AttributeName="cost")]
        public string Cost { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName="vertex")]
    public class Vertex {
        [XmlElement(ElementName="edge")]
        public List<Edge> Edge { get; set; }
    }

    [XmlRoot(ElementName="graph")]
    public class Graph {
        [XmlElement(ElementName="vertex")]
        public List<Vertex> Vertex { get; set; }
    }

    [XmlRoot(ElementName="travellingSalesmanProblemInstance")]
    public class TravellingSalesmanProblemInstance {
        [XmlElement(ElementName="name")]
        public string Name { get; set; }
        [XmlElement(ElementName="source")]
        public string Source { get; set; }
        [XmlElement(ElementName="description")]
        public string Description { get; set; }
        [XmlElement(ElementName="doublePrecision")]
        public string DoublePrecision { get; set; }
        [XmlElement(ElementName="ignoredDigits")]
        public string IgnoredDigits { get; set; }
        [XmlElement(ElementName="graph")]
        public Graph Graph { get; set; }
    }



    public class XMLConverter : MonoBehaviour
    {
        public int fileStatus = -1;
        public List<TextAsset> xmlRawFiles;
        public XmlSerializer serializer;  
        public static XMLConverter inst;
        public TMPro.TMP_Dropdown fileDropDown;
        void Awake()
        {
            inst = this;
        }
        // Start is called before the first frame update
        void Start()
        {
            List<TMPro.TMP_Dropdown.OptionData> optionList = new List<TMPro.TMP_Dropdown.OptionData>();
            fileDropDown.ClearOptions();
            optionList.Add(new TMPro.TMP_Dropdown.OptionData("None"));
            for(int i = 0;i<xmlRawFiles.Count;i++)
            {
                optionList.Add(new TMPro.TMP_Dropdown.OptionData(xmlRawFiles[i].name));
            }
            fileDropDown.AddOptions(optionList);
        }

        // Update is called once per frame
        void Update()
        {
            
        }
        public void SetFileLoad(Int32 newValue)
        {            
            fileStatus=newValue-1;
        }
        
        public void LoadXML()
        {
            EntMgr.inst.CleanUpAll();
            if(EntMgr.inst.algoritmOnline);
                EntMgr.inst.ToggleHillClimb();
            if(fileStatus>=0)
            {
                serializer = new XmlSerializer(typeof(TravellingSalesmanProblemInstance));
                
                using (StringReader reader = new StringReader(xmlRawFiles[fileStatus].text))
                {
                    var tsp = (TravellingSalesmanProblemInstance)serializer.Deserialize(reader);

                    //Node0
                    EntMgr.inst.MakeNodeObj(Vector3.zero);
                    float nodeU = float.Parse(tsp.Graph.Vertex[0].Edge[0].Cost);
                    //Node1
                    EntMgr.inst.MakeNodeObj(new Vector3(nodeU,0,0));  
                    //Node2
                    float node3X = (MathF.Pow(float.Parse(tsp.Graph.Vertex[0].Edge[1].Cost),2)
                    -MathF.Pow(float.Parse(tsp.Graph.Vertex[1].Edge[1].Cost),2)
                    +MathF.Pow(float.Parse(tsp.Graph.Vertex[0].Edge[0].Cost),2))
                    /(2*float.Parse(tsp.Graph.Vertex[0].Edge[0].Cost));

                    float node3Y = MathF.Sqrt(MathF.Pow(float.Parse(tsp.Graph.Vertex[0].Edge[1].Cost),2)-MathF.Pow(node3X,2));

                    EntMgr.inst.MakeNodeObj(new Vector3(node3X,node3Y,0));  
                    //Node3
                    for(int i = 3;i<tsp.Graph.Vertex.Count;i++)
                    {
                        float nodeNX = (MathF.Pow(float.Parse(tsp.Graph.Vertex[i].Edge[0].Cost),2)
                        -MathF.Pow(float.Parse(tsp.Graph.Vertex[i].Edge[1].Cost),2)
                        +MathF.Pow(nodeU,2))
                        /(2*nodeU);

                        float nodeNY = (MathF.Pow(float.Parse(tsp.Graph.Vertex[i].Edge[0].Cost),2)
                        -MathF.Pow(float.Parse(tsp.Graph.Vertex[i].Edge[2].Cost),2)
                        +MathF.Pow(node3X,2)
                        +MathF.Pow(node3Y,2)
                        -(2*node3X*nodeNX))
                        /(2*node3Y);

                        EntMgr.inst.MakeNodeObj(new Vector3(nodeNX,nodeNY,0));  
                    }
                    float maxLength =0f;
                    foreach(Vertex vert in tsp.Graph.Vertex)
                    {
                        foreach(Edge edge in vert.Edge)
                        {
                            if(float.Parse(edge.Cost)>maxLength)
                                maxLength=float.Parse(edge.Cost);
                        }
                    }
                    CameraController.inst.ResizeScreen(maxLength);
                    CameraController.inst.SetLocation(EntMgr.inst.nodeList[0].transform.position);
                    //deltaRad = MathF.Atan2(float.Parse(tsp.Graph.Vertex[0].Edge[2].Cost),float.Parse(tsp.Graph.Vertex[1].Edge[2].Cost));
                    
                    //EntMgr.inst.AddNode(Instantiate(PlacementMgr.inst.node,new Vector3(float.Parse(tsp.Graph.Vertex[0].Edge[0].Cost)*100,0,0),Quaternion.identity,PlacementMgr.inst.entContainer).GetComponent<NodeEnt>());
                }
            }
        }
    }

}
