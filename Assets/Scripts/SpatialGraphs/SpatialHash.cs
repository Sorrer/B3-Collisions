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

        prisms = new Dictionary<int, ArrayList>();
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
        int cx = (int)v.x / cellSize;
        int cy = (int)v.y / cellSize;
        int cz = (int)v.z / cellSize;

        int cl = length / cellSize;
        int ch = height / cellSize;
        
        return (int)(cx * cl * ch + cy * cl + cz);
    }

    public Prism[] GetNeighbors(Prism p) {
        ArrayList ret = new ArrayList();

        foreach(Vector3 v in p.points) {
            int i = hash(v);
            
            ret.AddRange(prisms[i]);

            //Clean up own hash table
            while (ret.Contains(p))
                ret.Remove(p);
        }

        return (Prism[])ret.ToArray(typeof(Prism));
    }
}