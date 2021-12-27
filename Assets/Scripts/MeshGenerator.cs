using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    public CubeGrid cubeGrid;

    void OnDrawGizmos()
    {
        if (cubeGrid != null)
        {
            for (int x = 0; x < cubeGrid.cubes.GetLength(0); x++)
            {
                for (int y = 0; y < cubeGrid.cubes.GetLength(1); y++)
                {
                    for (int z = 0; z < cubeGrid.cubes.GetLength(2); z++)
                    {
                        Gizmos.color = (cubeGrid.cubes[x, y, z].GetCorner(1).state==States.Wall) ? new Color(): Color.white;
                        Gizmos.DrawCube(cubeGrid.cubes[x,y,z].GetCorner(0).position,Vector3.one*0.1f);

                        Gizmos.color = (cubeGrid.cubes[x, y, z].GetCorner(1).state == States.Wall) ? new Color() : Color.white;
                        Gizmos.DrawCube(cubeGrid.cubes[x, y, z].GetCorner(1).position, Vector3.one * 0.1f);

                        Gizmos.color = (cubeGrid.cubes[x, y, z].GetCorner(1).state == States.Wall) ? new Color() : Color.white;
                        Gizmos.DrawCube(cubeGrid.cubes[x, y, z].GetCorner(2).position, Vector3.one * 0.1f);

                        Gizmos.color = (cubeGrid.cubes[x, y, z].GetCorner(1).state == States.Wall) ? new Color() : Color.white;
                        Gizmos.DrawCube(cubeGrid.cubes[x, y, z].GetCorner(3).position, Vector3.one * 0.1f);

                        Gizmos.color = (cubeGrid.cubes[x, y, z].GetCorner(1).state == States.Wall) ? new Color() : Color.white;
                        Gizmos.DrawCube(cubeGrid.cubes[x, y, z].GetCorner(4).position, Vector3.one * 0.1f);

                        Gizmos.color = (cubeGrid.cubes[x, y, z].GetCorner(1).state == States.Wall) ? new Color() : Color.white;
                        Gizmos.DrawCube(cubeGrid.cubes[x, y, z].GetCorner(5).position, Vector3.one * 0.1f);

                        Gizmos.color = (cubeGrid.cubes[x, y, z].GetCorner(1).state == States.Wall) ? new Color() : Color.white;
                        Gizmos.DrawCube(cubeGrid.cubes[x, y, z].GetCorner(6).position, Vector3.one * 0.1f);

                        Gizmos.color = (cubeGrid.cubes[x, y, z].GetCorner(1).state == States.Wall) ? new Color() : Color.white;
                        Gizmos.DrawCube(cubeGrid.cubes[x, y, z].GetCorner(7).position, Vector3.one * 0.1f);
                    }
                }
            }
        }
    }

    void TriangulateCube(Cube cube)
    {
        
        int[] t = Table.TriTable.SubArray(cube.GetConfiguration(),16);
   
        foreach (ControlNode controlNode in cube.GetCorners())
        {

            
            
        }


       // int indexA = Table.cornerIndexAFromEdge[t];
        //int indexB = Table.cornerIndexBFromEdge[t];


    }

    void MeshFromPoints(params Node[] points)
    {

    }

    public void GenerateMesh(Cell[,,] map, float size)
    {
        cubeGrid = new CubeGrid(map, size);

        for (int x = 0; x < cubeGrid.cubes.GetLength(0); x++)
        {
            for (int y = 0; y < cubeGrid.cubes.GetLength(1); y++)
            {
                for (int z = 0; z < cubeGrid.cubes.GetLength(2); z++)
                {
                    TriangulateCube(cubeGrid.cubes[x,y,z]);
                }
            }
        }
    }

}
