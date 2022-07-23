using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TERRAIN_TYPE
{
    EMPTY, FLOOR, WALL, BRIDGE, DOOR, STAIR
}
public enum TOUCH_INPUT_TYPE
{
    NONE, MOVE_INTERRUPT, PLAYER_MOVE_TO_POS, PLAYER_ENTER_STAIR
}

public class TouchInfo
{
    public TOUCH_INPUT_TYPE type;
    public Vector3 move_to_pos;
    public MapInfo from;
    public MapInfo to;

    public TouchInfo(TOUCH_INPUT_TYPE type) //none, move_interrupt
    {
        this.type = type;
    }
    public TouchInfo(TOUCH_INPUT_TYPE type, MapInfo from, MapInfo to) //player_enter_stair
    {
        this.type = type;
        this.from = from;
        this.to = to;
    }
    public TouchInfo(TOUCH_INPUT_TYPE type, Vector3 touch_pos)
    {
        this.type = type;
        this.move_to_pos = touch_pos;
    }
}


public class GameSystemManager : MonoBehaviour
{
    [SerializeField]
    GameObject empty_prefab;
    [SerializeField]
    GameObject floor_prefab;
    [SerializeField]
    GameObject wall_prefab;
    [SerializeField]
    GameObject bridge_prefab;
    [SerializeField]
    GameObject door_prefab;
    [SerializeField]
    GameObject stair_prefab;

    [SerializeField]
    GameObject enemy_crab_prefab;

    [SerializeField]
    GameObject player_object;

    [SerializeField]
    Camera cam;

    float touch_began_time;
    float prev_action_time;

    PlayerInfo playerInfo;

    MapInfo cur_mapInfo;
    int[,] map_arr;
    List<GameObject> terrain_list = new List<GameObject>();
    List<EnemyInfo> enemy_list = new List<EnemyInfo>();
    Dictionary<EnemyInfo, GameObject> enemy_object_dict = new Dictionary<EnemyInfo, GameObject>();

    Dungeon dungeon;

    void Start()
    {
        Application.targetFrameRate = 20; // 배터리 최적화
        prev_action_time = Time.time;

        int difficulty = 3;
        dungeon = new Dungeon(difficulty);
        playerInfo = new PlayerInfo();

        MapInfo root = dungeon.hierarchy_list[0].mapInfos_of_layer[0][0];
        MapInfo first_map = dungeon.hierarchy_list[0].mapInfos_of_layer[1][0];

        ChangeMap(root, first_map);
    }

    private void Update()
    {
        //get input
        TouchInfo touch_info = GetTouch();

        //update info - by touch_info
        switch (touch_info.type)
        {
            case TOUCH_INPUT_TYPE.NONE:
                break;
            case TOUCH_INPUT_TYPE.MOVE_INTERRUPT:
                playerInfo.cur_path.Clear();
                break;
            case TOUCH_INPUT_TYPE.PLAYER_MOVE_TO_POS:
                playerInfo.SetPathTo((int)touch_info.move_to_pos.x, (int)touch_info.move_to_pos.y, map_arr, enemy_list);
                break;
            case TOUCH_INPUT_TYPE.PLAYER_ENTER_STAIR:
                ChangeMap(touch_info.from, touch_info.to);
                break;
        }

        //update info - every 0.1f sec (by turn)
        if (playerInfo.cur_path.Count > 0)
        {
            float action_interval = Time.time - prev_action_time;
            if (action_interval > 0.1f)
            {
                //player move
                if(playerInfo.DetectObstacle(playerInfo.cur_path[0].Item1, playerInfo.cur_path[0].Item2, enemy_list))
                {
                    playerInfo.cur_path.Clear();
                }
                else
                {
                    playerInfo.SetPos(playerInfo.cur_path[0].Item1, playerInfo.cur_path[0].Item2);
                    playerInfo.cur_path.RemoveAt(0);
                }
                
                //enemy move
                foreach(EnemyInfo enemy in enemy_list)
                {
                    if(enemy.DetectPlayer(playerInfo, map_arr))
                    {
                        enemy.SetPathTo(playerInfo.pos_x, playerInfo.pos_y, map_arr, enemy_list, playerInfo);
                        if(enemy.cur_path.Count > 0)
                        {
                            if (enemy.DetectObstacle(enemy.cur_path[0].Item1, enemy.cur_path[0].Item2, enemy_list, playerInfo))
                            {
                                enemy.SetPathTo(playerInfo.pos_x, playerInfo.pos_y, map_arr, enemy_list, playerInfo);
                            }
                            else
                            {
                                enemy.SetPos(enemy.cur_path[0].Item1, enemy.cur_path[0].Item2);
                                enemy.cur_path.RemoveAt(0);
                            }
                        }
                    }
                    else
                    {
                        if(enemy.cur_path.Count > 0)
                        {
                            if (enemy.DetectObstacle(enemy.cur_path[0].Item1, enemy.cur_path[0].Item2, enemy_list, playerInfo))
                            {
                                enemy.SetPathTo(enemy.cur_path[enemy.cur_path.Count -1].Item1, enemy.cur_path[enemy.cur_path.Count - 1].Item2, map_arr, enemy_list, playerInfo);
                            }
                            else
                            {
                                enemy.SetPos(enemy.cur_path[0].Item1, enemy.cur_path[0].Item2);
                                enemy.cur_path.RemoveAt(0);
                            }
                        }
                        else
                        {
                            List<SquareSpaceInfo> space_list = ((SquareRoomMapInfo)cur_mapInfo).space_list;
                            int idx = Random.Range(0, space_list.Count);
                            int move_x = Random.Range(space_list[idx].start_x + 1, space_list[idx].end_x - 1);
                            int move_y = Random.Range(space_list[idx].start_y + 1, space_list[idx].end_y - 1);
                            enemy.SetPathTo(move_x, move_y, map_arr, enemy_list, playerInfo);
                        }
                    }
                }

                prev_action_time = Time.time;
            }
        }

        //draw
        player_object.transform.position = new Vector3(playerInfo.pos_x, playerInfo.pos_y, 0);
        foreach(EnemyInfo enemy in enemy_list)
        {
            enemy_object_dict[enemy].transform.position = new Vector3(enemy.pos_x, enemy.pos_y, 0);
        }
    }


    private void ChangeMap(MapInfo from, MapInfo to)
    {
        //이전 정보 삭제
        foreach (GameObject cur in this.terrain_list)
        {
            Destroy(cur);
        }
        this.terrain_list.Clear();
        foreach(EnemyInfo cur in this.enemy_list)
        {
            Destroy(enemy_object_dict[cur]);
        }
        this.enemy_list = new List<EnemyInfo>();
        this.enemy_object_dict = new Dictionary<EnemyInfo, GameObject>();

        //정보 init
        (int[,], List<EnemyInfo>) infos = ((SquareRoomMapInfo)to).GetMapInfos();
        this.cur_mapInfo = to;
        this.map_arr = infos.Item1;
        this.enemy_list = infos.Item2;

        //draw terrain
        for (int i = 0; i < this.map_arr.GetLength(0); i++)
        {
            for (int j = 0; j < this.map_arr.GetLength(1); j++)
            {
                if (this.map_arr[i, j] == (int)TERRAIN_TYPE.EMPTY) terrain_list.Add(Instantiate(empty_prefab, new Vector3(j, i, 0), Quaternion.identity));
                else if (this.map_arr[i, j] == (int)TERRAIN_TYPE.FLOOR) terrain_list.Add(Instantiate(floor_prefab, new Vector3(j, i, 0), Quaternion.identity));
                else if (this.map_arr[i, j] == (int)TERRAIN_TYPE.WALL) terrain_list.Add(Instantiate(wall_prefab, new Vector3(j, i, 0), Quaternion.identity));
                else if (this.map_arr[i, j] == (int)TERRAIN_TYPE.BRIDGE) terrain_list.Add(Instantiate(bridge_prefab, new Vector3(j, i, 0), Quaternion.identity));
                else if (this.map_arr[i, j] == (int)TERRAIN_TYPE.DOOR) terrain_list.Add(Instantiate(door_prefab, new Vector3(j, i, 0), Quaternion.identity));
                else if (this.map_arr[i, j] == (int)TERRAIN_TYPE.STAIR) terrain_list.Add(Instantiate(stair_prefab, new Vector3(j, i, 0), Quaternion.identity));
            }
        }

        //이동한 star 위치에 player위치
        StairInfo target_stair = null;
        foreach (StairInfo cur in this.cur_mapInfo.stair_list)
        {
            if (cur.connected_map.id == from.id) target_stair = cur;
        }
        if (target_stair != null)
        {
            playerInfo.SetPos(target_stair.pos_x, target_stair.pos_y);
            player_object.transform.position = new Vector3(playerInfo.pos_x, target_stair.pos_y, 0);
        }
        else //일어나면 안되는 일!
        {
            Debug.Log("Fatal error : stair find failure");
            List<SquareSpaceInfo> space_list = ((SquareRoomMapInfo)cur_mapInfo).space_list;
            int idx = Random.Range(0, space_list.Count);
            int spawn_x = Random.Range(space_list[idx].start_x + 1, space_list[idx].end_x - 1);
            int spawn_y = Random.Range(space_list[idx].start_y + 1, space_list[idx].end_y - 1);
            playerInfo.SetPos(spawn_x, spawn_y);
            player_object.transform.position = new Vector3(spawn_x, spawn_y, 0);
        }

        //draw enemies
        foreach (EnemyInfo cur in this.enemy_list)
        {
           enemy_object_dict[cur] = Instantiate(enemy_crab_prefab, new Vector3(cur.pos_x, cur.pos_y, 0), Quaternion.identity);
        }

        cam.transform.position = new Vector3(playerInfo.pos_x, playerInfo.pos_y, -10);
    }

    private TouchInfo GetTouch()
    {
        TouchInfo touch_info = new TouchInfo(TOUCH_INPUT_TYPE.NONE);

        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                touch_began_time = Time.time;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                float touch_interval = Time.time - touch_began_time;
                if (touch_interval >= 0.2f) return new TouchInfo(TOUCH_INPUT_TYPE.NONE); // touch interval 짧으면 NONE return함

                if (playerInfo.cur_path.Count > 0) return new TouchInfo(TOUCH_INPUT_TYPE.MOVE_INTERRUPT); //이동중에 누르면 이동 취소
                else
                {
                    Vector3 touch_pos = Camera.main.ScreenToWorldPoint(touch.position);
                    touch_pos = new Vector3(touch_pos.x + 0.3f, touch_pos.y + 0.3f, 0); //보정치 0.3f 더해주면 좀 더 터치가 정확해짐

                    if (playerInfo.pos_x == (int)touch_pos.x && playerInfo.pos_y == (int)touch_pos.y)
                    {
                        if (map_arr[(int)touch_pos.y, (int)touch_pos.x] == (int)TERRAIN_TYPE.STAIR)
                        {
                            foreach (StairInfo cur in cur_mapInfo.stair_list)
                            {
                                if (cur.pos_x == (int)touch_pos.x && cur.pos_y == (int)touch_pos.y) // 해당하는 stair를 찾음
                                {
                                    return new TouchInfo(TOUCH_INPUT_TYPE.PLAYER_ENTER_STAIR, this.cur_mapInfo, cur.connected_map);
                                }
                            }
                        }
                    }
                    else
                    {
                        return new TouchInfo(TOUCH_INPUT_TYPE.PLAYER_MOVE_TO_POS, touch_pos);
                    }
                }
            }
        }
        return touch_info;
    }
}