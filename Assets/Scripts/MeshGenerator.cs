using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    private CubeGrid _cubeGrid;

    private List<Vector3> _vertexBuffer=new List<Vector3>();
    private List<int> _indexBuffer=new List<int>();

    public void GenerateMesh(Cell[,,] map, float size)
    {
        _cubeGrid = new CubeGrid(map, size);
        _vertexBuffer = new List<Vector3>();
        _indexBuffer = new List<int>();

        for (int x = 0; x < _cubeGrid.cubes.GetLength(0); x++)
        {
            for (int y = 0; y < _cubeGrid.cubes.GetLength(1); y++)
            {
                for (int z = 0; z < _cubeGrid.cubes.GetLength(2); z++)
                {
                    TriangulateCube(_cubeGrid.cubes[x, y, z]);
                }
            }
        }
        
        Mesh mesh = new Mesh();

        GetComponent<MeshFilter>().mesh = mesh;

        mesh.vertices = _vertexBuffer.ToArray();
        mesh.triangles = _indexBuffer.ToArray();
        mesh.RecalculateNormals();
    }

    //private void OnDrawGizmos()
    //{
    //    if (_cubeGrid != null)
    //    {
    //        for (int x = 0; x < _cubeGrid.cubes.GetLength(0); x++)
    //        {
    //            for (int y = 0; y < _cubeGrid.cubes.GetLength(1); y++)
    //            {
    //                for (int z = 0; z < _cubeGrid.cubes.GetLength(2); z++)
    //                {
    //                    Gizmos.color = (_cubeGrid.cubes[x, y, z].GetCorner(0).state==States.Wall) ? Color.black: Color.white;
    //                    Gizmos.DrawCube(_cubeGrid.cubes[x,y,z].GetCorner(0).position,Vector3.one*0.1f);

    //                    Gizmos.color = (_cubeGrid.cubes[x, y, z].GetCorner(1).state == States.Wall) ? Color.black : Color.white;
    //                    Gizmos.DrawCube(_cubeGrid.cubes[x, y, z].GetCorner(1).position, Vector3.one * 0.1f);

    //                    Gizmos.color = (_cubeGrid.cubes[x, y, z].GetCorner(2).state == States.Wall) ? Color.black : Color.white;
    //                    Gizmos.DrawCube(_cubeGrid.cubes[x, y, z].GetCorner(2).position, Vector3.one * 0.1f);

    //                    Gizmos.color = (_cubeGrid.cubes[x, y, z].GetCorner(3).state == States.Wall) ? Color.black : Color.white;
    //                    Gizmos.DrawCube(_cubeGrid.cubes[x, y, z].GetCorner(3).position, Vector3.one * 0.1f);

    //                    Gizmos.color = (_cubeGrid.cubes[x, y, z].GetCorner(4).state == States.Wall) ? Color.black : Color.white;
    //                    Gizmos.DrawCube(_cubeGrid.cubes[x, y, z].GetCorner(4).position, Vector3.one * 0.1f);

    //                    Gizmos.color = (_cubeGrid.cubes[x, y, z].GetCorner(5).state == States.Wall) ? Color.black : Color.white;
    //                    Gizmos.DrawCube(_cubeGrid.cubes[x, y, z].GetCorner(5).position, Vector3.one * 0.1f);

    //                    Gizmos.color = (_cubeGrid.cubes[x, y, z].GetCorner(6).state == States.Wall) ? Color.black : Color.white;
    //                    Gizmos.DrawCube(_cubeGrid.cubes[x, y, z].GetCorner(6).position, Vector3.one * 0.1f);

    //                    Gizmos.color = (_cubeGrid.cubes[x, y, z].GetCorner(7).state == States.Wall) ? Color.black : Color.white;
    //                    Gizmos.DrawCube(_cubeGrid.cubes[x, y, z].GetCorner(7).position, Vector3.one * 0.1f);
    //                }
    //            }
    //        }
    //    }
    //}

    private void TriangulateCube(Cube cube)
    {
        
        int[] indices = Table.TriTable.SubArray(cube.GetConfiguration(),16);
   
        //foreach (ControlNode controlNode in cube.GetCorners())
        //{
        //}


        List<Node> points = new List<Node>();
        foreach (int index in indices)
        {
            if(index!=-1)
                points.Add(cube.GetNode(index)); 
        }
        MeshFromPoints(points.ToArray());
       // int indexA = Table.cornerIndexAFromEdge[t];
        //int indexB = Table.cornerIndexBFromEdge[t];


    }

    private void MeshFromPoints(params Node[] points)
    {
        AssignVertices(points);

        if (points.Length >= 3)
        {
            CreateTriangle(points[0], points[1], points[2]);
        }
        if (points.Length >= 6)
        {
            CreateTriangle(points[3], points[4], points[5]);
        }
        if (points.Length >= 9)
        {
            CreateTriangle(points[6], points[7], points[8]);
        }
        if (points.Length >= 12)
        {
            CreateTriangle(points[9], points[10], points[11]);
        }
        if (points.Length >= 15)
        {
            CreateTriangle(points[12], points[13], points[14]);
        }

    }

    private void AssignVertices(Node[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].vertexIndex == -1)
            {
                points[i].vertexIndex = _vertexBuffer.Count;

                _vertexBuffer.Add(points[i].position);
            }
        }
    }

    private void CreateTriangle(Node a,Node b, Node c)
    {
        _indexBuffer.Add(a.vertexIndex);
        _indexBuffer.Add(b.vertexIndex);
        _indexBuffer.Add(c.vertexIndex);
    }


}
