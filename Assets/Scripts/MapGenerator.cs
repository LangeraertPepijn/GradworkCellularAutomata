using UnityEngine;
using System;
using System.Collections.Generic;
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
        public int neighbourCount;
    }

    struct Coord
    {
        public int xCoord;
        public int yCoord;
        public int zCoord;

        public Coord(int x, int y)
        {
            xCoord = x;
            yCoord = y;
            zCoord = 0;
        }
        public Coord(int x, int y, int z)
        {
            xCoord = x;
            yCoord = y;
            zCoord = z;
        }

    }

    [SerializeField] private GameObject _cube=null;
    [SerializeField] private string _randomSeed;
    [SerializeField] private bool _useRandomSeed;
    [SerializeField] private bool _Use3D=false;
    [SerializeField] private bool _MakeEdgesWalls=true;
    [SerializeField] [Range(0,1)]private float _randomFillPercent = 0.5f;
    //[SerializeField] [Range(0,1)]private float _changeChance = 0.1f;
    [SerializeField] [Range(0,100)]private int _sizeThreshold = 5;
    [SerializeField] [Range(1, 50)] private int _width=50;
    [SerializeField] [Range(1, 50)] private int _height=50;
    [SerializeField] [Range(1, 50)] private int _depth=50;
    [SerializeField] [Range(0,20)] private int _iterations =5;
    [SerializeField] [Range(0,30)] private int _neighbourWallCountToChange = 4;
    [SerializeField] [Range(0,30)]private int _neighbourEmptyCountToChange = 4;

    [SerializeField] private bool _showSlice;
    [SerializeField] private bool _showWalls=true;
    [SerializeField] private bool _showEmpty=true;
    [SerializeField] private bool _showShell=true;
    [SerializeField] [Range(0, 100)] private int _sliceIndex=0;

    [CanBeNull] private Cell[,] _map;
    private States[,] _stateBuffer;
    [CanBeNull] private Cell[,,] _map3D;
    private States[,,] _stateBuffer3D;
    private System.Random _randomNumberGenerator;

    // Start is called before the first frame update
    void Start()
    {
        if(_Use3D)
            GenerateMap3D();
        else
            GenerateMap();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Camera.main.transform.position = new  Vector3(0,(_width+_height)/2.0f,0);
            // GenerateMap();
            if (_Use3D)
            {
                if(_map!=null)
                    ClearMap();
                GenerateMap3D();
            }
            else
            {
                if(_map3D!=null)
                    ClearMap3D();
                GenerateMap();

            }
        }
        else if (Input.GetMouseButtonDown(1)&&_Use3D)
        {

            IterateStatesOnce3D();
            UpdateCubes3D();
        }
    }



    //2D Code
    /// <summary>
    /// check if the cell lies on the map or not
    /// </summary>
    /// <param name="x">x coord of cell</param>
    /// <param name="y">y coord of cell</param>
    /// <returns></returns>
    bool IsInMap(int x, int y)
    {
        return x >= 0 && x < _width && y >= 0 && y < _height;
    }

    // Generate the Cellular Automata Map
    void GenerateMap()
    {
        if (_map != null)
            ClearMap();
        _map = new Cell[_width, _height];
        _stateBuffer = new States[_width, _height];

       // if (_useRandomSeed)
          //  _randomSeed = System.DateTime.Now.ToString();

        //_randomNumberGenerator = new System.Random(_randomSeed.GetHashCode());

        RandomFillMap();
        IterateStates();
        //ExamineMap();
        UpdateCubes();
    }

    // remove old cubes
    void ClearMap()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {

                    if (_map[x, y].mesh)
                        Destroy(_map[x, y].mesh);
                
            }
        }
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
                    //_map[x, y].state = (_randomNumberGenerator.Next(0, 100) / 100.0f < _randomFillPercent) ? States.Wall : States.Empty;
                    _map[x, y].state = (Random.Range(0.0f, 1.0f) < _randomFillPercent) ? States.Wall : States.Empty;
               
                }

            }
        }
    }

    //check if the surrounding cells are walls
    int GetSurroundingWallCount(int indexX, int indexY)
    {
        int wallcount = 0;

        for (int neighbourX = indexX-1; neighbourX <= indexX+1; neighbourX++)
        {
            for (int neighbourY = indexY - 1; neighbourY <= indexY + 1; neighbourY++)
            {
                if (IsInMap(neighbourX, neighbourY))
                {
                    //check if not cell itself
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

    // iterate using rules over the map to change it
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
                        //_map[x, y].state = States.Wall;
                        _stateBuffer[x, y] = States.Wall;
                    }
                    else if (8 - neighbourWallTiles > _neighbourEmptyCountToChange)
                    {
                        //_map[x, y].state = States.Empty;
                        _stateBuffer[x, y] = States.Empty;

                    }

                }
            }

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _map[x, y].state = _stateBuffer[x, y];
                }
            }
        }
    }

    //flood fill check to see which tiles are part of what region
    List<Coord> GetRegionTiles(int startX, int startY)
    {
        List<Coord> cells = new List<Coord>();
        int[,] mapFlags = new int[_width, _height];
        States cellState = _map[startX, startY].state;

        Queue<Coord> queue = new Queue<Coord>();

        queue.Enqueue(new Coord(startX,startY));
        mapFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            Coord cell = queue.Dequeue();
            cells.Add(cell);

            for (int x = cell.xCoord - 1; x <= cell.xCoord + 1; x++)
            {
                for (int y = cell.yCoord - 1; y <= cell.yCoord + 1; y++)
                {
                    if (IsInMap(x, y)&&(y==cell.yCoord||x==cell.xCoord))
                    {
                        if (mapFlags[x, y] == 0 && _map[x, y].state == cellState)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coord(x,y));
                        }
                    }
                }
            }
        }

        return cells;
    }

    /// <summary>
    /// Returns all regions with the given state
    /// </summary>
    /// <param name="state"> state of the regions to return</param>
    /// <returns></returns>
    List<List<Coord>> GetRegionsOfState(States state)
    {
        List<List<Coord>> regions = new List<List<Coord>>();

        int[,] mapFlags = new int[_width, _height];

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (mapFlags[x, y] == 0 && _map[x, y].state == state)
                {
                    List<Coord> region = GetRegionTiles(x, y);
                    regions.Add(region);

                    foreach (Coord cell in region)
                    {
                        mapFlags[cell.xCoord, cell.yCoord] = 1;
                    }
                }
            }
        }

        return regions;
    }

    // if the room is too small remove it
    void ExamineMap()
    {
        List<List<Coord>> regions = GetRegionsOfState(States.Empty);

        
        foreach (List<Coord> region in regions)
        {
            if (region.Count < _sizeThreshold)
            {
                foreach (Coord cell in region)
                {
                    _map[cell.xCoord, cell.yCoord].state = States.Wall;
                }
            }
        }
    }

    void UpdateCubes()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {

                if (_map[x, y].mesh)
                {
                    MeshRenderer renderer = _map[x, y].mesh.GetComponent<MeshRenderer>();
                    if (renderer)
                        renderer.material.color =
                            (_map[x, y].state == States.Wall) ? Color.black : Color.white;
                    if ((!_showWalls && _map[x, y].state == States.Wall) ||
                        (!_showEmpty && _map[x, y].state == States.Empty) || (!_showShell &&
                                                                              (x == 0 || x == _width - 1 || y == 0 ||
                                                                               y == _height - 1)))
                        _map[x, y].mesh.gameObject.SetActive(false);
                    else
                        _map[x, y].mesh.gameObject.SetActive(true);
                }
                else
                {
                    Vector3 pos = new Vector3(-_width / 2 + x + 0.5f, -_height / 2 + y + 0.5f,
                        0);
                    _map[x, y].mesh = Instantiate(_cube);
                    _map[x, y].mesh.transform.position = pos;

                    MeshRenderer renderer = _map[x, y].mesh.GetComponent<MeshRenderer>();
                    if (renderer)
                        renderer.material.color =
                            (_map[x, y].state == States.Wall) ? Color.black : Color.white;

                    if ((!_showWalls && _map[x, y].state == States.Wall) ||
                        (!_showEmpty && _map[x, y].state == States.Empty) || (!_showShell &&
                                                                              (x == 0 || x == _width - 1 || y == 0 ||
                                                                               y == _height - 1)))
                        _map[x, y].mesh.gameObject.SetActive(false);
                    else
                        _map[x, y].mesh.gameObject.SetActive(true);
                }

            }
        }

    }

    //--------------------------------------------
    //--------------------------------------------
    //--------------------------------------------

    //3D code

    /// <summary>
    /// check if the cell lies on the map or not 3D
    /// </summary>
    /// <param name="x">x coord of cell</param>
    /// <param name="y">y coord of cell</param>
    /// <param name="z">z coord of cell</param>
    /// <returns></returns>
    bool IsInMap3D(int x, int y,int z)
    {
        return x >= 0 && x < _width && y >= 0 && y < _height&&z>=0&&z<_depth;
    }

    // Generate the Cellular Automata Map
    void GenerateMap3D()
    {
       
        if(_map3D!=null)
         ClearMap3D();
        _map3D = new Cell[_width, _height,_depth];
        _stateBuffer3D = new States[_width, _height,_depth];


       // if (_useRandomSeed)
          //  _randomSeed = System.DateTime.Now.ToString();
        
       // _randomNumberGenerator = new System.Random(_randomSeed.GetHashCode());

      

        RandomFillMap3D();
        IterateStates3D();
        ExamineMap3D();
        UpdateCubes3D();
    }

    // remove old cubes
    void ClearMap3D()
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


    //check if the surrounding cells are walls
    int GetSurroundingWallCount3D(int indexX, int indexY,int indexZ)
    {
        int wallcount = 0;

        for (int neighbourX = indexX - 1; neighbourX <= indexX + 1; neighbourX++)
        {
            for (int neighbourY = indexY - 1; neighbourY <= indexY + 1; neighbourY++)
            {
                for (int neighbourZ = indexZ - 1; neighbourZ <= indexZ + 1; neighbourZ++)
                {
                    if (IsInMap3D(neighbourX,neighbourY,neighbourZ))
                    {
                        //check if not cell itself
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


    // Fill the map with random values based on the RandomFill Percent
    void RandomFillMap3D()
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
                        _map3D[x, y,z].state = States.Wall;
                    }
                    else
                    {
                        _map3D[x, y, z].state = (Random.Range(0, 1.0f) < _randomFillPercent)
                            ? States.Wall
                            : States.Empty;
                            //_map3D[x, y, z].state = (_randomNumberGenerator.Next(0, 100) / 100.0f < _randomFillPercent)
                            //? States.Wall
                            //: States.Empty;

                    }
                }

            }
        }
    }


    // iterate using rules over the map to change it once to see what changes
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

                    _map3D[x, y, z].neighbourCount = GetSurroundingWallCount3D(x, y, z);
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
        Debug.Log("The amount of walls changed is" +w +"the amount of empty added is" +e);

    }

    // iterate using rules over the map to change it
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

                        _map3D[x,y,z].neighbourCount = GetSurroundingWallCount3D(x, y,z);
                        if (_map3D[x, y, z].neighbourCount > _neighbourWallCountToChange)
                        {
                           // _map3D[x, y,z].state = States.Wall;
                            _stateBuffer3D[x, y,z] = States.Wall;
                        }
                        else if (26 - _map3D[x, y, z].neighbourCount > _neighbourEmptyCountToChange)
                        {
                            //_map3D[x, y,z].state = States.Empty;
                            _stateBuffer3D[x, y,z] = States.Empty;

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
    List<Coord> GetRegionTiles3D(int startX, int startY,int startZ)
    {
        List<Coord> cells = new List<Coord>();
        int[,,] mapFlags = new int[_width, _height,_depth];
        States cellState = _map3D[startX, startY,startZ].state;

        Queue<Coord> queue = new Queue<Coord>();

        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY,startZ] = 1;

        while (queue.Count > 0)
        {
            Coord cell = queue.Dequeue();
            cells.Add(cell);

            for (int x = cell.xCoord - 1; x <= cell.xCoord + 1; x++)
            {
                for (int y = cell.yCoord - 1; y <= cell.yCoord + 1; y++)
                {
                    for (int z = cell.zCoord - 1; z <= cell.zCoord + 1; z++)
                    {
                        if (IsInMap3D(x, y,z) && (y == cell.yCoord || x == cell.xCoord||z==cell.zCoord))
                        {
                            if (mapFlags[x, y,z] == 0 && _map3D[x, y,z].state == cellState)
                            {
                                mapFlags[x, y,z] = 1;
                                queue.Enqueue(new Coord(x, y));
                            }
                        }
                    }
                }
            }
        }

        return cells;
    }

    /// <summary>
    /// Returns all regions with the given state
    /// </summary>
    /// <param name="state"> state of the regions to return</param>
    /// <returns></returns>
    List<List<Coord>> GetRegionsOfState3D(States state)
    {
        List<List<Coord>> regions = new List<List<Coord>>();

        int[,,] mapFlags = new int[_width, _height,_depth];

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                for (int z = 0; z < _depth; z++)
                {
                    if (mapFlags[x, y,z] == 0 && _map3D[x, y,z].state == state)
                    {
                        List<Coord> region = GetRegionTiles(x, y);
                        regions.Add(region);

                        foreach (Coord cell in region)
                        {
                            mapFlags[cell.xCoord, cell.yCoord,cell.zCoord] = 1;
                        }
                    }
                }
            }
        }

        return regions;
    }

    // if the room is too small remove it
    void ExamineMap3D()
    {
        List<List<Coord>> regions = GetRegionsOfState(States.Empty);

        foreach (List<Coord> region in regions)
        {
            if (region.Count < _sizeThreshold)
            {
                foreach (Coord cell in region)
                {
                    _map[cell.xCoord, cell.yCoord].state = States.Wall;
                }
            }
        }
    }


    void UpdateCubes3D()
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
                        if ((!_showWalls && _map3D[x, y, z].state == States.Wall) ||
                            (!_showEmpty && _map3D[x, y, z].state == States.Empty) || _map3D[x, y, z].neighbourCount==26
                            || (!_showShell &&
                            (x == 0 || x == _width - 1 || y == 0 || y == _height - 1 || z == 0 || z == _depth - 1)))
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

                        if ((!_showWalls && _map3D[x, y, z].state == States.Wall) ||
                            (!_showEmpty && _map3D[x, y, z].state == States.Empty) || _map3D[x, y, z].neighbourCount== 26 || (!_showShell &&
                            (x == 0 || x == _width - 1 || y == 0 || y == _height - 1 || z == 0 || z == _depth - 1)))
                            _map3D[x, y, z].mesh.gameObject.SetActive(false);
                        else
                            _map3D[x, y, z].mesh.gameObject.SetActive(true);
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
