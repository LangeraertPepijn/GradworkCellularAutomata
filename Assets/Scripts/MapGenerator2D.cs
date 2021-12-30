using UnityEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using JetBrains.Annotations;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;


public class MapGenerator2D :  MapGenerator
{

    [CanBeNull] private Cell[,] _map;
    private States[,] _stateBuffer;
    


    //2D Code
    /// <summary>
    /// check if the cell lies on the map or not
    /// </summary>
    /// <param name="x">x coord of cell</param>
    /// <param name="y">y coord of cell</param>
    /// <returns></returns>
    private bool IsInMap(int x, int y)
    {
        return x >= 0 && x < _width && y >= 0 && y < _height;
    }

    // Generate the Cellular Automata Map
    protected override void GenerateMap()
    {
        if (_map != null)
            ClearMap();
        _map = new Cell[_width, _height];
        _stateBuffer = new States[_width, _height];


        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                _map[x,y].color=Color.white;
            }
        }
         if (_useRandomSeed) 
             _seed = System.DateTime.Now.ToString();

        if (_randomNumberGenerator == null)
            _randomNumberGenerator = new System.Random(_seed.GetHashCode());

        RandomFillMap();
        IterateStates();
        ExamineMap();
        UpdateCubes();
    }

    // remove old cubes
    protected override void ClearMap()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {

                    if (_map[x, y].mesh)
                        Destroy(_map[x, y].mesh);
                
            }
        }
    }

    // Fill the map with random values
    protected override void RandomFillMap()
    {
     
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (_MakeEdgesWalls && (x == 0 || x == _width - 1 || y == 0 || y == _height - 1))
                { 
                    _map[x, y].state = States.Wall;
                
                }
                else
                {
                    _map[x, y].state = (_randomNumberGenerator.Next(0, 100) / 100.0f < _randomFillPercent) ? States.Wall : States.Empty;
                    //_map[x, y].state = (Random.Range(0.0f, 1.0f) < _randomFillPercent) ? States.Wall : States.Empty;
               
                }

            }
        }
    }

    //check if the surrounding cells are walls
    private int GetSurroundingWallCount(int indexX, int indexY)
    {
        int wallcount = 0;

        for (int neighbourX = indexX-1; neighbourX <= indexX+1; neighbourX++)
        {
            for (int neighbourY = indexY - 1; neighbourY <= indexY + 1; neighbourY++)
            {
                if (IsInMap(neighbourX, neighbourY))
                {
                    //check if not cell itself
                    if (indexX != neighbourX || indexY != neighbourY)
                    {
                        wallcount += (int)_map[neighbourX, neighbourY].state;
                    }
                }
                else if(_MakeEdgesWalls)
                {
                    wallcount++;
                }
                else
                {
                    wallcount += (Random.Range(0.0f,1.0f) < _randomFillPercent) ? 1 : 0;
                }
            }
        }

        return wallcount;
    }

    protected override void IterateStatesOnce()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                int neighbourWallTiles = GetSurroundingWallCount(x, y);
                if (neighbourWallTiles > _neighbourWallCountToChange)
                {
                    //_map[x, y].state = States.Wall;
                    _stateBuffer[x, y] = States.Wall;
                }
                else if (8 - neighbourWallTiles > _neighbourEmptyCountToChange)
                {
                    //_map[x, y].state = States.Empty;
                    _stateBuffer[x, y] = States.Empty;

                }

            }
        }

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                _map[x, y].state = _stateBuffer[x, y];
            }
        }
    }

    // iterate using rules over the map to change it
    protected override void IterateStates()
    {
        for (int i = 0; i < _iterations; i++)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    int neighbourWallTiles = GetSurroundingWallCount(x, y);
                    if (neighbourWallTiles > _neighbourWallCountToChange)
                    {
                        //_map[x, y].state = States.Wall;
                        _stateBuffer[x, y] = States.Wall;
                    }
                    else if (8 - neighbourWallTiles > _neighbourEmptyCountToChange)
                    {
                        //_map[x, y].state = States.Empty;
                        _stateBuffer[x, y] = States.Empty;

                    }

                }
            }

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _map[x, y].state = _stateBuffer[x, y];
                }
            }
        }
    }

    //flood fill check to see which tiles are part of what region
    private List<Coord> GetRegionTiles(int startX, int startY)
    {
        List<Coord> cells = new List<Coord>();
        int[,] mapFlags = new int[_width, _height];
        States cellState = _map[startX, startY].state;

        Queue<Coord> queue = new Queue<Coord>();

        queue.Enqueue(new Coord(startX,startY));
        mapFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            Coord cell = queue.Dequeue();
            cells.Add(cell);

            for (int x = cell.xCoord - 1; x <= cell.xCoord + 1; x++)
            {
                for (int y = cell.yCoord - 1; y <= cell.yCoord + 1; y++)
                {
                    if (IsInMap(x, y)&&(y==cell.yCoord||x==cell.xCoord))
                    {
                        if (mapFlags[x, y] == 0 && _map[x, y].state == cellState)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coord(x,y));
                        }
                    }
                }
            }
        }

        return cells;
    }

    /// <summary>
    /// Returns all regions with the given state
    /// </summary>
    /// <param name="state"> state of the regions to return</param>
    /// <returns></returns>
    protected override List<List<Coord>> GetRegionsOfState(States state)
    {
        List<List<Coord>> regions = new List<List<Coord>>();

        int[,] mapFlags = new int[_width, _height];

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (mapFlags[x, y] == 0 && _map[x, y].state == state)
                {
                    List<Coord> region = GetRegionTiles(x, y);
                    regions.Add(region);

                    foreach (Coord cell in region)
                    {
                        mapFlags[cell.xCoord, cell.yCoord] = 1;
                    }
                }
            }
        }

        return regions;
    }

    // if the room is too small remove it
    protected override void ExamineMap()
    {
        List<List<Coord>> regions = GetRegionsOfState(States.Empty);
        List<Room> survivingRooms = new List<Room>();

        foreach (List<Coord> region in regions)
        {
            if (region.Count < _sizeThreshold)
            {
                foreach (Coord cell in region)
                {
                    _map[cell.xCoord, cell.yCoord].state = States.Wall;
                }
            }
            else
            {
                survivingRooms.Add(new Room(region, _map));
            }
        }
        //sort form big to small
        if (survivingRooms.Count > 0)
        {
            survivingRooms.Sort();
            survivingRooms[0].IsMainRoom = true;
            survivingRooms[0].IsAccessibleFormMainRoom = true;
        }

        List<Corridor> corridors = new List<Corridor>();
        if(_connectClosestRooms)
            corridors=ConnectClosestRooms(survivingRooms);
        if (_useDigger)
            DigCorridors(survivingRooms,corridors);
    }

    //Connecting the rooms

    protected override List<Corridor> ConnectClosestRooms(List<Room> roomsToConnect)
    {
        List<Room> roomsConnectedToMain = new List<Room>();
        List<Room> roomsNotConnectedToMain = new List<Room>();
        List<Corridor> corridors = new List<Corridor>();

        if (_forceAccessibilityFromMainRoom)
        {
            foreach (Room room in roomsToConnect)
            {
                if (room.IsAccessibleFormMainRoom)
                {
                    roomsConnectedToMain.Add(room);
                }
                else
                {
                    roomsNotConnectedToMain.Add(room);
                }
            }
        }
        else
        {
            roomsConnectedToMain = roomsToConnect;
            roomsNotConnectedToMain = roomsToConnect;
        }
        float shortestDistance = float.MaxValue;
        Coord bestFirstCell = new Coord();
        Coord bestSecondCell = new Coord();
        Room bestFirstRoom = new Room();
        Room bestSecondRoom = new Room();

        foreach (Room firstRoom in roomsConnectedToMain)
        {
            //if you dont need acces to the largest room and the room is already connected skip that room
            if(!_forceAccessibilityFromMainRoom&&firstRoom.ConnectedRooms.Count>0)
                continue;

            foreach (Room secondRoom in roomsNotConnectedToMain)
            {
                //if the room is itself skip it or if the rooms are already connected skip 
                if (firstRoom == secondRoom|| firstRoom.IsConnected(secondRoom))
                    continue;


                for (int indexFirstRoom = 0; indexFirstRoom < firstRoom.EdgeCells.Count; indexFirstRoom++)
                {
                    for (int indexSecondRoom = 0; indexSecondRoom < secondRoom.EdgeCells.Count; indexSecondRoom++)
                    {
                        float distance = Vector2Int.Distance(firstRoom.EdgeCells[indexFirstRoom], secondRoom.EdgeCells[indexSecondRoom]);

                        if (distance < shortestDistance)
                        {

                            shortestDistance = distance;
                            bestFirstCell = firstRoom.EdgeCells[indexFirstRoom];
                            bestSecondCell = secondRoom.EdgeCells[indexSecondRoom];
                            bestFirstRoom = firstRoom;
                            bestSecondRoom = secondRoom;
                        }
                    }
                }
            }
            if (shortestDistance < float.MaxValue - 1.0f&&!_forceAccessibilityFromMainRoom)
            {
                corridors.Add(CreateCorridor(bestFirstRoom, bestSecondRoom, bestFirstCell, bestSecondCell));
                shortestDistance = float.MaxValue;
            }
        }
        if (shortestDistance < float.MaxValue - 1.0f && _forceAccessibilityFromMainRoom)
        {
            corridors.Add(CreateCorridor(bestFirstRoom, bestSecondRoom, bestFirstCell, bestSecondCell));
            shortestDistance = float.MaxValue;
            if (roomsNotConnectedToMain.Count>0)
                corridors.AddRange(ConnectClosestRooms(roomsToConnect));
        }

        return corridors;
    }

    protected override Corridor CreateCorridor(Room firstRoom, Room secondRoom, Coord firstRoomCell,
    Coord secondRoomCell)
    {
        Room.ConnectRooms(firstRoom, secondRoom);
        Vector3 posFirstCell = new Vector3(-_width / 2 + firstRoomCell.xCoord + 0.5f,
            -_height / 2 + firstRoomCell.yCoord + 0.5f, 0);
        Vector3 posSecondCell = new Vector3(-_width / 2 + secondRoomCell.xCoord + 0.5f,
            -_height / 2 + secondRoomCell.yCoord + 0.5f, 0);
        Debug.DrawLine(posFirstCell, posSecondCell, Color.red, 5);
        Debug.Log("ConnectionFound");

        List<Coord> line = GetLine(firstRoomCell, secondRoomCell);
        Corridor corridor = new Corridor();

        foreach (Coord cell in line)
        {
            corridor.AddCells(DrawCircle(cell));
        }

        corridor.ConnectedRooms = new List<Room> { firstRoom, secondRoom };
        corridor.AddCells( line);
        corridor.CalcEdges(_map);

        return corridor;
    }

     private  List<Coord> DrawCircle(Coord cell)
    {
        int radius = Random.Range(_corridorRadius.x, _corridorRadius.y);

        List<Coord> cells = new List<Coord>();
        for (int x = -radius; x < radius; x++)
        {
            for (int y = -radius; y < radius; y++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    int xToChange = cell.xCoord + x;
                    int yToChange = cell.yCoord + y;

                    if (IsInMap(xToChange, yToChange))
                    {

                        cells.Add(new Coord(xToChange,yToChange));
                        _map[xToChange, yToChange].state = States.Empty;
                    }
                }
            }
        }

        return cells;
    }

    List<Coord> GetLine(Coord from, Coord to)
    {
        List<Coord> line = new List<Coord>();

        int x = from.xCoord;
        int y = from.yCoord;

        int dx = to.xCoord - from.xCoord;
        int dy = to.yCoord - from.yCoord;


        bool inverted = false;

        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);

        int longest = Math.Abs(dx);
        int shortest = Math.Abs(dy);

        if (longest < shortest)
        {
            inverted = true;
            (shortest, longest) = (longest, shortest);

            (step, gradientStep) = (gradientStep, step);
    
        }

        int gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new Coord(x,y));
            if (inverted)
                y += step;
            else
            {
                x += step;
            }

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                }
                else
                {
                    y += gradientStep;
                    
                }
                gradientAccumulation -= longest;
            }

        }

        return line;

    }

    protected override void UpdateCubes()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {

                if (_map[x, y].mesh)
                {
                    MeshRenderer renderer = _map[x, y].mesh.GetComponent<MeshRenderer>();
                    if (renderer)
                        renderer.material.color =
                            (_map[x, y].state == States.Wall) ? Color.black : Color.white;
                    if ((!_showWalls && _map[x, y].state == States.Wall) ||
                        (!_showEmpty && _map[x, y].state == States.Empty) || (_shellEmpty && _map[x, y].neighbourCount == 8) || (!_showShell &&
                                                                              (x == 0 || x == _width - 1 || y == 0 ||
                                                                               y == _height - 1)))
                        _map[x, y].mesh.gameObject.SetActive(false);
                    else
                        _map[x, y].mesh.gameObject.SetActive(true);
                }
                else
                {
                    Vector3 pos = new Vector3(-_width / 2 + x + 0.5f, -_height / 2 + y + 0.5f,
                        0);
                    _map[x, y].mesh = Instantiate(_cube);
                    _map[x, y].mesh.transform.position = pos;

                    MeshRenderer renderer = _map[x, y].mesh.GetComponent<MeshRenderer>();
                    if (renderer)
                    {
                        if(_colorRegions)
                            renderer.material.color =
                            (_map[x, y].state == States.Wall) ? Color.black : _map[x, y].color ;
                        else
                            renderer.material.color =
                            (_map[x, y].state == States.Wall) ? Color.black : Color.white;
                    }

                    if ((!_showWalls && _map[x, y].state == States.Wall) ||
                        (!_showEmpty && _map[x, y].state == States.Empty) || (_shellEmpty && _map[x, y].neighbourCount == 8) || (!_showShell &&
                                                                              (x == 0 || x == _width - 1 || y == 0 ||
                                                                               y == _height - 1)))
                        _map[x, y].mesh.gameObject.SetActive(false);
                    else
                        _map[x, y].mesh.gameObject.SetActive(true);
                }

            }
        }

    }

    protected override List<Corridor> DigCorridors(List<Room> rooms, List<Corridor> corridors)
    {

        Area currentRoom = new Room();
        Vector2Int direction = GetNewDirection(new Vector2Int());
        Coord cell = new Coord(-10,-10);
        List<Corridor> newCorridors = new List<Corridor>();
        int breakOutCounter = 0;
        while (rooms.Count>0)
        {


            if (corridors.Count > 0)
            {
                currentRoom = rooms[_randomNumberGenerator.Next(0, rooms.Count)];
                GetDigPos(currentRoom, ref cell, ref direction);
            }
            else
            {
                if (corridors.Count>0 && _randomNumberGenerator.Next(0, 100) / 100.0f > _corridorFromRoomChance)
                {

                    currentRoom = corridors[_randomNumberGenerator.Next(0, corridors.Count-1)];
                    GetDigPos(currentRoom, ref cell, ref direction);

                }
                else
                {
                    currentRoom = rooms[_randomNumberGenerator.Next(0, rooms.Count-1)];
                    GetDigPos(currentRoom, ref cell, ref direction);

                }
            }

            Corridor potentialCorridor = GeneratePotentialCorridor(cell, direction);
            if(potentialCorridor!=null)
                potentialCorridor.CalcEdges(_map);

            if (potentialCorridor != null)
            {
                Room roomToRemove = null;
                foreach (Room room in rooms)
                {

                    if (room.Cells.Contains(potentialCorridor.Last()))
                    {
                        if (currentRoom != room)
                        {
                            _map[potentialCorridor.Last().xCoord, potentialCorridor.Last().yCoord].state = States.Empty;
                            _map[potentialCorridor.Last().xCoord, potentialCorridor.Last().yCoord].color = Color.magenta;
                            potentialCorridor.RemoveCell(potentialCorridor.Last());

                            if (currentRoom is Corridor corridor)
                            {
                                foreach (Room connectedRoom in corridor.ConnectedRooms)
                                {
                                    Room.ConnectRooms(connectedRoom, room);
                                }

                                corridor.AddConnectToRoom(room);
                            }
                            else
                            {
                                Room.ConnectRooms((Room)currentRoom, room);
                            }

                            newCorridors.Add(potentialCorridor);

                            foreach (Coord edgeCell in potentialCorridor.Cells)
                            {
                                if (_map != null)
                                {
   
                                    _map[edgeCell.xCoord, edgeCell.yCoord].state = States.Empty;
                                    _map[edgeCell.xCoord, edgeCell.yCoord].color = Color.blue;
                                    
                                }
                            }

                            roomToRemove = room;

                        }

                    }
                }

                rooms.Remove(roomToRemove);
            }

            ++breakOutCounter;
      
            if (breakOutCounter > _corridorCounter)
                return newCorridors;
        }

        return newCorridors;
    }

    private Corridor GeneratePotentialCorridor(Coord start, Vector2Int dir)
    {

        Corridor potentialCorridor = new Corridor();
        potentialCorridor.AddCell(start);
        Coord currentCell = start;

        int length = 0;

        int turns = 0;

        while (turns <= _turnCount)
        {
            ++turns;

            length = _randomNumberGenerator.Next(_corridorLengthMinMax.x, _corridorLengthMinMax.y);

            while (length > 0)
            {
                --length;

                currentCell = currentCell + dir;
                if (!IsInMap(currentCell.xCoord, currentCell.yCoord))
                    return null;

                if(_map != null && _map[currentCell.xCoord,currentCell.yCoord].state==States.Empty)
                {
                    potentialCorridor.AddCell(currentCell);
                    return potentialCorridor;
                }

                if (!CorridorSpacingCheck(currentCell, dir))
                    return null;

                potentialCorridor.AddCell(currentCell);

            }

            dir=GetNewDirection(dir);
        }

        return null;
    }

    private Vector2Int GetNewDirection(Vector2Int oldDir)
    {
        Vector2Int otherDirection = oldDir;
        do
        {

            switch (_randomNumberGenerator.Next(0, 4))
            {
                case 0:
                    otherDirection = Vector2Int.up;
                    break;
                case 1:
                    otherDirection = Vector2Int.right;
                    break;
                case 2:
                    otherDirection = Vector2Int.down;
                    break;
                case 3:
                    otherDirection = Vector2Int.left;
                    break;

            }

            if (_createDeadEnds)
                return otherDirection;


        } while (otherDirection.x == -oldDir.x && otherDirection.y == -oldDir.y);

        return otherDirection;
    }

    private bool CorridorSpacingCheck(Coord cell, Vector2Int direction)
    {


        foreach (int r in Enumerable.Range(-_corridorSpacing,  _corridorSpacing ).ToList())
        {
            if (direction.x == 0)//north or south
            {
                if (IsInMap(cell.xCoord + r, cell.yCoord))
                    if (_map[cell.xCoord + r, cell.yCoord].state != States.Wall)
                        return false;
            }
            else if (direction.y == 0)//east west
            {
                if (IsInMap(cell.xCoord, cell.yCoord + r))
                    if (_map[cell.xCoord, cell.yCoord + r].state != States.Wall)
                        return false;
            }

        }

        return true;
    }

    private void GetDigPos(Area room, ref Coord cell, ref Vector2Int dir)
    {
        Coord edgeCell = room.EdgeCells[_randomNumberGenerator.Next(0, room.EdgeCells.Count)];

        for (int x = edgeCell.xCoord - 1; x < edgeCell.xCoord + 1; x++)
        {
            for (int y = edgeCell.yCoord - 1; y < edgeCell.yCoord + 1; y++)
            {
                //von neumann neighbourhood
                if (IsInMap(x, y) && (x == edgeCell.xCoord || y == edgeCell.yCoord) &&
                    _map[x, y].state == States.Wall)
                {

                    cell = edgeCell;
                    dir = new Vector2Int(x, y) - new Vector2Int(edgeCell.xCoord, edgeCell.yCoord);
                    return;
                }
            }
        }

    }

    //void OnDrawGizmos()
    //{
    //    if (_map3D != null)
    //    {
    //        for (int x = 0; x < _width; x++)
    //        {
    //            for (int y = 0; y < _height; y++)
    //            {
    //                for (int z = 0; z < _depth; z++)
    //                {
    //                    if (_showSlice&&y==_sliceIndex)
    //                    {
    //                        if(_showWalls)
    //                        Gizmos.color = (_map3D[x, y, z].state == States.Empty)
    //                            ? new Color(0, 1, 1, 0.7f)
    //                            : new Color(1, 0, 0, 0.1f);
    //                        else
    //                        {
    //                            Gizmos.color = (_map3D[x, y, z].state == States.Empty)
    //                                ? new Color(1, 1, 1, 0.7f)
    //                                : new Color(1, 0, 0, 0.0f);
    //                        }
    //                        Vector3 pos = new Vector3(-_width / 2 + x + 0.5f, -_height / 2 + y + 0.5f,
    //                            -_depth / 2 + z + 0.5f);
    //                        Gizmos.DrawCube(pos, Vector3.one);
    //                    }
    //                    else if(!_showSlice)
    //                    {
    //                        if (_showWalls)
    //                            Gizmos.color = (_map3D[x, y, z].state == States.Empty)
    //                                ? new Color(0, 1, 1, 0.7f)
    //                                : new Color(1, 0, 0, 0.1f);
    //                        else
    //                        {
    //                            Gizmos.color = (_map3D[x, y, z].state == States.Empty)
    //                                ? new Color(1, 1, 1, 0.7f)
    //                                : new Color(1, 0, 0, 0.0f);
    //                        }
    //                        //Gizmos.color = (_map[x, y] == States.Empty)
    //                        //    ? new Color(1, 1, 1, 0.1f)
    //                        //    : new Color(0, 0, 0, 0.2f);
    //                        //Vector3 pos = new Vector3(-_width / 2 + x + 0.5f,0, -_height / 2 + y + 0.5f);
    //                        //Gizmos.color = (_map3D[x, y, z] == States.Empty)
    //                        //    ? new Color(0, 1, 1, 0.3f)
    //                        //    : new Color(1, 0, 0, 0.1f);
    //                        Vector3 pos = new Vector3(-_width / 2 + x + 0.5f, -_height / 2 + y + 0.5f,
    //                            -_depth / 2 + z + 0.5f);
    //                        Gizmos.DrawCube(pos, Vector3.one);
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}
}
