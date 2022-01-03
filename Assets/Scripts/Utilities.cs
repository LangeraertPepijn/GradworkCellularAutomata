using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public enum States : int
{
    Empty = 0,
    Wall = 1
}

public enum Axis
{
    x = 0,
    y = 1,
    z = 2
}

public class Chunk
{
    public Cell[,] Map { get; }
    public Cell[,,] Map3D { get; }
    public int DistIndex { get; }
    public int CreationIndex { get; }

    public Chunk(Cell[,] map,int creationIndex,int distIndex)
    {
        Map = map;
        CreationIndex = creationIndex;
        DistIndex = distIndex;
    }
    public Chunk(Cell[,,] map,int creationIndex, int distIndex)
    {
        Map3D = map;
        CreationIndex = creationIndex;
        DistIndex = distIndex;
    }

    public List<int> GetNeighbouringIndices()
    {
        List<int> indices = new List<int>();


        //DistIndex*4 right
        if (CreationIndex <= DistIndex)
        {
            indices.Add(ChunkMap.CalcIndex(DistIndex + 1, CreationIndex));

        }
        else if (CreationIndex <= DistIndex * 3)
        {
            indices.Add(ChunkMap.CalcIndex(DistIndex - 1, CreationIndex + 2));

        }
        else
        {
            indices.Add(ChunkMap.CalcIndex(DistIndex + 1, CreationIndex + 4));

        }

        //up
        if (CreationIndex <= DistIndex * 2)
        {
            indices.Add(ChunkMap.CalcIndex(DistIndex + 1, CreationIndex + 1));

        }
        else if (CreationIndex < DistIndex * 4)
        {
            indices.Add(ChunkMap.CalcIndex(DistIndex - 1, CreationIndex - 3));

        }
        else
        {
            indices.Add(ChunkMap.CalcIndex(DistIndex + 1, 0));

        }

        //left
        if (CreationIndex <= DistIndex)
        {
            indices.Add(ChunkMap.CalcIndex(DistIndex - 1, CreationIndex));

        }
        else if (CreationIndex <= DistIndex * 3)
        {
            indices.Add(ChunkMap.CalcIndex(DistIndex + 1, CreationIndex + 2));

        }
        else
        {
            indices.Add(ChunkMap.CalcIndex(DistIndex - 1, -4));

        }

        //down
        if (CreationIndex == 0)
        {
            indices.Add(ChunkMap.CalcIndex(DistIndex + 1, 7 + 4 * DistIndex));

        }
        else if (CreationIndex <= DistIndex * 2)
        {
            indices.Add(ChunkMap.CalcIndex(DistIndex - 1, CreationIndex - 1));

        }
        else if (CreationIndex < DistIndex * 4)
        {
            indices.Add(ChunkMap.CalcIndex(DistIndex + 1, CreationIndex + 3));

        }

        // righttopDIag
        if (CreationIndex <= DistIndex)
        {
            indices.Add(ChunkMap.CalcIndex(DistIndex + 2, CreationIndex + 1));

        }
        else if (CreationIndex <= DistIndex * 2)
        {
            indices.Add(ChunkMap.CalcIndex(DistIndex, CreationIndex - 1));

        }
        else if (CreationIndex <= DistIndex * 3)
        {
            indices.Add(ChunkMap.CalcIndex(DistIndex - 2, CreationIndex - 5));

        }
        else if (CreationIndex < DistIndex * 4)
        {
            indices.Add(ChunkMap.CalcIndex(DistIndex , CreationIndex +1));

        }
        else
        {
            
            indices.Add(ChunkMap.CalcIndex(DistIndex , 0));
        }

        // lefttopDIag
        if (CreationIndex < DistIndex)
        {
            indices.Add(ChunkMap.CalcIndex(DistIndex , CreationIndex + 1));

        }
        else if (CreationIndex <= DistIndex * 2)
        {
            indices.Add(ChunkMap.CalcIndex(DistIndex+2, CreationIndex + 3));

        }
        else if (CreationIndex <= DistIndex * 3)
        {
            indices.Add(ChunkMap.CalcIndex(DistIndex , CreationIndex - 1));

        }
        else if (CreationIndex < DistIndex * 4)
        {
            indices.Add(ChunkMap.CalcIndex(DistIndex-2, CreationIndex - 7));

        }
        else
        {

            indices.Add(ChunkMap.CalcIndex(DistIndex-2, 0));
        }


        // leftbotDIag
        if(CreationIndex==0)
        {
            indices.Add(ChunkMap.CalcIndex(DistIndex , 3+DistIndex*4));
        }
        else if (CreationIndex < DistIndex)
        {
            indices.Add(ChunkMap.CalcIndex(DistIndex-2, CreationIndex - 1));

        }
        else if (CreationIndex < DistIndex * 2)
        {
            indices.Add(ChunkMap.CalcIndex(DistIndex , CreationIndex + 1));

        }
        else if (CreationIndex <= DistIndex * 3)
        {
            indices.Add(ChunkMap.CalcIndex(DistIndex+2, CreationIndex +5));

        }
        else if (CreationIndex < DistIndex * 4)
        {
            indices.Add(ChunkMap.CalcIndex(DistIndex , CreationIndex - 1));

        }

        // rightbotDIag
        if (CreationIndex == 0)
        {
            indices.Add(ChunkMap.CalcIndex(DistIndex+2, 7 + DistIndex * 4));
        }
        else if (CreationIndex <= DistIndex)
        {
            indices.Add(ChunkMap.CalcIndex(DistIndex , CreationIndex - 1));

        }
        else if (CreationIndex < DistIndex * 2)
        {
            indices.Add(ChunkMap.CalcIndex(DistIndex-2, CreationIndex -3));

        }
        else if (CreationIndex < DistIndex * 3)
        {
            indices.Add(ChunkMap.CalcIndex(DistIndex, CreationIndex + 1));

        }
        else if (CreationIndex < DistIndex * 4)
        {
            indices.Add(ChunkMap.CalcIndex(DistIndex +2, CreationIndex +7));

        }

        return indices;
    }
}

public class ChunkMap
{
    public Dictionary<int,Chunk> _map=new Dictionary<int, Chunk>();

    public static int CalcIndex(Chunk chunk)
    {
        return chunk.CreationIndex + chunk.DistIndex * 1000;
    }
    public static int CalcIndex(int distIndex,int creationIndex)
    {
        return creationIndex + distIndex * 1000;
    }

    public void Add(Chunk chunk)
    {
        int index = chunk.CreationIndex + chunk.DistIndex * 1000;
        _map[index] = chunk;
    }
    public Chunk Get(int index)
    {
        if(_map.ContainsKey(index))
            return _map[index];
        else
            return null;
        
    }
}
public struct Cell
{
    public States state;
    public GameObject mesh;
    public int neighbourCount;
    public Color color;

    public override string ToString()
    {
        return "state: " + (state == States.Empty ? "Empty" : "Wall");
    }

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

    public Coord(Vector2Int other)
    {
        xCoord = other.x;
        yCoord = other.y;
        zCoord = 0;
    }
    public Coord(Vector3Int other)
    {
        xCoord = other.x;
        yCoord = other.y;
        zCoord = other.z;
    }
    public override string ToString()
    {
        return "x: " + xCoord+ "x: " + " " + yCoord+ "x: "+zCoord;
    }
    public static implicit operator Vector3Int(Coord value) => new Vector3Int(value.xCoord, value.yCoord, value.zCoord);
    public static implicit operator Vector2Int(Coord value) => new Vector2Int(value.xCoord, value.yCoord);

    public static Coord operator +(Coord a, Vector3Int b)
    {
        Coord result = new Coord();
        result.xCoord = a.xCoord + b.x;
        result.yCoord = a.yCoord + b.y;
        result.zCoord = a.zCoord + b.z;
        return result;
    }  
    public static Coord operator +(Vector3Int a, Coord b)
    {
        Coord result = new Coord();
        result.xCoord = a.x + b.xCoord;
        result.yCoord = a.y + b.yCoord;
        result.zCoord = a.z + b.zCoord;
        return result;
    }
    public static Coord operator -(Coord a, Vector3Int b)
    {
        Coord result = new Coord();
        result.xCoord = a.xCoord - b.x;
        result.yCoord = a.yCoord - b.y;
        result.zCoord = a.zCoord - b.z;
        return result;
    }
    public static Coord operator -(Vector3Int a, Coord b)
    {
        Coord result = new Coord();
        result.xCoord = a.x - b.xCoord;
        result.yCoord = a.y - b.yCoord;
        result.zCoord = a.z - b.zCoord;
        return result;
    }


    public static Coord operator +(Coord a, Vector2Int b)
    {
        Coord result = a;
        result.xCoord = a.xCoord + b.x;
        result.yCoord = a.yCoord + b.y;
        return result;
    }
    public static Coord operator +(Vector2Int a, Coord b)
    {
        Coord result = b;
        result.xCoord = a.x + b.xCoord;
        result.yCoord = a.y + b.yCoord;
        return result;
    }
    public static Coord operator -(Coord a, Vector2Int b)
    {
        Coord result = a;
        result.xCoord = a.xCoord - b.x;
        result.yCoord = a.yCoord - b.y;
        return result;
    }
    public static Coord operator -(Vector2Int a, Coord b)
    {
        Coord result = b;
        result.xCoord = a.x - b.xCoord;
        result.yCoord = a.y - b.yCoord;
        return result;
    }

}

public abstract class Area: IComparable<Area>
{

    protected List<Coord> _cells=new List<Coord>();
    protected List<Coord> _edgeCells=new List<Coord>();
    protected List<Room> _connectedRooms=new List<Room>();
    protected int _size=0;
    protected int _floorIndex;
    protected int _roofIndex;

    public int Size => _size;

    public int FloorIndex => _floorIndex;

    public int RoofIndex => _roofIndex;

    public List<Room> ConnectedRooms
    {
        get => _connectedRooms;
        set => _connectedRooms = value;
    }


    public List<Coord> Cells => _cells;

    public List<Coord> EdgeCells => _edgeCells;

    public bool IsConnected(Room otherRoom)
    {
        return _connectedRooms.Contains(otherRoom);
    }

    public int CompareTo(Area other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        return other._size.CompareTo(_size);
    }
}

public class Room : Area
{

    private bool _isMainRoom;
    private bool _isAccessibleFormMainRoom;

    public Room()
    {
    }

    //2D constructor
    public Room(List<Coord> roomCells, Cell[,] map)
    {
        _cells = roomCells;
        _size = roomCells.Count;
        _connectedRooms = new List<Room>();

        _edgeCells = new List<Coord>();
        foreach (Coord cell in roomCells)
        {
            for (int x = cell.xCoord - 1; x <= cell.xCoord + 1; x++)
            {
                for (int y = cell.yCoord - 1; y <= cell.yCoord + 1; y++)
                {
                    //von neumann neighbours only
                    if (x >= 0 && x < map.GetLength(0) && y >= 0 && y < map.GetLength(1))
                    {
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
    }

    //3D constructor
    public Room(List<Coord> roomCells, Cell[,,] map)
    {
        _cells = roomCells;
        _size = roomCells.Count;
        _connectedRooms = new List<Room>();

        _roofIndex = 0;
        _floorIndex = int.MaxValue;

        _edgeCells = new List<Coord>();
        foreach (Coord cell in roomCells)
        {
            if (_roofIndex < cell.yCoord)
                _roofIndex = cell.yCoord;
            if (_floorIndex > cell.yCoord)
                _floorIndex = cell.yCoord;

            for (int x = cell.xCoord - 1; x <= cell.xCoord + 1; x++)
            {
                for (int y = cell.yCoord - 1; y <= cell.yCoord + 1; y++)
                {
    
                    for (int z = cell.zCoord - 1; z <= cell.zCoord + 1; z++)
                    {
                        if (x >= 0 && x < map.GetLength(0) && y >= 0 && y < map.GetLength(1) && z >= 0 &&
                            z < map.GetLength(2))
                        {
                            //von neumann neighbours only
                            if (x == cell.xCoord || y == cell.yCoord || z == cell.zCoord)
                            {
                                if (map[x, y, z].state == States.Wall)
                                {
                                    _edgeCells.Add(cell);
                                }
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

    public bool IsAccessibleFormMainRoom
    {
        get => _isAccessibleFormMainRoom;
        set
        {
            if (value != _isAccessibleFormMainRoom)
            {
                _isAccessibleFormMainRoom = value;
                if (value)
                {
                    foreach (var connectedRoom in ConnectedRooms)
                    {
                        connectedRoom.IsAccessibleFormMainRoom = true;
                    }
                }
            }
        }
    }


    public static void ConnectRooms(Room firstRoom, Room secondRoom)
    {
        if (firstRoom.IsAccessibleFormMainRoom)
        {
            secondRoom.IsAccessibleFormMainRoom = true;
        }
        else if (secondRoom.IsAccessibleFormMainRoom)
        {
            firstRoom.IsAccessibleFormMainRoom = true;
        }

        firstRoom.ConnectedRooms.Add(secondRoom);
        secondRoom.ConnectedRooms.Add(firstRoom);
    }

}

public class Corridor : Area
{
    
    public Corridor()
    {
    }

    //2D constructor
    public Corridor(List<Coord> corridorCells, Cell[,] map)
    {
        _cells = corridorCells;
        _size = corridorCells.Count;
        _connectedRooms = new List<Room>();

        _edgeCells = new List<Coord>();
        foreach (Coord cell in corridorCells)
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

    public void AddConnectToRoom(Room room)
    {
        _connectedRooms.Add(room);
    }
    //3D constructor
    public Corridor(List<Coord> corridorCells, Cell[,,] map)
    {
        _cells = corridorCells;
        _size = corridorCells.Count;
        _connectedRooms = new List<Room>();

        _roofIndex = 0;
        _floorIndex = int.MaxValue;

        _edgeCells = new List<Coord>();
        foreach (Coord cell in corridorCells)
        {
            if (_roofIndex < cell.yCoord)
                _roofIndex = cell.yCoord;
            if (_floorIndex > cell.yCoord)
                _floorIndex = cell.yCoord;

            for (int x = cell.xCoord - 1; x <= cell.xCoord + 1; x++)
            {
                for (int y = cell.yCoord - 1; y <= cell.yCoord + 1; y++)
                {

                    for (int z = cell.zCoord - 1; z <= cell.zCoord + 1; z++)
                    {
                        if (x >= 0 && x < map.GetLength(0) && y >= 0 && y < map.GetLength(1) && z >= 0 &&
                            z < map.GetLength(2))
                        {
                            //von neumann neighbours only
                            if (x == cell.xCoord || y == cell.yCoord || z == cell.zCoord)
                            {
                                if (map[x, y, z].state == States.Wall)
                                {
                                    _edgeCells.Add(cell);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public Corridor(List<Coord> corridorCells,bool is3D=false)
    {

        _cells = corridorCells;
        _size = corridorCells.Count;
        _connectedRooms = new List<Room>();
        _edgeCells = new List<Coord>();
        if(is3D)
        {
            
            _roofIndex = 0;
            _floorIndex = int.MaxValue;

        }
    }

    public Coord Last()
    {
        return Cells.Last();
    }
    public Coord First()
    {
        return Cells.First();
    }
    public void CalcEdges(Cell[,] map)
    {
        foreach (Coord cell in _cells)
        {
            for (int x = cell.xCoord - 1; x <= cell.xCoord + 1; x++)
            {
                for (int y = cell.yCoord - 1; y <= cell.yCoord + 1; y++)
                {
                    //von neumann neighbours only
                    if (x >= 0 && x < map.GetLength(0) && y >= 0 && y < map.GetLength(1))
                    {

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
    }

    public void CalcEdges(Cell[,,] map)
    {
        foreach (Coord cell in _cells)
        {
            if (_roofIndex < cell.yCoord)
                _roofIndex = cell.yCoord;
            if (_floorIndex > cell.yCoord)
                _floorIndex = cell.yCoord;

            for (int x = cell.xCoord - 1; x <= cell.xCoord + 1; x++)
            {
                for (int y = cell.yCoord - 1; y <= cell.yCoord + 1; y++)
                {

                    for (int z = cell.zCoord - 1; z <= cell.zCoord + 1; z++)
                    {
                        if (x >= 0 && x < map.GetLength(0) && y >= 0 && y < map.GetLength(1) && z >= 0 &&
                            z < map.GetLength(2))
                        {
                            //von neumann neighbours only
                            if (x == cell.xCoord || y == cell.yCoord || z == cell.zCoord)
                            {
                                if (map[x, y, z].state == States.Wall)
                                {
                                    _edgeCells.Add(cell);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public void ConnectRooms(Room firstRoom, Room secondRoom)
    {
        ConnectedRooms.Add(firstRoom);
        ConnectedRooms.Add(secondRoom);
    }

    public void AddCells(List<Coord> cells)
    {
        if(cells!=null&&cells.Count>0)
            _cells.AddRange(cells);
    }
    public void AddCell(Coord cell)
    {
        _cells.Add(cell);
    }
    public void RemoveCell(Coord cell)
    {
        _cells.Remove(cell);
    }
}


public class Cube
{
    private List<ControlNode> corners;
    private List<Node> nodes;
    private int configuration;
    public Cube (List<ControlNode> cubeCorners)
    {
        corners = cubeCorners;
        nodes = new List<Node>();
        nodes.Add(cubeCorners[0].right);
        nodes.Add(cubeCorners[2].front);
        nodes.Add(cubeCorners[3].right);
        nodes.Add(cubeCorners[3].front);

        nodes.Add(cubeCorners[4].right);
        nodes.Add(cubeCorners[6].front);
        nodes.Add(cubeCorners[7].right);
        nodes.Add(cubeCorners[7].front);

        nodes.Add(cubeCorners[0].above);
        nodes.Add(cubeCorners[1].above);
        nodes.Add(cubeCorners[2].above);
        nodes.Add(cubeCorners[3].above);



        configuration = 0;
        for (int i = 0; i < cubeCorners.Count; i++)
        {
            if (cubeCorners[i].state== States.Wall)
            {
                configuration += 1 << i;
            }
        }
    }

    public int GetConfiguration()
    {
        return configuration;
    }
    public ControlNode GetCorner(int index)
    {
        return corners[index];
    }
    public List<ControlNode> GetCorners()
    {
        return corners;
    }
    public Node GetNode(int index)
    {
        return nodes[index];
    }
}

public class Node
{
    public Vector3 position;
    public int vertexIndex = -1;

    public Node(Vector3 pos)
    {
        position = pos;
    }
}

public class ControlNode:Node
{

   
    public States state;
    public Node above, right, front;

    public ControlNode(Vector3 pos, States stateInit,float size):base(pos)
    {
        above = new Node(pos + Vector3.forward * size / 2f);
        front = new Node(pos + Vector3.up * size / 2f);
        right = new Node(pos + Vector3.right * size / 2f);

        state = stateInit;

    }
}

public class Table
{

    public static readonly int[,] TriTable = new int[256, 16]
    {
        { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 0, 1, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 1, 8, 3, 9, 8, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 0, 8, 3, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 9, 2, 10, 0, 2, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 2, 8, 3, 2, 10, 8, 10, 9, 8, -1, -1, -1, -1, -1, -1, -1 },
        { 3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 0, 11, 2, 8, 11, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 1, 9, 0, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 1, 11, 2, 1, 9, 11, 9, 8, 11, -1, -1, -1, -1, -1, -1, -1 },
        { 3, 10, 1, 11, 10, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 0, 10, 1, 0, 8, 10, 8, 11, 10, -1, -1, -1, -1, -1, -1, -1 },
        { 3, 9, 0, 3, 11, 9, 11, 10, 9, -1, -1, -1, -1, -1, -1, -1 },
        { 9, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 4, 3, 0, 7, 3, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 0, 1, 9, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 4, 1, 9, 4, 7, 1, 7, 3, 1, -1, -1, -1, -1, -1, -1, -1 },
        { 1, 2, 10, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 3, 4, 7, 3, 0, 4, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1 },
        { 9, 2, 10, 9, 0, 2, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1 },
        { 2, 10, 9, 2, 9, 7, 2, 7, 3, 7, 9, 4, -1, -1, -1, -1 },
        { 8, 4, 7, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 11, 4, 7, 11, 2, 4, 2, 0, 4, -1, -1, -1, -1, -1, -1, -1 },
        { 9, 0, 1, 8, 4, 7, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1 },
        { 4, 7, 11, 9, 4, 11, 9, 11, 2, 9, 2, 1, -1, -1, -1, -1 },
        { 3, 10, 1, 3, 11, 10, 7, 8, 4, -1, -1, -1, -1, -1, -1, -1 },
        { 1, 11, 10, 1, 4, 11, 1, 0, 4, 7, 11, 4, -1, -1, -1, -1 },
        { 4, 7, 8, 9, 0, 11, 9, 11, 10, 11, 0, 3, -1, -1, -1, -1 },
        { 4, 7, 11, 4, 11, 9, 9, 11, 10, -1, -1, -1, -1, -1, -1, -1 },
        { 9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 9, 5, 4, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 0, 5, 4, 1, 5, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 8, 5, 4, 8, 3, 5, 3, 1, 5, -1, -1, -1, -1, -1, -1, -1 },
        { 1, 2, 10, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 3, 0, 8, 1, 2, 10, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1 },
        { 5, 2, 10, 5, 4, 2, 4, 0, 2, -1, -1, -1, -1, -1, -1, -1 },
        { 2, 10, 5, 3, 2, 5, 3, 5, 4, 3, 4, 8, -1, -1, -1, -1 },
        { 9, 5, 4, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 0, 11, 2, 0, 8, 11, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1 },
        { 0, 5, 4, 0, 1, 5, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1 },
        { 2, 1, 5, 2, 5, 8, 2, 8, 11, 4, 8, 5, -1, -1, -1, -1 },
        { 10, 3, 11, 10, 1, 3, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1 },
        { 4, 9, 5, 0, 8, 1, 8, 10, 1, 8, 11, 10, -1, -1, -1, -1 },
        { 5, 4, 0, 5, 0, 11, 5, 11, 10, 11, 0, 3, -1, -1, -1, -1 },
        { 5, 4, 8, 5, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1 },
        { 9, 7, 8, 5, 7, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 9, 3, 0, 9, 5, 3, 5, 7, 3, -1, -1, -1, -1, -1, -1, -1 },
        { 0, 7, 8, 0, 1, 7, 1, 5, 7, -1, -1, -1, -1, -1, -1, -1 },
        { 1, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 9, 7, 8, 9, 5, 7, 10, 1, 2, -1, -1, -1, -1, -1, -1, -1 },
        { 10, 1, 2, 9, 5, 0, 5, 3, 0, 5, 7, 3, -1, -1, -1, -1 },
        { 8, 0, 2, 8, 2, 5, 8, 5, 7, 10, 5, 2, -1, -1, -1, -1 },
        { 2, 10, 5, 2, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1 },
        { 7, 9, 5, 7, 8, 9, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1 },
        { 9, 5, 7, 9, 7, 2, 9, 2, 0, 2, 7, 11, -1, -1, -1, -1 },
        { 2, 3, 11, 0, 1, 8, 1, 7, 8, 1, 5, 7, -1, -1, -1, -1 },
        { 11, 2, 1, 11, 1, 7, 7, 1, 5, -1, -1, -1, -1, -1, -1, -1 },
        { 9, 5, 8, 8, 5, 7, 10, 1, 3, 10, 3, 11, -1, -1, -1, -1 },
        { 5, 7, 0, 5, 0, 9, 7, 11, 0, 1, 0, 10, 11, 10, 0, -1 },
        { 11, 10, 0, 11, 0, 3, 10, 5, 0, 8, 0, 7, 5, 7, 0, -1 },
        { 11, 10, 5, 7, 11, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 0, 8, 3, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 9, 0, 1, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 1, 8, 3, 1, 9, 8, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1 },
        { 1, 6, 5, 2, 6, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 1, 6, 5, 1, 2, 6, 3, 0, 8, -1, -1, -1, -1, -1, -1, -1 },
        { 9, 6, 5, 9, 0, 6, 0, 2, 6, -1, -1, -1, -1, -1, -1, -1 },
        { 5, 9, 8, 5, 8, 2, 5, 2, 6, 3, 2, 8, -1, -1, -1, -1 },
        { 2, 3, 11, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 11, 0, 8, 11, 2, 0, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1 },
        { 0, 1, 9, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1 },
        { 5, 10, 6, 1, 9, 2, 9, 11, 2, 9, 8, 11, -1, -1, -1, -1 },
        { 6, 3, 11, 6, 5, 3, 5, 1, 3, -1, -1, -1, -1, -1, -1, -1 },
        { 0, 8, 11, 0, 11, 5, 0, 5, 1, 5, 11, 6, -1, -1, -1, -1 },
        { 3, 11, 6, 0, 3, 6, 0, 6, 5, 0, 5, 9, -1, -1, -1, -1 },
        { 6, 5, 9, 6, 9, 11, 11, 9, 8, -1, -1, -1, -1, -1, -1, -1 },
        { 5, 10, 6, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 4, 3, 0, 4, 7, 3, 6, 5, 10, -1, -1, -1, -1, -1, -1, -1 },
        { 1, 9, 0, 5, 10, 6, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1 },
        { 10, 6, 5, 1, 9, 7, 1, 7, 3, 7, 9, 4, -1, -1, -1, -1 },
        { 6, 1, 2, 6, 5, 1, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1 },
        { 1, 2, 5, 5, 2, 6, 3, 0, 4, 3, 4, 7, -1, -1, -1, -1 },
        { 8, 4, 7, 9, 0, 5, 0, 6, 5, 0, 2, 6, -1, -1, -1, -1 },
        { 7, 3, 9, 7, 9, 4, 3, 2, 9, 5, 9, 6, 2, 6, 9, -1 },
        { 3, 11, 2, 7, 8, 4, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1 },
        { 5, 10, 6, 4, 7, 2, 4, 2, 0, 2, 7, 11, -1, -1, -1, -1 },
        { 0, 1, 9, 4, 7, 8, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1 },
        { 9, 2, 1, 9, 11, 2, 9, 4, 11, 7, 11, 4, 5, 10, 6, -1 },
        { 8, 4, 7, 3, 11, 5, 3, 5, 1, 5, 11, 6, -1, -1, -1, -1 },
        { 5, 1, 11, 5, 11, 6, 1, 0, 11, 7, 11, 4, 0, 4, 11, -1 },
        { 0, 5, 9, 0, 6, 5, 0, 3, 6, 11, 6, 3, 8, 4, 7, -1 },
        { 6, 5, 9, 6, 9, 11, 4, 7, 9, 7, 11, 9, -1, -1, -1, -1 },
        { 10, 4, 9, 6, 4, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 4, 10, 6, 4, 9, 10, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1 },
        { 10, 0, 1, 10, 6, 0, 6, 4, 0, -1, -1, -1, -1, -1, -1, -1 },
        { 8, 3, 1, 8, 1, 6, 8, 6, 4, 6, 1, 10, -1, -1, -1, -1 },
        { 1, 4, 9, 1, 2, 4, 2, 6, 4, -1, -1, -1, -1, -1, -1, -1 },
        { 3, 0, 8, 1, 2, 9, 2, 4, 9, 2, 6, 4, -1, -1, -1, -1 },
        { 0, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 8, 3, 2, 8, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1 },
        { 10, 4, 9, 10, 6, 4, 11, 2, 3, -1, -1, -1, -1, -1, -1, -1 },
        { 0, 8, 2, 2, 8, 11, 4, 9, 10, 4, 10, 6, -1, -1, -1, -1 },
        { 3, 11, 2, 0, 1, 6, 0, 6, 4, 6, 1, 10, -1, -1, -1, -1 },
        { 6, 4, 1, 6, 1, 10, 4, 8, 1, 2, 1, 11, 8, 11, 1, -1 },
        { 9, 6, 4, 9, 3, 6, 9, 1, 3, 11, 6, 3, -1, -1, -1, -1 },
        { 8, 11, 1, 8, 1, 0, 11, 6, 1, 9, 1, 4, 6, 4, 1, -1 },
        { 3, 11, 6, 3, 6, 0, 0, 6, 4, -1, -1, -1, -1, -1, -1, -1 },
        { 6, 4, 8, 11, 6, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 7, 10, 6, 7, 8, 10, 8, 9, 10, -1, -1, -1, -1, -1, -1, -1 },
        { 0, 7, 3, 0, 10, 7, 0, 9, 10, 6, 7, 10, -1, -1, -1, -1 },
        { 10, 6, 7, 1, 10, 7, 1, 7, 8, 1, 8, 0, -1, -1, -1, -1 },
        { 10, 6, 7, 10, 7, 1, 1, 7, 3, -1, -1, -1, -1, -1, -1, -1 },
        { 1, 2, 6, 1, 6, 8, 1, 8, 9, 8, 6, 7, -1, -1, -1, -1 },
        { 2, 6, 9, 2, 9, 1, 6, 7, 9, 0, 9, 3, 7, 3, 9, -1 },
        { 7, 8, 0, 7, 0, 6, 6, 0, 2, -1, -1, -1, -1, -1, -1, -1 },
        { 7, 3, 2, 6, 7, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 2, 3, 11, 10, 6, 8, 10, 8, 9, 8, 6, 7, -1, -1, -1, -1 },
        { 2, 0, 7, 2, 7, 11, 0, 9, 7, 6, 7, 10, 9, 10, 7, -1 },
        { 1, 8, 0, 1, 7, 8, 1, 10, 7, 6, 7, 10, 2, 3, 11, -1 },
        { 11, 2, 1, 11, 1, 7, 10, 6, 1, 6, 7, 1, -1, -1, -1, -1 },
        { 8, 9, 6, 8, 6, 7, 9, 1, 6, 11, 6, 3, 1, 3, 6, -1 },
        { 0, 9, 1, 11, 6, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 7, 8, 0, 7, 0, 6, 3, 11, 0, 11, 6, 0, -1, -1, -1, -1 },
        { 7, 11, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 3, 0, 8, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 0, 1, 9, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 8, 1, 9, 8, 3, 1, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1 },
        { 10, 1, 2, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 1, 2, 10, 3, 0, 8, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1 },
        { 2, 9, 0, 2, 10, 9, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1 },
        { 6, 11, 7, 2, 10, 3, 10, 8, 3, 10, 9, 8, -1, -1, -1, -1 },
        { 7, 2, 3, 6, 2, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 7, 0, 8, 7, 6, 0, 6, 2, 0, -1, -1, -1, -1, -1, -1, -1 },
        { 2, 7, 6, 2, 3, 7, 0, 1, 9, -1, -1, -1, -1, -1, -1, -1 },
        { 1, 6, 2, 1, 8, 6, 1, 9, 8, 8, 7, 6, -1, -1, -1, -1 },
        { 10, 7, 6, 10, 1, 7, 1, 3, 7, -1, -1, -1, -1, -1, -1, -1 },
        { 10, 7, 6, 1, 7, 10, 1, 8, 7, 1, 0, 8, -1, -1, -1, -1 },
        { 0, 3, 7, 0, 7, 10, 0, 10, 9, 6, 10, 7, -1, -1, -1, -1 },
        { 7, 6, 10, 7, 10, 8, 8, 10, 9, -1, -1, -1, -1, -1, -1, -1 },
        { 6, 8, 4, 11, 8, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 3, 6, 11, 3, 0, 6, 0, 4, 6, -1, -1, -1, -1, -1, -1, -1 },
        { 8, 6, 11, 8, 4, 6, 9, 0, 1, -1, -1, -1, -1, -1, -1, -1 },
        { 9, 4, 6, 9, 6, 3, 9, 3, 1, 11, 3, 6, -1, -1, -1, -1 },
        { 6, 8, 4, 6, 11, 8, 2, 10, 1, -1, -1, -1, -1, -1, -1, -1 },
        { 1, 2, 10, 3, 0, 11, 0, 6, 11, 0, 4, 6, -1, -1, -1, -1 },
        { 4, 11, 8, 4, 6, 11, 0, 2, 9, 2, 10, 9, -1, -1, -1, -1 },
        { 10, 9, 3, 10, 3, 2, 9, 4, 3, 11, 3, 6, 4, 6, 3, -1 },
        { 8, 2, 3, 8, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1 },
        { 0, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 1, 9, 0, 2, 3, 4, 2, 4, 6, 4, 3, 8, -1, -1, -1, -1 },
        { 1, 9, 4, 1, 4, 2, 2, 4, 6, -1, -1, -1, -1, -1, -1, -1 },
        { 8, 1, 3, 8, 6, 1, 8, 4, 6, 6, 10, 1, -1, -1, -1, -1 },
        { 10, 1, 0, 10, 0, 6, 6, 0, 4, -1, -1, -1, -1, -1, -1, -1 },
        { 4, 6, 3, 4, 3, 8, 6, 10, 3, 0, 3, 9, 10, 9, 3, -1 },
        { 10, 9, 4, 6, 10, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 4, 9, 5, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 0, 8, 3, 4, 9, 5, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1 },
        { 5, 0, 1, 5, 4, 0, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1 },
        { 11, 7, 6, 8, 3, 4, 3, 5, 4, 3, 1, 5, -1, -1, -1, -1 },
        { 9, 5, 4, 10, 1, 2, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1 },
        { 6, 11, 7, 1, 2, 10, 0, 8, 3, 4, 9, 5, -1, -1, -1, -1 },
        { 7, 6, 11, 5, 4, 10, 4, 2, 10, 4, 0, 2, -1, -1, -1, -1 },
        { 3, 4, 8, 3, 5, 4, 3, 2, 5, 10, 5, 2, 11, 7, 6, -1 },
        { 7, 2, 3, 7, 6, 2, 5, 4, 9, -1, -1, -1, -1, -1, -1, -1 },
        { 9, 5, 4, 0, 8, 6, 0, 6, 2, 6, 8, 7, -1, -1, -1, -1 },
        { 3, 6, 2, 3, 7, 6, 1, 5, 0, 5, 4, 0, -1, -1, -1, -1 },
        { 6, 2, 8, 6, 8, 7, 2, 1, 8, 4, 8, 5, 1, 5, 8, -1 },
        { 9, 5, 4, 10, 1, 6, 1, 7, 6, 1, 3, 7, -1, -1, -1, -1 },
        { 1, 6, 10, 1, 7, 6, 1, 0, 7, 8, 7, 0, 9, 5, 4, -1 },
        { 4, 0, 10, 4, 10, 5, 0, 3, 10, 6, 10, 7, 3, 7, 10, -1 },
        { 7, 6, 10, 7, 10, 8, 5, 4, 10, 4, 8, 10, -1, -1, -1, -1 },
        { 6, 9, 5, 6, 11, 9, 11, 8, 9, -1, -1, -1, -1, -1, -1, -1 },
        { 3, 6, 11, 0, 6, 3, 0, 5, 6, 0, 9, 5, -1, -1, -1, -1 },
        { 0, 11, 8, 0, 5, 11, 0, 1, 5, 5, 6, 11, -1, -1, -1, -1 },
        { 6, 11, 3, 6, 3, 5, 5, 3, 1, -1, -1, -1, -1, -1, -1, -1 },
        { 1, 2, 10, 9, 5, 11, 9, 11, 8, 11, 5, 6, -1, -1, -1, -1 },
        { 0, 11, 3, 0, 6, 11, 0, 9, 6, 5, 6, 9, 1, 2, 10, -1 },
        { 11, 8, 5, 11, 5, 6, 8, 0, 5, 10, 5, 2, 0, 2, 5, -1 },
        { 6, 11, 3, 6, 3, 5, 2, 10, 3, 10, 5, 3, -1, -1, -1, -1 },
        { 5, 8, 9, 5, 2, 8, 5, 6, 2, 3, 8, 2, -1, -1, -1, -1 },
        { 9, 5, 6, 9, 6, 0, 0, 6, 2, -1, -1, -1, -1, -1, -1, -1 },
        { 1, 5, 8, 1, 8, 0, 5, 6, 8, 3, 8, 2, 6, 2, 8, -1 },
        { 1, 5, 6, 2, 1, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 1, 3, 6, 1, 6, 10, 3, 8, 6, 5, 6, 9, 8, 9, 6, -1 },
        { 10, 1, 0, 10, 0, 6, 9, 5, 0, 5, 6, 0, -1, -1, -1, -1 },
        { 0, 3, 8, 5, 6, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 10, 5, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 11, 5, 10, 7, 5, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 11, 5, 10, 11, 7, 5, 8, 3, 0, -1, -1, -1, -1, -1, -1, -1 },
        { 5, 11, 7, 5, 10, 11, 1, 9, 0, -1, -1, -1, -1, -1, -1, -1 },
        { 10, 7, 5, 10, 11, 7, 9, 8, 1, 8, 3, 1, -1, -1, -1, -1 },
        { 11, 1, 2, 11, 7, 1, 7, 5, 1, -1, -1, -1, -1, -1, -1, -1 },
        { 0, 8, 3, 1, 2, 7, 1, 7, 5, 7, 2, 11, -1, -1, -1, -1 },
        { 9, 7, 5, 9, 2, 7, 9, 0, 2, 2, 11, 7, -1, -1, -1, -1 },
        { 7, 5, 2, 7, 2, 11, 5, 9, 2, 3, 2, 8, 9, 8, 2, -1 },
        { 2, 5, 10, 2, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1 },
        { 8, 2, 0, 8, 5, 2, 8, 7, 5, 10, 2, 5, -1, -1, -1, -1 },
        { 9, 0, 1, 5, 10, 3, 5, 3, 7, 3, 10, 2, -1, -1, -1, -1 },
        { 9, 8, 2, 9, 2, 1, 8, 7, 2, 10, 2, 5, 7, 5, 2, -1 },
        { 1, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 0, 8, 7, 0, 7, 1, 1, 7, 5, -1, -1, -1, -1, -1, -1, -1 },
        { 9, 0, 3, 9, 3, 5, 5, 3, 7, -1, -1, -1, -1, -1, -1, -1 },
        { 9, 8, 7, 5, 9, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 5, 8, 4, 5, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1 },
        { 5, 0, 4, 5, 11, 0, 5, 10, 11, 11, 3, 0, -1, -1, -1, -1 },
        { 0, 1, 9, 8, 4, 10, 8, 10, 11, 10, 4, 5, -1, -1, -1, -1 },
        { 10, 11, 4, 10, 4, 5, 11, 3, 4, 9, 4, 1, 3, 1, 4, -1 },
        { 2, 5, 1, 2, 8, 5, 2, 11, 8, 4, 5, 8, -1, -1, -1, -1 },
        { 0, 4, 11, 0, 11, 3, 4, 5, 11, 2, 11, 1, 5, 1, 11, -1 },
        { 0, 2, 5, 0, 5, 9, 2, 11, 5, 4, 5, 8, 11, 8, 5, -1 },
        { 9, 4, 5, 2, 11, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 2, 5, 10, 3, 5, 2, 3, 4, 5, 3, 8, 4, -1, -1, -1, -1 },
        { 5, 10, 2, 5, 2, 4, 4, 2, 0, -1, -1, -1, -1, -1, -1, -1 },
        { 3, 10, 2, 3, 5, 10, 3, 8, 5, 4, 5, 8, 0, 1, 9, -1 },
        { 5, 10, 2, 5, 2, 4, 1, 9, 2, 9, 4, 2, -1, -1, -1, -1 },
        { 8, 4, 5, 8, 5, 3, 3, 5, 1, -1, -1, -1, -1, -1, -1, -1 },
        { 0, 4, 5, 1, 0, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 8, 4, 5, 8, 5, 3, 9, 0, 5, 0, 3, 5, -1, -1, -1, -1 },
        { 9, 4, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 4, 11, 7, 4, 9, 11, 9, 10, 11, -1, -1, -1, -1, -1, -1, -1 },
        { 0, 8, 3, 4, 9, 7, 9, 11, 7, 9, 10, 11, -1, -1, -1, -1 },
        { 1, 10, 11, 1, 11, 4, 1, 4, 0, 7, 4, 11, -1, -1, -1, -1 },
        { 3, 1, 4, 3, 4, 8, 1, 10, 4, 7, 4, 11, 10, 11, 4, -1 },
        { 4, 11, 7, 9, 11, 4, 9, 2, 11, 9, 1, 2, -1, -1, -1, -1 },
        { 9, 7, 4, 9, 11, 7, 9, 1, 11, 2, 11, 1, 0, 8, 3, -1 },
        { 11, 7, 4, 11, 4, 2, 2, 4, 0, -1, -1, -1, -1, -1, -1, -1 },
        { 11, 7, 4, 11, 4, 2, 8, 3, 4, 3, 2, 4, -1, -1, -1, -1 },
        { 2, 9, 10, 2, 7, 9, 2, 3, 7, 7, 4, 9, -1, -1, -1, -1 },
        { 9, 10, 7, 9, 7, 4, 10, 2, 7, 8, 7, 0, 2, 0, 7, -1 },
        { 3, 7, 10, 3, 10, 2, 7, 4, 10, 1, 10, 0, 4, 0, 10, -1 },
        { 1, 10, 2, 8, 7, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 4, 9, 1, 4, 1, 7, 7, 1, 3, -1, -1, -1, -1, -1, -1, -1 },
        { 4, 9, 1, 4, 1, 7, 0, 8, 1, 8, 7, 1, -1, -1, -1, -1 },
        { 4, 0, 3, 7, 4, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 4, 8, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 9, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 3, 0, 9, 3, 9, 11, 11, 9, 10, -1, -1, -1, -1, -1, -1, -1 },
        { 0, 1, 10, 0, 10, 8, 8, 10, 11, -1, -1, -1, -1, -1, -1, -1 },
        { 3, 1, 10, 11, 3, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 1, 2, 11, 1, 11, 9, 9, 11, 8, -1, -1, -1, -1, -1, -1, -1 },
        { 3, 0, 9, 3, 9, 11, 1, 2, 9, 2, 11, 9, -1, -1, -1, -1 },
        { 0, 2, 11, 8, 0, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 3, 2, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 2, 3, 8, 2, 8, 10, 10, 8, 9, -1, -1, -1, -1, -1, -1, -1 },
        { 9, 10, 2, 0, 9, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 2, 3, 8, 2, 8, 10, 0, 1, 8, 1, 10, 8, -1, -1, -1, -1 },
        { 1, 10, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 1, 3, 8, 9, 1, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 0, 9, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 0, 3, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }
    };

}

public static class Extensions
{
    public static int [] SubArray(this int[,] array, int offset, int length)
    {
        int[] result = new int[length];

        for (int j = 0; j < length; j++)
        {
            result[j] = Table.TriTable[offset, j];
        }

        return result;
    }
}

public class CubeGrid
{
    public Cube[,,] cubes;

    public CubeGrid(Cell[,,] map, float size)
    {
        int nodeCountX = map.GetLength(0);
        int nodeCountY = map.GetLength(1);
        int nodeCountZ = map.GetLength(2);

        float mapWidth = nodeCountX * size;
        float mapHeight = nodeCountY * size;
        float mapDepth = nodeCountZ * size;

        ControlNode[,,] controlNodes = new ControlNode[nodeCountX, nodeCountY, nodeCountZ];

        for (int x = 0; x < nodeCountX; x++)
        {
            for (int y = 0; y < nodeCountY; y++)
            {
                for (int z = 0; z < nodeCountZ; z++)
                {
                    Vector3 pos = new Vector3((-mapWidth / 2.0f )+( x * size) + (size/2.0f), (-mapHeight / 2.0f) + (y * size) - (size / 2.0f),
                        (-mapDepth / 2.0f) + (z * size) - (size / 2.0f));

                    controlNodes[x, y, z] = new ControlNode(pos, map[x, y, z].state, size);
                }
            }
        }

        cubes = new Cube[nodeCountX - 1, nodeCountY - 1, nodeCountZ - 1];

        for (int x = 0; x < nodeCountX - 1; x++)
        {
            for (int y = 0; y < nodeCountY - 1; y++)
            {
                for (int z = 0; z < nodeCountZ - 1; z++)
                {
                    List<ControlNode> temp = new List<ControlNode>(8)
                        {
                            controlNodes[x, y + 1, z],
                            controlNodes[x + 1, y + 1, z],
                            controlNodes[x + 1, y, z],
                            controlNodes[x, y, z],
                            controlNodes[x, y + 1, z + 1],
                            controlNodes[x + 1, y + 1, z + 1],
                            controlNodes[x + 1, y, z + 1],
                            controlNodes[x, y, z + 1]
                        };
                    cubes[x, y, z] = new Cube(temp);
                }
            }
        }
    }
}