using System.Collections;
using System.Collections.Generic;
using Collisions;
using UnityEngine;

public class EPA
{

    public struct Edge {
        public int edgeIndex;
        public Vector3 a, b;
        public float Distance;
        public Vector3 Normal;
    }
    public static void Execute(Vector3[] PointsA, Vector3[] PointsB, List<Vector3> Simplex, out float Depth, out Vector3 Normal) {

        while (true) {
            Edge edge = FindClosestEdge(Simplex);

            Vector3 p = GJK.Support(edge.Normal, PointsA, PointsB);

            float d = Vector3.Dot(p, edge.Normal);

            if (d - edge.Distance < 0.00001f) {
                Normal = edge.Normal;
                Depth = d + 0.005f;
                return;
            } else {
                Simplex.Insert(edge.edgeIndex, p);
            }
        }
        
    }


    public static Edge FindClosestEdge(List<Vector3> Simplex) {
        Edge closest = new Edge();
        closest.Distance = float.MaxValue;

        for (int i = 0; i < Simplex.Count; i++) {
            int j = 0;
            if (i + 1 != Simplex.Count) {
                j = i + 1;
            }

            Vector3 a = Simplex[i];
            Vector3 b = Simplex[j];

            Vector3 e = b - a;
            Vector3 originA = a;
            Vector3 t = Vector3.Cross(e, Vector3.Cross(originA, e));
            t.Normalize();

            float d = Vector3.Dot(t, a);

            if (d < closest.Distance) {
                closest.Distance = d;
                closest.Normal = t;
                closest.a = a;
                closest.b = b; //j
                closest.edgeIndex = j;
            }

        }


        return closest;
    }
}
