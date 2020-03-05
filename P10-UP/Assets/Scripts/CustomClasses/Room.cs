using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public readonly GameObject room;
    private List<Portal> portalsInRoom, portalsToRoom;
    public Room(GameObject room, List<Portal> portalsInRoom, List<Portal> portalsToRoom)
    {
        this.room = room;
        portalsInRoom = new List<Portal>();
        portalsToRoom = new List<Portal>();
    }

    public void AddPortalToRoom()
    {

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
