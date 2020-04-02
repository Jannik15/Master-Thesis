using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public readonly GameObject gameObject;
    public Grid roomGrid;
    private int roomId;
    private List<Portal> portalsInRoom, portalsToRoom;
    private List<Transform> playerCollisionObjectsInRoom = new List<Transform>(), noPlayerCollisionObjectsInRoom = new List<Transform>();
    public Room(GameObject gameObject, int roomId, Grid grid)
    {
        this.gameObject = gameObject;
        this.roomId = roomId;
        this.roomGrid = grid;
        portalsInRoom = new List<Portal>();
        portalsToRoom = new List<Portal>();

        noPlayerCollisionObjectsInRoom.AddRange(gameObject.GetComponentsInChildren<Transform>());
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
    /// <summary>
    /// Use this when the object cannot move from the room it is instantiated in.
    /// </summary>
    public void InstantiateStaticObjectInRoom(GameObject objectToAdd, Tile tileToPlaceObjectOn, bool playerCanCollide)
    {
        tileToPlaceObjectOn.PlaceObject(objectToAdd);
        Transform[] objectToAddAndItsChildren = objectToAdd.GetComponentsInChildren<Transform>();
        if (playerCanCollide)
        {
            playerCollisionObjectsInRoom.AddRange(objectToAddAndItsChildren);
        }
        else
        {
            noPlayerCollisionObjectsInRoom.AddRange(objectToAddAndItsChildren);
        }
        objectToAdd.transform.SetParent(gameObject.transform);
    }

    public void AddObjectToRoom(Transform objectToAdd, bool playerCanCollide)
    {
        Transform[] objectToAddAndItsChildren = objectToAdd.GetComponentsInChildren<Transform>();
        if (playerCanCollide)
        {
            playerCollisionObjectsInRoom.AddRange(objectToAddAndItsChildren);
        }
        else
        {
            noPlayerCollisionObjectsInRoom.AddRange(objectToAddAndItsChildren);
        }
        objectToAdd.SetParent(gameObject.transform);
    }

    public void AddObjectsToRoom(IEnumerable<Transform> objectsToAdd, bool playerCanCollide)
    {
        if (playerCanCollide)
        {
            playerCollisionObjectsInRoom.AddRange(objectsToAdd);
        }
        else
        {
            noPlayerCollisionObjectsInRoom.AddRange(objectsToAdd);
        }
        foreach (var objectToAdd in objectsToAdd)
        {
            objectToAdd.SetParent(gameObject.transform);
        }
    }

    public void RemoveObjectFromRoom(Transform objectToRemove)
    {
        Transform[] objectToRemoveAndItsChildren = objectToRemove.GetComponentsInChildren<Transform>();
        for (int i = 0; i < objectToRemoveAndItsChildren.Length; i++)
        {
            if (noPlayerCollisionObjectsInRoom.Contains(objectToRemoveAndItsChildren[i]))
            {
                noPlayerCollisionObjectsInRoom.Remove(objectToRemoveAndItsChildren[i]);
            }
            else if (playerCollisionObjectsInRoom.Contains(objectToRemoveAndItsChildren[i]))
            {
                playerCollisionObjectsInRoom.Remove(objectToRemoveAndItsChildren[i]);
            }
            else
            {
                Debug.Log("Attempted to remove " + objectToRemove.name + " from Room " + gameObject.name + " but that object was not found in the rooms object list.");
            }
        }
    }

    public void SetLayer(int layerForPlayerCollisionObjects, int layerForNoPlayerCollisionObjects)
    {
        for (int i = 0; i < noPlayerCollisionObjectsInRoom.Count; i++)
        {
            noPlayerCollisionObjectsInRoom[i].gameObject.layer = layerForNoPlayerCollisionObjects;
        }
        for (int i = 0; i < playerCollisionObjectsInRoom.Count; i++)
        {
            playerCollisionObjectsInRoom[i].gameObject.layer = layerForPlayerCollisionObjects;
        }
    } 
}
