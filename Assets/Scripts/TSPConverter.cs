using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TSPConverter : MonoBehaviour
{
    public int fileStatus = -1;
    public TMPro.TMP_Dropdown fileDropDown;
    public static TSPConverter inst;
    public List<TextAsset> tspRawFiles;
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
        for(int i = 0;i<tspRawFiles.Count;i++)
        {
            optionList.Add(new TMPro.TMP_Dropdown.OptionData(tspRawFiles[i].name));
        }
        fileDropDown.AddOptions(optionList);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetFileLoad(int newValue)
    {            
        fileStatus=newValue-1;
    }
    // Start is called before the first frame update
    // void Start()
    // {
    //     var tspFiles = new DirectoryInfo("Assets/TSPFiles");
    //     fileList = tspFiles.EnumerateFiles();
    //     List<TMPro.TMP_Dropdown.OptionData> optionList = new List<TMPro.TMP_Dropdown.OptionData>();
    //     fileDropDown.ClearOptions();
    //     optionList.Add(new TMPro.TMP_Dropdown.OptionData("None"));
    //     foreach (var file in fileList)
    //     {
    //         if(file.Name.ToCharArray()[file.Name.Length-1]=='p')
    //             optionList.Add(new TMPro.TMP_Dropdown.OptionData(file.Name));
    //     }
    //     fileDropDown.AddOptions(optionList);
    //     if(runInEditMode && filePathList.Count==0)
    //         SetUpStringList();
    // }

    public void LoadTSP()
    {
        EntMgr.inst.CleanUpAll();
        if(EntMgr.inst.algoritmOnline)
            EntMgr.inst.ToggleHillClimb();
        if(fileStatus>=0)
        {
            using (StringReader reader = new StringReader(tspRawFiles[fileStatus].text))
            {
                string line;
                bool cordSectionFound = false;
                while ((line = reader.ReadLine()) != null) 
                {
                    if(line =="EOF")
                        break;
                    if(!cordSectionFound && line == "NODE_COORD_SECTION")
                        cordSectionFound=true;
                    else if(cordSectionFound)
                    {
                        Vector3 location = Vector3.zero ;
                        string[] chunks = line.Split(' ');
                        if(chunks.Length<3)
                        {
                            Debug.LogError("Invalid Parse");
                            Debug.LogError(line);
                            continue;
                        }
                        location.x=float.Parse(chunks[1]);
                        location.y=float.Parse(chunks[2]);
                        
                        EntMgr.inst.MakeNodeObj(location); 
                    }
                        
                }
            }
            float maxLength =0f;
            EntMgr.inst.AddAllPaths();
            if(EntMgr.inst.nodeList.Count<2)
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
}
