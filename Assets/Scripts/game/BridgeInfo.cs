using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BridgeInfo
{
    public SquareSpaceInfo from_space;
    public SquareSpaceInfo to_space;
    public List<(int, int)> path_arr;

    public BridgeInfo(SquareSpaceInfo from_space, SquareSpaceInfo to_space)
    {
        this.from_space = from_space;
        this.to_space = to_space;
        path_arr = new List<(int, int)>();

        int mid_x1 = (from_space.start_x + from_space.end_x) / 2;
        int mid_y1 = (from_space.start_y + from_space.end_y) / 2;
        int mid_x2 = (to_space.start_x + to_space.end_x) / 2;
        int mid_y2 = (to_space.start_y + to_space.end_y) / 2;

        path_arr = FindBridgePath(mid_x1, mid_y1, mid_x2, mid_y2);
    }

    private List<(int, int)> FindBridgePath(int x1, int y1, int x2, int y2)
    {
        List<(int, int)> ret_arr = new List<(int, int)>();
        int dx; int dy;
        if (x1 < x2) dx = 1;
        else dx = -1;
        if (y1 < y2) dy = 1;
        else dy = -1;

        for (int x = x1; x != x2; x += dx)
        {
            ret_arr.Add((x, y1));
        }
        ret_arr.Add((x2, y1));

        for (int y = y1; y != y2; y += dy)
        {
            ret_arr.Add((x2, y));
        }
        ret_arr.Add((x2, y2));

        return ret_arr;
    }
}