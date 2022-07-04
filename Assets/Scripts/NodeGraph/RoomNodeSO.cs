using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RoomNodeSO : ScriptableObject
{
  //  [HideInInspector]
    public string id;
    //[HideInInspector]
    public List<string> parentRoomNodeIDList = new List<string>();
    //[HideInInspector]
    public List<string> childRoomNodeIDList = new List<string>();
//    [HideInInspector]
    public RoomNodeTypeSO roomNodeType;
    [HideInInspector]
    public RoomNodeGraphSO roomNodeGraph;
    [HideInInspector]
    public RoomNodeTypeListSO roomNodeTypeList;
    [HideInInspector]
    public bool isLeftClickDragging = false;
    [HideInInspector]
    public bool isSelected = false;


#if UNITY_EDITOR
    [HideInInspector]
    public Rect rect;

    public void Initialise(Rect rect, RoomNodeGraphSO nodeGraph, RoomNodeTypeSO roomNodeType)
    {
        this.rect = rect;
        this.id = Guid.NewGuid().ToString();
        this.name = "RoomNode";
        this.roomNodeGraph = nodeGraph;
        this.roomNodeType = roomNodeType;

        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;

    }

    public void Draw(GUIStyle nodeStyle)
    {
        GUILayout.BeginArea(rect, nodeStyle);
        EditorGUI.BeginChangeCheck();
        int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);
        if (roomNodeType.isEntrance || parentRoomNodeIDList.Count > 0)
        {
            EditorGUILayout.LabelField(roomNodeTypeList.list[selected].roomNodeTypeName);
        } else
        {
            int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());
            roomNodeType = roomNodeTypeList.list[selection];
            if (roomNodeTypeList.list[selected].isCorridor && !roomNodeTypeList.list[selection].isCorridor || !roomNodeTypeList.list[selected].isCorridor
                && roomNodeTypeList.list[selection].isCorridor || !roomNodeTypeList.list[selected].isBossRoom && roomNodeTypeList.list[selection].isBossRoom)
            {
                if (childRoomNodeIDList.Count > 0)
                {
                    for (int i = 0; i < childRoomNodeIDList.Count; i++)
                    {
                        RoomNodeSO childRoomNode = roomNodeGraph.GetRoomNode(childRoomNodeIDList[i]);
                        if (childRoomNode != null && childRoomNode.isSelected)
                        {
                            RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);
                            childRoomNode.RemoveParentRoomNodeIDFromRoomNode(id);
                        }
                    }
                }
            }
        }
        
        if(EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(this);
        }
        GUILayout.EndArea();
    }

    public string[] GetRoomNodeTypesToDisplay()
    {
        string[] roomArray = new string[roomNodeTypeList.list.Count];
        for(int i = 0; i < roomNodeTypeList.list.Count; i++ )
        {
            if (roomNodeTypeList.list[i].displayInNodeGraphEditor)
            {
                roomArray[i] = roomNodeTypeList.list[i].roomNodeTypeName;
            }
        }

        return roomArray;
    }

    public void ProcessEvents(Event currentEvent) 
    { 
        switch(currentEvent.type) 
        {
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;
            default:
                break;
        }
    }

    private void ProcessMouseDownEvent(Event currentEvent) 
    { 
        if(currentEvent.button == 0)
        {
            ProcessLeftClickDownEvent();
        }
        if(currentEvent.button == 1)
        {
            ProcessRightClickDownEvent(currentEvent);
        }
    }

    private void ProcessMouseUpEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            ProcessLeftClickUpEvent();
        }
    }

    private void ProcessMouseDragEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            ProcessLeftClickDragEvent(currentEvent);
        }
    }

    private void ProcessLeftClickUpEvent()
    {
        if(isLeftClickDragging)
        {
            isLeftClickDragging = false;
        }
    }

    private void ProcessLeftClickDownEvent()
    {
        Selection.activeObject = this;
        if (isSelected) 
        {
            isSelected = false;
        } else
        {
            isSelected = true;
        }
    }

    private void ProcessRightClickDownEvent(Event currentEvent)
    {
        roomNodeGraph.setNodeToDrawConnectionLineFrom(this, currentEvent.mousePosition);
    }

    private void ProcessLeftClickDragEvent(Event currentEvent)
    {
        isLeftClickDragging = true;
        DragNode(currentEvent.delta);
        GUI.changed = true;
    }

    public void DragNode(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }

    public bool AddChildRoomNodeIDToRoomNode(string childId)
    {
        if(IsChildRoomValid(childId))
        {
            childRoomNodeIDList.Add(childId);
            return true;
        }
        return false;
    }

    public bool IsChildRoomValid(string childId)
    {
        bool isConnectedBossRoomAlready = false;
        foreach(RoomNodeSO roomNode in roomNodeGraph.roomNodeList)
        {
            if(roomNode.roomNodeType.isBossRoom && roomNode.parentRoomNodeIDList.Count > 0)
            {
                isConnectedBossRoomAlready = true;
            }
           
        }
        if(roomNodeGraph.GetRoomNode(childId).roomNodeType.isBossRoom && isConnectedBossRoomAlready)
        {
            return false;
        }
        if (roomNodeGraph.GetRoomNode(childId).roomNodeType.isNone)
        {
            return false;
        }
        if (childRoomNodeIDList.Contains(childId))
        {
            return false;
        }

        if (parentRoomNodeIDList.Contains(childId))
        {
            return false;
        }

        if (id == childId)
        {
            return false;
        }

        if(roomNodeGraph.GetRoomNode(childId).parentRoomNodeIDList.Count > 0)
        {
            return false;
        }

        if(roomNodeGraph.GetRoomNode(childId).roomNodeType.isCorridor && roomNodeType.isCorridor)
        {
            return false;
        }
        if (!roomNodeGraph.GetRoomNode(childId).roomNodeType.isCorridor && !roomNodeType.isCorridor)
        {
            return false;
        }

        if(roomNodeGraph.GetRoomNode(childId).roomNodeType.isCorridor && childRoomNodeIDList.Count >= Settings.maxChildCorridors)
        {
            return false;
        }
        if(roomNodeGraph.GetRoomNode(childId).roomNodeType.isEntrance)
        {
            return false;
        }
        if(!roomNodeGraph.GetRoomNode(childId).roomNodeType.isCorridor && childRoomNodeIDList.Count > 0)
        {
            return false;
        }
        return true;  
    }
    public bool AddParentRoomNodeIDToRoomNode(string parentId)
    {
        parentRoomNodeIDList.Add(parentId);
        return true;
    }

    public bool RemoveChildRoomNodeIDFromRoomNode(string RoomNodeId)
    {
        if(childRoomNodeIDList.Contains(RoomNodeId))
        {
            childRoomNodeIDList.Remove(RoomNodeId);
            return true;
        }
        return false;
    }
    public bool RemoveParentRoomNodeIDFromRoomNode(string RoomNodeId)
    {
        if (parentRoomNodeIDList.Contains(RoomNodeId))
        {
            parentRoomNodeIDList.Remove(RoomNodeId);
            return true;
        }
        return false;
    }

#endif
}
