using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public readonly GameObject gameObject;
    public  Grid roomGrid;
    private int roomId;
    private List<Portal> portalsInRoom, portalsToRoom;
    private Grid grid;
    private List<Transform> objectsInRoom = new List<Transform>();
    public Room(GameObject gameObject, int roomId, Grid grid)
    {
        this.gameObject = gameObject;
        this.roomId = roomId;
        this.roomGrid = grid;
        portalsInRoom = new List<Portal>();
        portalsToRoom = new List<Portal>();

        objectsInRoom.AddRange(gameObject.GetComponentsInChildren<Transform>());
    }

    public int GetRoomId()
    {
        return roomId;
    }

    public void AddPortalInRoom(Portal portal)
    {
        if (portalsInRoom == null)
        {
            portalsInRoom = new List<Portal>();
        }
        portalsInRoom.Add(portal);
    }
    public void AddPortalToRoom(Portal portal)
    {
        if (portalsToRoom == null)
        {
            portalsToRoom = new List<Portal>();
        }
        portalsToRoom.Add(portal);
    }

    public Portal GetPortalInRoom(int index)
    {
        return portalsInRoom[index];
    }

    public Portal GetPortalToRoom(int index)
    {
        return portalsInRoom[index];
    }
    public List<Portal> GetPortalsInRoom()
    {
        return portalsInRoom;
    }
    public List<Portal> GetPortalsToRoom()
    {
        return portalsToRoom;
    }

    public void SetLayer(int layer)
    {
        for (int i = 0; i < objectsInRoom.Count; i++)
        {
            objectsInRoom[i].gameObject.layer = layer;
        }
    } 
}
