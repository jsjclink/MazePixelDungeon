using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum ITEM_TYPE
{
    WEAPON, ARMOR, ARTIFACT, FOOD
}
public enum SPECIFIC_ITEM_TYPE
{
    NONE
}
public enum ITEM_NAME
{
    NONE, SWORD_01, AX_01, ARTIFACT_01, ARTIFACT_02, ARTIFACT_03, ARMOR_01, ARMOR_02, FOOD_01
}

[Serializable]
public class ItemInfo
{
    public int pos_x;
    public int pos_y;
    public ITEM_TYPE item_type;
    public SPECIFIC_ITEM_TYPE specific_item_type;
    public ITEM_NAME item_name;
    public Sprite item_image;
    public int enchant_cnt;

    public ItemInfo(int pos_x, int pos_y, ITEM_TYPE item_type, SPECIFIC_ITEM_TYPE specific_item_type, ITEM_NAME item_name, int enchant_cnt)
    {
        this.pos_x = pos_x;
        this.pos_y = pos_y;
        this.item_type = item_type;
        this.specific_item_type = specific_item_type;
        this.item_name = item_name;
        this.enchant_cnt = enchant_cnt;
        Sprite[] sprites = Resources.LoadAll<Sprite>("items");
        switch (item_name)
        {
            case ITEM_NAME.NONE:
                break;
            case ITEM_NAME.SWORD_01:
                this.item_image = sprites[66];
                break;
            case ITEM_NAME.AX_01:
                this.item_image = sprites[67];
                break;
            case ITEM_NAME.ARTIFACT_01:
                this.item_image = sprites[164];
                break;
            case ITEM_NAME.ARMOR_01:
                this.item_image = sprites[118];
                break;
            case ITEM_NAME.ARMOR_02:
                this.item_image = sprites[119];
                break;
            case ITEM_NAME.FOOD_01:
                this.item_image = sprites[275];
                break;
        }
    }
}
