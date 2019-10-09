using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    Grid grid;
    public Transform StartPosition;
    public Transform TargetPosition;

    private void Awake()
    {
        grid = GetComponent<Grid>();
    }

    private void Update()
    {
        FindPath(StartPosition.position, TargetPosition.position);
    }
    void FindPath(Vector3 _StartPos, Vector3 _TargetPos)
    {
        Node StartNode = grid.NodeFromWorldPosition(_StartPos);
        Node TargetNode = grid.NodeFromWorldPosition(_TargetPos);

        List<Node> OpenList = new List<Node>();
        HashSet<Node> ClosedList = new HashSet<Node>();

        OpenList.Add(StartNode);

        while (OpenList.Count > 0)
        {
            Node CurrentNode = OpenList[0];
            for (int i = 1; i<OpenList.Count; i++)
            {
                if(OpenList[i].FCost < CurrentNode.FCost || OpenList[i].FCost == CurrentNode.FCost && OpenList[i].hCost < CurrentNode.hCost)
                {
                    CurrentNode = OpenList[i];
                }
            }
            OpenList.Remove(CurrentNode);
            ClosedList.Add(CurrentNode);

            if (CurrentNode == TargetNode)
            {
                GetFinalPath(StartNode, TargetNode);
                break;
            }

            foreach (Node NeighborNode in grid.GetNeighborNodes(CurrentNode))
            {
                if (!NeighborNode.isWall || ClosedList.Contains(NeighborNode))
                {
                    continue;
                }
                int moveCost = CurrentNode.gCost + GetManhattenDistance(CurrentNode, NeighborNode);

                if (!OpenList.Contains(NeighborNode) || moveCost < NeighborNode.FCost)
                {
                    NeighborNode.gCost = moveCost;
                    NeighborNode.hCost = GetManhattenDistance(NeighborNode, TargetNode);
                    NeighborNode.Parent = CurrentNode;

                    if (!OpenList.Contains(NeighborNode))
                    {
                        OpenList.Add(NeighborNode);
                    }
                }
            }
        }
    }
    void GetFinalPath(Node _StartingNode, Node _EndNode)
    {
        List<Node> FinalPath = new List<Node>();
        Node CurrentNode = _EndNode;

        while (CurrentNode != _StartingNode)
        {
            FinalPath.Add(CurrentNode);
            CurrentNode = CurrentNode.Parent;
        }

        FinalPath.Reverse();

        grid.FinalPath = FinalPath;
    }

    int GetManhattenDistance(Node _NodeA, Node _NodeB)
    {
        int ix = Mathf.Abs(_NodeA.gridX - _NodeB.gridX);
        int iy = Mathf.Abs(_NodeA.gridY - _NodeB.gridY);

        return ix + iy;
    }
}
