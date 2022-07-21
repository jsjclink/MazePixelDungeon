using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SquareRoom
{
    public class MapOption
    {
        public int num_of_room;
        public (int, int, int, int) padding;

        public MapOption(int num_of_room, (int, int, int, int) padding)
        {
            this.num_of_room = num_of_room;
            this.padding = padding;
        }
    }

    public class SquareSpaceInfo
    {
        public int start_x;
        public int start_y;
        public int end_x;
        public int end_y;

        public SquareSpaceInfo(int s_x, int s_y, int e_x, int e_y)
        {
            start_x = s_x;
            start_y = s_y;
            end_x = e_x;
            end_y = e_y;
        }
    }

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

            for(int x = x1; x != x2; x += dx)
            {
                ret_arr.Add((x, y1));
            }
            ret_arr.Add((x2, y1));

            for(int y = y1; y != y2; y += dy)
            {
                ret_arr.Add((x2, y));
            }
            ret_arr.Add((x2, y2));

            return ret_arr;
        }
    }


    public class SquareRoomMap
    {
        public int width; public int height;
        public List<SquareSpaceInfo> space_list;

        public SquareRoomMap(int width, int height)
        {
            this.width = width; this.height = height;
            space_list = DivideRect(new SquareSpaceInfo(2, 2, width - 2, height - 2), true);
        }

        public SquareRoomMap(int width, int height, MapOption mapOption)
        {
            this.width = width; this.height = height;
            space_list = DivideRect(new SquareSpaceInfo(2, 2, width - 2, height - 2), true);

            int remove_cnt = 0;
            if (space_list.Count - mapOption.num_of_room > 0) remove_cnt = space_list.Count - mapOption.num_of_room;
            for (int i = 0; i < remove_cnt; i++) RemoveRandomSpace();

            AddPaddingToRoom(mapOption.padding);
        }

        public int[,] GetMapArr()
        {
            int[,] map = new int[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    map[i, j] = 0;
                }
            }

            foreach (SquareSpaceInfo cur in space_list)
            {
                for (int i = cur.start_y; i < cur.end_y; i++)
                {
                    for (int j = cur.start_x; j < cur.end_x; j++)
                    {
                        if (i == cur.start_y || j == cur.start_x || i == cur.end_y - 1 || j == cur.end_x - 1) map[i, j] = 2; // wall
                        else map[i, j] = 1;
                    }
                }
            }

            List<SquareSpaceInfo> unconnected_space_list = new List<SquareSpaceInfo>(space_list);
            List<BridgeInfo> bridge_list = new List<BridgeInfo>();

            while(unconnected_space_list.Count > 1)
            {
                SquareSpaceInfo space1 = unconnected_space_list[0];
                SquareSpaceInfo space2 = unconnected_space_list[1];

                unconnected_space_list.RemoveAt(0);
                unconnected_space_list.RemoveAt(0);

                unconnected_space_list.Add(space1);

                bridge_list.Add(new BridgeInfo(space1, space2));
            }

            foreach(BridgeInfo cur in bridge_list)
            {
                for(int i = 0; i < cur.path_arr.Count; i++)
                {
                    int x = cur.path_arr[i].Item1;
                    int y = cur.path_arr[i].Item2;
                    if (map[y, x] == 2) map[y, x] = 4; //wall이면 문
                    else if (map[y, x] == 0) map[y, x] = 3;
                }
            }

            foreach (SquareSpaceInfo cur in space_list)
            {
                for (int i = cur.start_y; i < cur.end_y; i++)
                {
                    for (int j = cur.start_x; j < cur.end_x; j++)
                    {
                        if (map[i, j] == 4)
                        {
                            if (map[i+1, j] != 3 && map[i-1, j] != 3 && map[i, j+1] != 3 && map[i, j-1] != 3) //주변에 bridge가 없으면 문을 없앰 (벽 끝에 연속적으로 생성된 문을 floor로 바꿈)
                            {
                                map[i, j] = 1;
                            }
                        }
                    }
                }
            }

            return map;
        }

        private void RemoveRandomSpace()
        {
            space_list.RemoveAt(Random.Range(0, space_list.Count));
        }

        private void AddPaddingToRoom((int, int, int, int) padding)
        {
            int top = padding.Item1;
            int bottom = padding.Item2;
            int left = padding.Item3;
            int right = padding.Item4;
            Debug.Log("t b l r : " + top + ", " + bottom + ", " + left + ", " + right);
            foreach (SquareSpaceInfo cur in space_list)
            {
                if (cur.start_x + left + 7 < cur.end_x) cur.start_x += left;
                if (cur.start_y + top + 7 < cur.end_y) cur.start_y += top;
                if (cur.end_x - right - 7 > cur.start_x) cur.end_x -= right;
                if (cur.end_y - bottom - 7 > cur.start_y) cur.end_y -= bottom;
            }
        }

        private List<SquareSpaceInfo> DivideRect(SquareSpaceInfo cur, bool vertical)
        {
            if (cur.end_x - cur.start_x < 15 || cur.end_y - cur.start_y < 15) return new List<SquareSpaceInfo>();

            List<SquareSpaceInfo> arr1 = new List<SquareSpaceInfo>();
            List<SquareSpaceInfo> arr2 = new List<SquareSpaceInfo>();
            List<SquareSpaceInfo> ret_arr = new List<SquareSpaceInfo>();

            if (vertical)
            {
                int weight1 = Random.Range(1, 3);
                int weight2 = Random.Range(1, 3);
                int mid = (cur.start_x * weight1 + cur.end_x * weight2) / (weight1 + weight2);

                bool divide_vertical;
                if (mid - cur.start_x > cur.end_y - cur.start_y) divide_vertical = true;
                else divide_vertical = false;
                arr1 = DivideRect(new SquareSpaceInfo(cur.start_x, cur.start_y, mid, cur.end_y), divide_vertical);
                if (arr1.Count == 0)
                {
                    SquareSpaceInfo new_space = new SquareSpaceInfo(cur.start_x, cur.start_y, mid, cur.end_y);
                    ret_arr.Add(new_space);
                }
                while (arr1.Count > 0)
                {
                    ret_arr.Add(arr1[0]);
                    arr1.RemoveAt(0);
                }
                if (cur.end_x - mid > cur.end_y - cur.start_y) divide_vertical = true;
                else divide_vertical = false;
                arr2 = DivideRect(new SquareSpaceInfo(mid, cur.start_y, cur.end_x, cur.end_y), divide_vertical);
                if (arr2.Count == 0)
                {
                    SquareSpaceInfo new_space = new SquareSpaceInfo(mid, cur.start_y, cur.end_x, cur.end_y);
                    ret_arr.Add(new_space);
                }
                while (arr2.Count > 0)
                {
                    ret_arr.Add(arr2[0]);
                    arr2.RemoveAt(0);
                }
            }
            else
            {
                int weight1 = Random.Range(1, 3);
                int weight2 = Random.Range(1, 3);
                int mid = (cur.start_y * weight1 + cur.end_y * weight2) / (weight1 + weight2);

                bool divide_vertical;
                if (cur.end_x - cur.start_x > mid - cur.start_y) divide_vertical = true;
                else divide_vertical = false;
                arr1 = DivideRect(new SquareSpaceInfo(cur.start_x, cur.start_y, cur.end_x, mid), divide_vertical);
                if (arr1.Count == 0)
                {
                    SquareSpaceInfo new_space = new SquareSpaceInfo(cur.start_x, cur.start_y, cur.end_x, mid);
                    ret_arr.Add(new_space);
                }
                while (arr1.Count > 0)
                {
                    ret_arr.Add(arr1[0]);
                    arr1.RemoveAt(0);
                }
                if (cur.end_x - cur.start_x > cur.end_y - mid) divide_vertical = true;
                else divide_vertical = false;
                arr2 = DivideRect(new SquareSpaceInfo(cur.start_x, mid, cur.end_x, cur.end_y), divide_vertical);
                if (arr2.Count == 0)
                {
                    SquareSpaceInfo new_space = new SquareSpaceInfo(cur.start_x, mid, cur.end_x, cur.end_y);
                    ret_arr.Add(new_space);
                }
                while (arr2.Count > 0)
                {
                    ret_arr.Add(arr2[0]);
                    arr2.RemoveAt(0);
                }
            }
            return ret_arr;
        }
    }
}
