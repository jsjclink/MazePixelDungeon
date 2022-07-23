using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum TERRAIN_TYPE
{
    EMPTY, FLOOR, WALL, BRIDGE, DOOR, STAIR
}
public enum TOUCH_INPUT_TYPE
{
    NONE, MOVE_INTERRUPT, PLAYER_MOVE_TO_POS, PLAYER_ENTER_STAIR, PLAYER_ATTACK_ENEMY
}
public enum TURN_TYPE
{
    ENEMY_TURN, PLAYER_TURN
}

public class TouchInfo
{
    public TOUCH_INPUT_TYPE type;
    public Vector3 move_to_pos;
    public MapInfo from;
    public MapInfo to;
    public EnemyInfo enemy;

    public TouchInfo(TOUCH_INPUT_TYPE type) //NONE, MOVE_INTERRUPT
    {
        this.type = type;
    }
    public TouchInfo(TOUCH_INPUT_TYPE type, MapInfo from, MapInfo to) // PLAYER_ENTER_STAIR
    {
        this.type = type;
        this.from = from;
        this.to = to;
    }
    public TouchInfo(TOUCH_INPUT_TYPE type, Vector3 touch_pos) // PLAYER_MOVE_TO_POS
    {
        this.type = type;
        this.move_to_pos = touch_pos;
    }
    public TouchInfo(TOUCH_INPUT_TYPE type, EnemyInfo enemy) // ATTACK_ENEMY
    {
        this.type = type;
        this.enemy = enemy;
    }
}

public class TurnInfo
{
    public TURN_TYPE turn_type;
    public UnitInfo unit;

    public TurnInfo(TURN_TYPE turn_type, UnitInfo unit)
    {
        this.turn_type = turn_type;
        this.unit = unit;
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

    Queue<TurnInfo> turn_queue = new Queue<TurnInfo>();

    Dungeon dungeon;

    int cnt = 0;

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
                playerInfo.SetState(UNIT_STATE.IDLE);
                break;
            case TOUCH_INPUT_TYPE.PLAYER_MOVE_TO_POS:
                playerInfo.SetPathTo((int)touch_info.move_to_pos.x, (int)touch_info.move_to_pos.y, map_arr, enemy_list);
                if(playerInfo.cur_path.Count != 0)
                {
                    playerInfo.SetState(UNIT_STATE.MOVING);
                    if (turn_queue.Count == 0) turn_queue.Enqueue(new TurnInfo(TURN_TYPE.PLAYER_TURN, playerInfo));
                }
                else
                {
                    playerInfo.SetState(UNIT_STATE.IDLE);
                }   
                break;
            case TOUCH_INPUT_TYPE.PLAYER_ENTER_STAIR:
                ChangeMap(touch_info.from, touch_info.to);
                playerInfo.SetState(UNIT_STATE.IDLE);
                break;
            case TOUCH_INPUT_TYPE.PLAYER_ATTACK_ENEMY:
                playerInfo.SetState(UNIT_STATE.ENGAGING, touch_info.enemy);
                if (turn_queue.Count == 0) turn_queue.Enqueue(new TurnInfo(TURN_TYPE.PLAYER_TURN, playerInfo));
                break;
        }

        //update info - every 0.1f sec (by turn)
        if(turn_queue.Count > 0)
        {
            float action_interval = Time.time - prev_action_time;
            if (action_interval > 0.2f)
            {
                List<EnemyInfo> turn_used_enemy_list = new List<EnemyInfo>();

                //ENGAGE
                TurnInfo turn_info = turn_queue.Dequeue();
                switch (turn_info.turn_type)
                {
                    case TURN_TYPE.PLAYER_TURN:
                        Debug.Log("turn_queue.Count : " + turn_queue.Count);
                        if (playerInfo.unit_state == UNIT_STATE.IDLE || playerInfo.unit_state == UNIT_STATE.SLEEPING) break;
                        switch (playerInfo.unit_state)
                        {
                            case UNIT_STATE.IDLE:
                                break;
                            case UNIT_STATE.MOVING:
                                Debug.Log("cur_path.count : " + playerInfo.cur_path.Count);
                                if (playerInfo.DetectObstacle(playerInfo.cur_path[0].Item1, playerInfo.cur_path[0].Item2, enemy_list))
                                {
                                    playerInfo.cur_path.Clear();
                                }
                                else
                                {
                                    playerInfo.SetPos(playerInfo.cur_path[0].Item1, playerInfo.cur_path[0].Item2);
                                    playerInfo.cur_path.RemoveAt(0);
                                }
                                if (playerInfo.cur_path.Count == 0) playerInfo.SetState(UNIT_STATE.IDLE);
                                break;
                            case UNIT_STATE.ENGAGING:
                                Debug.Log("ATTACKED ENEMY");
                                playerInfo.SetState(UNIT_STATE.IDLE);
                                break;
                            case UNIT_STATE.SLEEPING:
                                break;
                        }

                        foreach (EnemyInfo enemy in enemy_list)
                        {
                            bool contains = false;
                            foreach (UnitInfo e in turn_used_enemy_list) if (e == enemy) contains = true;
                            if (contains) continue;
                            int dx = playerInfo.pos_x - (int)enemy.pos_x;
                            int dy = playerInfo.pos_y - (int)enemy.pos_y;
                            if ((dx == -1 || dx == 0 || dx == 1) && (dy == -1 || dy == 0 || dy == 1))
                            {
                                turn_queue.Enqueue(new TurnInfo(TURN_TYPE.ENEMY_TURN, enemy));
                                turn_used_enemy_list.Add(enemy);
                            }
                        }
                        if (playerInfo.unit_state != UNIT_STATE.IDLE && playerInfo.unit_state != UNIT_STATE.SLEEPING) turn_queue.Enqueue(new TurnInfo(TURN_TYPE.PLAYER_TURN, playerInfo));

                        string output = "turn_queue : ";
                        foreach (TurnInfo cur in turn_queue)
                        {
                            output += cur.turn_type + ",";
                        }
                        Debug.Log(output);


                        break;
                    case TURN_TYPE.ENEMY_TURN:
                        playerInfo.hp -= 5;
                        break;
                }
                Debug.Log("playerInfo.hp" + playerInfo.hp);


                //attack을 queue에 넣은 애들 말고는 움직여야함
                foreach (EnemyInfo enemy in enemy_list)
                {
                    if (enemy.DetectPlayer(playerInfo, map_arr))
                    {
                        enemy.SetPathTo(playerInfo.pos_x, playerInfo.pos_y, map_arr, enemy_list, playerInfo);
                        if (enemy.cur_path.Count > 0)
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
                        if (enemy.cur_path.Count > 0)
                        {
                            if (enemy.DetectObstacle(enemy.cur_path[0].Item1, enemy.cur_path[0].Item2, enemy_list, playerInfo))
                            {
                                enemy.SetPathTo(enemy.cur_path[enemy.cur_path.Count - 1].Item1, enemy.cur_path[enemy.cur_path.Count - 1].Item2, map_arr, enemy_list, playerInfo);
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
        if (player_object.transform.position.x != playerInfo.pos_x || player_object.transform.position.y != playerInfo.pos_y)
        {
            player_object.transform.position = new Vector3(playerInfo.pos_x, playerInfo.pos_y, 0);
            cam.transform.position = new Vector3(playerInfo.pos_x, playerInfo.pos_y, -10);
        }
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

        //만약 ui를 터치 했을 때
        if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) return touch_info;

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
                else                                                                                      //이동중이 아닐 때 누르면(멈춰있을 때)
                {
                    Vector3 touch_pos = Camera.main.ScreenToWorldPoint(touch.position);
                    touch_pos = new Vector3(touch_pos.x + 0.3f, touch_pos.y + 0.3f, 0); //보정치 0.3f 더해주면 좀 더 터치가 정확해짐

                    if (playerInfo.pos_x == (int)touch_pos.x && playerInfo.pos_y == (int)touch_pos.y) //나를 눌렀을 때
                    {
                        if (map_arr[(int)touch_pos.y, (int)touch_pos.x] == (int)TERRAIN_TYPE.STAIR)   //그 위치가 계단과 같을 때
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
                    else                                                                               //나를 누르지 않았을 때
                    {
                        int dx = playerInfo.pos_x - (int)touch_pos.x;
                        int dy = playerInfo.pos_y - (int)touch_pos.y;
                        if ((dx == -1 || dx == 0 || dx == 1) && (dy == -1 || dy == 0 || dy == 1))       //내 주변을 눌렀는데
                        {
                            foreach (EnemyInfo enemy in enemy_list)
                            {
                                if (enemy.pos_x == (int)touch_pos.x && enemy.pos_y == (int)touch_pos.y) //그게 enemy면
                                {
                                    Debug.Log("ENEMY CLICKED in dist 1");
                                    return new TouchInfo(TOUCH_INPUT_TYPE.PLAYER_ATTACK_ENEMY, enemy);  //attack enemy
                                }
                            }
                        }

                        return new TouchInfo(TOUCH_INPUT_TYPE.PLAYER_MOVE_TO_POS, touch_pos);
                    }
                }
            }
        }
        return touch_info;
    }
}