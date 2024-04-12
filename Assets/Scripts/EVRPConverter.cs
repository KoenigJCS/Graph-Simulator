using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

[SerializeField]
/// <summary>
/// Contains the data for the EVRP header
/// </summary>
public struct EVRPData
{
    public int optimalValue;
    public int vehicles;
    public int dimension;
    public int stations;
    public int capacity;
    public int energyCapacity;
    public float energyConsumption;
    public string edgeWeightType;

    public EVRPData(int r_optimalValue=-1,int r_vehicles=-1,int r_dimension=-1
    ,int r_stations=-1,int r_capacity=-1,int r_energyCapacity=-1,float r_energyConsumption=-1f,string r_edgeWeightType="NAN")
    {
        optimalValue=r_optimalValue;
        vehicles=r_vehicles;
        dimension=r_dimension;
        stations=r_stations;
        capacity=r_capacity;
        energyCapacity=r_energyCapacity;
        energyConsumption=r_energyConsumption;
        edgeWeightType=r_edgeWeightType;
    }

    public bool isValidData()
    {
        return optimalValue!=-1 && vehicles!=-1 && dimension!=-1 
        && stations!=-1 && capacity!=-1 && energyCapacity!=-1 
        && energyConsumption!=-1 && energyConsumption !=-1f && edgeWeightType!="NAN";
    }
}

public class EVRPConverter : MonoBehaviour
{
    public int fileStatus = -1;
    [SerializeField] private TMPro.TMP_Dropdown fileDropDown;
    public static EVRPConverter inst;
    [SerializeField] private List<TextAsset> evrpRawFiles;
    [SerializeField] public EVRPData evrpData;
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
        for(int i = 0;i<evrpRawFiles.Count;i++)
        {
            optionList.Add(new TMPro.TMP_Dropdown.OptionData(evrpRawFiles[i].name));
        }
        fileDropDown.AddOptions(optionList);
        evrpData = new();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetFileLoad(int newValue)
    {            
        fileStatus=newValue-1;
    }

    enum LineSection
    {
        Dataheader,
        NodeCoordSection,
        DemandSection,
        StationCoordSection,
        DepotSection
    }

    public void LoadEVRP()
    {
        EntMgr.inst.CleanUpAll();
        if(fileStatus<0)
            return;
        using StringReader reader = new(evrpRawFiles[fileStatus].text);
        string line;
        LineSection lineSection = LineSection.Dataheader;
        while ((line = reader.ReadLine()) != null)
        {
            Debug.Log(lineSection.ToString());
            if (line == "EOF")
                break;
            switch (lineSection)
            {
            case LineSection.Dataheader:
                if (line.Contains("NODE_COORD_SECTION"))
                {
                    lineSection = LineSection.NodeCoordSection;
                    break;
                }
                else if (line.Contains("OPTIMAL_VALUE:"))
                    try
                    {
                        evrpData.optimalValue = int.Parse(line.Substring(15));
                    }
                    catch (System.Exception)
                    {
                        evrpData.optimalValue = 0;
                    }

                else if (line.Contains("VEHICLES:"))
                    evrpData.vehicles = int.Parse(line.Substring(10));
                else if (line.Contains("DIMENSION:"))
                    evrpData.dimension = int.Parse(line.Substring(11));
                else if (line.Contains("STATIONS:"))
                    evrpData.stations = int.Parse(line.Substring(10));
                else if (line.Contains("ENERGY_CAPACITY:"))
                    evrpData.energyCapacity = int.Parse(line.Substring(17));
                else if (line.Contains("CAPACITY:"))
                    evrpData.capacity = int.Parse(line.Substring(10));
                else if (line.Contains("ENERGY_CONSUMPTION:"))
                    evrpData.energyConsumption = float.Parse(line.Substring(20));
                else if (line.Contains("EDGE_WEIGHT_TYPE:"))
                    evrpData.edgeWeightType = line.Substring(18);
                break;
            case LineSection.NodeCoordSection:
                if (line.Contains("DEMAND_SECTION"))
                {
                    lineSection = LineSection.DemandSection;
                    break;
                }
                else
                {
                    Vector3 location = Vector3.zero ;
                    string[] chunks = line.Split(' ');
                    if(chunks.Length<3)
                    {
                        Debug.LogError("Invalid Parse");
                        Debug.LogError(line);
                        continue;
                    }
                    
                    location.x=int.Parse(chunks[1]);
                    location.y=int.Parse(chunks[2]);

                    EntMgr.inst.MakeNodeObj(location,newID:int.Parse(chunks[0]));
                }
                break;
            case LineSection.DemandSection:
                if (line.Contains("STATIONS_COORD_SECTION"))
                {
                    lineSection = LineSection.StationCoordSection;
                    break;
                }
                string[] demandChunks = line.Split(' ');
                EntMgr.inst.nodeList[int.Parse(demandChunks[0])-1].demand = int.Parse(demandChunks[1]);
                break;
            case LineSection.StationCoordSection:
                if (line.Contains("DEPOT_SECTION"))
                {
                    lineSection = LineSection.DepotSection;
                    break;
                }
                EntMgr.inst.nodeList[int.Parse(line)-1].SetType(NodeType.Station);
                break;
            case LineSection.DepotSection:
                if (line.Contains("-1") || line.Contains("EOF"))
                    break;
                EntMgr.inst.nodeList[int.Parse(line)-1].SetType(NodeType.Depot);
                break;

            default:
                return;
            }

            // else if(cordSectionFound)
            // {
            //     Vector3 location = Vector3.zero ;
            //     string[] chunks = line.Split(' ');
            //     if(chunks.Length<3)
            //     {
            //         Debug.LogError("Invalid Parse");
            //         Debug.LogError(line);
            //         continue;
            //     }
            //     location.x=float.Parse(chunks[1]);
            //     location.y=float.Parse(chunks[2]);

            //     EntMgr.inst.MakeNodeObj(location); 
            // }

        }
        if (!evrpData.isValidData())
        {
            Debug.LogError("Bad Header!");
            return;
        }
        float maxLength =0f;
        EntMgr.inst.AddAllPaths();
        if(EntMgr.inst.nodeList.Count!=evrpData.dimension)
            Debug.LogError("Not Enoguh Nodes");
        
        Road longestRoad = new Road(EntMgr.inst.nodeList[0],EntMgr.inst.nodeList[1]);
        foreach(var singlePath in EntMgr.inst.roadDict)
        {
            if(singlePath.Value>maxLength)
                    maxLength=singlePath.Value;
        }
        CameraController.inst.ResizeScreen(maxLength);
        CameraController.inst.SetLocation((longestRoad.startNode.transform.position-longestRoad.endNode.transform.position)/2+longestRoad.startNode.transform.position);
    }
}
