using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UNIT_TYPE
{
    PLAYER, ENEMY_RAT, ENEMY_GNOLL, ENEMY_CRAB, ENEMY_BOOS_01
}
public enum UNIT_STATE
{
    IDLE, MOVING, ENGAGING, SLEEPING
}
public enum ANIMATION_STATE
{
    IDLE, MOVING, ENGAGING, SLEEPING
}

public class UnitInfo
{
    public UNIT_TYPE unit_type;
    public UNIT_STATE unit_state;

    public UnitInfo()
    {

    }
}
