using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public readonly GameObject gameObject;
    public Grid roomGrid;
    private int roomID, stencilValue;
    private List<Portal> portalsInRoom, portalsToRoom;
    private Grid grid;
    public List<Transform> playerCollisionObjectsInRoom = new List<Transform>(), noPlayerCollisionObjectsInRoom = new List<Transform>();
    public Room(GameObject gameObject, int roomID, Grid grid)
    {
        this.gameObject = gameObject;
        this.roomID = roomID; // Room id is currently just its index in the rooms list
        this.roomGrid = grid;
        portalsInRoom = new List<Portal>();
        portalsToRoom = new List<Portal>();

        playerCollisionObjectsInRoom.AddRange(gameObject.GetComponentsInChildren<Transform>());
    }

    public void UpdateRoomStencil(Transform portal, int newStencilValue, int renderQueue)
    {
        stencilValue = newStencilValue;
        CustomUtilities.UpdateStencils(gameObject, portal, newStencilValue, renderQueue);
    }

    public int GetStencilValue()
    {
        return stencilValue;
    }

    public int GetRoomID()
    {
        return roomID;
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
    public List<Portal> GetPortalsInRoom()
    {
        return portalsInRoom;
    }
    public List<Portal> GetPortalsToRoom()
    {
        return portalsToRoom;
    }

    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="objectToAdd"></param>
    /// <param name="tileToPlaceObjectOn"></param>
    /// <param name="parent"></param>
    /// <param name="playerCanCollide"></param>
    public void InstantiateStaticObjectInRoom(GameObject objectToAdd, Tile tileToPlaceObjectOn, Transform parent, bool playerCanCollide)
    {
        GameObject objectIsAdded = tileToPlaceObjectOn.PlaceObject(objectToAdd, parent);
        if (objectIsAdded != null)
        {
            Transform[] objectToAddAndItsChildren = objectIsAdded.GetComponentsInChildren<Transform>();
            if (playerCanCollide)
            {
                playerCollisionObjectsInRoom.AddRange(objectToAddAndItsChildren);
            }
            else
            {
                noPlayerCollisionObjectsInRoom.AddRange(objectToAddAndItsChildren);
            }
        }
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
            if (noPlayerCollisionObjectsInRoom[i] != null)
            {
                noPlayerCollisionObjectsInRoom[i].gameObject.layer = layerForNoPlayerCollisionObjects;
            }
            else
            {
                //Debug.Log("You forgot to remove an object from the room that is set to not collide with the player!");
            }
        }
        for (int i = 0; i < playerCollisionObjectsInRoom.Count; i++)
        {
            if (playerCollisionObjectsInRoom[i] != null)
            {
                playerCollisionObjectsInRoom[i].gameObject.layer = layerForPlayerCollisionObjects;
            }
            else
            {
                //Debug.Log("You forgot to remove an object from the room that is set to collide with the player!");
            }
        }
        Transform[] allChildren = gameObject.GetComponentsInChildren<Transform>();
        for (int i = 0; i < allChildren.Length; i++)
        {
            if (!playerCollisionObjectsInRoom.Contains(allChildren[i]) &&
                !noPlayerCollisionObjectsInRoom.Contains(allChildren[i]))
            {
                allChildren[i].gameObject.layer = layerForNoPlayerCollisionObjects;
                Debug.Log(allChildren[i].gameObject.name + " was not assigned to room " + gameObject.name + " but is a child of it!");
            }
        

        }
    } 
}
