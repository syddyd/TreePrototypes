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
    Collider2D thisCollider;
    
    void Awake()
    {
        leftSide = transform.position.x - transform.lossyScale.x / 2;
        rightSide = transform.position.x + transform.lossyScale.x / 2;
        surface = transform.position.y + transform.lossyScale.y / 2 + 0.1f;
        thisCollider = gameObject.GetComponent<Collider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
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
            //StartCoroutine(LiftPlayer(target));
        }
    }

    /*private IEnumerator LiftPlayer(Vector2 target)
    {
        print("lifting");
        thisCollider.isTrigger = true;
        while (Math.Abs(player.transform.position.x - target.x) > 0.2 || Math.Abs(player.transform.position.y - target.y) > 0.2)
        {
            print("passed condition");
            Vector3.MoveTowards(player.transform.position, target, Time.deltaTime);
            yield return null;
        }
        thisCollider.isTrigger = false;
    }*/
}
