using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapGenerator3D : MapGenerator
{

    [SerializeField] [Range(1, 50)] private int _depth = 50;
    [Space(10)]
    [Header("3D unique variables")]
    [SerializeField] private bool _generateMesh = true;

    public bool GenerateMesh
    {
        set => _generateMesh=value;
    }
    public int Depth
    {
        set => _depth = value;
    }

    [CanBeNull] private Cell[,,] _map3D;
    private States[,,] _stateBuffer3D;

    public Cell[,,] Map => _map3D;

    //3D code

    /// <summary>
    /// check if the cell lies on the map or not 3D
    /// </summary>
    /// <param name="x">x coord of cell</param>
    /// <param name="y">y coord of cell</param>
    /// <param name="z">z coord of cell</param>
    /// <returns></returns>
    public bool IsInMap3D(int x, int y, int z)
    {
        return x >= 0 && x < _width && y >= 0 && y < _height && z >= 0 && z < _depth;
    }

    private void Start()
    {
        GenerateMap();
    }

    // Generate the Cellular Automata Map

    public async Task GenerateMapAsync()
    {
        if (_map3D != null)
            ClearMap();
        _map3D = new Cell[_width, _height, _depth];
        _stateBuffer3D = new States[_width, _height, _depth];



        if (_useRandomSeed)
            _seed = System.DateTime.Now.ToString();
        if (_randomNumberGenerator == null)
            _randomNumberGenerator = new System.Random(_seed.GetHashCode());




        RandomFillMap();
        IterateStates();
        ExamineMap();
        if (_generateMesh)
        {

            MeshGenerator meshGenerator = GetComponent<MeshGenerator>();
            if (meshGenerator)
                meshGenerator.GenerateMesh(_map3D, 1);
        }
        else
        {
            UpdateCubes();
        }



    }

    public override void GenerateMap()
    {

        if (_map3D != null)
            ClearMap();
        _map3D = new Cell[_width, _height, _depth];
        _stateBuffer3D = new States[_width, _height, _depth];



        if (_useRandomSeed)
            _seed = System.DateTime.Now.ToString();
        if (_randomNumberGenerator==null)
            _randomNumberGenerator = new System.Random(_seed.GetHashCode());
  



        RandomFillMap();
        IterateStates();
        ExamineMap();
        if (_generateMesh)
        {

            MeshGenerator meshGenerator = GetComponent<MeshGenerator>();
            if (meshGenerator)
                meshGenerator.GenerateMesh(_map3D, 1);
        }
        else
        {
            UpdateCubes();
        }



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
                    else if (_makeEdgesWalls)
                    {
                        wallcount++;
                    }
                    else if (_makeShellSameAsNeighbour)
                    {
                        wallcount += (int)_map3D[indexX, indexY, indexZ].state;
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
                    if (_makeEdgesWalls && (x == 0 || x == _width - 1 || y == 0 || y == _height - 1 || z == 0 ||
                                            z == _depth - 1))
                    {
                        _map3D[x, y, z].state = States.Wall;
                    }
                    else
                    {
                        //if(_useRandomSeed)
                        //_map3D[x, y, z].state = (Random.Range(0, 1.0f) < _randomFillPercent)
                        //    ? States.Wall
                        //    : States.Empty;
                        //else
                        _map3D[x, y, z].state = (_randomNumberGenerator.Next(0, 100) / 100.0f < _randomFillPercent) ? States.Wall : States.Empty;

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
            int equalityIndex = 0;
            for (int x = cell.xCoord - 1; x <= cell.xCoord + 1; x++)
            {
                if (x == cell.xCoord)
                    ++equalityIndex;
                for (int y = cell.yCoord - 1; y <= cell.yCoord + 1; y++)
                {
                    if (y == cell.yCoord)
                        ++equalityIndex;
                    for (int z = cell.zCoord - 1; z <= cell.zCoord + 1; z++)
                    {
                        if (z == cell.zCoord)
                            ++equalityIndex;
                        if (IsInMap3D(x, y, z) &&equalityIndex==2&& mapFlags[x, y, z] == 0 && _map3D[x, y, z].state == cellState)
                        {
          
                                mapFlags[x, y, z] = 1;
                                queue.Enqueue(new Coord(x, y,z));
                            
                        }

                        if (z == cell.zCoord)
                            --equalityIndex;
                    }
                    if (y == cell.yCoord)
                        --equalityIndex;
                }
                if (x == cell.xCoord)
                    --equalityIndex;
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
        if(_colorRegions)
            ColourRegions();
        //sort form big to small
        if (survivingRooms.Count > 0)
        {
            survivingRooms.Sort();
            survivingRooms[0].IsMainRoom = true;
            survivingRooms[0].IsAccessibleFormMainRoom = true;
        }

        List<Corridor> corridors = new List<Corridor>();
        if (_connectClosestRooms)
            corridors.AddRange(ConnectClosestRooms(survivingRooms));
        if (_useDigger)
        {
            corridors.AddRange(DigCorridors(survivingRooms,corridors));
        }
    }

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
            if (!_forceAccessibilityFromMainRoom && firstRoom.ConnectedRooms.Count > 0)
                continue;

            foreach (Room secondRoom in roomsNotConnectedToMain)
            {
                //if the room is itself skip it or if the rooms are already connected skip 
                if (firstRoom == secondRoom || firstRoom.IsConnected(secondRoom))
                    continue;
                //3 is a var
                //if (firstRoom.FloorIndex >= secondRoom.RoofIndex + 3 || firstRoom.RoofIndex + 3 <= secondRoom.FloorIndex)
                //    continue;
                for (int indexFirstRoom = 0; indexFirstRoom < firstRoom.EdgeCells.Count; indexFirstRoom++)
                {
                    //if (firstRoom.FloorIndex == firstRoom.EdgeCells[indexFirstRoom].yCoord || firstRoom.RoofIndex == firstRoom.EdgeCells[indexFirstRoom].yCoord)
                    //    continue;
                    /* else*/
                    if ( firstRoom.FloorIndex + 0.3f * (firstRoom.RoofIndex - firstRoom.FloorIndex) < firstRoom.EdgeCells[indexFirstRoom].yCoord)
                        continue;
                    for (int indexSecondRoom = 0; indexSecondRoom < secondRoom.EdgeCells.Count; indexSecondRoom++)
                    {

                        //if (secondRoom.FloorIndex == secondRoom.EdgeCells[indexSecondRoom].yCoord || secondRoom.RoofIndex == secondRoom.EdgeCells[indexSecondRoom].yCoord)
                        //    continue;
                        if ( secondRoom.FloorIndex + 0.3f * (secondRoom.RoofIndex - secondRoom.FloorIndex) < secondRoom.EdgeCells[indexSecondRoom].yCoord)
                            continue;


                        Vector3Int dir = (Vector3Int)secondRoom.EdgeCells[indexSecondRoom] - (Vector3Int)firstRoom.EdgeCells[indexFirstRoom];
                        Vector3Int temp = new Vector3Int(dir.x, 0, dir.z);
                        //debug
                        var angle = Vector3.Angle(dir, temp);
                        if (Vector3.Angle(dir, temp)>30||temp.Equals(Vector3Int.zero))
                        {
                            continue;
                        }



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
            if (roomsNotConnectedToMain.Count > 0)
               corridors.AddRange(ConnectClosestRooms(roomsToConnect));
        }

        return corridors;
    }


    // calculates a line from one part coord to the other and returns a list of coordinates which are part of that line
    List<Coord> GetLine(Coord from, Coord to)
    {
        List<Coord> line = new List<Coord>();

        int x = from.xCoord; 
        int y = from.yCoord;
        int z = from.zCoord;

        int dx = to.xCoord - from.xCoord;
        int dy = to.yCoord - from.yCoord;
        int dz = to.zCoord - from.zCoord;


        int step = 0;
        int gradientStep = 0;
        int extraStep = 0;
        
        int longest=Mathf.Max(Math.Abs(dx), Math.Abs(dy), Math.Abs(dz));
        int shortest = 0;
        int extra = 0;

        Enum test = Axis.x;
        Enum longestAxis = Axis.x;
        Enum shortestAxis = Axis.x;

        if (longest == Math.Abs(dx))
        {
            longestAxis = Axis.x;
            shortest=/*Mathf.Min( */Math.Abs(dy)/*, Math.Abs(dz))*/;
            //if (shortest == Math.Abs(dy))
            {
                shortestAxis = Axis.y;
                step = Math.Sign(dx);
                gradientStep = Math.Sign(dy);
                extraStep = Math.Sign(dz);
                test = Axis.z;
                extra = Math.Abs(dz);
               
            }
           // else
           // {
            //    shortestAxis = Axis.z;
            //    step = Math.Sign(dx);
            //    gradientStep = Math.Sign(dz);
            //    extraStep = Math.Sign(dy);
            //    test = Axis.y;
            //    extra = Math.Abs(dy);
                
            //}
        }
        else if (longest == Math.Abs(dy))
        {
            longestAxis = Axis.y;
            shortest=/*Mathf.Min( Math.Abs(dx), */Math.Abs(dz)/*)*/;
            //if (shortest == Math.Abs(dz))
           // {
                step = Math.Sign(dy);
                gradientStep = Math.Sign(dz);
                extraStep = Math.Sign(dx);
                shortestAxis = Axis.z;
                test = Axis.x;
                extra = Math.Abs(dx);
              
           // }
            //else
            //{
            //    extraStep = Math.Sign(dz);
            //    step = Math.Sign(dy);
            //    gradientStep = Math.Sign(dx);
            //    shortestAxis = Axis.x;
            //    test = Axis.z;
            //    extra = Math.Abs(dz);
           
            //}
        }
        else
        {
            longestAxis = Axis.z;
            shortest=/*Mathf.Min( */Math.Abs(dy)/*, Math.Abs(dx))*/;
           // if (shortest == Math.Abs(dy))
           // {
                extraStep = Math.Sign(dx);
                step = Math.Sign(dz);
                gradientStep = Math.Sign(dy);
                shortestAxis = Axis.y;
                test = Axis.x;
                extra = Math.Abs(dx);
            
            //}
            //else
            //{
            //    extraStep = Math.Sign(dy);
            //    step = Math.Sign(dz);
            //    gradientStep = Math.Sign(dx);
            //    shortestAxis = Axis.x;
            //    test = Axis.y;
            //    extra = Math.Abs(dy);
               
            //}
        }
        

        int gradientAccumulation = longest / 2;
        int extraGradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new Coord(x, y,z));

            switch (longestAxis)
            {
                case Axis.x:
                    x += step;
                    break;
                case Axis.y:
                    y += step;
                    break;
                case Axis.z:
                    z += step;
                    break;

            }

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {

                switch (shortestAxis)
                {
                    case Axis.x:
                     
                        x += gradientStep;
                        break;
                    case Axis.y:
                      
                        y += gradientStep;
                        break;
                    case Axis.z:
                        z += gradientStep;
                        break;

                }

                gradientAccumulation -= longest;
            }

            extraGradientAccumulation += extra;
            if (extraGradientAccumulation >= longest)
            {

                switch (test)
                {
                    case Axis.x:
                        x += extraStep;
                        break;
                    case Axis.y:
                        y += extraStep;
                        break;
                    case Axis.z:
                        z += extraStep;
                        break;

                }

                extraGradientAccumulation -= longest;
            }


        }




        return line;

    }

    // changes the cells in a radius to empty
    private List<Coord> DrawSphere(Coord cell,Color color)
    {
        int radius = Random.Range(_corridorRadius.x, _corridorRadius.y);
        List<Coord> cells = new List<Coord>();
        for (int x = -radius; x < radius; x++)
        {
            for (int y = -radius; y < radius; y++)
            {
                for (int z = -radius; z < radius; z++)
                {
                    if (x * x + y * y+ z*z <= radius * radius)
                    {
                        int xToChange = cell.xCoord + x;
                        int yToChange = cell.yCoord + y;
                        int zToChange = cell.zCoord + z;

                        if (IsInMap3D(xToChange, yToChange, zToChange))
                        {
                            cells.Add(new Coord(xToChange, yToChange,zToChange));
                            _map3D[xToChange, yToChange,zToChange].state = States.Empty;
                            _map3D[xToChange, yToChange,zToChange].color = color;
                        }
                    }
                }
            }
        }

        return cells;
    }

    //creates a corridor in between two rooms 
    protected override Corridor CreateCorridor(Room firstRoom, Room secondRoom, Coord firstRoomCell, Coord secondRoomCell)
    {
        Room.ConnectRooms(firstRoom, secondRoom);
        Vector3 posFirstCell = new Vector3(-_width / 2 + firstRoomCell.xCoord + 0.5f, -_height / 2 + firstRoomCell.yCoord + 0.5f, -_depth / 2 + firstRoomCell.zCoord + 0.5f);
        Vector3 posSecondCell = new Vector3(-_width / 2 + secondRoomCell.xCoord + 0.5f, -_height / 2 + secondRoomCell.yCoord + 0.5f, -_depth / 2 + secondRoomCell.zCoord + 0.5f);

        _map3D[firstRoomCell.xCoord, firstRoomCell.yCoord, firstRoomCell.zCoord].color.g = 1;
        _map3D[secondRoomCell.xCoord, secondRoomCell.yCoord, secondRoomCell.zCoord].color.g = 1;
        Debug.DrawLine(posFirstCell, posSecondCell, Color.red, 50);
        Debug.Log("ConnectionFound");

        List<Coord> line = GetLine(firstRoomCell, secondRoomCell);
        Corridor corridor = new Corridor();

        foreach (Coord cell in line)
        {
            corridor.AddCells(DrawSphere(cell, Color.white));
        }
        corridor.AddCells(line);
        corridor.CalcEdges(_map3D);
        return corridor;
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
                            if (_map3D[x, y, z].color == new Color())
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
                        Vector3 pos = new Vector3(-_width / 2 + x , -_height / 2 + y ,
                            -_depth / 2 + z );
                        _map3D[x, y, z].mesh = Instantiate(_cube);
                        _map3D[x, y, z].mesh.transform.position = pos;

                        MeshRenderer renderer = _map3D[x, y, z].mesh.GetComponent<MeshRenderer>();
                        if (renderer)
                        {
                            if (_map3D[x, y, z].color == new Color())
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

    protected override List<Corridor> DigCorridors(List<Room> rooms, List<Corridor> corridors)
    {
        Area currentRoom = new Room();
        Vector3Int direction = GetNewDirection(new Vector3Int());
        Coord cell = new Coord(-10, -10);
        List<Corridor> newCorridors = new List<Corridor>();
        int breakOutCounter = 0;
        while (rooms.Count > 0)
        {


            if (corridors.Count > 0)
            {
                currentRoom = rooms[_randomNumberGenerator.Next(0, rooms.Count)];
                GetDigPos(currentRoom, ref cell, ref direction);
            }
            else
            {
                if (corridors.Count > 0 && _randomNumberGenerator.Next(0, 100) / 100.0f > _corridorFromRoomChance)
                {

                    currentRoom = corridors[_randomNumberGenerator.Next(0, corridors.Count - 1)];
                    GetDigPos(currentRoom, ref cell, ref direction);

                }
                else
                {
                    currentRoom = rooms[_randomNumberGenerator.Next(0, rooms.Count - 1)];
                    GetDigPos(currentRoom, ref cell, ref direction);

                }
            }

            Corridor potentialCorridor = GeneratePotentialCorridor(cell, direction);
            if (potentialCorridor != null)
                potentialCorridor.CalcEdges(_map3D);


            if (potentialCorridor != null)
            {
                Room roomToRemove = null;
                foreach (Room room in rooms)
                {

                    if (room.Cells.Contains(potentialCorridor.Last()))
                    {
                        if (currentRoom != room)
                        {
                            //color debug show end node
                            _map3D[potentialCorridor.Last().xCoord, potentialCorridor.Last().yCoord, potentialCorridor.Last().zCoord].state = States.Empty;
                            _map3D[potentialCorridor.Last().xCoord, potentialCorridor.Last().yCoord, potentialCorridor.Last().zCoord].color = Color.green;
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

                            List<Coord> newCells = new List<Coord>();
                            foreach (Coord edgeCell in potentialCorridor.Cells)
                            {
                                if (_map3D != null)
                                {

                                    _map3D[edgeCell.xCoord, edgeCell.yCoord,edgeCell.zCoord].state = States.Empty;
                                    _map3D[edgeCell.xCoord, edgeCell.yCoord,edgeCell.zCoord].color = Color.magenta;
                                

                                }
                                newCells.AddRange(DrawSphere(edgeCell,Color.magenta));
                            }
                            potentialCorridor.AddCells(newCells);
                            newCorridors.Add(potentialCorridor);
                            roomToRemove = room;

                        }

                    }
                }

                rooms.Remove(roomToRemove);
            }

            ++breakOutCounter;

            if (breakOutCounter > _breakOutValue)
                return newCorridors;
        }

        return newCorridors;

    }


    private Corridor GeneratePotentialCorridor(Coord start, Vector3Int dir)
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

            if (dir.y < 0.1f && dir.y > -0.1f)
            {
                while (length > 0)
                {
                    --length;

                    currentCell = currentCell + dir;

                    if (!IsInMap3D(currentCell.xCoord, currentCell.yCoord, currentCell.zCoord))
                        return null;

                    if (_map3D != null && _map3D[currentCell.xCoord, currentCell.yCoord, currentCell.zCoord].state ==
                        States.Empty)
                    {
                        if ((Vector3Int)currentCell == (start + dir))
                        {
                            return null;
                        }
                        potentialCorridor.AddCell(currentCell);
                        return potentialCorridor;
                    }

                    if (!CorridorSpacingCheck(currentCell, dir)) 
                        return null;

                    potentialCorridor.AddCell(currentCell);

                }
            }
            else
            {
                Vector3Int offset = length * dir;
                int sign = Math.Sign(offset.y);
                offset.y = (int)(offset.y * 0.3f+ sign);
                Coord endCell = currentCell + offset;
                List<Coord> line = GetLine(currentCell, endCell);

                foreach (Coord cell in line)
                {
                    //cool if removed
                    currentCell = cell;
                    if (!IsInMap3D(cell.xCoord, cell.yCoord, cell.zCoord))
                        return null;

                    if (_map3D != null && _map3D[cell.xCoord, cell.yCoord, cell.zCoord].state ==
                        States.Empty)
                    {
                        if ((Vector3Int)currentCell == (start + dir))
                        {
                            return null;
                        }
                        potentialCorridor.AddCell(cell);
                        return potentialCorridor;
                    }
                    if (!CorridorSpacingCheck(currentCell, dir))
                        return null;


                    potentialCorridor.AddCell(cell);
                }
            }
            dir = GetNewDirection(dir);
        }
        return null;
    }


    private bool CorridorSpacingCheck(Coord cell, Vector3Int direction)
    {
        for (int r=-_corridorSpacing;r<=_corridorSpacing;r++)
        {
            if (direction.x == 0)//north or south
            {
                if (direction.y == 0)
                {
                    if (IsInMap3D(cell.xCoord + r, cell.yCoord, cell.zCoord))
                        if (_map3D[cell.xCoord + r, cell.yCoord, cell.zCoord].state != States.Wall)
                            return false;

                }
                else
                {
                    if (IsInMap3D(cell.xCoord + r, cell.yCoord+r, cell.zCoord))
                        if (_map3D[cell.xCoord + r, cell.yCoord+r, cell.zCoord].state != States.Wall)
                            return false;
                }
            }
            else if (direction.z == 0) //east west
            {
                if (direction.y == 0)
                {
                    if (IsInMap3D(cell.xCoord, cell.yCoord, cell.zCoord + r))
                        if (_map3D[cell.xCoord, cell.yCoord, cell.zCoord + r].state != States.Wall)
                            return false;
                }
                else
                {
                    if (IsInMap3D(cell.xCoord, cell.yCoord + r, cell.zCoord + r))
                        if (_map3D[cell.xCoord, cell.yCoord + r, cell.zCoord + r].state != States.Wall)
                            return false;
                }
            }

        }

        return true;
    }
    private void GetDigPos(Area room, ref Coord cell, ref Vector3Int dir)
    {
        Coord edgeCell = room.EdgeCells[_randomNumberGenerator.Next(0, room.EdgeCells.Count)];



        int equalityIndex = 0;
        for (int x = edgeCell.xCoord - 1; x <= edgeCell.xCoord + 1; x++)
        {
            if (x == edgeCell.xCoord)
                ++equalityIndex;
            for (int y = edgeCell.yCoord - 1; y <= edgeCell.yCoord + 1; y++)
            {
                if (y == edgeCell.yCoord)
                    ++equalityIndex;
                for (int z = edgeCell.zCoord - 1; z <= edgeCell.zCoord + 1; z++)
                {
                    if (z == edgeCell.zCoord)
                        ++equalityIndex;

                    //von neumann neighbourhood
                    if (IsInMap3D(x, y, z) && equalityIndex == 2 && _map3D[x, y, z].state == States.Wall)
                    {

                        dir = new Vector3Int(x, y,z) - new Vector3Int(edgeCell.xCoord, edgeCell.yCoord,edgeCell.zCoord);
                        if (dir.y == 0)
                        {
                            cell = edgeCell;
                            return;
                        }
                        else
                        {
                            GetDigPos(room,ref cell,ref dir);
                            return;
                        }

                    }

                    if (z == edgeCell.zCoord)
                        --equalityIndex;
                }
                if (y == edgeCell.yCoord)
                    --equalityIndex;
            }
            if (x == edgeCell.xCoord)
                --equalityIndex;
        }
    }

    private Vector3Int GetNewDirection(Vector3Int oldDir)
    {
        Vector3Int otherDirection = oldDir;
        do
        {

            switch (_randomNumberGenerator.Next(0, 12))
            {
                case 0:
                    otherDirection = new Vector3Int(1,1,0);
                    break;
                case 1:
                    otherDirection = Vector3Int.right;
                    break;
                case 2:
                    otherDirection = new Vector3Int(1, -1, 0);
                    break;
                case 3:
                    otherDirection = new Vector3Int(-1, 1, 0);
                    break;
                case 4:
                    otherDirection = Vector3Int.left;
                    break;
                case 5:
                    otherDirection = new Vector3Int(-1, -1, 0);
                    break;
                case 6:
                    otherDirection = new Vector3Int(0, 1, 1);
                    break;
                case 7:
                    otherDirection = Vector3Int.forward;
                    break;
                case 8:
                    otherDirection = new Vector3Int(0, -1, 1);
                    break;
                case 9:
                    otherDirection = new Vector3Int(0, 1, -1);
                    break;
                case 10:
                    otherDirection = Vector3Int.back;
                    break;
                case 11:
                    otherDirection = new Vector3Int(0, -1, -1);
                    break;
            }

            if (_createDeadEnds)
                return otherDirection;


        } while ( (otherDirection.x == -oldDir.x && otherDirection.y == -oldDir.y) 
                 || (otherDirection.x == -oldDir.x && otherDirection.z == -oldDir.z)
                 || (otherDirection.z == -oldDir.z && otherDirection.y == -oldDir.y));

        return otherDirection;
    }

}
