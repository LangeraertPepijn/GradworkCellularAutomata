using UnityEngine;
using System;
using Random = UnityEngine.Random;


public class MapGenerator : MonoBehaviour
{

    public enum States : int
    {
        Empty = 0,
        Wall = 1
    }


    [SerializeField] private string _randomSeed;
    [SerializeField] private bool _useRandomSeed;
    [SerializeField] private bool _MakeEdgesWalls;
    [SerializeField] [Range(0,1)]private float _randomFillPercent = 0.5f;
    [SerializeField] private int _width=50;
    [SerializeField] private int _height=50;
    [SerializeField] private int _iterations =5;
    [SerializeField] private int _neighbourWallCountToChange = 4;

    private States[,] _map;
    private System.Random _randomNumberGenerator;

    // Start is called before the first frame update
    void Start()
    {
        GenerateMap();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Camera.main.transform.position = new  Vector3(0,(_width+_height)/2.0f,0);
            GenerateMap();
        }
    }

    // Generate the Cellular Automata Map
    void GenerateMap()
    {
        _map = new States[_width, _height];

        if (_useRandomSeed)
            _randomSeed = System.DateTime.Now.ToString();

        _randomNumberGenerator = new System.Random(_randomSeed.GetHashCode());

        RandomFillMap();
        IterateStates();
    }

    //check if the surrounding cells are walls
    int GetSurroundingWallCount(int indexX, int indexY)
    {
        int wallcount = 0;

        for (int neighbourX = indexX-1; neighbourX <= indexX+1; neighbourX++)
        {
            for (int neighbourY = indexY - 1; neighbourY <= indexY + 1; neighbourY++)
            {
                if (neighbourX >= 0 && neighbourX < _width && neighbourY >= 0 && neighbourY < _height)
                {
                    if (indexX != neighbourX || indexY != neighbourY)
                    {
                        wallcount += (int)_map[neighbourX, neighbourY];
                    }
                }
                else if(_MakeEdgesWalls)
                {
                    wallcount++;
                }
                else
                {
                    wallcount += (_randomNumberGenerator.Next(0, 100) / 100.0f < _randomFillPercent) ? 1 : 0;
                }
            }
        }

        return wallcount;
    }

    // Fill the map with random values
    void RandomFillMap()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (_MakeEdgesWalls && (x == 0 || x == _width - 1 || y == 0 || y == _height - 1))
                {
                    _map[x, y] = States.Wall;

                }
                else
                {
                    _map[x, y] = (_randomNumberGenerator.Next(0, 100) / 100.0f < _randomFillPercent) ? States.Wall : States.Empty;
                }

            }
        }
    }


    void IterateStates()
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
                        _map[x, y] = States.Wall;
                    }
                    else if (neighbourWallTiles < _neighbourWallCountToChange)
                    {
                        _map[x, y] = States.Empty;

                    }

                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (_map != null)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    Gizmos.color = (_map[x, y] == States.Empty) ? Color.white : Color.black;
                    Vector3 pos = new Vector3(-_width / 2 + x + 0.5f, 0, -_height / 2 + y + 0.5f);
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }
    }
}
