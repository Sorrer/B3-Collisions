using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class QuadNode
{
    public int ID = 0;

    public int depth = 0;
    private float _size = 3.0f;

    public Vector3 center = Vector3.zero;

    public List<Prism> occupyingPrisms;
    public List<Vector3> occupyingPoints;

    private Vector3[] corners = new Vector3[4];

    public float Size()
    {
        return _size;
    }

    public void SetSize(float size)
    {
        _size = size;
        FillCorners();
    }

    public bool PointInQuadNode(Vector3 point)
    {
        return (corners[0].x <= point.x
             && corners[0].z <= point.z
             && corners[3].x >= point.x
             && corners[3].z >= point.z);
    }

    private void FillCorners()
    {
        int cornerIndex = 0;

        //Debug.Log(cornerIndex + "," + corners.Length);

        for (int x = -1; x <= 1; x += 2)
        {
            for (int y = -1; y <= 1; y += 2)
            {
                corners[cornerIndex] = center + new Vector3(x, 0, y) * _size;
                cornerIndex++;
            }
        }
    }

    public void DrawQuad(Color drawColour)
    {
        Debug.DrawLine(corners[0], corners[2], drawColour);
        Debug.DrawLine(corners[2], corners[3], drawColour);
        Debug.DrawLine(corners[3], corners[1], drawColour);
        Debug.DrawLine(corners[1], corners[0], drawColour);
    }
}

public class QuadTree : MonoBehaviour
{
    public float maxDepth = 4.0f;
    public int maxPointsInCell = 1;

    public float totalSize = 20.0f;

    public bool drawTree = false;
    public int depthToDrawFrm = 0;

    public Transform testPoint;

    private QuadNode[] quadTreeNodes;
    private Dictionary<int, int> quadNodeIdToIndex = new Dictionary<int, int>();

    private void Update()
    {
        for (int i = 0; i < quadTreeNodes.Length; i++)
        {
            if(quadTreeNodes[i] != null)
            {
                if(quadTreeNodes[i].depth == depthToDrawFrm || drawTree)
                {
                    //Debug.Log("Drawing quad : " + i);
                    //Debug.Log(quadTreeNodes[i]._Size());
                    quadTreeNodes[i].DrawQuad(Color.cyan);
                }
            }
        }

        int testIndex = GetBoundingQuadNodeIndex(testPoint.position);

        List<int> testPointNeighbours = NeighbouringToCheckCells(testIndex);
        for (int i = 0; i < testPointNeighbours.Count; i++)
        {
            quadTreeNodes[testPointNeighbours[i]].DrawQuad(Color.red);
        }
    }

    private int GetBoundingQuadNodeIndex(Vector3 point)
    {
        int qNIndex = 0;

        int curCheckIndex = 1;
        int curCheckingDepth = 1;

        bool found = true;

        while(curCheckingDepth < maxDepth && found)
        {
            for (int i = curCheckIndex; i < quadTreeNodes.Length; i++)
            {
                if (quadTreeNodes[i] != null && quadTreeNodes[i].PointInQuadNode(point))
                {
                    //Debug.Log("FOUND CONTAINING QUAD : " + i);
                    qNIndex = i;
                    curCheckIndex = (4 * i) + 1;
                    found = true;
                    break;
                }
                else
                {
                    found = false;
                }
            }

            curCheckingDepth++;
        }
        //Debug.Log(curCheckIndex);

        Debug.DrawLine(transform.position, quadTreeNodes[qNIndex].center, Color.green);

        return qNIndex;
    }

    public void GenerateQuadTreeOfPts(List<GameObject> points)
    {
        int lengthQuadTree = GetTotalLength();
        quadTreeNodes = new QuadNode[lengthQuadTree];

        for (int i = 0; i < points.Count; i++)
        {
            InsertPoint(points[i].transform.position, 0);
        }
    }

    private void InsertPoint(Vector3 point, int quadNodeIndex)
    {
        if(quadTreeNodes[quadNodeIndex] == null)
        {
            CreateQuadNode(quadNodeIndex);
        }

        quadTreeNodes[quadNodeIndex].occupyingPoints.Add(point);

        //Debug.Log("Added points");

        if (quadTreeNodes[quadNodeIndex].depth < maxDepth
        && quadTreeNodes[quadNodeIndex].occupyingPoints.Count >= maxPointsInCell)
        {
            int childNodeS = 4 * quadNodeIndex;

            for (int i = 1; i <= 4; i++)
            {
                //Debug.Log(childNodeS + i + " / " + quadTreeNodes.Length);
                if (quadTreeNodes[childNodeS + i] == null)
                {
                    //Debug.Log("Created child node : " + childNodeS + i);
                    CreateQuadNode(childNodeS + i);
                }
                if (quadTreeNodes[childNodeS + i].PointInQuadNode(point))
                {
                    //Debug.Log("Added point in child : " + childNodeS + i);
                    InsertPoint(point, childNodeS + i);
                    break;
                }
            }
        }
    }

    private void CreateQuadNode(int quadNodeIndex)
    {
        int quadPos = quadNodeIndex % 4;
        float curSize = totalSize;

        quadTreeNodes[quadNodeIndex] = new QuadNode();
        quadTreeNodes[quadNodeIndex].occupyingPoints = new List<Vector3>();

        if (quadNodeIndex > 0)
        {
            int parentIndex = (quadNodeIndex - quadPos) / 4;
            if (quadPos == 0)
            {
                parentIndex -= 1;
            }

            int quadDepth = quadTreeNodes[parentIndex].depth + 1;
            quadTreeNodes[quadNodeIndex].depth = quadDepth;

            int keyQP = quadPos == 0 ? 4 : quadPos;
            quadTreeNodes[quadNodeIndex].ID = keyQP + 10 * quadTreeNodes[parentIndex].ID;
            quadNodeIdToIndex.Add(quadTreeNodes[quadNodeIndex].ID, quadNodeIndex);

            curSize = quadTreeNodes[parentIndex].Size() / 2;

            Vector3 centerOfset = Vector3.zero;

            Vector3 parentCenter = quadTreeNodes[parentIndex].center;

            if (quadPos == 1)
            {
                centerOfset = new Vector3(-1.0f, 0.0f, 1.0f) * curSize;
            }
            else if (quadPos == 2)
            {
                centerOfset = new Vector3(-1.0f, 0.0f, -1.0f) * curSize;
            }
            else if (quadPos == 3)
            {
                centerOfset = new Vector3(1.0f, 0.0f, -1.0f) * curSize;
            }
            else if (quadNodeIndex != 0)
            {
                centerOfset = new Vector3(1.0f, 0.0f, 1.0f) * curSize;
            }

            quadTreeNodes[quadNodeIndex].center = parentCenter + centerOfset;
            quadTreeNodes[quadNodeIndex].SetSize(curSize);
        }
        else
        {
            quadNodeIdToIndex.Add(0, 0);
            quadTreeNodes[quadNodeIndex].SetSize(curSize);
            quadTreeNodes[quadNodeIndex].ID = 0;
        }
    }

    private int GenerateQuadNodeID(int quadNodeIndex)
    {
        if(quadNodeIndex == 0)
        {
            return 0;
        }

        return quadTreeNodes[quadNodeIndex].depth + 10 * GenerateQuadNodeID(quadNodeIndex / 4);
    }

    private List<int> NeighbouringToCheckCells(int quadNodeIndex)
    {
        List<int> returnIndecies = new List<int>();

        if(quadNodeIndex == 0 || quadTreeNodes[quadNodeIndex] == null)
        {
            return returnIndecies;
        }

        int curID = quadTreeNodes[quadNodeIndex].ID;
        int[] nodeIDs = GetIntsFromNum(curID);

        if(nodeIDs[0] != 1 && nodeIDs[0] != 4)
        {
            int[] upNeigh = GetUpNeighbour(nodeIDs);

            int value = -1;
            if (quadNodeIdToIndex.TryGetValue(GetNumFrmInts(upNeigh), out value))
            {
                returnIndecies.Add(value);
            }

            if (upNeigh[0] != 1 && upNeigh[0] != 2)
            {
                int[] toLeftNeigh = GetLeftNeighbour(upNeigh);

                value = -1;
                if (quadNodeIdToIndex.TryGetValue(GetNumFrmInts(toLeftNeigh), out value))
                {
                    returnIndecies.Add(value);
                }
            }
        }

        if (nodeIDs[0] != 1 && nodeIDs[0] != 2)
        {
            int[] leftNeigh = GetLeftNeighbour(nodeIDs);

            int value = -1;
            if (quadNodeIdToIndex.TryGetValue(GetNumFrmInts(leftNeigh), out value))
            {
                returnIndecies.Add(value);
            }

            if (leftNeigh[0] != 2 && leftNeigh[0] != 3)
            {
                int[] botLft = GetDownNeighbour(leftNeigh);

                value = -1;
                if(quadNodeIdToIndex.TryGetValue(GetNumFrmInts(botLft), out value))
                {
                    returnIndecies.Add(value);
                }
            }
        }

        return returnIndecies;
    }

    private int[] GetLeftNeighbour(int[] a)
    {
        int startFlipFrmIndex = a.Length - 1; ;
        for (int i = a.Length - 1; i >= 0; i--)
        {
            if(a[i] == 1 || a[i] == 2)
            {
                startFlipFrmIndex = i - 1;
            }
            else
            {
                break;
            }
        }

        if(startFlipFrmIndex < 0)
        {
            return new int[0];
        }

        int[] leftNode = (int[])a.Clone();
        for (int i = startFlipFrmIndex; i < a.Length; i++)
        {
            leftNode[i] = FlipTableHori(leftNode[i]);
        }

        return leftNode;
    }

    private int[] GetUpNeighbour(int[] a)
    {
        int startFlipFrmIndex = a.Length - 1; ;
        for (int i = a.Length - 1; i >= 0; i--)
        {
            if (a[i] == 1 || a[i] == 4)
            {
                startFlipFrmIndex = i - 1;
            }
            else
            {
                break;
            }
        }

        if (startFlipFrmIndex < 0)
        {
            return new int[0];
        }

        int[] upNode = (int[])a.Clone();
        for (int i = startFlipFrmIndex; i < a.Length; i++)
        {
            upNode[i] = FlipTableVert(upNode[i]);
        }

        return upNode;
    }

    private int[] GetDownNeighbour(int[] a)
    {
        int startFlipFrmIndex = a.Length - 1; ;
        for (int i = a.Length - 1; i >= 0; i--)
        {
            if (a[i] == 2 || a[i] == 3)
            {
                startFlipFrmIndex = i - 1;
            }
            else
            {
                break;
            }
        }

        if (startFlipFrmIndex < 0)
        {
            return new int[0];
        }

        int[] downNode = (int[])a.Clone();
        for (int i = startFlipFrmIndex; i < a.Length; i++)
        {
            downNode[i] = FlipTableVert(downNode[i]);
        }

        return downNode;
    }

    private int FlipTableVert(int a)
    {
        switch (a)
        {
            case 1:
                return 2;
            case 2:
                return 1;
            case 3:
                return 4;
            case 4:
                return 3;
            default:
                return -1;
        }
    }

    private int FlipTableHori(int a)
    {
        switch (a)
        {
            case 1:
                return 4;
            case 2:
                return 3;
            case 3:
                return 2;
            case 4:
                return 1;
            default:
                return -1;
        }
    }

    private int[] GetIntsFromNum(int num)
    {
        List<int> listOfInts = new List<int>();
        while (num > 0)
        {
            listOfInts.Add(num % 10);
            num = num / 10;
        }
        listOfInts.Reverse();
        return listOfInts.ToArray();
    }

    private int GetNumFrmInts(int[] ints)
    {
        int num = 0;
        for (int i = 0; i < ints.Length; i++)
        {
            num = 10 * num + ints[i];
        }
        return num;
    }

    private int GetTotalLength()
    {
        int totalNodes = 0;
        for (int i = 0; i <= maxDepth; i++)
        {
            totalNodes += (int)Mathf.Pow(4, i);
        }

        return totalNodes;
    }
}
