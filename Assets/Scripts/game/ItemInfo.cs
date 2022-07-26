using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ITEM_TYPE
{
    WEAPON, ARMOR, ARTIFACT
}
public enum SPECIFIC_ITEM_TYPE
{
    NONE
}
public enum ITEM_NAME
{
    NONE, SWORD_01, AX_01, ARTIFACT_01, ARMOR_01, ARMOR_02
}


public class ItemInfo
{
    public ITEM_TYPE item_type;
    public SPECIFIC_ITEM_TYPE specific_item_type;
    public ITEM_NAME item_name;
    public int enchant_cnt;

    public ItemInfo(ITEM_TYPE item_type, SPECIFIC_ITEM_TYPE specific_item_type, ITEM_NAME item_name, int enchant_cnt)
    {
        this.item_type = item_type;
        this.specific_item_type = specific_item_type;
        this.item_name = item_name;
        this.enchant_cnt = enchant_cnt;
    }
}
