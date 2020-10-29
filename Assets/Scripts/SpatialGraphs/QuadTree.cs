using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum QuadNodeType
{
    TreeNode,
    LeafNode
}

public class QuadNode
{
    public int ID = 0;

    public int depth = 0;
    private float _size = 3.0f;

    public Vector3 center = Vector3.zero;

    public List<int> occupyingPointsIndex;
    public List<Vector3> occupyingPoints;

    private Vector3[] corners = new Vector3[4];

    public QuadNodeType type;

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

    public bool drawFullTree = true;
    public int drawOnlyThisDepth = 0;

    public Transform testPoint;

    public int lastLayerIndex = 0;

    public QuadNode[] quadTreeNodes;
    private Dictionary<int, int> quadNodeIdToIndex = new Dictionary<int, int>();

    public float maxPrizimSIze = 3.0f;

    public int clampMaxDepth = 5;

    private void Update()
    {
        if(quadTreeNodes != null)
        {
            for (int i = 0; i < quadTreeNodes.Length; i++)
            {
                if (quadTreeNodes[i] != null)
                {
                    if (quadTreeNodes[i].depth == drawOnlyThisDepth || drawFullTree)
                    {
                        //Debug.Log("Drawing quad : " + i);
                        //Debug.Log(quadTreeNodes[i]._Size());
                        quadTreeNodes[i].DrawQuad(Color.cyan);
                    }
                }
            }
        }

        int testIndex = GetBoundingQuadNodeIndex(testPoint.position);
        //Debug.Log(quadTreeNodes[testIndex].ID);
        List<int> testPointNeighbours = new List<int>();
        D_NeighbouringToCheckCells(testIndex, ref testPointNeighbours);
        for (int i = 0; i < testPointNeighbours.Count; i++)
        {
            quadTreeNodes[testPointNeighbours[i]].DrawQuad(Color.red);
        }
        //Debug.Log(quadTreeNodes[testIndex].ID + " : " + testPointNeighbours.Count);
        //Debug.Log(lastLayerIndex + 1);
    }

    private int GetBoundingQuadNodeIndex(Vector3 point)
    {
        int qNIndex = 0;

        int checkAt = 0;
        int curCheckingDepth = 0;

        if(quadTreeNodes != null)
        {
            while (curCheckingDepth <= maxDepth)
            {
                if (checkAt < quadTreeNodes.Length && quadTreeNodes[checkAt] != null && quadTreeNodes[checkAt].PointInQuadNode(point))
                {
                    qNIndex = checkAt;
                    checkAt = 4 * checkAt + 1;
                    curCheckingDepth++;
                }
                else if (checkAt > quadTreeNodes.Length)
                {
                    break;
                }
                else
                {
                    checkAt++;
                }
            }
            Debug.DrawLine(transform.position, quadTreeNodes[qNIndex].center, Color.green);
        }

        return qNIndex;
    }

    public void GenerateQuadTreeOfPts(List<GameObject> points)
    {
        maxDepth = ((int)totalSize / (int)(maxPrizimSIze / 2));

        maxDepth = Mathf.Clamp(maxDepth, 0, clampMaxDepth);

        int lengthQuadTree = GetTotalLength((int)maxDepth);
        quadTreeNodes = new QuadNode[lengthQuadTree];
        quadNodeIdToIndex.Clear();

        for (int i = 0; i < points.Count; i++)
        {
            InsertPoint(points[i].transform.position, i, 0);
        }

        lastLayerIndex = lengthQuadTree - (int)Mathf.Pow(4, maxDepth);
        //Debug.Log(lengthQuadTree);
    }

    private void InsertPoint(Vector3 point, int pointIndex, int quadNodeIndex)
    {
        if (quadTreeNodes[quadNodeIndex] == null)
        {
            CreateQuadNode(quadNodeIndex);
        }

        //Debug.Log("Added points");

        quadTreeNodes[quadNodeIndex].occupyingPoints.Add(point);
        quadTreeNodes[quadNodeIndex].occupyingPointsIndex.Add(pointIndex);

        if (quadTreeNodes[quadNodeIndex].depth < maxDepth
        && quadTreeNodes[quadNodeIndex].occupyingPoints.Count >= maxPointsInCell)
        {
            quadTreeNodes[quadNodeIndex].type = QuadNodeType.TreeNode;
            int childNodeS = 4 * quadNodeIndex;

            for (int i = 1; i <= 4; i++)
            {
                //Debug.Log(childNodeS + i + " / " + quadTreeNodes.Length);
                if (quadTreeNodes[childNodeS + i] == null)
                {
                    //Debug.Log("Created child node : " + childNodeS + i);
                    CreateQuadNode(childNodeS + i);
                    quadTreeNodes[childNodeS + i].type = QuadNodeType.TreeNode;
                }
                if (quadTreeNodes[childNodeS + i].PointInQuadNode(point))
                {
                    //Debug.Log("Added point in child : " + childNodeS + i);
                    InsertPoint(point, pointIndex, childNodeS + i);
                    quadTreeNodes[childNodeS + i].type = QuadNodeType.LeafNode;

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
        quadTreeNodes[quadNodeIndex].occupyingPointsIndex = new List<int>();

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

    public void NeighbouringToCheckCells(int quadNodeIndex, ref List<int> returnIndecies)
    {
        returnIndecies = new List<int>();

        if (quadNodeIndex == 0 || quadTreeNodes[quadNodeIndex] == null)
        {
            return;
        }

        int curID = quadTreeNodes[quadNodeIndex].ID;
        int[] nodeIDs = GetIntsFromNum(curID);

        //AddNeighbourNodeToList(GetIntsFromNum(curID), ref returnIndecies);

        int lastEleIndex = nodeIDs.Length - 1;
        if (HasUp(nodeIDs))
        {
            int[] upNeigh = GetUpNeighbour(nodeIDs);

            AddNeighbourNodeToList(upNeigh, ref returnIndecies);

            if (HasLeft(upNeigh))
            {
                int[] toLeftNeigh = GetLeftNeighbour(upNeigh);

                AddNeighbourNodeToList(toLeftNeigh, ref returnIndecies);
            }
        }

        if (HasLeft(nodeIDs))
        {
            int[] leftNeigh = GetLeftNeighbour(nodeIDs);

            AddNeighbourNodeToList(leftNeigh, ref returnIndecies);
            if (HasDown(leftNeigh))
            {
                int[] botLft = GetDownNeighbour(leftNeigh);

                AddNeighbourNodeToList(botLft, ref returnIndecies);
            }
        }
    }

    public List<int> D_NeighbouringToCheckCells(int quadNodeIndex, ref List<int> nodeIndecies)
    {
        nodeIndecies = new List<int>();

        List<int> returnIndecies = new List<int>();

        if (quadNodeIndex == 0 || quadTreeNodes[quadNodeIndex] == null)
        {
            return returnIndecies;
        }

        int curID = quadTreeNodes[quadNodeIndex].ID;
        int[] nodeIDs = GetIntsFromNum(curID);

        D_AddNeighbourNodeToList(GetIntsFromNum(curID), ref nodeIndecies, ref returnIndecies);

        int lastEleIndex = nodeIDs.Length - 1;
        if (HasUp(nodeIDs))
        {
            int[] upNeigh = GetUpNeighbour(nodeIDs);

            D_AddNeighbourNodeToList(upNeigh, ref nodeIndecies, ref returnIndecies);
            
            if (HasLeft(upNeigh))
            {
                int[] toLeftNeigh = GetLeftNeighbour(upNeigh);

                D_AddNeighbourNodeToList(toLeftNeigh, ref nodeIndecies, ref returnIndecies);
            }
        }

        if (HasLeft(nodeIDs))
        {
            int[] leftNeigh = GetLeftNeighbour(nodeIDs);

            D_AddNeighbourNodeToList(leftNeigh, ref nodeIndecies, ref returnIndecies);

            if (HasDown(leftNeigh))
            {
                int[] botLft = GetDownNeighbour(leftNeigh);

                D_AddNeighbourNodeToList(botLft, ref nodeIndecies, ref returnIndecies);
            }
        }

        return returnIndecies;
    }

    private bool HasUp(int[] nums)
    {
        if (nums.Length == 0)
        {
            return false;
        }

        int lastEleIndex = nums.Length - 1;

        if (nums[0] != 1 && nums[0] != 4)
        {
            return true;
        }

        if (nums[lastEleIndex] == 1 || nums[lastEleIndex] == 4)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private bool HasLeft(int[] nums)
    {
        if (nums.Length == 0)
        {
            return false;
        }

        int lastEleIndex = nums.Length - 1;

        if (nums[0] != 1 && nums[0] != 2)
        {
            return true;
        }

        if (nums[lastEleIndex] == 1 || nums[lastEleIndex] == 2)
        {
            return false;
        }
        else
        {
            return true;
        }

    }

    private bool HasDown(int[] nums)
    {
        if (nums.Length == 0)
        {
            return false;
        }

        int lastEleIndex = nums.Length - 1;

        if (nums[0] != 2 && nums[0] != 3)
        {
            return true;
        }

        if (nums[lastEleIndex] == 2 || nums[lastEleIndex] == 3)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void AddNeighbourNodeToList(int[] nodeID, ref List<int> prismIndecies)
    {
        if (nodeID.Length == 0) return;
        int value = -1;

        while(!quadNodeIdToIndex.TryGetValue(GetNumFrmInts(nodeID), out value))
        {
            nodeID = ParentId(nodeID);
            if(nodeID.Length == 0)
            {
                break;
            }
        }

        if (value != -1)
        {
            if (quadTreeNodes[value].occupyingPointsIndex.Count > 0)
            {
                prismIndecies.AddRange(quadTreeNodes[value].occupyingPointsIndex);
            }
        }
    }

    private int[] ParentId(int[] childID)
    {
        int[] parentID = new int[childID.Length - 1];
        for (int i = 0; i < parentID.Length; i++)
        {
            parentID[i] = childID[i];
        }
        return parentID;
    }

    private void D_AddNeighbourNodeToList(int[] nodeID, ref List<int> indecies, ref List<int> prismIndecies)
    {
        if (nodeID.Length == 0) return;
        int value = -1;

        while (!quadNodeIdToIndex.TryGetValue(GetNumFrmInts(nodeID), out value))
        {
            nodeID = ParentId(nodeID);
            if (nodeID.Length == 0)
            {
                break;
            }
        }

        if (value != -1)
        {
            if (quadTreeNodes[value].occupyingPointsIndex.Count > 0)
            {
                indecies.Add(value);
                prismIndecies.AddRange(quadTreeNodes[value].occupyingPointsIndex);
            }
        }

    }

    private int[] GetLeftNeighbour(int[] a)
    {
        int startFlipFrmIndex = a.Length - 1;
        for (int i = a.Length - 1; i >= 0; i--)
        {
            if (a[i] == 1 || a[i] == 2)
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

    private int GetTotalLength(int _maxDepth)
    {
        int totalNodes = 0;
        for (int i = 0; i <= _maxDepth; i++)
        {
            totalNodes += (int)Mathf.Pow(4, i);
        }

        return totalNodes;
    }
}
