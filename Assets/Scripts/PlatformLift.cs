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
    private bool fireAble = true;
    
    void Awake()
    {
        leftSide = transform.position.x - transform.lossyScale.x / 2;
        rightSide = transform.position.x + transform.lossyScale.x / 2;
        surface = transform.position.y + transform.lossyScale.y / 2 + 0.15f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        print("collision");
        if (collision.gameObject.CompareTag("Player") && fireAble)
        {
            player = collision.gameObject;
            surface += player.transform.lossyScale.y/2f;
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
            fireAble = false;
            player.SendMessage("GrabPlatform", target);
        }
    }
}
