using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpatialHash : SpatialGraph {
    //Dictionary<int, ArrayList<Prism>> prisms;
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
        
    }

    public void Remove(Prism p) {

    }

    private int hash(Vector2 v) {
        return 0;
    }

    public Prism[] GetNeighbors(Prism p) {
        Prism[] ret = {};

        return ret;
    }
}