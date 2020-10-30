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

    public QuadNode[] children;

    public float Size()
    {
        return _size;
    }

    public void SetSize(float size)
    {
        _size = size;
        FillCorners();
    }

    public bool BoundsPoint(Vector3 point)
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

    public int maxNeighbourDepth = 3;

    public float totalSize = 20.0f;

    public bool drawFullTree = true;
    public int drawOnlyThisDepth = 0;

    public Transform testPoint;

    public int lastLayerIndex = 0;

    public float maxPrizimSIze = 3.0f;

    public int clampMaxDepth = 5;

    public QuadNode root;

    public int offset = 1;

    public List<QuadNode> leafNodes = new List<QuadNode>();

    private void Update()
    {
        for (int i = 0; i < leafNodes.Count; i++)
        {
            if (drawFullTree)
            {
                leafNodes[i].DrawQuad(Color.yellow);
            }
            //Debug.Log(leafNodes[i].ID +" : " + leafNodes[i].occupyingPoints.Count);
            //if (leafNodes[i].occupyingPoints.Count > 0)
            //{
            //    for (int j = 0; j < leafNodes[i].occupyingPoints.Count; j++)
            //    {
            //        Debug.Log(leafNodes[i].occupyingPoints[j]);
            //    }
            //}
        }

        //DrawQuadTreeFrm(root);

        //int testIndex = GetBoundingQuadNodeIndex(testPoint.position, root);
        //
        ////Debug.Log(quadTreeNodes[testIndex].ID);
        //List<int> testPointNeighbours = new List<int>();
        ////Debug.Log(D_NeighbouringToCheckCells(ValidQuadNodeOfID(root, GetIntsFromNum(testIndex), offset), ref testPointNeighbours).Count);
        //D_NeighbouringToCheckCells(ValidQuadNodeOfID(root, GetIntsFromNum(testIndex), offset), ref testPointNeighbours);
        //for (int i = 0; i < testPointNeighbours.Count; i++)
        //{
        //    //Debug.Log(testPointNeighbours[i]);
        //    QuadNode curQuad = ValidQuadNodeOfID(root, GetIntsFromNum(testPointNeighbours[i]), 0);
        //    if(curQuad != null)
        //    {
        //        //Debug.Log("Drawing quad : " + curQuad.ID);
        //        curQuad.DrawQuad(Color.red);
        //    }
        //}

        //List<QuadNode> leafsOfThis = new List<QuadNode>();
        //LeafNodes(GetQuadFrmID(testIndex, offset), ref leafsOfThis);
        //
        ////Debug.Log(leafsOfThis.Count);
        //
        //for (int i = 0; i < leafsOfThis.Count; i++)
        //{
        //    leafsOfThis[i].DrawQuad(Color.green);
        //}

        //Debug.Log(quadTreeNodes[testIndex].ID + " : " + testPointNeighbours.Count);
        //Debug.Log(lastLayerIndex + 1);
    }

    private int GetBoundingQuadNodeIndex(Vector3 point, QuadNode beginNode)
    {
        if(beginNode == null)
        {
            return -1;
        }

        if (!beginNode.BoundsPoint(point))
        {
            return beginNode.ID;
        }

        QuadNode curNode = beginNode;
        bool found = false;
        while(curNode.children != null)
        {
            found = false;
            for (int i = 0; i < curNode.children.Length; i++)
            {
                if (curNode.children[i] != null && curNode.children[i].BoundsPoint(point))
                {
                    curNode = curNode.children[i];
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                break;
            }
        }

        return curNode.ID;
    }

    public void GenerateQuadTreeOfPts(List<GameObject> points, ref QuadNode root)
    {
        //maxDepth = ((int)totalSize / (int)(maxPrizimSIze / 2));

        //maxDepth = Mathf.Clamp(maxDepth, 0, clampMaxDepth);

        for (int i = 0; i < points.Count; i++)
        {
            InsertPoint(points[i].transform.position, i, ref root, null, -1);
        }
        LeafNodes(root, ref leafNodes);
        //Debug.Log(lengthQuadTree);
    }

    private void InsertPoint(Vector3 point, int pointIndex, ref QuadNode insertIn, QuadNode parentQuad, int childIndex)
    {
        if(insertIn == null)
        {
            CreateQuadNode(ref insertIn, parentQuad, childIndex);
        }

        //Debug.Log("Added points to : " + insertIn.ID);
        //Debug.Log("Adding : " + point + " to : " + insertIn.ID);

        insertIn.occupyingPoints.Add(point);
        insertIn.occupyingPointsIndex.Add(pointIndex);

        if (insertIn.depth < maxDepth
        && (insertIn.occupyingPoints.Count > maxPointsInCell || insertIn.children != null))
        {
            //Debug.Log("Added to new child in : " + insertIn.ID);
            insertIn.type = QuadNodeType.TreeNode;

            if (insertIn.children == null)
            {
                //Debug.Log("Subdivided : " + insertIn.ID);
                insertIn.children = new QuadNode[4];
            }

            //Debug.Log(insertIn.occupyingPoints.Count);
            for (int j = 0; j < 4; j++)
            {
                if (insertIn.children[j] == null)
                {
                    CreateQuadNode(ref insertIn.children[j], insertIn, j);
                    insertIn.type = QuadNodeType.LeafNode;
                    //Debug.Log("Created null child : " + insertIn.children[j].ID);
                }

                for (int k = 0; k < insertIn.occupyingPoints.Count; k++)
                {
                    //print("ADDED ALREADY: " + insertIn.occupyingPoints[k]);
                    Vector3 ptToAdd = insertIn.occupyingPoints[k];
                    //Vector3 ptToAdd = point;
                    int ptIndexToAdd = insertIn.occupyingPointsIndex[k];
                    //int ptIndexToAdd = pointIndex;
                    if (insertIn.children[j].BoundsPoint(ptToAdd))
                    {
                        //Debug.Log("ASKING TO ADD point : " + ptToAdd + " in child : " + insertIn.children[j].ID);
                        InsertPoint(ptToAdd, ptIndexToAdd, ref insertIn.children[j], insertIn, j);
                        insertIn.children[j].type = QuadNodeType.LeafNode;
                    }
                }
            }
            insertIn.occupyingPoints = new List<Vector3>();
            insertIn.occupyingPointsIndex = new List<int>();
        }
    }

    private void CreateQuadNode(ref QuadNode createNode, QuadNode parentNode, int childIndex)
    {
        float curSize = totalSize;

        createNode = new QuadNode();
        createNode.occupyingPoints = new List<Vector3>();
        createNode.occupyingPointsIndex = new List<int>();

        if (parentNode != null)
        {
            int quadPos = childIndex + 1;

            int quadDepth = parentNode.depth + 1;
            createNode.depth = quadDepth;

            int keyQP = quadPos == 0 ? 4 : quadPos;
            createNode.ID = keyQP + 10 * parentNode.ID;

            //if(createNode.ID == 45212)
            //{
            //    Debug.Log("THE KEY WITH NUMBER 5 ON IT WAS FOUND" + createNode.depth);
            //}

            //Debug.Log(createNode.ID);

            curSize = parentNode.Size() / 2;

            Vector3 centerOfset = Vector3.zero;

            Vector3 parentCenter = parentNode.center;

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
            else
            {
                centerOfset = new Vector3(1.0f, 0.0f, 1.0f) * curSize;
            }

            createNode.center = parentCenter + centerOfset;
            createNode.SetSize(curSize);
        }
        else
        {
            createNode.SetSize(curSize);
            createNode.ID = 0;
            createNode.depth = 0;
        }
    }

    public void NeighbouringToCheckCells(QuadNode quadNode, ref List<int> returnIndecies)
    {
        returnIndecies = new List<int>();

        if (quadNode == null || quadNode.depth == 0)
        {
            return;
        }

        int curID = quadNode.ID;
        int[] nodeIDs = GetIntsFromNum(curID);

        while (nodeIDs.Length > maxNeighbourDepth)
        {
            nodeIDs = ParentId(nodeIDs);
        }

        AddNeighbourNodeToList(nodeIDs, ref returnIndecies, root);

        int lastEleIndex = nodeIDs.Length - 1;
        if (HasUp(nodeIDs))
        {
            int[] upNeigh = GetUpNeighbour(nodeIDs);

            AddNeighbourNodeToList(upNeigh, ref returnIndecies, root);

            if (HasLeft(upNeigh))
            {
                int[] toLeftNeigh = GetLeftNeighbour(upNeigh);

                AddNeighbourNodeToList(toLeftNeigh, ref returnIndecies, root);
            }
        }

        if (HasLeft(nodeIDs))
        {
            int[] leftNeigh = GetLeftNeighbour(nodeIDs);

            AddNeighbourNodeToList(leftNeigh, ref returnIndecies, root);
            if (HasDown(leftNeigh))
            {
                int[] botLft = GetDownNeighbour(leftNeigh);

                AddNeighbourNodeToList(botLft, ref returnIndecies, root);
            }
        }
    }

    public List<int> D_NeighbouringToCheckCells(QuadNode quadNode, ref List<int> nodeIndecies)
    {
        nodeIndecies = new List<int>();

        List<int> returnIndecies = new List<int>();

        if (quadNode == null || quadNode.depth == 0)
        {
            return returnIndecies;
        }

        int curID = quadNode.ID;
        int[] nodeIDs = GetIntsFromNum(curID);

        while(nodeIDs.Length > maxNeighbourDepth)
        {
            nodeIDs = ParentId(nodeIDs);
        }

        D_AddNeighbourNodeToList(nodeIDs, ref nodeIndecies, ref returnIndecies, root);

        int lastEleIndex = nodeIDs.Length - 1;
        if (HasUp(nodeIDs))
        {
            int[] upNeigh = GetUpNeighbour(nodeIDs);

            D_AddNeighbourNodeToList(upNeigh, ref nodeIndecies, ref returnIndecies, root);

            if (HasLeft(upNeigh))
            {
                int[] toLeftNeigh = GetLeftNeighbour(upNeigh);

                D_AddNeighbourNodeToList(toLeftNeigh, ref nodeIndecies, ref returnIndecies, root);
            }
        }

        if (HasLeft(nodeIDs))
        {
            int[] leftNeigh = GetLeftNeighbour(nodeIDs);

            D_AddNeighbourNodeToList(leftNeigh, ref nodeIndecies, ref returnIndecies, root);

            if (HasDown(leftNeigh))
            {
                int[] botLft = GetDownNeighbour(leftNeigh);

                D_AddNeighbourNodeToList(botLft, ref nodeIndecies, ref returnIndecies, root);
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

        for (int i = 0; i < nums.Length; i++)
        {
            if(nums[i] != 1 && nums[i] != 4)
            {
                return true;
            }
        }

        return false;
    }

    private bool HasLeft(int[] nums)
    {
        if (nums.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < nums.Length; i++)
        {
            if (nums[i] != 1 && nums[i] != 2)
            {
                return true;
            }
        }

        return false;
    }

    private bool HasDown(int[] nums)
    {
        if (nums.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < nums.Length; i++)
        {
            if (nums[i] != 2 && nums[i] != 3)
            {
                return true;
            }
        }

        return false;
    }

    private void AddNeighbourNodeToList(int[] nodeID, ref List<int> prismIndecies, QuadNode beginQuad)
    {
        if (nodeID == null) return;
        if (nodeID.Length == 0) return;

        QuadNode curQuad = GetQuadFrmID(nodeID, offset);

        List<QuadNode> leafNodes = new List<QuadNode>();
        LeafNodes(curQuad, ref leafNodes);

        for (int i = 0; i < leafNodes.Count; i++)
        {
            if (leafNodes[i].occupyingPoints.Count >= 0)
            {
                prismIndecies.AddRange(leafNodes[i].occupyingPointsIndex);
            }
        }

    }

    private QuadNode ValidQuadNodeOfID(QuadNode beginQuad, int[] nodeID, int ofsett)
    {
        //if (nodeID == null) return null;
        if (nodeID.Length == 0) return null;
        //if (beginQuad.children == null) return beginQuad;
        //if (beginQuad.children.Length == 0) return beginQuad;

        int curNodeIDIndex = 1;
        QuadNode curQuad = beginQuad.children[nodeID[0] - 1];

        //if (curQuad == null) return beginQuad;
        //for (int i = 0; i < nodeID.Length; i++)
        //{
        //    Debug.Log(nodeID[i]);
        //}

        //Debug.Log(curNodeIDIndex < (nodeID.Length - ofsett) && !(curQuad.children == null));
        //int depth = Mathf.Min(nodeID.Length, maxNeighbourDepth);
        while (curNodeIDIndex < (nodeID.Length) 
            && curQuad != null
            && curQuad.children != null)
        {
            //Debug.Log(nodeID[curNodeIDIndex] - 1);
            curQuad = curQuad.children[nodeID[curNodeIDIndex] - 1];
            curNodeIDIndex++;
            if (curNodeIDIndex > (nodeID.Length))
            {
                break;
            }
        }

        return curQuad;
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

    private void D_AddNeighbourNodeToList(int[] nodeID, ref List<int> indecies, ref List<int> prismIndecies, QuadNode startFrm)
    {
        if (nodeID.Length == 0) return;

        QuadNode curQuad = GetQuadFrmID(nodeID, offset);

        List<QuadNode> leafNodes = new List<QuadNode>();
        LeafNodes(curQuad, ref leafNodes);

        for (int i = 0; i < leafNodes.Count; i++)
        {
            if(leafNodes[i].occupyingPoints.Count >= 0)
            {
                indecies.Add(leafNodes[i].ID);
                prismIndecies.AddRange(leafNodes[i].occupyingPointsIndex);
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

    private void DrawQuadTreeFrm(QuadNode startFrm)
    {
        if(startFrm != null && startFrm.children != null)
        {
            for (int i = 0; i < 4; i++)
            {
                if(startFrm.children[i] != null && startFrm.children[i].type == QuadNodeType.LeafNode)
                {
                    startFrm.children[i].DrawQuad(Color.cyan);
                }
                DrawQuadTreeFrm(startFrm.children[i]);
            }
        }
    }

    public void LeafNodes(QuadNode startFrm, ref List<QuadNode> leafNodes)
    {
        if (startFrm != null && startFrm.children != null)
        {
            for (int i = 0; i < 4; i++)
            {
                //if (startFrm.children[i] != null && startFrm.children[i].type == QuadNodeType.LeafNode)
                //{
                //    //leafNodes.Add(startFrm.children[i]);
                //    //startFrm.children[i].DrawQuad(Color.cyan);
                //}
                LeafNodes(startFrm.children[i], ref leafNodes);
            }
        }else if(startFrm != null && startFrm.children == null)
        {
            leafNodes.Add(startFrm);
        }
    }

    private void DrawQuadNodeUsnID(int[] ID)
    {
        QuadNode curNode = root.children[ID[0] - 1];
        int nxtIndex = 1;

        while(nxtIndex < ID.Length && curNode.children != null)
        {
            curNode = curNode.children[ID[nxtIndex] - 1];
            nxtIndex++;
            if (nxtIndex >= ID.Length)
            {
                break;
            }
        }

        curNode.DrawQuad(Color.yellow);
    }

    private QuadNode GetQuadFrmID(int ID, int ofsett)
    {
        if (ID <= 0) return null;
        int[] nodeID = GetIntsFromNum(ID);
        QuadNode curNode = root.children[nodeID[0] - 1];
        int nxtIndex = 1;

        while (nxtIndex < (nodeID.Length - ofsett) && curNode.children != null)
        {
            curNode = curNode.children[nodeID[nxtIndex] - 1];
            nxtIndex++;
            if (nxtIndex >= (nodeID.Length - ofsett))
            {
                break;
            }
        }

        return curNode;

    }

    private QuadNode GetQuadFrmID(int[] nodeID, int ofsett)
    {
        if (nodeID.Length <= 0) return null;
        QuadNode curNode = root.children[nodeID[0] - 1];
        int nxtIndex = 1;

        while (nxtIndex < (nodeID.Length - ofsett) && curNode.children != null)
        {
            curNode = curNode.children[nodeID[nxtIndex] - 1];
            nxtIndex++;
            if (nxtIndex >= (nodeID.Length - ofsett))
            {
                break;
            }
        }

        return curNode;

    }

}
