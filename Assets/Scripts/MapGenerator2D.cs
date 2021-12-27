using UnityEngine;
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
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

       // if (_useRandomSeed)
          //  _seed = System.DateTime.Now.ToString();

        //_randomNumberGenerator = new System.Random(_seed.GetHashCode());

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
                    //_map[x, y].state = (_randomNumberGenerator.Next(0, 100) / 100.0f < _randomFillPercent) ? States.Wall : States.Empty;
                    _map[x, y].state = (Random.Range(0.0f, 1.0f) < _randomFillPercent) ? States.Wall : States.Empty;
               
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
            survivingRooms[0].IsAccessableFormMainRoom = true;
        }

        ConnectClosestRooms(survivingRooms);
    }

    //Connecting the rooms

    protected override void ConnectClosestRooms(List<Room> roomsToConnect)
    {
        List<Room> roomsConnectedToMain = new List<Room>();
        List<Room> roomsNotConnectedToMain = new List<Room>();

        if (_forceAccessibilityFromMainRoom)
        {
            foreach (Room room in roomsToConnect)
            {
                if (room.IsAccessableFormMainRoom)
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
                CreateCorridor(bestFirstRoom, bestSecondRoom, bestFirstCell, bestSecondCell);
                shortestDistance = float.MaxValue;
            }
        }
        if (shortestDistance < float.MaxValue - 1.0f && _forceAccessibilityFromMainRoom)
        {
            CreateCorridor(bestFirstRoom, bestSecondRoom, bestFirstCell, bestSecondCell);
            shortestDistance = float.MaxValue;
            if (roomsNotConnectedToMain.Count>0)
             ConnectClosestRooms(roomsToConnect);
        }
    }

    protected override void CreateCorridor(Room firstRoom, Room secondRoom, Coord firstRoomCell, Coord secondRoomCell)
    {
        Room.ConnectRooms(firstRoom, secondRoom);
        Vector3 posFirstCell = new Vector3(-_width / 2 + firstRoomCell.xCoord + 0.5f, -_height / 2 + firstRoomCell.yCoord + 0.5f, 0);
        Vector3 posSecondCell = new Vector3(-_width / 2 + secondRoomCell.xCoord + 0.5f, -_height / 2 + secondRoomCell.yCoord + 0.5f, 0);
        Debug.DrawLine(posFirstCell, posSecondCell, Color.red, 5);
        Debug.Log("ConnectionFound");

        List<Coord> line = GetLine(firstRoomCell, secondRoomCell);

        foreach (Coord cell in line)
        {
            DrawCircle(cell);
        }
    }

    void DrawCircle(Coord cell)
    {
        int radius = Random.Range(_corridorRadius.x, _corridorRadius.y);
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
                        _map[xToChange, yToChange].state = States.Empty;
                    }
                }
            }
        }
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
