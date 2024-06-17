using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.iOS;
using HutongGames.PlayMaker;
using Unity.VisualScripting;
using System;
using HutongGames.PlayMaker.Actions;

public class PlayerController : MonoBehaviour
{
    public Rigidbody2D rb;
    private Transform trans;
    private float moveDirection;
    private float groundMove;
    private float airMove;
    private bool moveAble;

    [SerializeField] public float lerpRate = 4f;
    [Range(0.1f, 10f)] public float jumpPower;
    public float fallMultiplier = 1.1f;
    [Range(0.1f, 100f)] public float moveSpeed;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        trans = rb.transform;
        moveAble = true;
    }

    public void Move(InputAction.CallbackContext context)
    {
        groundMove = context.ReadValue<Vector2>().x;
        airMove = groundMove / 2f;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (moveAble)
        {
            if (isGrounded())
            {
                if (context.canceled)
                {
                    print("low jump");
                    rb.velocity += Vector2.up * jumpPower * 0.5f;
                }
                else
                {
                    rb.velocity += Vector2.up * jumpPower;
                }
            }
            else
            {
                StartCoroutine(JumpMemorizer(context));
            }
        }
    }

    //Method which accounts for the case where the player presses button too early 
    private IEnumerator JumpMemorizer(InputAction.CallbackContext context)
    {
        for (int i = 0; i < 30; i++)
        {
            if (isGrounded())
            {
                print("jump retry");
                Jump(context);
                break;
            }
            yield return null;
        }
    }

    public void FixedUpdate()
    {
        if (moveAble)
        {
            if (!isGrounded())
            {
                moveDirection = airMove;
            }
            else
            {
                moveDirection = groundMove;
            }
            if (rb.velocity.y < 0)
            {
                rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y + fallMultiplier * Physics.gravity.y * Time.deltaTime);
            }
            else
            {
                rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);
            }
        }
    }

    public void GrabPlatform(Vector2 target)
    {
        if (!isGrounded())
        {
            moveAble = false;
            StartCoroutine(LiftPlatform(target));
        }
    }

    private IEnumerator LiftPlatform(Vector2 target)
    {
        float t = 0;
        Vector2 start = transform.position;
        while (t < 0.99f)
        {
            print(target.x+","+target.y);
            t += lerpRate * Time.deltaTime;
            transform.position = Vector2.Lerp(start, target, t);
            yield return null;
        }
        moveAble = true;
        transform.position = target;
    }

    private bool isGrounded()
    {
        //Debug.DrawLine(trans.position, new Vector3(trans.position.x, trans.position.y -10f, trans.position.z), Color.red);
        return Physics2D.Raycast(trans.position, Vector2.down, 1.6f, 8);
    }
}
