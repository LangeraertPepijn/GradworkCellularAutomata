using UnityEngine;
using System;
using System.Data.SqlTypes;
using JetBrains.Annotations;
using Random = UnityEngine.Random;


public class MapGenerator : MonoBehaviour
{

    public enum States : int
    {
        Empty = 0,
        Wall = 1
    }
   
    public struct Cell
    {
        public States state;
        public GameObject mesh;
    }

    [SerializeField] private GameObject _cube=null;
    [SerializeField] private string _randomSeed;
    [SerializeField] private bool _useRandomSeed;
    [SerializeField] private bool _MakeEdgesWalls;
    [SerializeField] [Range(0,1)]private float _randomFillPercent = 0.5f;
    [SerializeField] private int _width=50;
    [SerializeField] private int _height=50;
    [SerializeField] private int _depth=50;
    [SerializeField] private int _iterations =5;
    [SerializeField][Range(0,30)] private int _neighbourWallCountToChange = 4;
    [SerializeField] [Range(0,30)]private int _neighbourEmptyCountToChange = 4;

    [SerializeField] private bool _showSlice;
    [SerializeField] private bool _showWalls;
    [SerializeField] private bool _showEmpty;
    [SerializeField] private bool _showShell;
    [SerializeField] [Range(0, 100)] private int _sliceIndex=0;

    private Cell[,] _map;
    [CanBeNull] private Cell[,,] _map3D;
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
           // Camera.main.transform.position = new  Vector3(0,(_width+_height)/2.0f,0);
           // GenerateMap();
           GenerateMap3D();
        }
        else if (Input.GetMouseButtonDown(1))
        {

            IterateStatesOnce3D();
            UpdateCubes();
        }
    }

    // Generate the Cellular Automata Map
    void GenerateMap()
    {
        
        _map = new Cell[_width, _height];

        if (_useRandomSeed)
            _randomSeed = System.DateTime.Now.ToString();

        _randomNumberGenerator = new System.Random(_randomSeed.GetHashCode());

        RandomFillMap();
        IterateStates();

    }

    void UpdateCubes()
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
                                renderer.material.color =
                                    (_map3D[x, y, z].state == States.Wall) ? Color.black : Color.white;
                            if ((!_showWalls && _map3D[x, y, z].state == States.Wall)|| (!_showEmpty && _map3D[x, y, z].state == States.Empty) || (_showShell && (x == 0 || x == _width - 1 || y == 0 || y == _height - 1 || z == 0 || z == _depth - 1)))
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
                                renderer.material.color =
                                    (_map3D[x, y, z].state == States.Wall) ? Color.black : Color.white;

                            if( (!_showWalls && _map3D[x, y, z].state == States.Wall)|| (!_showEmpty && _map3D[x, y, z].state == States.Empty)|| (_showShell && (x == 0 || x == _width - 1 || y == 0 || y == _height - 1 || z == 0 || z == _depth - 1)))
                                _map3D[x, y, z].mesh.gameObject.SetActive(false);
                            else
                                _map3D[x, y, z].mesh.gameObject.SetActive(true);
                    }


                    }
                }
            }
        
    }

    void clearMap()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                for (int z = 0; z < _depth; z++)
                {
                    if(_map3D[x, y, z].mesh)
                        Destroy(_map3D[x,y,z].mesh);
                }
            }
        }
    }
    void GenerateMap3D()
    {
       
        if(_map3D!=null)
         clearMap();
        _map3D = new Cell[_width, _height,_depth];


        if (_useRandomSeed)
            _randomSeed = System.DateTime.Now.ToString();

        _randomNumberGenerator = new System.Random(_randomSeed.GetHashCode());

      

        RandomFillMap3D();
        IterateStates3D();
        UpdateCubes();
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
                        wallcount += (int)_map[neighbourX, neighbourY].state;
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
    int GetSurroundingWallCount3D(int indexX, int indexY,int indexZ)
    {
        int wallcount = 0;

        for (int neighbourX = indexX - 1; neighbourX <= indexX + 1; neighbourX++)
        {
            for (int neighbourY = indexY - 1; neighbourY <= indexY + 1; neighbourY++)
            {
                for (int neighbourZ = indexZ - 1; neighbourZ <= indexZ + 1; neighbourZ++)
                {
                    if (neighbourX >= 0 && neighbourX < _width && neighbourY >= 0 && neighbourY < _height&&neighbourZ>=0 &&neighbourZ<_depth)
                    {
                        if (neighbourX != indexX || neighbourY != indexY || neighbourZ != indexZ)
                        {
                            wallcount += (int)_map3D[neighbourX, neighbourY,neighbourZ].state;
                        }
                    }
                    else if (_MakeEdgesWalls)
                    {
                        wallcount++;
                    }
                    else
                    {
                        wallcount += (_randomNumberGenerator.Next(0, 100) / 100.0f < _randomFillPercent) ? 1 : 0;
                    }
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
                    _map[x, y].state = States.Wall;
                }
                else
                {
                    _map[x, y].state = (_randomNumberGenerator.Next(0, 100) / 100.0f < _randomFillPercent) ? States.Wall : States.Empty;
                }

            }
        }
    }

    void RandomFillMap3D()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                for (int z = 0; z < _depth; z++)
                {
                    if (_MakeEdgesWalls && (x == 0 || x == _width - 1 || y == 0 || y == _height - 1||z==0||z==_depth-1))
                    {
                        _map3D[x, y,z].state = States.Wall;

                    }
                    else
                    {
                        _map3D[x, y,z].state = (_randomNumberGenerator.Next(0, 100) / 100.0f < _randomFillPercent)
                            ? States.Wall
                            : States.Empty;
                    }
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
                        _map[x, y].state = States.Wall;
                    }
                    else if (8-neighbourWallTiles >_neighbourEmptyCountToChange)
                    {
                        _map[x, y].state = States.Empty;

                    }

                }
            }
        }
    }

    void IterateStatesOnce3D()
    {
        float w = 0;
        float e = 0;
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                for (int z = 0; z < _depth; z++)
                {

                    int neighbourWallTiles = GetSurroundingWallCount3D(x, y, z);
                    if (neighbourWallTiles > _neighbourWallCountToChange)
                    {
                        _map3D[x, y, z].state = States.Wall;
                        w++;
                    }
                    else if (28 - neighbourWallTiles > _neighbourEmptyCountToChange)
                    {
                        _map3D[x, y, z].state = States.Empty;
                        e++;

                    }
                }
            }
        }
        Debug.Log("The amount of walls changed is" +w +"the amount of empty added is" +e);

    }

    void IterateStates3D()
    {
        for (int i = 0; i < _iterations; i++)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    for (int z = 0; z < _depth; z++)
                    {

                        int neighbourWallTiles = GetSurroundingWallCount3D(x, y,z);
                        if (neighbourWallTiles > _neighbourWallCountToChange)
                        {
                            _map3D[x, y,z].state = States.Wall;
                        }
                        else if (28 - neighbourWallTiles > _neighbourEmptyCountToChange)
                        {
                            _map3D[x, y,z].state = States.Empty;

                        }
                    }
                }
            }
        }
    }

    //void OnDrawGizmos()
    //{
    //    if (_map3D != null)
    //    {
    //        for (int x = 0; x < _width; x++)
    //        {
    //            for (int y = 0; y < _height; y++)
    //            {
    //                for (int z = 0; z < _depth; z++)
    //                {
    //                    if (_showSlice&&y==_sliceIndex)
    //                    {
    //                        if(_showWalls)
    //                        Gizmos.color = (_map3D[x, y, z].state == States.Empty)
    //                            ? new Color(0, 1, 1, 0.7f)
    //                            : new Color(1, 0, 0, 0.1f);
    //                        else
    //                        {
    //                            Gizmos.color = (_map3D[x, y, z].state == States.Empty)
    //                                ? new Color(1, 1, 1, 0.7f)
    //                                : new Color(1, 0, 0, 0.0f);
    //                        }
    //                        Vector3 pos = new Vector3(-_width / 2 + x + 0.5f, -_height / 2 + y + 0.5f,
    //                            -_depth / 2 + z + 0.5f);
    //                        Gizmos.DrawCube(pos, Vector3.one);
    //                    }
    //                    else if(!_showSlice)
    //                    {
    //                        if (_showWalls)
    //                            Gizmos.color = (_map3D[x, y, z].state == States.Empty)
    //                                ? new Color(0, 1, 1, 0.7f)
    //                                : new Color(1, 0, 0, 0.1f);
    //                        else
    //                        {
    //                            Gizmos.color = (_map3D[x, y, z].state == States.Empty)
    //                                ? new Color(1, 1, 1, 0.7f)
    //                                : new Color(1, 0, 0, 0.0f);
    //                        }
    //                        //Gizmos.color = (_map[x, y] == States.Empty)
    //                        //    ? new Color(1, 1, 1, 0.1f)
    //                        //    : new Color(0, 0, 0, 0.2f);
    //                        //Vector3 pos = new Vector3(-_width / 2 + x + 0.5f,0, -_height / 2 + y + 0.5f);
    //                        //Gizmos.color = (_map3D[x, y, z] == States.Empty)
    //                        //    ? new Color(0, 1, 1, 0.3f)
    //                        //    : new Color(1, 0, 0, 0.1f);
    //                        Vector3 pos = new Vector3(-_width / 2 + x + 0.5f, -_height / 2 + y + 0.5f,
    //                            -_depth / 2 + z + 0.5f);
    //                        Gizmos.DrawCube(pos, Vector3.one);
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}
}
