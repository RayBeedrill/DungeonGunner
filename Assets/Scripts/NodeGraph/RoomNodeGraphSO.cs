using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeGraph", menuName = "Scriptable Objects/Dungeon/Room Node Graph")]
public class RoomNodeGraphSO : ScriptableObject
{
    [HideInInspector]
    public RoomNodeTypeListSO roomNodeTypeList;
    [HideInInspector]
    public List<RoomNodeSO> roomNodeList = new List<RoomNodeSO>();
    [HideInInspector]
    public Dictionary<string, RoomNodeSO> roomNodeDictionary = new Dictionary<string, RoomNodeSO>();

    private void Awake()
    {
        LoadRoomNodeDictionary();
    }

    private void LoadRoomNodeDictionary()
    {
        roomNodeDictionary.Clear();

        foreach(RoomNodeSO node in roomNodeList) 
        {
            roomNodeDictionary[node.id] = node;
        }
    }

    public RoomNodeSO GetRoomNode(string roomNodeId) 
    {
        if(roomNodeDictionary.TryGetValue(roomNodeId, out RoomNodeSO roomNode))
        {
            return roomNode;
        }
        return null;
    }

#if UNITY_EDITOR
    [HideInInspector]
    public RoomNodeSO roomNodeToDrawLineFrom = null;
    [HideInInspector]
    public Vector2 linePosition;

    public void OnValidate()
    {
        LoadRoomNodeDictionary();
    }
    public void setNodeToDrawConnectionLineFrom(RoomNodeSO node, Vector2 position)
    {
        roomNodeToDrawLineFrom = node;
        linePosition = position;
    }

    public void RemoveRoomNode(string id)
    {
        roomNodeList = roomNodeList.FindAll(item => item.id != id);
        roomNodeDictionary.Remove(id);
    }
#endif

}
