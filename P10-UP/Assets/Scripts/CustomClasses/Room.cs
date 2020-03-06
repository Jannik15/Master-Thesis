using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public readonly GameObject room;
    private int roomId;
    private List<Portal> portalsInRoom, portalsToRoom;
    public Room(GameObject room, int roomId)
    {
        this.room = room;
        this.roomId = roomId;
        portalsInRoom = new List<Portal>();
        portalsToRoom = new List<Portal>();
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
}
