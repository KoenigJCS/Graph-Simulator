using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionMgr : MonoBehaviour
{
    public static SelectionMgr inst;
    public int selectedIndex = 0;
    public NodeEnt selectedEntity;
    bool isSelecting = false;
    Vector3 mousePos1;
    public List<NodeEnt> selectedNodes = new List<NodeEnt>();
    private void Awake() {
        inst = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyUp(KeyCode.Tab))
        {
            SelectNextEnt();
        }
        if(PlacementMgr.inst.placementMode==6)
        {
        //Selecting
            if(Input.GetMouseButtonDown(0))
            {
                UnselectAll();
                isSelecting = true;
                mousePos1 = Input.mousePosition;
            }
        }
        if(Input.GetMouseButtonUp(0))
        {
            selectedNodes.Clear();
            foreach(NodeEnt ent in EntMgr.inst.nodeList)
            {
                if(InSelectedZone(ent.transform.position))
                {
                    selectedNodes.Add(ent);
                    ent.SetSelected(true);
                }
            }
            mousePos1 = Input.mousePosition;
            isSelecting = false;
        }
    }

    void OnGUI() 
    {
        if(isSelecting)
        {
            Rect rect = Utils.GetScreenRect(mousePos1, Input.mousePosition);
            Utils.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
            Utils.DrawScreenBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));
        }
    }

    void SelectNextEnt()
    {
        selectedIndex = (selectedIndex >= EntMgr.inst.nodeList.Count - 1 ? 0 : selectedIndex + 1);
        selectedEntity = EntMgr.inst.nodeList[selectedIndex];
        UnselectAll();
        selectedEntity.SetSelected(true);
        selectedNodes.Add(selectedEntity);
    }

    void UnselectAll()
    {
        foreach(NodeEnt node in EntMgr.inst.nodeList)
        {
            node.SetSelected(false);
        }
    }

    bool InSelectedZone(Vector3 position)
    {
        if(!isSelecting)
            return false;
        var camera = Camera.main;
        var viewportBounds = Utils.GetViewportBounds(camera, mousePos1, Input.mousePosition);

        return viewportBounds.Contains(camera.WorldToViewportPoint(position));
    }

    public void SetSelectedColor(int color)
    {
        Color pickedColor;
        switch (color)
        {
            case 1:
                pickedColor=Color.red;
                break;
            case 2:
                pickedColor=Color.green;
                break;
            case 3:
                pickedColor=Color.blue;
                break;
            default:
                pickedColor=Color.white;
                break;
        } 
        foreach(NodeEnt node in selectedNodes)
        {
            node.SetColor(pickedColor);
        }    
    }
}
