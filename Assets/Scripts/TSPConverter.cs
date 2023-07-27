using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TSPConverter : MonoBehaviour
{
        public int fileStatus = -1;
        public TMPro.TMP_Dropdown fileDropDown;
        public static TSPConverter inst;
        void Awake()
        {
            inst = this;
        }
        // Start is called before the first frame update
        void Start()
        {
            var tspFiles = new DirectoryInfo("Assets/TSPFiles");
            var fileList = tspFiles.EnumerateFiles();
            List<TMPro.TMP_Dropdown.OptionData> optionList = new List<TMPro.TMP_Dropdown.OptionData>();
            fileDropDown.ClearOptions();
            optionList.Add(new TMPro.TMP_Dropdown.OptionData("None"));
            foreach (var file in fileList)
            {
                optionList.Add(new TMPro.TMP_Dropdown.OptionData(file.Name));
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
}
