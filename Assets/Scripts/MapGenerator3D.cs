using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapGenerator3D : MapGenerator
{

    [SerializeField] [Range(1, 50)] private int _depth = 50;
    [SerializeField] private bool _colorRegions = true;

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
    public bool IsInMap3D(int x, int y, int z)
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



        if (_useRandomSeed)
            _seed = System.DateTime.Now.ToString();
        if (_randomNumberGenerator==null)
            _randomNumberGenerator = new System.Random(_seed.GetHashCode());
  



        RandomFillMap();
        IterateStates();
        ExamineMap();
        UpdateCubes();

        MeshGenerator meshGenerator =GetComponent<MeshGenerator>();

        if(meshGenerator)
            meshGenerator.GenerateMesh(_map3D,1);
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
                    if (_MakeEdgesWalls && (x == 0 || x == _width - 1 || y == 0 || y == _height - 1 || z == 0 ||
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
            survivingRooms[0].IsAccessableFormMainRoom = true;
        }
        ConnectClosestRooms(survivingRooms);
        
    }

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
                    if (firstRoom.FloorIndex + 0.3f * (firstRoom.RoofIndex - firstRoom.FloorIndex) < firstRoom.EdgeCells[indexFirstRoom].yCoord)
                        continue;
                    for (int indexSecondRoom = 0; indexSecondRoom < secondRoom.EdgeCells.Count; indexSecondRoom++)
                    {

                        //if (secondRoom.FloorIndex == secondRoom.EdgeCells[indexSecondRoom].yCoord || secondRoom.RoofIndex == secondRoom.EdgeCells[indexSecondRoom].yCoord)
                        //    continue;
                        if (secondRoom.FloorIndex + 0.3f * (secondRoom.RoofIndex - secondRoom.FloorIndex) < secondRoom.EdgeCells[indexSecondRoom].yCoord)
                            continue;


                        Vector3Int dir = (Vector3Int)secondRoom.EdgeCells[indexSecondRoom] - (Vector3Int)firstRoom.EdgeCells[indexFirstRoom];
                        Vector3Int temp = new Vector3Int(dir.x, 0, dir.z);
                        //debug
                        var angle = Vector3.Angle(dir, temp);
                        if (Vector3.Angle(dir, temp)>30||temp.Equals(Vector3Int.zero))
                        {
                            continue;
                        }
                       // Debug.Log(angle);

          

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
                CreateCorridor(bestFirstRoom, bestSecondRoom, bestFirstCell, bestSecondCell);
                shortestDistance = float.MaxValue;
            }
        }
        if (shortestDistance < float.MaxValue - 1.0f && _forceAccessibilityFromMainRoom)
        {
            CreateCorridor(bestFirstRoom, bestSecondRoom, bestFirstCell, bestSecondCell);
            shortestDistance = float.MaxValue;
            if (roomsNotConnectedToMain.Count > 0)
                ConnectClosestRooms(roomsToConnect);
        }
    }


    public enum axis 
    {
        x = 0,
        y = 1,
        z = 2
    }

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

        Enum test = axis.x;
        Enum longestAxis = axis.x;
        Enum shortestAxis = axis.x;

        if (longest == Math.Abs(dx))
        {
            longestAxis = axis.x;
            shortest=Mathf.Min( Math.Abs(dy), Math.Abs(dz));
            if (shortest == Math.Abs(dy))
            {
                shortestAxis = axis.y;
                step = Math.Sign(dx);
                gradientStep = Math.Sign(dy);
                extraStep = Math.Sign(dz);
                test = axis.z;
                extra = Math.Abs(dz);
               
            }
            else
            {
                shortestAxis = axis.z;
                step = Math.Sign(dx);
                gradientStep = Math.Sign(dz);
                extraStep = Math.Sign(dy);
                test = axis.y;
                extra = Math.Abs(dy);
                
            }
        }
        else if (longest == Math.Abs(dy))
        {
            longestAxis = axis.y;
            shortest=Mathf.Min( Math.Abs(dx), Math.Abs(dz));
            if (shortest == Math.Abs(dz))
            {
                step = Math.Sign(dy);
                gradientStep = Math.Sign(dz);
                extraStep = Math.Sign(dx);
                shortestAxis = axis.z;
                test = axis.x;
                extra = Math.Abs(dx);
              
            }
            else
            {
                extraStep = Math.Sign(dz);
                step = Math.Sign(dy);
                gradientStep = Math.Sign(dx);
                shortestAxis = axis.x;
                test = axis.z;
                extra = Math.Abs(dz);
           
            }
        }
        else
        {
            longestAxis = axis.z;
            shortest=Mathf.Min( Math.Abs(dy), Math.Abs(dx));
            if (shortest == Math.Abs(dy))
            {
                extraStep = Math.Sign(dx);
                step = Math.Sign(dz);
                gradientStep = Math.Sign(dy);
                shortestAxis = axis.y;
                test = axis.x;
                extra = Math.Abs(dx);
            
            }
            else
            {
                extraStep = Math.Sign(dy);
                step = Math.Sign(dz);
                gradientStep = Math.Sign(dx);
                shortestAxis = axis.x;
                test = axis.y;
                extra = Math.Abs(dy);
               
            }
        }
        

        int gradientAccumulation = longest / 2;
        int extraGradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new Coord(x, y,z));

            switch (longestAxis)
            {
                case axis.x:
                    x += step;
                    break;
                case axis.y:
                    y += step;
                    break;
                case axis.z:
                    z += step;
                    break;

            }

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {

                switch (shortestAxis)
                {
                    case axis.x:
                     
                        x += gradientStep;
                        break;
                    case axis.y:
                      
                        y += gradientStep;
                        break;
                    case axis.z:
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
                    case axis.x:
                        x += extraStep;
                        break;
                    case axis.y:
                        y += extraStep;
                        break;
                    case axis.z:
                        z += extraStep;
                        break;

                }

                extraGradientAccumulation -= longest;
            }


        }




        return line;

    }


    void DrawSphere(Coord cell)
    {
        int radius = Random.Range(_corridorRadius.x, _corridorRadius.y);
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
                            _map3D[xToChange, yToChange,zToChange].state = States.Empty;
                        }
                    }
                }
            }
        }
    }
    protected override void CreateCorridor(Room firstRoom, Room secondRoom, Coord firstRoomCell, Coord secondRoomCell)
    {
        Room.ConnectRooms(firstRoom, secondRoom);
        Vector3 posFirstCell = new Vector3(-_width / 2 + firstRoomCell.xCoord + 0.5f, -_height / 2 + firstRoomCell.yCoord + 0.5f, -_depth / 2 + firstRoomCell.zCoord + 0.5f);
        Vector3 posSecondCell = new Vector3(-_width / 2 + secondRoomCell.xCoord + 0.5f, -_height / 2 + secondRoomCell.yCoord + 0.5f, -_depth / 2 + secondRoomCell.zCoord + 0.5f);

        _map3D[firstRoomCell.xCoord, firstRoomCell.yCoord, firstRoomCell.zCoord].color.g = 1;
        _map3D[secondRoomCell.xCoord, secondRoomCell.yCoord, secondRoomCell.zCoord].color.g = 1;
        Debug.DrawLine(posFirstCell, posSecondCell, Color.red, 50);
        Debug.Log("ConnectionFound");

        List<Coord> line = GetLine(firstRoomCell, secondRoomCell);

        foreach (Coord cell in line)
        {
            DrawSphere(cell);
        }
    }


    private List<Cube> CreateCubes()
    {
        List<Cube> cubes = new List<Cube>();


        


        return cubes;
    }

    private void testCube()
    {
        int cubeIndex = 0;

        for (int i = 0; i < 8; i++)
        {
            if (_map3D[0, 0, 0].state == States.Wall)
            {
                cubeIndex = 1 << i;
            }
        }
    }
    //private void MarchingSquares()
    //{

    //    for (int x = 0; x < _width; x++)
    //    {
    //        for (int y = 0; y < _height; y++)
    //        {
    //            for (int z = 0; z < _depth; z++)
    //            {
    //                CalcWallIndices(x,y,z);
    //                NextStep(x,y,z);
    //            }
    //        }
    //    }
    //}

    //private void NextStep(int x, int y, int z)
    //{
    //    int cubeIndex=0;
    //    for (int i = 0; i < 8; i++)
    //    {
    //        if (_map3D[x, y, z].index[i] > 0)
    //        {
    //            cubeIndex = 1 << i;
    //        }


    //        for (int j = 0; j < 16; j++)
    //        {
    //            int t =Table.TriTable[i, j];

    //            int indexA = Table.cornerIndexAFromEdge[t];
    //            int indexB = Table.cornerIndexBFromEdge[t];
    //            Vector3 vertexPos= (cube)
    //        }
    //    }
    //}

    //private Vector3 GetCorner(int xCoord, int yCoord, int zCoord)
    //{
    //    int counter = 0;
    //    for (int x = xCoord - 1; x <= xCoord + 1; x++)
    //    {
    //        if (x == xCoord)
    //            ++equalityIndex;
    //        for (int y = yCoord - 1; y <= yCoord + 1; y++)
    //        {
    //            if (y == yCoord)
    //                ++equalityIndex;
    //            for (int z = zCoord - 1; z <= zCoord + 1; z++)
    //            {
    //                if (z == zCoord)
    //                    ++equalityIndex;
    //                ////State wall Might need to be state Empty
    //                ////State wall Might need to be state Empty
    //                ////State wall Might need to be state Empty
    //                ////State wall Might need to be state Empty
    //                ////State wall Might need to be state Empty
    //                ////State wall Might need to be state Empty
    //                ////State wall Might need to be state Empty
    //                if (IsInMap3D(x, y, z) && equalityIndex == 2 && _map3D[x, y, z].state == States.Wall)
    //                {

    //                    _map3D[x, y, z].index[i] = 1;

    //                }
    //                i++;

    //                if (z == zCoord)
    //                    --equalityIndex;
    //            }
    //            if (y == yCoord)
    //                --equalityIndex;
    //        }
    //        if (x == xCoord)
    //            --equalityIndex;
    //    }
    //}

    //private void CalcWallIndices(int xCoord,int yCoord,int zCoord)
    //{
    //    int i = 0;
    //    int equalityIndex = 0;

    //    for (int x =  xCoord- 1; x <= xCoord + 1; x++)
    //    {
    //        if (x == xCoord)
    //            ++equalityIndex;
    //        for (int y = yCoord - 1; y <= yCoord + 1; y++)
    //        {
    //            if (y == yCoord)
    //                ++equalityIndex;
    //            for (int z = zCoord - 1; z <= zCoord + 1; z++)
    //            {
    //                if (z == zCoord)
    //                    ++equalityIndex;
    //                ////State wall Might need to be state Empty
    //                ////State wall Might need to be state Empty
    //                ////State wall Might need to be state Empty
    //                ////State wall Might need to be state Empty
    //                ////State wall Might need to be state Empty
    //                ////State wall Might need to be state Empty
    //                ////State wall Might need to be state Empty
    //                if (IsInMap3D(x, y, z) && equalityIndex == 2 && _map3D[x, y, z].state == States.Wall)
    //                {

    //                    _map3D[x, y, z].index[i] = 1;

    //                }
    //                i++;

    //                if (z == zCoord)
    //                    --equalityIndex;
    //            }
    //            if (y == yCoord)
    //                --equalityIndex;
    //        }
    //        if (x == xCoord)
    //            --equalityIndex;
    //    }


    //}

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
                        Vector3 pos = new Vector3(-_width / 2 + x , -_height / 2 + y ,
                            -_depth / 2 + z );
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
