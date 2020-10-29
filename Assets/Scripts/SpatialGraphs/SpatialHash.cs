using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpatialHash : SpatialGraph {
    Dictionary<int, ArrayList> prisms;
    int width = 0;
    int length = 0;
    int height = 0;
    int cellSize = 0;

    public SpatialHash(int w, int l, int h, int c) {
        width = w;
        length = l;
        height = h;
        cellSize = c;
    }

    public void Insert(Prism p) {
        foreach(Vector3 v in p.points) {
            int i = hash(v);
            ArrayList l = prisms[i];

            if (!l.Contains(p))
                l.Add(p);
        }
    }

    public void Remove(Prism p) {
        foreach(Vector3 v in p.points) {
            int i = hash(v);
            ArrayList l = prisms[i];

            l.Remove(p);
        }
    }

    private int hash(Vector3 v) {
        return (int)(v.x * length * height + v.y * length + v.z);
    }

    public Prism[] GetNeighbors(Prism p) {
        Prism[] ret = {};

        return ret;
    }
}