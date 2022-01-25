using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

public abstract class MapGenerator : MonoBehaviour
{
    [Header("Layout variables")] [SerializeField]
    protected bool _makeEdgesWalls = true;

    [SerializeField] protected bool _useRandomSeed;
    [SerializeField] protected string _seed;
    [SerializeField] [Range(0, 1)] protected float _randomFillPercent = 0.5f;
    [SerializeField] [Range(0, 100)] protected int _sizeThreshold = 5;
    [SerializeField] [Range(0, 100)] protected int _iterations = 5;
    [SerializeField] [Range(0, 30)] protected int _neighbourWallCountToChange = 4;
    [SerializeField] [Range(0, 30)] protected int _neighbourEmptyCountToChange = 4;

    [Space(20)] [Header("Visualization variables")] [SerializeField]
    protected bool _colorRegions = true;

    [SerializeField] protected GameObject _cube = null;
    [SerializeField] protected bool _showWalls = true;
    [SerializeField] protected bool _showEmpty = true;
    [SerializeField] protected bool _showShell = true;
    [SerializeField] protected bool _shellEmpty = true;
    [SerializeField] protected bool _visualize = true;
    [SerializeField] protected bool _makeShellSameAsNeighbour = true;

    [Space(20)] [Header("Shortest connections")] [Header("Connection variables")] [SerializeField]
    protected bool _connectClosestRooms = true;

    [SerializeField] protected bool _forceAccessibilityFromMainRoom = true;
    [SerializeField] protected Vector2Int _corridorRadius = new Vector2Int(1, 1);

    [Header("Digger connections")] [SerializeField]
    protected bool _useDigger = false;

    [SerializeField] [Range(0, 100)] protected int _breakOutValue = 1;
    [SerializeField] [Range(0, 10)] protected int _connectionMax = 1;
    [SerializeField] [Range(1, 10)] protected int _corridorSpacing = 1;
    [SerializeField] protected Vector2Int _corridorLengthMinMax = new Vector2Int(1, 10);
    [SerializeField] [Range(1, 10)] protected int _turnCount = 0;
    [SerializeField] [Range(0, 1)] protected float _corridorFromRoomChance = 0.7f;
    [SerializeField] protected bool _createDeadEnds = false;
    [SerializeField] [Range(0f, 1f)] protected float _roomChance = 0.1f;



    [Space(20)] [Header("Dimension variables")] [SerializeField] [Range(1, 50)]
    protected int _width = 50;

    [SerializeField] [Range(1, 50)] protected int _height = 50;

    protected System.Random _randomNumberGenerator = null;

    protected Vector3Int _offset=new Vector3Int();

    public Vector3Int Offset
    {
        get => _offset;
        set => _offset = value;
    }

    protected MapGenerator()
    {}

   // public MapGenerator(

   // //[Header("Layout variables")] [SerializeField]
   // bool makeEdgesWalls,
   // bool useRandomSeed,
   // string seed,
   // float randomFillPercent,
   // int sizeThreshold,
   // int iterations,
   // int neighbourWallCountToChange,
   // int neighbourEmptyCountToChange,

   // //[Space(20)] [Header("Visualization variables")] [SerializeField]
   // bool colorRegions,
   // GameObject cube,
   // bool showWalls,
   // bool showEmpty,
   // bool showShell,
   // bool shellEmpty,
   // bool makeShellSameAsNeighbour,

   // //[Space(20)] [Header("Shortest connections")] [Header("Connection variables")] [SerializeField]
   // bool connectClosestRooms,
   // bool forceAccessibilityFromMainRoom,
   // Vector2Int corridorRadius,

   //// [Header("Digger connections")] [SerializeField]
   // bool useDigger,
   // int breakOutValue,
   // int connectionMax,
   // int corridorSpacing,
   // Vector2Int corridorLengthMinMax,
   // int turnCount,
   // float corridorFromRoomChance,
   // bool createDeadEnds,

   // //[Header("Digger connections")] [SerializeField]
   // int width,
   // int height)


   // {
   //     _makeEdgesWalls = makeEdgesWalls;
   //     _useRandomSeed = useRandomSeed;
   //     _seed = seed;
   //     _randomFillPercent = randomFillPercent;
   //     _sizeThreshold = sizeThreshold;
   //     _iterations = iterations;
   //     _neighbourWallCountToChange = neighbourWallCountToChange;
   //     _neighbourEmptyCountToChange = neighbourEmptyCountToChange;

   //     _colorRegions = colorRegions;
   //     _cube = cube;
   //     _showWalls = showWalls;
   //     _showEmpty = showEmpty;
   //     _showShell = showShell;
   //     _shellEmpty = shellEmpty;
   //     _makeShellSameAsNeighbour = makeShellSameAsNeighbour;

   //     _connectClosestRooms = connectClosestRooms;
   //     _forceAccessibilityFromMainRoom = forceAccessibilityFromMainRoom;
   //     _corridorRadius = corridorRadius;


   //     _useDigger = useDigger;
   //     _breakOutValue = breakOutValue;
   //     _connectionMax = connectionMax;
   //     _corridorSpacing = corridorSpacing;
   //     _corridorLengthMinMax = corridorLengthMinMax;
   //     _turnCount = turnCount;
   //     _corridorFromRoomChance = corridorFromRoomChance;
   //     _createDeadEnds = createDeadEnds;

   //     _width = width;
   //     _height = height;
   // }

    protected static void Assign(ref MapGenerator dst, in MapGenerator src)
    {
        dst._makeEdgesWalls = src._makeEdgesWalls;
        dst._useRandomSeed = src._useRandomSeed;
        dst._seed = src._seed;
        dst._randomFillPercent = src._randomFillPercent;
        dst._sizeThreshold = src._sizeThreshold;
        dst._iterations = src._iterations;
        dst._neighbourWallCountToChange = src._neighbourWallCountToChange;
        dst._neighbourEmptyCountToChange = src._neighbourEmptyCountToChange;

        dst._colorRegions = src._colorRegions;
        dst._cube = src._cube;
        dst._showWalls = src._showWalls;
        dst._showEmpty = src._showEmpty;
        dst._showShell = src._showShell;
        dst._shellEmpty = src._shellEmpty;
        dst._visualize = src._visualize;
        dst._makeShellSameAsNeighbour = src._makeShellSameAsNeighbour;

        dst._connectClosestRooms = src._connectClosestRooms;
        dst._forceAccessibilityFromMainRoom = src._forceAccessibilityFromMainRoom;
        dst._corridorRadius = src._corridorRadius;

        dst._useDigger = src._useDigger;
        dst._roomChance = src._roomChance;
        dst._breakOutValue = src._breakOutValue;
        dst._connectionMax = src._connectionMax;
        dst._corridorSpacing = src._corridorSpacing;
        dst._corridorLengthMinMax = src._corridorLengthMinMax;
        dst._turnCount = src._turnCount;
        dst._corridorFromRoomChance = src._corridorFromRoomChance;
        dst._createDeadEnds = src._createDeadEnds;
     
        dst._width = src._width;
        dst._height = src._height;
    }

    // Start is called before the first frame update
    //protected void Start()
    //{
    //    GenerateMap();
    //}


    protected void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
        //    GenerateMap();
        //}
        //else if (Input.GetMouseButtonDown(1))
        //{

        //    IterateStatesOnce();
        //    UpdateCubes();
        //}
    }
    public abstract Cell[,,] GetMap();
    public abstract void SetMap(Cell[,,] map);
    public abstract void GenerateMap();
    public async virtual Task<int> GenerateMapAsync(int index)
    {
        return index;
    }
    protected abstract void ClearMap();
    protected abstract void RandomFillMap();
    protected abstract void IterateStatesOnce();
    protected abstract void IterateStates();
    protected abstract List<List<Coord>> GetRegionsOfState(States state);
    public abstract void ExamineMap();
    protected abstract Corridor CreateCorridor(Room firstRoom, Room secondRoom, Coord firstRoomCell, Coord secondRoomCell);
    protected abstract List<Corridor> ConnectClosestRooms(List<Room> roomsToConnect);
    public abstract void UpdateCubes();
    protected abstract List<Corridor> DigCorridors(List<Room> rooms,List<Corridor> corridors);
    
}
