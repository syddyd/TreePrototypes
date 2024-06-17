using System;
using System.Collections;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using UnityEngine;

public class PlatformLift : MonoBehaviour
{
    private float leftSide;
    private float rightSide;
    private float surface;
    Vector2 target;
    GameObject player;
    
    void Awake()
    {
        leftSide = transform.position.x - transform.lossyScale.x / 2;
        rightSide = transform.position.x + transform.lossyScale.x / 2;
        surface = transform.position.y + transform.lossyScale.y / 2 + 1.6f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        gameObject.SetActive(false);
        print("collision");
        if (collision.gameObject.CompareTag("Player"))
        {
            player = collision.gameObject;
            if (player.transform.position.x < leftSide)
            {
                target = new Vector2(leftSide, surface);
            }
            else if (player.transform.position.x > rightSide)
            {
                target = new Vector2(rightSide, surface);
            }
            else
            {
                target = new Vector2(player.transform.position.x, surface);
            }
            player.SendMessage("GrabPlatform", target);
            gameObject.SetActive(true);
        }
    }
}
