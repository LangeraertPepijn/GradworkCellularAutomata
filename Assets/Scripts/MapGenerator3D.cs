using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class MapGenerator3D : MapGenerator
{

    [SerializeField] [Range(1, 50)] private int _depth = 50;

    [CanBeNull] private Cell[,,] _map3D;
    private States[,,] _stateBuffer3D;

    //3D code

    /// <summary>
    /// check if the cell lies on the map or not 3D
    /// </summary>
    /// <param name="x">x coord of cell</param>
    /// <param name="y">y coord of cell</param>
    /// <param name="z">z coord of cell</param>
    /// <returns></returns>
    private bool IsInMap3D(int x, int y, int z)
    {
        return x >= 0 && x < _width && y >= 0 && y < _height && z >= 0 && z < _depth;
    }

    // Generate the Cellular Automata Map
    protected override void GenerateMap()
    {

        if (_map3D != null)
            ClearMap();
        _map3D = new Cell[_width, _height, _depth];
        _stateBuffer3D = new States[_width, _height, _depth];


        // if (_useRandomSeed)
        //  _randomSeed = System.DateTime.Now.ToString();

        // _randomNumberGenerator = new System.Random(_randomSeed.GetHashCode());



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
                for (int z = 0; z < _depth; z++)
                {
                    if (_map3D[x, y, z].mesh)
                        Destroy(_map3D[x, y, z].mesh);
                }
            }
        }
    }


    //check if the surrounding cells are walls
    private int GetSurroundingWallCount(int indexX, int indexY, int indexZ)
    {
        int wallcount = 0;

        for (int neighbourX = indexX - 1; neighbourX <= indexX + 1; neighbourX++)
        {
            for (int neighbourY = indexY - 1; neighbourY <= indexY + 1; neighbourY++)
            {
                for (int neighbourZ = indexZ - 1; neighbourZ <= indexZ + 1; neighbourZ++)
                {
                    if (IsInMap3D(neighbourX, neighbourY, neighbourZ))
                    {
                        //check if not cell itself
                        if (neighbourX != indexX || neighbourY != indexY || neighbourZ != indexZ)
                        {
                            wallcount += (int)_map3D[neighbourX, neighbourY, neighbourZ].state;
                        }
                    }
                    else if (_MakeEdgesWalls)
                    {
                        wallcount++;
                    }
                    else
                    {
                        wallcount += (Random.Range(0.0f,1.0f) < _randomFillPercent) ? 1 : 0;
                    }
                }
            }
        }

        return wallcount;
    }


    // Fill the map with random values based on the RandomFill Percent
    protected override void RandomFillMap()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                for (int z = 0; z < _depth; z++)
                {
                    if (_MakeEdgesWalls && (x == 0 || x == _width - 1 || y == 0 || y == _height - 1 || z == 0 ||
                                            z == _depth - 1))
                    {
                        _map3D[x, y, z].state = States.Wall;
                    }
                    else
                    {
                        _map3D[x, y, z].state = (Random.Range(0, 1.0f) < _randomFillPercent)
                            ? States.Wall
                            : States.Empty;
                        //_map3D[x, y, z].state = (_randomNumberGenerator.Next(0, 100) / 100.0f < _randomFillPercent)
                        //? States.Wall
                        //: States.Empty;

                    }
                }

            }
        }
    }


    // iterate using rules over the map to change it once to see what changes
    protected override void IterateStatesOnce()
    {
        float w = 0;
        float e = 0;
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                for (int z = 0; z < _depth; z++)
                {

                    _map3D[x, y, z].neighbourCount = GetSurroundingWallCount(x, y, z);
                    if (_map3D[x, y, z].neighbourCount > _neighbourWallCountToChange)
                    {
                        //_map3D[x, y, z].state = States.Wall;

                        _stateBuffer3D[x, y, z] = States.Wall;
                        w++;
                    }
                    else if (26 - _map3D[x, y, z].neighbourCount > _neighbourEmptyCountToChange)
                    {
                        //_map3D[x, y, z].state = States.Empty;

                        _stateBuffer3D[x, y, z] = States.Empty;
                        e++;

                    }

                    //if (Random.Range(0.0f, 1.0f) < _changeChance)
                    //{
                    //    _stateBuffer3D[x,y,z] = (States)(((int)_map3D[x,y,z].state+1)%2);
                    //}
                }
            }
        }
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                for (int z = 0; z < _depth; z++)
                {
                    _map3D[x, y, z].state = _stateBuffer3D[x, y, z];
                }
            }
        }
        Debug.Log("The amount of walls changed is" + w + "the amount of empty added is" + e);

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
                    for (int z = 0; z < _depth; z++)
                    {

                        _map3D[x, y, z].neighbourCount = GetSurroundingWallCount(x, y, z);
                        if (_map3D[x, y, z].neighbourCount > _neighbourWallCountToChange)
                        {
                            // _map3D[x, y,z].state = States.Wall;
                            _stateBuffer3D[x, y, z] = States.Wall;
                        }
                        else if (26 - _map3D[x, y, z].neighbourCount > _neighbourEmptyCountToChange)
                        {
                            //_map3D[x, y,z].state = States.Empty;
                            _stateBuffer3D[x, y, z] = States.Empty;

                        }
                    }
                }
            }
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    for (int z = 0; z < _depth; z++)
                    {
                        _map3D[x, y, z].state = _stateBuffer3D[x, y, z];
                    }
                }
            }
            Debug.Log("Iteration Done");
        }
    }

    //flood fill check to see which tiles are part of what region
    private List<Coord> GetRegionTiles(int startX, int startY, int startZ)
    {
        List<Coord> cells = new List<Coord>();
        int[,,] mapFlags = new int[_width, _height, _depth];
        States cellState = _map3D[startX, startY, startZ].state;

        Queue<Coord> queue = new Queue<Coord>();

        queue.Enqueue(new Coord(startX, startY,startZ));
        mapFlags[startX, startY, startZ] = 1;

        while (queue.Count > 0)
        {
            Coord cell = queue.Dequeue();
            cells.Add(cell);

            for (int x = cell.xCoord - 1; x <= cell.xCoord + 1; x++)
            {
                for (int y = cell.yCoord - 1; y <= cell.yCoord + 1; y++)
                {
                    for (int z = cell.zCoord - 1; z <= cell.zCoord + 1; z++)
                    {
                        if (IsInMap3D(x, y, z) && (y == cell.yCoord || x == cell.xCoord || z == cell.zCoord))
                        {
                            if (mapFlags[x, y, z] == 0 && _map3D[x, y, z].state == cellState)
                            {
                                mapFlags[x, y, z] = 1;
                                queue.Enqueue(new Coord(x, y,z));
                            }
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

        int[,,] mapFlags = new int[_width, _height, _depth];

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                for (int z = 0; z < _depth; z++)
                {
                    if (mapFlags[x, y, z] == 0 && _map3D[x, y, z].state == state)
                    {
                        List<Coord> region = GetRegionTiles(x, y, z);
                        regions.Add(region);

                        foreach (Coord cell in region)
                        {
                            mapFlags[cell.xCoord, cell.yCoord, cell.zCoord] = 1;
                        }
                    }
                }
            }
        }

        return regions;
    }

    void ColourRegions()
    {
        var regions = GetRegionsOfState(States.Empty);
        int i=0;
        foreach (var region in regions)
        {
            foreach (var cell in region)
            {
                _map3D[cell.xCoord, cell.yCoord, cell.zCoord].color = new Color(i*1.0f/regions.Count, i * 1.0f / regions.Count, i * 1.0f / regions.Count*2, 0.7f);
            }
            i++;
        }

       
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
                    _map3D[cell.xCoord, cell.yCoord, cell.zCoord].state = States.Wall;
                }
            }
            else
            {
                survivingRooms.Add(new Room(region,_map3D));
            }
        }
        ColourRegions();
        ConnectClosestRooms(survivingRooms);
        
    }

    protected override void ConnectClosestRooms(List<Room> roomsToConnect)
    {
        float shortestDistance = float.MaxValue;
        Coord bestFirstCell = new Coord();
        Coord bestSecondCell = new Coord();
        Room bestFirstRoom = new Room();
        Room bestSecondRoom = new Room();

        foreach (Room firstRoom in roomsToConnect)
        {
            foreach (Room secondRoom in roomsToConnect)
            {
                if (firstRoom == secondRoom)
                    continue;
                if (firstRoom.IsConnected(secondRoom))
                    break;
                //3 is a var
                //if (firstRoom.FloorIndex >= secondRoom.RoofIndex+3 || firstRoom.RoofIndex+3 <= secondRoom.FloorIndex)
                //    continue;
                for (int indexFirstRoom = 0; indexFirstRoom < firstRoom.EdgeCells.Count; indexFirstRoom++)
                {
                    if (firstRoom.FloorIndex == firstRoom.EdgeCells[indexFirstRoom].yCoord || firstRoom.RoofIndex == firstRoom.EdgeCells[indexFirstRoom].yCoord)
                        continue;
                    //else if (firstRoom.FloorIndex + 0.3f * (firstRoom.RoofIndex - firstRoom.FloorIndex) < firstRoom.EdgeCells[indexFirstRoom].yCoord)
                    //    continue;
                    for (int indexSecondRoom = 0; indexSecondRoom < secondRoom.EdgeCells.Count; indexSecondRoom++)
                    {

                        if (secondRoom.FloorIndex == secondRoom.EdgeCells[indexSecondRoom].yCoord || secondRoom.RoofIndex == secondRoom.EdgeCells[indexSecondRoom].yCoord)
                            continue;
                        //else if ((Vector3Int)secondRoom.EdgeCells[indexSecondRoom] -
                        //         (Vector3Int)firstRoom.EdgeCells[indexFirstRoom]))


                        Vector3 dir = (Vector3Int)secondRoom.EdgeCells[indexSecondRoom] -
                                          (Vector3Int)firstRoom.EdgeCells[indexFirstRoom];
                        Vector3 temp = new Vector3(dir.x, 0, dir.z);
                        var angle = Vector3.Angle(dir, temp);
                        if (Vector3.Angle(dir, temp)>30)
                        {
                            continue;
                        }

                        //if (secondRoom.FloorIndex + 0.3f * (secondRoom.RoofIndex - secondRoom.FloorIndex) < secondRoom.EdgeCells[indexSecondRoom].yCoord)
                        //    continue;

                        float distance = Vector3Int.Distance(firstRoom.EdgeCells[indexFirstRoom], secondRoom.EdgeCells[indexSecondRoom]);

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
            if (shortestDistance < float.MaxValue - 1.0f)
            {
                CreateCorridor(bestFirstRoom, bestSecondRoom, bestFirstCell, bestSecondCell);
                shortestDistance = float.MaxValue;
            }
        }
    }


    

    protected override void CreateCorridor(Room firstRoom, Room secondRoom, Coord firstRoomCell, Coord secondRoomCell)
    {
        Room.ConnectRooms(firstRoom, secondRoom);
        Vector3 posFirstCell = new Vector3(-_width / 2 + firstRoomCell.xCoord + 0.5f, -_height / 2 + firstRoomCell.yCoord + 0.5f, -_depth / 2 + firstRoomCell.zCoord + 0.5f);
        Vector3 posSecondCell = new Vector3(-_width / 2 + secondRoomCell.xCoord + 0.5f, -_height / 2 + secondRoomCell.yCoord + 0.5f, -_depth / 2 + secondRoomCell.zCoord + 0.5f);
        Debug.DrawLine(posFirstCell, posSecondCell, Color.red, 50);
        Debug.Log("ConnectionFound");
    }


    protected override void UpdateCubes()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                for (int z = 0; z < _depth; z++)
                {
                    if (_map3D[x, y, z].mesh)
                    {
                        MeshRenderer renderer = _map3D[x, y, z].mesh.GetComponent<MeshRenderer>();
                        if (renderer)
                        {
                            if (_map3D[x, y, z].color.a < 0.01f)
                            {
                                renderer.material.color = (_map3D[x, y, z].state == States.Wall) ? Color.black : Color.white;
                            }
                            else
                            {

                                renderer.material.color = _map3D[x, y, z].color;
                            }
                        }

                        if ((!_showWalls && _map3D[x, y, z].state == States.Wall) ||
                            (!_showEmpty && _map3D[x, y, z].state == States.Empty) || (_shellEmpty && _map3D[x, y, z].neighbourCount == 26)
                            || (!_showShell &&
                                (x == 0 || x == _width - 1 || y == 0 || y == _height - 1 || z == 0 || z == _depth - 1)))
                            _map3D[x, y, z].mesh.gameObject.SetActive(false);
                        else
                            _map3D[x, y, z].mesh.gameObject.SetActive(true);
                    }
                    else
                    {
                        Vector3 pos = new Vector3(-_width / 2 + x + 0.5f, -_height / 2 + y + 0.5f,
                            -_depth / 2 + z + 0.5f);
                        _map3D[x, y, z].mesh = Instantiate(_cube);
                        _map3D[x, y, z].mesh.transform.position = pos;

                        MeshRenderer renderer = _map3D[x, y, z].mesh.GetComponent<MeshRenderer>();
                        if (renderer)
                        {
                            if (_map3D[x, y, z].color.a <0.01f)
                            {
                                renderer.material.color = (_map3D[x, y, z].state == States.Wall) ? Color.black : Color.white;
                            }
                            else
                            {
                       
                                renderer.material.color = _map3D[x, y, z].color;
                            }
                        }

                        if ((!_showWalls && _map3D[x, y, z].state == States.Wall) ||
                            (!_showEmpty && _map3D[x, y, z].state == States.Empty) || (_shellEmpty&&_map3D[x, y, z].neighbourCount == 26) || (!_showShell &&
                            (x == 0 || x == _width - 1 || y == 0 || y == _height - 1 || z == 0 || z == _depth - 1)))
                            _map3D[x, y, z].mesh.gameObject.SetActive(false);
                        else
                            _map3D[x, y, z].mesh.gameObject.SetActive(true);
                    }


                }
            }
        }

    }
}
