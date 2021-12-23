using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum States : int
{
    Empty = 0,
    Wall = 1
}

public struct Cell
{
    public States state;
    public GameObject mesh;
    public int neighbourCount;
    public Color color;
}

public struct Coord
{
    public int xCoord;
    public int yCoord;
    public int zCoord;

    public Coord(int x, int y)
    {
        xCoord = x;
        yCoord = y;
        zCoord = 0;
    }
    public Coord(int x, int y, int z)
    {
        xCoord = x;
        yCoord = y;
        zCoord = z;
    }

    public static implicit operator Vector3Int(Coord value) => new Vector3Int(value.xCoord, value.yCoord, value.zCoord);
    public static implicit operator Vector2Int(Coord value) => new Vector2Int(value.xCoord, value.yCoord);
}

public class Room :IComparable<Room>
{
    private List<Coord> _cells;
    private List<Coord> _edgeCells;
    private List<Room> _connectedRooms;
    private int _roomSize;
    private bool _isMainRoom;
    private bool _isAccessableFormMainRoom;

    private int _floorIndex;
    private int _roofIndex;

    public Room(){ }

    //2D constructor
    public Room(List<Coord> roomCells, Cell[,] map)
    {
        _cells = roomCells;
        _roomSize = roomCells.Count;
        _connectedRooms = new List<Room>();

        _edgeCells = new List<Coord>();
        foreach (Coord cell in roomCells)
        {
            for (int x = cell.xCoord - 1; x <= cell.xCoord + 1; x++)
            {
                for (int y = cell.yCoord - 1; y <= cell.yCoord + 1; y++)
                {
                    //von neumann neighbours only
                    if (x == cell.xCoord || y == cell.yCoord)
                    {
                        if (map[x, y].state == States.Wall)
                        {
                            _edgeCells.Add(cell);
                        }
                    }
                }
            }
        }
    }

    //3D constructor
    public Room(List<Coord> roomCells, Cell[,,] map)
    {
        _cells = roomCells;
        _roomSize = roomCells.Count;
        _connectedRooms = new List<Room>();

        _roofIndex = 0;
        _floorIndex = int.MaxValue;

        _edgeCells = new List<Coord>();
        foreach (Coord cell in roomCells)
        {
            for (int x = cell.xCoord - 1; x <= cell.xCoord + 1; x++)
            {
                for (int y = cell.yCoord - 1; y <= cell.yCoord + 1; y++)
                {
                    if (_roofIndex < y)
                        _roofIndex = y;
                    else if (_floorIndex > y)
                        _floorIndex = y;
                    for (int z = cell.zCoord - 1; z <= cell.zCoord + 1; z++)
                    {
                        //von neumann neighbours only
                        if (x == cell.xCoord || y == cell.yCoord||z==cell.zCoord)
                        {
                            if (map[x, y,z].state == States.Wall)
                            {
                                _edgeCells.Add(cell);
                            }
                        }
                    }
                }
            }
        }
    }

    public bool IsMainRoom
    {
        get => _isMainRoom;
        set => _isMainRoom = value;
    }

    public bool IsAccessableFormMainRoom
    {
        get => _isAccessableFormMainRoom;
        set
        {
            if (value != _isAccessableFormMainRoom)
            {
                _isAccessableFormMainRoom = value;
                if (value)
                {
                    foreach (var connectedRoom in ConnectedRooms)
                    {
                        connectedRoom.IsAccessableFormMainRoom = true;
                    }
                }
            }
        } 
    }

    public int RoomSize => _roomSize;

    public int FloorIndex => _floorIndex;

    public int RoofIndex => _roofIndex;

    public List<Room> ConnectedRooms => _connectedRooms;

    public List<Coord> Cells => _cells;

    public List<Coord> EdgeCells => _edgeCells;

    public static void ConnectRooms(Room firstRoom, Room secondRoom)
    {
        if (firstRoom.IsAccessableFormMainRoom)
        {
            secondRoom.IsAccessableFormMainRoom = true;
        }
        else if (secondRoom.IsAccessableFormMainRoom)
        {
            firstRoom.IsAccessableFormMainRoom = true;
        }

        firstRoom.ConnectedRooms.Add(secondRoom);
        secondRoom.ConnectedRooms.Add(firstRoom);
    }



    public bool IsConnected(Room otherRoom)
    {
        return _connectedRooms.Contains(otherRoom);
    }

    public int CompareTo(Room other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        return other._roomSize.CompareTo(_roomSize);
    }
}
