using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerInfoPopUp : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    Image image;
    [SerializeField]
    GameObject player;
    [SerializeField]
    TMP_Text text;
    [SerializeField]
    GameSystemManager gameSystemManager;

    [SerializeField]
    GameObject player_info_pop_up;

    public void Start()
    {
        image.sprite = player.GetComponent<SpriteRenderer>().sprite;
        image.color = new Color(1, 1, 1, 1);

        text.text = gameSystemManager.player_info.layer_idx + "-" + gameSystemManager.player_info.map_idx;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GameObject player_info = Instantiate(player_info_pop_up, new Vector3(540, 960), Quaternion.identity, GameObject.Find("Canvas").transform);
        player_info.name = "PlayerInfoPopUp";

        player_info.GetComponentsInChildren<Image>()[2].sprite = image.sprite;
        player_info.GetComponentsInChildren<TMP_Text>()[0].text = "HP : " + gameSystemManager.player_info.cur_hp + "/" + gameSystemManager.player_info.max_hp;
        player_info.GetComponentsInChildren<TMP_Text>()[1].text = "Attack_pt : " + gameSystemManager.player_info.attack_pt;
        player_info.GetComponentsInChildren<TMP_Text>()[2].text = "Hunger : " + gameSystemManager.player_info.hunger;
        player_info.GetComponentsInChildren<TMP_Text>()[3].text = "Pos : " + gameSystemManager.player_info.layer_idx + "-" + gameSystemManager.player_info.map_idx;
    }

    

}
