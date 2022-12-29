using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Points: IEquatable<Points>
{
    public int X;
    public int Y;
    GameObject chesspiecePoint;

    public Points(int x, int y)
    {
        X = x; 
        Y = y;
    }

    public Points(GameObject go, int x, int y)
    {
        chesspiecePoint = go;
        X = x;
        Y = y;
    }

    public bool SingleEquals(int x, int y)
    {
        return X == x && Y == y;
    }

    public bool ComparePoints(Points x)
    {
        return X==x.X && Y==x.Y;
    }

    public static bool DualEquals(Points x,Points y)
    {
        return x.X == y.X && x.Y == y.Y;
    }

    public bool Equals(Points other)
    {
        return X == other.X && Y == other.Y;
    }

    public class EqualityComparer : IEqualityComparer<Points>
    {
        public bool Equals(Points x, Points y)
        {
            return x.X == y.X && x.Y == y.Y;
        }
        public int GetHashCode(Points x)
        {
            return x.X ^ x.Y;
        }
    }
}
