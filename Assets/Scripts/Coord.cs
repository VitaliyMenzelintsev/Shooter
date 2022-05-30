using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Coord
{
    public int x;                // обьявили поля
    public int y;

    public Coord(int _x, int _y) // метод, принимающий и назначающий координаты
    {
        x = _x;
        y = _y;
    }
    public static bool operator ==(Coord c1, Coord c2)
    {
        return c1.x == c2.x && c1.y == c2.y;
    }

    public static bool operator !=(Coord c1, Coord c2)
    {
        return !(c1 == c2);
    }

    public static Vector3 ToPosition(int x, int y, Vector2 mapSize)
    {
        return new Vector3(-mapSize.x / 2 + 0.5f + x, 0, -mapSize.y / 2 + 0.5f + y);
    }
}

