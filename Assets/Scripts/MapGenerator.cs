using System.Collections.Generic;
using UnityEngine;

public abstract class MapGenerator : MonoBehaviour
{
    [SerializeField] protected GameObject _cube = null;
    [SerializeField] protected string _randomSeed;
    [SerializeField] protected bool _useRandomSeed;
    [SerializeField] protected bool _MakeEdgesWalls = true;
    [SerializeField] [Range(0, 1)] protected float _randomFillPercent = 0.5f;
    [SerializeField] [Range(0, 100)] protected int _sizeThreshold = 5;
    [SerializeField] [Range(0, 20)] protected int _iterations = 5;
    [SerializeField] [Range(0, 30)] protected int _neighbourWallCountToChange = 4;
    [SerializeField] [Range(0, 30)] protected int _neighbourEmptyCountToChange = 4;
    [SerializeField] protected bool _showWalls = true;
    [SerializeField] protected bool _showEmpty = true;
    [SerializeField] protected bool _showShell = true;
    [SerializeField] protected bool _shellEmpty = true;

    [SerializeField] [Range(1, 50)] protected int _width = 50;
    [SerializeField] [Range(1, 50)] protected int _height = 50;

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
    protected abstract void CreateCorridor(Room firstRoom, Room secondRoom, Coord firstRoomCell, Coord secondRoomCell);
    protected abstract void ConnectClosestRooms(List<Room> roomsToConnect);
    protected abstract void UpdateCubes();

}
