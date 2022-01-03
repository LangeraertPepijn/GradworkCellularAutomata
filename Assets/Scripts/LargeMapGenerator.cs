using System.Collections;
using System.Collections.Generic;
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
    private MapGenerator _generator;

    private static int counter = 0;

    private ChunkMap _map= new ChunkMap();
    protected new void Start()
    {
        if (_is3D)
        {
          
            _generator=gameObject.AddComponent<MapGenerator3D>();
            Assign(ref _generator,this);
            MapGenerator3D temp = _generator as MapGenerator3D;
            if (temp != null)
            {
                temp.Depth = _depth;
                temp.GenerateMesh = _generateMesh;
            }
        }
        else
        {
            _generator = gameObject.AddComponent<MapGenerator2D>();
            Assign(ref _generator,this);
            ((MapGenerator2D)_generator).ClearCubes=false;
            //((MapGenerator2D)_generator).InitSurrMap();

        }
        GenerateMap();
    }


    public override void GenerateMap()
    {
        _generator.GenerateMap();
        if (_is3D)
        {
            Chunk temp= new Chunk((_generator as MapGenerator3D)?.Map,0,0);

        }
        else
        {
            ((MapGenerator2D)_generator).UseNeigbours = true;
            counter++;
            if (counter < 2)
                ((MapGenerator2D)_generator).InitSurrMap();

            Chunk firstChunk = new Chunk(((MapGenerator2D)_generator).Map, 0, 0);
            _map.Add(firstChunk);
            _generator.Offset = new Vector3Int(counter*2, 0, 0);
            var generator = (_generator as MapGenerator2D);
            Cell[,] surroundingCells = generator._mapSurr;

            Chunk nextChunk = new Chunk(((MapGenerator2D)_generator).Map, 0, counter);
            List<int>neighbouringIndices =nextChunk.GetNeighbouringIndices();
            for (int i = 0; i < neighbouringIndices.Count; i++)
            {
                Chunk temp = _map.Get(neighbouringIndices[i]);
                if (temp != null)
                {
                    int maxValue = 0;
                    
                    switch (i)
                    {
                        case 0:
                            maxValue = nextChunk.Map.GetLength(0)-1;
                            
                            for (int y = 0; y < temp.Map.GetLength(1); y++)
                            {
                                surroundingCells[maxValue, y+1].state = generator.Map[0, y].state;

                            }
                            break;
                        case 1:
                            maxValue = nextChunk.Map.GetLength(1)-1;
                            for (int x = 0; x < temp.Map.GetLength(0); x++)
                            {
                                surroundingCells[x+1, maxValue].state = generator.Map[x, 0].state;

                            }
                            break;
                        case 2:
                            maxValue = generator.Map.GetLength(0)-1;
                            for (int y = 0; y < temp.Map.GetLength(1); y++)
                            {
                                surroundingCells[0, y+1].state = generator.Map[maxValue, y].state;
                                Debug.Log(surroundingCells[0, y].state);
                            }
                            break;
                        case 3: ;
                            maxValue = generator.Map.GetLength(1)-1;
                            for (int x = 0; x < temp.Map.GetLength(0); x++)
                            {
                                surroundingCells[x+1, 0].state = generator.Map[x, maxValue].state;

                            }
                            break;

                        case 4:
                            surroundingCells[nextChunk.Map.GetLength(0)-1, nextChunk.Map.GetLength(1)-1].state = generator.Map[0, 0].state;
                            break;
                        case 5:
                            surroundingCells[0, nextChunk.Map.GetLength(1)-1].state = generator.Map[generator.Map.GetLength(0)-1, 0].state;
                            break;
                        case 6:
                            surroundingCells[0, 0].state = generator.Map[generator.Map.GetLength(0)-1, generator.Map.GetLength(1)-1].state;
                            break;
                        case 7:
                            surroundingCells[nextChunk.Map.GetLength(0)-1, 0].state = generator.Map[0, generator.Map.GetLength(1) - 1].state;
                            break;
                    }

                }
            }

            //for (int x = 0; x < test?.Map.GetLength(0); x++)
            //{
            //    for (int y = 0; y < test?.Map.GetLength(1); y++)
            //    {
            //        if (x == test?.Map.GetLength(0) - 1)
            //            tempoCells[x + 1, y] = test.Map[x, y];
            //        if (x == test?.Map.GetLength(0) - 1)
            //            tempoCells[x + 1, y] = test.Map[x, y];
            //        if (x == test?.Map.GetLength(0) - 1)
            //            tempoCells[x + 1, y] = test.Map[x, y];
            //        if (x == test?.Map.GetLength(0) - 1)
            //            tempoCells[x + 1, y] = test.Map[x, y];
            //    }
            //}
            generator._mapSurr = surroundingCells;
            generator.UpdateCubesDebug();
            if (counter < 2)
            {
                GenerateMap();

            }

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
}
