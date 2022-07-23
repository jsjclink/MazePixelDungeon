using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class Item : ScriptableObject
{
    public string item_name;
    public Sprite item_image;
    public string item_explanation;


    public Item()
    {
        this.item_name = "new Item";
        this.item_explanation = "qls dhqmwprxm dlqslek.";
    }

}
