using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Slot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    Image image;
    [SerializeField]
    GameObject ItemInfoPopUp;

    private Item _item;
    public Item item
    {
        get { return _item; }
        set
        {
            _item = value;
            if(_item != null)
            {
                image.sprite = item.item_image;
                image.color = new Color(1, 1, 1, 1);
            }
            else
            {
                image.color = new Color(1, 1, 1, 0);
            }
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if(_item != null)
        {
            GameObject item_info = Instantiate(ItemInfoPopUp, new Vector3(540, 960), Quaternion.identity, GameObject.Find("Canvas").transform);
            item_info.name = "ItemInfoPopUp";

            item_info.GetComponentsInChildren<Image>()[2].sprite = _item.item_image;
            item_info.GetComponentsInChildren<TMP_Text>()[0].text = _item.item_name;
            item_info.GetComponentsInChildren<TMP_Text>()[1].text = _item.item_explanation;
        }

    }
}
