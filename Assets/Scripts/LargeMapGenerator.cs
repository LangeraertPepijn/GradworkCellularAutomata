using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LargeMapGenerator : MapGenerator
{

    [SerializeField] [Range(1, 50)] private int _depth = 50;
    [Space(10)]
    [Header("3D unique variables")]
    [SerializeField] private bool _generateMesh = true;
    [Header("Map Type")]
    [SerializeField]
    private bool _is3D = false;
    [SerializeField] private Coord _chunkDimensions = new Coord(1,1,1);
    private List<MapGenerator> _generators=new List<MapGenerator>();

    private int _asyncCounter = 0;
    private const int _asyncCount = 5;


    //private ChunkMap _map= new ChunkMap();

    private void Start()
    {
        if (_is3D)
        {
            for (int i = 0; i < _asyncCount; i++)
            {

                MapGenerator generator = gameObject.AddComponent<MapGenerator3D>();
                Assign(ref generator, this);

                MapGenerator3D temp = generator as MapGenerator3D;
                if (temp != null)
                {
                    temp.Depth = _depth;
                    temp.GenerateMesh = _generateMesh;
                }
                _generators.Add(generator);
            }
        }
        else
        {
            for (int i = 0; i < _asyncCount; i++)
            {
                MapGenerator generator = gameObject.AddComponent<MapGenerator2D>();
                Assign(ref generator, this);
                _generators.Add(generator);
            }

        }
        GenerateMap();
    }


    public async void Test()
    {
        //int availableGen = 1000;
        
        //List<Task<int>> generations = new List<Task<int>>(_asyncCounter);
        //for (int x = 0; x < _chunkDimensions.xCoord; x++)
        //{
        //    for (int y = 0; y < _chunkDimensions.yCoord; y++)
        //    {
        //        if (_asyncCount > _asyncCounter)
        //        {
        //            _generators[_asyncCounter].Offset = new Vector3Int(x, y, 0);
        //            generations.Add(_generators[_asyncCounter].GenerateMapAsync(_asyncCounter));
        //            ++_asyncCounter;
        //        }

        //        if (generations.Count == _asyncCount)
        //        {
        //            if (availableGen < _asyncCounter)
        //            {
        //                _generators[availableGen].Offset = new Vector3Int(x, y, 0);
        //                generations.Add( _generators[availableGen].GenerateMapAsync(availableGen));
        //                availableGen = 1000;
        //                ++_asyncCounter;
        //            }
        //            Task<int> doneTask = await Task.WhenAny(generations);
        //            generations.Remove(doneTask);
        //            availableGen = doneTask.Result;
        //            --_asyncCounter;
        //            Debug.Log("done 1"+x+"  "+ y);
        //        }
        //    }
        //}


        for (int x = 0; x < _chunkDimensions.xCoord; x++)
        {
            for (int y = 0; y < _chunkDimensions.yCoord; y++)
            {
                _generators[0].Offset = new Vector3Int(x, y, 0);
                _generators[0].GenerateMap();
            }
        }
        //await Task.WhenAll(generations);
    }
    public override void GenerateMap()
    {
        Test();
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

    protected override void ExamineMap()
    {

    }

    protected override Corridor CreateCorridor(Room firstRoom, Room secondRoom, Coord firstRoomCell, Coord secondRoomCell)
    {
        return null;
    }

    protected override List<Corridor> ConnectClosestRooms(List<Room> roomsToConnect)
    {
        return null;
    }

    protected override void UpdateCubes()
    {
     
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
    //        Chunk temp= new Chunk((_generator as MapGenerator3D)?.Map,0,0);

    //    }
    //    else
    //    {
    //        ((MapGenerator2D)_generator).UseNeigbours = true;
    //        counter++;
    //        if (counter < 2)
    //            ((MapGenerator2D)_generator).InitSurrMap();
    //        Chunk firstChunk = new Chunk(((MapGenerator2D)_generator).Map, 0, 0);
    //        _map.Add(firstChunk);

    //        while (counter <= _chunkDimensions.xCoord + _chunkDimensions.yCoord-1 )
    //        {

    //            counter++;
    //            for (int i = 0; i < counter*4; i++)
    //            {
    //                Vector3Int pos = ChunkMap.Test(counter, i);

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

    //    Chunk nextChunk = new Chunk(((MapGenerator2D)_generator).Map, creationIndex, counter);
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
    //                    maxValue = nextChunk.Map.GetLength(0) - 1;

    //                    for (int y = 0; y < temp.Map.GetLength(1); y++)
    //                    {
    //                        surroundingCells[maxValue, y + 1].state = temp.Map[0, y].state;

    //                    }
    //                    break;
    //                case 1:
    //                    maxValue = nextChunk.Map.GetLength(1) - 1;
    //                    for (int x = 0; x < temp.Map.GetLength(0); x++)
    //                    {
    //                        surroundingCells[x + 1, maxValue].state = temp.Map[x, 0].state;

    //                    }
    //                    break;
    //                case 2:
    //                    maxValue = temp.Map.GetLength(0) - 1;
    //                    for (int y = 0; y < temp.Map.GetLength(1); y++)
    //                    {
    //                        surroundingCells[0, y + 1].state = temp.Map[maxValue, y].state;
    //                        Debug.Log(surroundingCells[0, y].state);
    //                    }
    //                    break;
    //                case 3:
    //                    ;
    //                    maxValue = temp.Map.GetLength(1) - 1;
    //                    for (int x = 0; x < temp.Map.GetLength(0); x++)
    //                    {
    //                        surroundingCells[x + 1, 0].state = temp.Map[x, maxValue].state;

    //                    }
    //                    break;

    //                case 4:
    //                    surroundingCells[nextChunk.Map.GetLength(0) - 1, nextChunk.Map.GetLength(1) - 1].state = temp.Map[0, 0].state;
    //                    break;
    //                case 5:
    //                    surroundingCells[0, nextChunk.Map.GetLength(1) - 1].state = temp.Map[generator.Map.GetLength(0) - 1, 0].state;
    //                    break;
    //                case 6:
    //                    surroundingCells[0, 0].state = generator.Map[generator.Map.GetLength(0) - 1, temp.Map.GetLength(1) - 1].state;
    //                    break;
    //                case 7:
    //                    surroundingCells[nextChunk.Map.GetLength(0) - 1, 0].state = generator.Map[0, temp.Map.GetLength(1) - 1].state;
    //                    break;
    //            }

    //        }
    //    }


    //    generator._mapSurr = surroundingCells;
    //    _generator.GenerateMap();
    //    generator.UpdateCubesDebug();
    //    nextChunk.Map = generator.Map;
    //    _map.Add(nextChunk);
    //}
}
