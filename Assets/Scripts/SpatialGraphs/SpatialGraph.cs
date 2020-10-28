using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SpatialObject {
    
    public Vector3 origin; //Center of all points
    public Vector3 bounding_box; //Size of bounding box

    public Prism prism; //Contains the data for the object
}


public interface SpatialGraph {
    void Insert(Prism p);
    void Remove(Prism p);
    Prism[] GetNeighbors(Prism p);
}
