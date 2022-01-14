using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PseudoCode : MonoBehaviour
{
    List<Coord> GetLine(Coord from, Coord to)
    {
        List<Coord> line = new List<Coord>();

        int x = from.xCoord;
        int y = from.yCoord;
        int z = from.zCoord;

        int dx = to.xCoord - from.xCoord;
        int dy = to.yCoord - from.yCoord;
        int dz = to.zCoord - from.zCoord;


        int step = 0;
        int gradientStep = 0;
        int SecondairyStep = 0;

        int longest = Mathf.Max(Math.Abs(dx), Math.Abs(dy), Math.Abs(dz));
        int shortest = 0;
        int middleValue = 0;

        Enum test = Axis.x;
        Enum longestAxis = Axis.x;
        Enum shortestAxis = Axis.x;

        if (longest == Math.Abs(dx))
        {
            longestAxis = Axis.x;
            shortest =Math.Abs(dy);
 
            {
                shortestAxis = Axis.y;
                step = Math.Sign(dx);
                gradientStep = Math.Sign(dy);
                SecondairyStep = Math.Sign(dz);
                test = Axis.z;
                middleValue = Math.Abs(dz);

            }

        }
        else if (longest == Math.Abs(dy))
        {
            longestAxis = Axis.y;
            shortest =Math.Abs(dz);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dz);
            SecondairyStep = Math.Sign(dx);
            shortestAxis = Axis.z;
            test = Axis.x;
            middleValue = Math.Abs(dx);
        }
        else
        {
            longestAxis = Axis.z;
            shortest =Math.Abs(dy);
            SecondairyStep = Math.Sign(dx);
            step = Math.Sign(dz);
            gradientStep = Math.Sign(dy);
            shortestAxis = Axis.y;
            test = Axis.x;
            middleValue = Math.Abs(dx);

        }


        int gradientAccumulation = longest / 2;                     //Math.Abs(dx) > Math.Abs(dy)
        int secondaryGradientAccumulation = longest / 2;            //Math.Abs(dz) > Math.Abs(dy)
        for (int i = 0; i < longest; i++)                           //int longest = Math.Abs(dx); 
        {                                                           //int shortest = Math.Abs(dy);
            line.Add(new Coord(x, y, z));                       //int middleValue = Math.Abs(dz); 
            x += step;                                              //int x = from.xCoord;    
                                                                    //int y = from.yCoord;
            gradientAccumulation += shortest;                       //int z = from.zCoord;
            if (gradientAccumulation >= longest)
            {
                y += gradientStep;
                gradientAccumulation -= longest;
            }

            secondaryGradientAccumulation += middleValue;
            if (secondaryGradientAccumulation >= longest)
            {
                z += SecondairyStep;
                secondaryGradientAccumulation -= longest;
            }
        }
        return line;

    }


    List<Coord> GetLine2D(Coord from, Coord to)
    {
        List<Coord> line = new List<Coord>();

        int x = from.xCoord;
        int y = from.yCoord;

        int dx = to.xCoord - from.xCoord;
        int dy = to.yCoord - from.yCoord;


        bool inverted = false;

        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);

        int longest = Math.Abs(dx);
        int shortest = Math.Abs(dy);

        if (longest < shortest)
        {
            inverted = true;
            (shortest, longest) = (longest, shortest);

            (step, gradientStep) = (gradientStep, step);

        }
                                               
        int gradientAccumulation = longest / 2;             //Math.Abs(dx) > Math.Abs(dy)
        for (int i = 0; i < longest; i++)                   //int longest = Math.Abs(dx); 
        {                                                   //int shortest = Math.Abs(dy);
            line.Add(new Coord(x, y));                  //int x = from.xCoord;
            x += step;                                      //int y = from.yCoord;        
            gradientAccumulation += shortest;               
            if (gradientAccumulation >= longest)
            {
                y += gradientStep;
                gradientAccumulation -= longest;
            }

        }

        return line;

    }
}
