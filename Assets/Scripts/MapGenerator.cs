using System.Collections.Generic;
using UnityEngine;

public abstract class MapGenerator : MonoBehaviour
{
    [Header("Layout variables")]
    [SerializeField] protected bool _MakeEdgesWalls = true;
    [SerializeField] protected bool _useRandomSeed;
    [SerializeField] protected string _seed;
    [SerializeField] [Range(0, 1)] protected float _randomFillPercent = 0.5f;
    [SerializeField] [Range(0, 100)] protected int _sizeThreshold = 5;
    [SerializeField] [Range(0, 20)] protected int _iterations = 5;
    [SerializeField] [Range(0, 30)] protected int _neighbourWallCountToChange = 4;
    [SerializeField] [Range(0, 30)] protected int _neighbourEmptyCountToChange = 4;
    [Space(20)]
    [Header("Visualization variables")]
    [SerializeField] protected bool _colorRegions = true;
    [SerializeField] protected GameObject _cube = null;
    [SerializeField] protected bool _showWalls = true;
    [SerializeField] protected bool _showEmpty = true;
    [SerializeField] protected bool _showShell = true;
    [SerializeField] protected bool _shellEmpty = true;
    [SerializeField] protected bool _makeShellSameAsNeighbour = true;
    [Space(20)]
    [Header("Shortest connections")]
    [Header("Connection variables")]
    [SerializeField] protected bool _connectClosestRooms = true;
    [SerializeField] protected bool _forceAccessibilityFromMainRoom = true;
    [SerializeField] protected Vector2Int _corridorRadius = new Vector2Int(1,1);
    [Header("Digger connections")]
    [SerializeField] protected bool _useDigger = false;
    [SerializeField] [Range(0,100)]protected int _corridorCounter = 1;
    [SerializeField] [Range(1,10)]protected int _corridorSpacing = 1;
    [SerializeField] protected Vector2Int _corridorLengthMinMax = new Vector2Int(1,10);
    [SerializeField] [Range(1, 10)] protected int _turnCount  = 0;
    [SerializeField] [Range(0, 1)] protected float _corridorFromRoomChance = 0.7f;
    [SerializeField] protected bool _createDeadEnds = false;



    [Space(20)]
    [Header("Dimension variables")]
    [SerializeField] [Range(1, 50)] protected int _width = 50;
    [SerializeField] [Range(1, 50)] protected int _height = 50;

    protected System.Random _randomNumberGenerator = null;

    // Start is called before the first frame update
    protected void Start()
    {
        GenerateMap();
    }

    protected void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GenerateMap();
        }
        else if (Input.GetMouseButtonDown(1))
        {

            IterateStatesOnce();
            UpdateCubes();
        }
    }
    protected abstract void GenerateMap();
    protected abstract void ClearMap();
    protected abstract void RandomFillMap();
    protected abstract void IterateStatesOnce();
    protected abstract void IterateStates();
    protected abstract List<List<Coord>> GetRegionsOfState(States state);
    protected abstract void ExamineMap();
    protected abstract Corridor CreateCorridor(Room firstRoom, Room secondRoom, Coord firstRoomCell, Coord secondRoomCell);
    protected abstract List<Corridor> ConnectClosestRooms(List<Room> roomsToConnect);
    protected abstract void UpdateCubes();
    protected abstract List<Corridor> DigCorridors(List<Room> rooms,List<Corridor> corridors);



}
