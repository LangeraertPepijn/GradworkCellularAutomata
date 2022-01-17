//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Random = UnityEngine.Random;

//public class PseudoCode : MonoBehaviour
//{
//    List<Coord> GetLine(Coord from, Coord to)
//    {
//        List<Coord> line = new List<Coord>();

//        int x = from.xCoord;
//        int y = from.yCoord;
//        int z = from.zCoord;

//        int dx = to.xCoord - from.xCoord;
//        int dy = to.yCoord - from.yCoord;
//        int dz = to.zCoord - from.zCoord;


//        int step = 0;
//        int gradientStep = 0;
//        int SecondairyStep = 0;

//        int longest = Mathf.Max(Math.Abs(dx), Math.Abs(dy), Math.Abs(dz));
//        int shortest = 0;
//        int middleValue = 0;

//        Enum test = Axis.x;
//        Enum longestAxis = Axis.x;
//        Enum shortestAxis = Axis.x;

//        if (longest == Math.Abs(dx))
//        {
//            longestAxis = Axis.x;
//            shortest =Math.Abs(dy);
 
//            {
//                shortestAxis = Axis.y;
//                step = Math.Sign(dx);
//                gradientStep = Math.Sign(dy);
//                SecondairyStep = Math.Sign(dz);
//                test = Axis.z;
//                middleValue = Math.Abs(dz);

//            }

//        }
//        else if (longest == Math.Abs(dy))
//        {
//            longestAxis = Axis.y;
//            shortest =Math.Abs(dz);

//            step = Math.Sign(dy);
//            gradientStep = Math.Sign(dz);
//            SecondairyStep = Math.Sign(dx);
//            shortestAxis = Axis.z;
//            test = Axis.x;
//            middleValue = Math.Abs(dx);
//        }
//        else
//        {
//            longestAxis = Axis.z;
//            shortest =Math.Abs(dy);
//            SecondairyStep = Math.Sign(dx);
//            step = Math.Sign(dz);
//            gradientStep = Math.Sign(dy);
//            shortestAxis = Axis.y;
//            test = Axis.x;
//            middleValue = Math.Abs(dx);

//        }


//        int gradientAccumulation = longest / 2;                     //Math.Abs(dx) > Math.Abs(dy)
//        int secondaryGradientAccumulation = longest / 2;            //Math.Abs(dz) > Math.Abs(dy)
//        for (int i = 0; i < longest; i++)                           //int longest = Math.Abs(dx); 
//        {                                                           //int shortest = Math.Abs(dy);
//            line.Add(new Coord(x, y, z));                       //int middleValue = Math.Abs(dz); 
//            x += step;                                              //int x = from.xCoord;    
//                                                                    int y = from.yCoord;
//            gradientAccumulation += shortest;                       //int z = from.zCoord;
//            if (gradientAccumulation >= longest)
//            {
//                y += gradientStep;
//                gradientAccumulation -= longest;
//            }

//            secondaryGradientAccumulation += middleValue;
//            if (secondaryGradientAccumulation >= longest)
//            {
//                z += SecondairyStep;
//                secondaryGradientAccumulation -= longest;
//            }
//        }
//        return line;

//    }


//    List<Coord> GetLine2D(Coord from, Coord to)
//    {
//        List<Coord> line = new List<Coord>();

//        int x = from.xCoord;
//        int y = from.yCoord;

//        int dx = to.xCoord - from.xCoord;
//        int dy = to.yCoord - from.yCoord;


//        bool inverted = false;

//        int step = Math.Sign(dx);
//        int gradientStep = Math.Sign(dy);

//        int longest = Math.Abs(dx);
//        int shortest = Math.Abs(dy);

//        if (longest < shortest)
//        {
//            inverted = true;
//            (shortest, longest) = (longest, shortest);

//            (step, gradientStep) = (gradientStep, step);

//        }
                                               
//        int gradientAccumulation = longest / 2;             //Math.Abs(dx) > Math.Abs(dy)
//        for (int i = 0; i < longest; i++)                   //int longest = Math.Abs(dx); 
//        {                                                   //int shortest = Math.Abs(dy);
//            line.Add(new Coord(x, y));                  //int x = from.xCoord;
//            x += step;                                      //int y = from.yCoord;        
//            gradientAccumulation += shortest;               
//            if (gradientAccumulation >= longest)
//            {
//                y += gradientStep;
//                gradientAccumulation -= longest;
//            }

//        }

//        return line;

//    }

//    private int _corridorFromRoomChance = new int();
//    private int _breakOutValue = new int();
//    private int _connectionMax = new int();
//    private int _corridorLengthMax;
//    private int _corridorLengthMin;
//    private int _turnCount;
//    private Cell[,] _map;
//    private double _roomCreationChance;

//    protected List<Corridor> DigCorridors(List<Room> rooms, List<Corridor> corridors)
//    {

//        Area currentRoom = new Room();
//        Vector2Int direction = new Vector2Int();
//        Coord cell = new Coord(-10, -10);
//        List<Corridor> newCorridors = new List<Corridor>();
//        int breakOutCounter = 0;
//        while (connectRooms())
//        {
//            {(1)
//            if (corridors.Count > 0 && Random.Range(0, 1.0f) > _corridorFromRoomChance)  
//            {
//                currentRoom = corridors[Random.Range(0, corridors.Count - 1)]; //currentRoom is where the corridor starts from
//                GetDigPosition(currentRoom, ref cell, ref direction);          //cell is the starting cell of the corridor
//            }                                                                  //direction is a vector to indicate where the corridor will go
//            else
//            {
//                currentRoom = rooms[Random.Range(0, rooms.Count - 1)];
//                GetDigPosition(currentRoom, ref cell, ref direction);
//            }
//            }
//            bool createRoom = false;
//            Corridor potentialCorridor = GeneratePotentialCorridor(cell, direction,ref createRoom); //figure 12
//            if (potentialCorridor != null)
//            {
//                if (createRoom)
//                    CreateRoom(potentialCorridor.Last()); //create a room at the end position of the corridor 
//                ChangeCells(potentialCorridor); //update the state of the cells so they are part of playable area
//            }
//            ++breakOutCounter;
//            if (breakOutCounter > _breakOutValue) //iteration limiter
//                return newCorridors;
//            if (newCorridors.Count >= _connectionMax) //maximum amount of corridors wanted
//                return newCorridors;
//        }

//        return newCorridors;
//    }

//    private void CreateRoom(Coord coord)
//    {
//        throw new NotImplementedException();
//    }

//    private bool connectRooms()
//    {
//        throw new NotImplementedException();
//    }

//    private void ChangeCells(Corridor potentialCorridor)
//    {
//        throw new NotImplementedException();
//    }


//    private Corridor GeneratePotentialCorridor(Coord start, Vector2Int dir,ref bool createRoom)
//    {

//        Corridor potentialCorridor = new Corridor();
//        potentialCorridor.AddCell(start);
//        Coord currentCell = start;
//        int length = 0;
//        int turns = 0;
//        int maxTurns = Random.Range(0, _turnCount);
//        while (turns <= maxTurns)
//        {
//            ++turns;
//            length = Random.Range(_corridorLengthMin, _corridorLengthMax);
//            while (length > 0)
//            {
//                --length;
//                currentCell += dir;
//                if (!IsInMap(currentCell.xCoord, currentCell.yCoord))
//                    return null;//no corridor
//                if (_map != null && _map[currentCell.xCoord, currentCell.yCoord].state == States.Empty)
//                {
//                    potentialCorridor.AddCell(currentCell);
//                    return potentialCorridor;//corridor
//                }
//                if (!CorridorSpacingCheck(currentCell, dir))
//                    return null;//no corridor
//                potentialCorridor.AddCell(currentCell);

//            }
//            dir = GetNewDirection(dir);
//        }
//        if (Random.Range(0, 1.0f) < _roomCreationChance)//only create room if we do not find a room
//        {
//            createRoom = true;
//            return potentialCorridor; // corridor that will end in new room
//        }
//        return null;
//    }

//    private bool IsInMap(int currentCellXCoord, int currentCellYCoord)
//    {
//        throw new NotImplementedException();
//    }

//    private Vector2Int GetNewDirection(Vector2Int oldDir)
//    {
//        Vector2Int otherDirection = oldDir;
//        do
//        {

//            switch (_randomNumberGenerator.Next(0, 4))
//            {
//                case 0:
//                    otherDirection = Vector2Int.up;
//                    break;
//                case 1:
//                    otherDirection = Vector2Int.right;
//                    break;
//                case 2:
//                    otherDirection = Vector2Int.down;
//                    break;
//                case 3:
//                    otherDirection = Vector2Int.left;
//                    break;

//            }

//            if (_createDeadEnds)
//                return otherDirection;


//        } while (otherDirection.x == -oldDir.x && otherDirection.y == -oldDir.y);

//        return otherDirection;
//    }

//    private bool CorridorSpacingCheck(Coord cell, Vector2Int direction)
//    {


//        foreach (int r in Enumerable.Range(-_corridorSpacing, _corridorSpacing).ToList())
//        {
//            if (direction.x == 0)//north or south
//            {
//                if (IsInMap(cell.xCoord + r, cell.yCoord))
//                    if (_map[cell.xCoord + r, cell.yCoord].state != States.Wall)
//                        return false;
//            }
//            else if (direction.y == 0)//east west
//            {
//                if (IsInMap(cell.xCoord, cell.yCoord + r))
//                    if (_map[cell.xCoord, cell.yCoord + r].state != States.Wall)
//                        return false;
//            }

//        }

//        return true;
//    }

//    private void GetDigPosition(Area room, ref Coord cell, ref Vector2Int dir)
//    {
//        if (room.EdgeCells.Count > 0)
//        {

//            Coord edgeCell = room.EdgeCells[_randomNumberGenerator.Next(0, room.EdgeCells.Count)];

//            for (int x = edgeCell.xCoord - 1; x < edgeCell.xCoord + 1; x++)
//            {
//                for (int y = edgeCell.yCoord - 1; y < edgeCell.yCoord + 1; y++)
//                {
//                    von neumann neighbourhood
//                    if (IsInMap(x, y) && (x == edgeCell.xCoord || y == edgeCell.yCoord) &&
//                        _map[x, y].state == States.Wall)
//                    {

//                        cell = edgeCell;
//                        dir = new Vector2Int(x, y) - new Vector2Int(edgeCell.xCoord, edgeCell.yCoord);
//                        return;
//                    }
//                }
//            }
//        }

//    }
//}
