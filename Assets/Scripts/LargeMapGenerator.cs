using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class LargeMapGenerator : MapGenerator
{

    [SerializeField] [Range(1, 50)] private int _depth = 50;

    [Space(10)] [Header("3D unique variables")] [SerializeField]
    private bool _generateMesh = true;
    [SerializeField] private int _meshSizeModifier = 1;
    [Header("GetMap Type")] [SerializeField]
    private bool _is3D = false;
    [SerializeField] private int _maxRoomHeight = 10;
    [SerializeField] private int _roomCutHeight = 4;
    [SerializeField] private int _roomCutChance = 90;
    [SerializeField] private bool _mineCenter = false;
    [SerializeField] private bool _growRooms = true;
    [SerializeField] [Range(0, 10)] private int _growIterations = 0;
    [SerializeField] private Coord _chunkDimensions = new Coord(1, 1, 1);
    private MapGenerator[,,] _generators;
    private Thread[,,] _threads;


    private Cell[,,] _map;

    private const int _asyncCount = 8;


    private int _asyncCounter = 0;


    //private ChunkMap _map= new ChunkMap();

    private void Start()
    {
        if (_is3D)
        {
            _threads = new Thread[_chunkDimensions.xCoord, _chunkDimensions.yCoord, _chunkDimensions.zCoord];
            _generators = new MapGenerator3D[_chunkDimensions.xCoord, _chunkDimensions.yCoord, _chunkDimensions.zCoord];
            for (int x = 0; x < _chunkDimensions.xCoord; x++)
            {
                for (int y = 0; y < _chunkDimensions.yCoord; y++)
                {
                    for (int z = 0; z < _chunkDimensions.zCoord; z++)
                    {
                        MapGenerator generator = gameObject.AddComponent<MapGenerator3D>();
                        Assign(ref generator, this);

                        MapGenerator3D generator3D = generator as MapGenerator3D;
                        if (generator3D != null)
                        {
                            generator3D.Depth = _depth;
                            generator3D.GenerateMesh = _generateMesh;
                            generator3D.MaxRoomHeight = _maxRoomHeight;
                            generator3D.RoomCutHeight = _roomCutHeight;
                            generator3D.RoomCutChance = _roomCutChance;
                            generator3D.MeshSizeModifier = _meshSizeModifier;
                            generator3D.MineCenter = _mineCenter;
                            generator3D.GrowIterations = _growIterations;
                            generator3D.GrowRoom = _growRooms;
                            generator3D.IsChunk = true;
                        }

                        _generators[x, y, z] = generator;
                        if(_generateMesh)
                        _generators[x, y, z].Offset = new Vector3Int(x* _meshSizeModifier, y* _meshSizeModifier, z* _meshSizeModifier);
                        else
                        _generators[x, y, z].Offset = new Vector3Int(x, y, z);
                        Vector3Int t = new Vector3Int(x, y, z);
                        _threads[x, y, z] = (new Thread(new ThreadStart(
                            () => { _generators[t.x, t.y, t.z].GenerateMap(); })));
                    }
                }
            }
        }
        else
        {
            _chunkDimensions.zCoord = 1;
            _depth = 1;
            _threads = new Thread[_chunkDimensions.xCoord, _chunkDimensions.yCoord, 1];
            _generators = new MapGenerator2D[_chunkDimensions.xCoord, _chunkDimensions.yCoord, 1];
            for (int x = 0; x < _chunkDimensions.xCoord; x++)
            {
                for (int y = 0; y < _chunkDimensions.yCoord; y++)
                {


                    MapGenerator generator = gameObject.AddComponent<MapGenerator2D>();
                    Assign(ref generator, this);

                    MapGenerator2D temp = generator as MapGenerator2D;


                    _generators[x, y, 0] = generator;
                    _generators[x, y, 0].Offset = new Vector3Int(x, y, 0);
                    Vector3Int t = new Vector3Int(x, y, 0);
                    _threads[x, y, 0] = (new Thread(new ThreadStart(
                        () => { _generators[t.x, t.y, 0].GenerateMap(); })));
                    

                }
            }

        }

        GenerateMap();
        CreateBigMap();
        UpdateCubes();
        ConnectBigMap();
        CreateBigMapV2();
        UpdateCubes();
    }

    private void ThreadFct3DExamine()
    {
        List<Thread> runningThreads = new List<Thread>();
        for (int x = 0; x < _chunkDimensions.xCoord - 1; x++)
        {
            for (int y = 0; y < _chunkDimensions.yCoord - 1; y++)
            {
                for (int z = 0; z < _chunkDimensions.zCoord - 1; z++)
                {
                    if (_asyncCounter < _asyncCount)
                    {
                        _threads[x, y, z].Start();
                        _asyncCounter++;
                        runningThreads.Add(_threads[x, y, z]);
                    }

                    if (_asyncCounter != _asyncCount) continue;
                    foreach (var thread in runningThreads)
                    {
                        thread.Join();
                        --_asyncCounter;
                    }
                    runningThreads.Clear();
                    
                }
            }
        }
        

        if (_asyncCounter > 0)
        {
            foreach (var thread in runningThreads)
            {
                thread.Join();
                --_asyncCounter;
            }
        }
    }

    private void ThreadedGeneration()
    {
        List<Thread> runningThreads = new List<Thread>();
        for (int x = 0; x < _chunkDimensions.xCoord; x++)
        {
            for (int y = 0; y < _chunkDimensions.yCoord; y++)
            {
                for (int z = 0; z < _chunkDimensions.zCoord; z++)
                {
                    if (_asyncCounter < _asyncCount)
                    {
                        _threads[x, y, z].Start();
                        _asyncCounter++;
                        runningThreads.Add(_threads[x, y, z]);
                    }

                    if (_asyncCounter == _asyncCount)
                    {
                        foreach (var thread in runningThreads)
                        {
                            thread.Join();
                            --_asyncCounter;
                        }
                        runningThreads.Clear();
                    }

                }
            }
        }

        if (_asyncCounter > 0)
        {
            foreach (var thread in runningThreads)
            {
                thread.Join();
                --_asyncCounter;
            }
        }
    }
    private void ThreadFct2DExamine()
    {
        //_asyncCounter = 0;
        List<Thread> runningThreads = new List<Thread>();
        for (int x = 0; x < _chunkDimensions.xCoord-1; x++)
        {
            for (int y = 0; y < _chunkDimensions.yCoord-1; y++)
            {

                if (_asyncCounter < _asyncCount)
                {
                    _threads[x, y, 0].Start();
                    _asyncCounter++;
                    runningThreads.Add(_threads[x, y, 0]);
                }

                if (_asyncCounter != _asyncCount) continue;
                foreach (var thread in runningThreads)
                {
                    thread.Join();
                    --_asyncCounter;
                }
                runningThreads.Clear();
            }
        }

        if (_asyncCounter > 0)
        {
            foreach (var thread in runningThreads)
            {
                thread.Join();
                --_asyncCounter;
            }
        }
    }
    public override Cell[,,] GetMap()
    {
        return _map;
    }

    public override void SetMap(Cell[,,] map)
    {
        _map = map;
    }

    public override void GenerateMap()
    {
     
        ThreadedGeneration();

    }

    private void CreateBigMap()
    {
        _map = new Cell[_chunkDimensions.xCoord * _width, _chunkDimensions.yCoord * _height, _chunkDimensions.zCoord * _depth];
        
        for (int x = 0; x < _generators.GetLength(0); x++)
        {
            for (int y = 0; y < _generators.GetLength(1); y++)
            {
                if (_is3D)
                {
                    for (int z = 0; z < _generators.GetLength(2); z++)
                    {
                        foreach (Cell cell in _generators[x, y, z].GetMap())
                        {
                            Coord cellCoord = new Coord((x * _width) + cell.coord.xCoord, (y * _height + cell.coord.yCoord), (z * _depth + cell.coord.zCoord));
                            _map[cellCoord.xCoord, cellCoord.yCoord, cellCoord.zCoord] = cell;
                            _map[cellCoord.xCoord, cellCoord.yCoord, cellCoord.zCoord].coord = cellCoord;
                        }
                    }
                }
                else
                {

                    foreach (Cell cell in _generators[x, y, 0].GetMap())
                    {
                        Coord cellCoord = new Coord((x * _width) + cell.coord.xCoord, (y * _height + cell.coord.yCoord), 0);
                        _map[cellCoord.xCoord, cellCoord.yCoord, cellCoord.zCoord] = cell;
                        _map[cellCoord.xCoord, cellCoord.yCoord, cellCoord.zCoord].coord = cellCoord;
                    }
                }
            }
        }
    }

    private void CreateBigMapV2()
    {
        for (int x = 0; x < _generators.GetLength(0)-1; x++)
        {
            for (int y = 0; y < _generators.GetLength(1)-1; y++)
            {
                if (_is3D)
                {
                    for (int z = 0; z < _generators.GetLength(2) - 1; z++)
                    {
                        foreach (Cell cell in _generators[x, y, z].GetMap())
                        {
                            _map[cell.coord.xCoord, cell.coord.yCoord, cell.coord.zCoord] = cell;
                        }
                    }
                }
                else
                {

                    foreach (Cell cell in _generators[x, y, 0].GetMap())
                    {
                        _map[cell.coord.xCoord,cell.coord.yCoord,cell.coord.zCoord] = cell;
                    }
                }
            }
        }
    }

    private void ConnectBigMap()
    {
        for (int x = 0; x < _generators.GetLength(0) - 1; x++)
        {
            for (int y = 0; y < _generators.GetLength(1) - 1; y++)
            {
                if (_is3D)
                {
                    for (int z = 0; z < _generators.GetLength(2) - 1; z++)
                    {
                        Cell[,,] mapToConnect = new Cell[_width, _height, _depth];
                        for (int ix = 0; ix < _width; ix++)
                        {
                            for (int iy = 0; iy < _height; iy++)
                            {
                                for (int iz = 0; iz < _depth; iz++)
                                {
                                    mapToConnect[ix, iy, iz] = _map[(_width / 2) + _width * x + ix,
                                    (_height / 2) + _height * y + iy,
                                    (_depth / 2) + _depth * z + iz];
                                }

                            }
                        }

                        _generators[x, y, z].SetMap(mapToConnect);
                    }
                }
                else
                {

                    Cell[,,] mapToConnect = new Cell[_width, _height, 1];
                    for (int ix = 0; ix < _width; ix++)
                    {
                        for (int iy = 0; iy < _height; iy++)
                        {

                            mapToConnect[ix, iy, 0] = _map[(_width / 2)+_width*x + ix, (_height / 2)+_height*y + iy,
                            0];


                        }
                    }

                    _generators[x, y, 0].SetMap(mapToConnect);
                }
            }
        }
        ExamineMap();
        if (_is3D)
        {
            ThreadFct3DExamine();
        }
        else
        {
          ThreadFct2DExamine();
        }
    }


    protected override void ClearMap()
    {

    }

    protected override void RandomFillMap()
    {

    }

    protected override void IterateStatesOnce()
    {

    }

    protected override void IterateStates()
    {

    }

    protected override List<List<Coord>> GetRegionsOfState(States state)
    {
        return null;
    }

    public override void ExamineMap()
    {
        if (_is3D)
        {
            for (int x = 0; x < _chunkDimensions.xCoord - 1; x++)
            {
                for (int y = 0; y < _chunkDimensions.yCoord - 1; y++)
                {
                    for (int z = 0; z < _chunkDimensions.zCoord - 1; z++)
                    {
                        Vector3Int indexVector = new Vector3Int(x, y, z);
                        _threads[x, y, z] = (new Thread(new ThreadStart(
                            () => { _generators[indexVector.x, indexVector.y, indexVector.z].ExamineMap(); })));

                    }
                }
            }

        }
        else
        {
            for (int x = 0; x < _chunkDimensions.xCoord-1; x++)
            {
                for (int y = 0; y < _chunkDimensions.yCoord-1; y++)
                {
                   
                    Vector3Int indexVector = new Vector3Int(x, y, 0);
                    _threads[x, y, 0] = (new Thread(new ThreadStart(
                        () => { _generators[indexVector.x, indexVector.y, 0].ExamineMap(); })));


                }
            }

        }
    }

    protected override Corridor CreateCorridor(Room firstRoom, Room secondRoom, Coord firstRoomCell, Coord secondRoomCell)
    {
        return null;
    }

    protected override List<Corridor> ConnectClosestRooms(List<Room> roomsToConnect)
    {
        return null;
    }

    public override void UpdateCubes()
    {
        //foreach (var generator in _generators)
        //{
        //    generator.UpdateCubes();
        //}
        //Debug.Log("updateCubes");
        if (_visualize)
        {
            if (_generateMesh)
            {
                MeshGenerator meshGenerator = GetComponent<MeshGenerator>();
                if (meshGenerator)
                    meshGenerator.GenerateMesh(_map, _meshSizeModifier);
            }
            else
            {

                UpdateBigMapCubes();
            }
        }
    }

    private void UpdateBigMapCubes()
    {
        GameObject chunk = new GameObject("BigMap");
        if (_is3D)
        {

            for (int x = 0; x < _map.GetLength(0); x++)
            {
                for (int y = 0; y < _map.GetLength(1); y++)
                {
                    for (int z = 0; z < _map.GetLength(2); z++)
                    {
                        //if (_map[x, y, z].mesh)
                        //{
                        //    MeshRenderer renderer = _map[x, y, z].mesh.GetComponent<MeshRenderer>();
                        //    if (renderer)
                        //    {
                        //        if (_map[x, y, z].color == new Color())
                        //        {
                        //            renderer.material.color = (_map[x, y, z].state == States.Wall) ? Color.black : Color.white;
                        //        }
                        //        else
                        //        {

                        //            renderer.material.color = _map[x, y, z].color;
                        //        }
                        //    }

                        //    if ((!_showWalls && _map[x, y, z].state == States.Wall) ||
                        //        (!_showEmpty && _map[x, y, z].state == States.Empty) || (_shellEmpty && _map[x, y, z].neighbourCount == 26)
                        //        || (!_showShell &&
                        //            (x == 0 || x == _width - 1 || y == 0 || y == _height - 1 || z == 0 || z == _depth - 1)))
                        //        _map[x, y, z].mesh.gameObject.SetActive(false);
                        //    else
                        //        _map[x, y, z].mesh.gameObject.SetActive(true);
                        //}
                        //else
                        {
                            Vector3 pos = new Vector3(
                                -(_width * _chunkDimensions.xCoord) / 2 + x +
                                (_width * _chunkDimensions.xCoord * _offset.x),
                                -(_height * _chunkDimensions.yCoord) / 2 + y +
                                (_height * _chunkDimensions.yCoord * _offset.y),
                                -(_depth * _chunkDimensions.zCoord) / 2 + z +
                                (_depth * _chunkDimensions.zCoord * _offset.z));
                            //_map3D[x, y, z].mesh = Instantiate(_cube,pos,Quaternion.identity);
                            _map[x, y, z].mesh = Instantiate(_cube, pos, Quaternion.identity, chunk.transform);
                            //_map3D[x, y, z].mesh.transform.position = pos;

                            MeshRenderer renderer = _map[x, y, z].mesh.GetComponent<MeshRenderer>();
                            if (renderer)
                            {
                                if (_map[x, y, z].color == new Color())
                                {
                                    renderer.material.color =
                                        (_map[x, y, z].state == States.Wall) ? Color.black : Color.white;
                                }
                                else
                                {

                                    renderer.material.color = _map[x, y, z].color;
                                }
                            }

                            if ((!_showWalls && _map[x, y, z].state == States.Wall) ||
                                (!_showEmpty && _map[x, y, z].state == States.Empty) ||
                                (_shellEmpty && _map[x, y, z].neighbourCount == 26) || (!_showShell &&
                                (x == 0 || x == _width - 1 || y == 0 || y == _height - 1 || z == 0 || z == _depth - 1)))
                                _map[x, y, z].mesh.gameObject.SetActive(false);
                            else
                                _map[x, y, z].mesh.gameObject.SetActive(true);
                        }


                    }
                }
            }
        }
        else
        {
            for (int x = 0; x < _map.GetLength(0); x++)
            {
                for (int y = 0; y < _map.GetLength(1); y++)
                {

                    //if (_map[x, y,0].mesh)
                    //{
                    //    MeshRenderer renderer = _map[x, y,0].mesh.GetComponent<MeshRenderer>();
                    //    if (renderer)
                    //        renderer.material.color =
                    //            (_map[x, y,0].state == States.Wall) ? Color.black : Color.white;
                    //    if ((!_showWalls && _map[x, y,0].state == States.Wall) ||
                    //        (!_showEmpty && _map[x, y,0].state == States.Empty) || (_shellEmpty && _map[x, y,0].neighbourCount == 8) || (!_showShell &&
                    //                                                              (x == 0 || x == _width - 1 || y == 0 ||
                    //                                                               y == _height - 1)))
                    //        _map[x, y,0].mesh.gameObject.SetActive(false);
                    //    else
                    //        _map[x, y,0].mesh.gameObject.SetActive(true);
                    //}
                    //else
                    {
                        Vector3 pos = new Vector3(-_width / 2 + x + 0.5f + (_width * _offset.x), -_height / 2 + y + 0.5f + (_height * _offset.y),
                            0);
                        _map[x, y,0].mesh = Instantiate(_cube);
                        _map[x, y,0].mesh.transform.position = pos;

                        MeshRenderer renderer = _map[x, y,0].mesh.GetComponent<MeshRenderer>();
                        if (renderer)
                        {
                            if (_colorRegions)
                                renderer.material.color =
                                (_map[x, y,0].state == States.Wall) ? Color.black : _map[x, y,0].color;
                            else
                                renderer.material.color =
                                (_map[x, y,0].state == States.Wall) ? Color.black : Color.white;
                        }

                        if ((!_showWalls && _map[x, y,0].state == States.Wall) ||
                            (!_showEmpty && _map[x, y,0].state == States.Empty) || (_shellEmpty && _map[x, y,0].neighbourCount == 8) || (!_showShell &&
                                                                                  (x == 0 || x == _width - 1 || y == 0 ||
                                                                                   y == _height - 1)))
                            _map[x, y,0].mesh.gameObject.SetActive(false);
                        else
                            _map[x, y,0].mesh.gameObject.SetActive(true);
                    }

                }
            }
        }

    }

    protected override List<Corridor> DigCorridors(List<Room> rooms, List<Corridor> corridors)
    {
        return null;
    }

    //protected new void Start()
    //{
    //    if (_is3D)
    //    {

    //        _generator=gameObject.AddComponent<MapGenerator3D>();
    //        Assign(ref _generator,this);
    //        MapGenerator3D temp = _generator as MapGenerator3D;
    //        if (temp != null)
    //        {
    //            temp.Depth = _depth;
    //            temp.GenerateMesh = _generateMesh;
    //        }
    //    }
    //    else
    //    {
    //        _generator = gameObject.AddComponent<MapGenerator2D>();
    //        Assign(ref _generator,this);
    //        ((MapGenerator2D)_generator).ClearCubes=false;
    //        ((MapGenerator2D)_generator).InitSurrMap();

    //    }
    //    GenerateMap();
    //}


    //public override void GenerateMap()
    //{
    //    _generator.GenerateMap();
    //    if (_is3D)
    //    {
    //        Chunk temp= new Chunk((_generator as MapGenerator3D)?.GetMap,0,0);

    //    }
    //    else
    //    {
    //        ((MapGenerator2D)_generator).UseNeigbours = true;
    //        counter++;
    //        if (counter < 2)
    //            ((MapGenerator2D)_generator).InitSurrMap();
    //        Chunk firstChunk = new Chunk(((MapGenerator2D)_generator).GetMap, 0, 0);
    //        _map.Add(firstChunk);

    //        while (counter <= _chunkDimensions.xCoord + _chunkDimensions.yCoord-1 )
    //        {

    //            counter++;
    //            for (int i = 0; i < counter*4; i++)
    //            {
    //                Vector3Int pos = ChunkMap.Generate2DMapAsync(counter, i);

    //                if (Math.Abs(pos.x)+ _chunkDimensions.xCoord / 2.0f < _chunkDimensions.xCoord&& Math.Abs(pos.y) + _chunkDimensions.yCoord / 2.0f < _chunkDimensions.yCoord)
    //                {
    //                    CalcSurroundingMap(pos,i);
    //                }

    //                if (_chunkDimensions.xCoord % 2 == 0)
    //                {

    //                    if ((pos.x > 0 &&
    //                         Math.Abs(pos.x) + _chunkDimensions.xCoord / 2 == _chunkDimensions.xCoord) &&
    //                        (Math.Abs(pos.y) + _chunkDimensions.yCoord / 2.0f < _chunkDimensions.yCoord))
    //                    {
    //                        pos.x += 1;
    //                        CalcSurroundingMap(pos,i);
    //                    }
    //                    }
    //                    else if ((pos.x < 0 &&
    //                              Math.Abs(pos.x) + _chunkDimensions.xCoord / 2 == _chunkDimensions.xCoord) &&
    //                             (pos.y < 0 && Math.Abs(pos.y) + _chunkDimensions.yCoord / 2 ==
    //                             _chunkDimensions.yCoord))
    //                    {

    //                        CalcSurroundingMap(pos);
    //                    }                        
    //                    else if (_chunkDimensions.yCoord % 2 == 0&& (pos.x > 0 && Math.Abs(pos.x) + _chunkDimensions.xCoord / 2 == _chunkDimensions.xCoord) && (pos.y > 0 && Math.Abs(pos.y) + _chunkDimensions.yCoord / 2 == _chunkDimensions.yCoord))
    //                    {

    //                        CalcSurroundingMap(pos,i);
    //                    }
    //                }
    //                if(_chunkDimensions.yCoord % 2 == 0)

    //                {
    //                    if ((pos.y > 0 && Math.Abs(pos.y) + _chunkDimensions.yCoord / 2 ==
    //                        _chunkDimensions.yCoord) && (Math.Abs(pos.x) + _chunkDimensions.xCoord / 2.0f <
    //                                                     _chunkDimensions.xCoord))
    //                    {

    //                        CalcSurroundingMap(pos,i);
    //                    }

    //                    else if ((pos.x < 0 &&
    //                              Math.Abs(pos.x) + _chunkDimensions.xCoord / 2 == _chunkDimensions.xCoord) &&
    //                             (pos.y < 0 && Math.Abs(pos.y) + _chunkDimensions.yCoord / 2 ==
    //                             _chunkDimensions.yCoord))
    //                    {

    //                        CalcSurroundingMap(pos);
    //                    }
    //                }
    //            }

    //        }


    //    }

    //}

    //private void CalcSurroundingMap(Vector3Int pos, int creationIndex)
    //{
    //    _generator.Offset = pos;
    //    var generator = (_generator as MapGenerator2D);

    //    Chunk nextChunk = new Chunk(((MapGenerator2D)_generator).GetMap, creationIndex, counter);
    //    List<int> neighbouringIndices = nextChunk.GetNeighbouringIndices();
    //    ((MapGenerator2D)_generator).InitSurrMap();
    //    Cell[,] surroundingCells = generator._mapSurr;
    //    for (int i = 0; i < neighbouringIndices.Count; i++)
    //    {
    //        Chunk temp = _map.Get(neighbouringIndices[i]);
    //        if (temp != null)
    //        {
    //            int maxValue = 0;

    //            switch (i)
    //            {
    //                case 0:
    //                    maxValue = nextChunk.GetMap.GetLength(0) - 1;

    //                    for (int y = 0; y < temp.GetMap.GetLength(1); y++)
    //                    {
    //                        surroundingCells[maxValue, y + 1].state = temp.GetMap[0, y].state;

    //                    }
    //                    break;
    //                case 1:
    //                    maxValue = nextChunk.GetMap.GetLength(1) - 1;
    //                    for (int x = 0; x < temp.GetMap.GetLength(0); x++)
    //                    {
    //                        surroundingCells[x + 1, maxValue].state = temp.GetMap[x, 0].state;

    //                    }
    //                    break;
    //                case 2:
    //                    maxValue = temp.GetMap.GetLength(0) - 1;
    //                    for (int y = 0; y < temp.GetMap.GetLength(1); y++)
    //                    {
    //                        surroundingCells[0, y + 1].state = temp.GetMap[maxValue, y].state;
    //                        Debug.Log(surroundingCells[0, y].state);
    //                    }
    //                    break;
    //                case 3:
    //                    ;
    //                    maxValue = temp.GetMap.GetLength(1) - 1;
    //                    for (int x = 0; x < temp.GetMap.GetLength(0); x++)
    //                    {
    //                        surroundingCells[x + 1, 0].state = temp.GetMap[x, maxValue].state;

    //                    }
    //                    break;

    //                case 4:
    //                    surroundingCells[nextChunk.GetMap.GetLength(0) - 1, nextChunk.GetMap.GetLength(1) - 1].state = temp.GetMap[0, 0].state;
    //                    break;
    //                case 5:
    //                    surroundingCells[0, nextChunk.GetMap.GetLength(1) - 1].state = temp.GetMap[generator.GetMap.GetLength(0) - 1, 0].state;
    //                    break;
    //                case 6:
    //                    surroundingCells[0, 0].state = generator.GetMap[generator.GetMap.GetLength(0) - 1, temp.GetMap.GetLength(1) - 1].state;
    //                    break;
    //                case 7:
    //                    surroundingCells[nextChunk.GetMap.GetLength(0) - 1, 0].state = generator.GetMap[0, temp.GetMap.GetLength(1) - 1].state;
    //                    break;
    //            }

    //        }
    //    }


    //    generator._mapSurr = surroundingCells;
    //    _generator.GenerateMap();
    //    generator.UpdateCubesDebug();
    //    nextChunk.GetMap = generator.GetMap;
    //    _map.Add(nextChunk);
    //}
}
