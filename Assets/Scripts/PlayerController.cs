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
    public BoxCollider2D boxCollider;
    public Animator animator; 
    public SpriteRenderer sprite;
    public Transform lightAttack;

    private Transform _lightAttackTrans;
    private SpriteRenderer _lightAttackSprite;
    
    private float moveDirection;
    private float groundMove;
    private float airMove;
    private bool moveAble;
    private float yScale;
    private int crouched = 0;
    private float lightAttackCooldown = 4;

    public int hp = 10;
    [SerializeField] public float lerpRate = 4f;
    [Range(0.1f, 10f)] public float jumpPower;

    public float crouchcheck = 4;
    public float fallMultiplier = 1.1f;
    [Range(0.1f, 100f)] public float moveSpeed;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        yScale = boxCollider.size.y/2f + Math.Abs(boxCollider.offset.y);
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();

        lightAttack = transform.Find("LightAtk");
        _lightAttackTrans = lightAttack.GetComponent<Transform>();
        _lightAttackSprite = lightAttack.GetComponent<SpriteRenderer>();
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

    public void Crouch(InputAction.CallbackContext context){
        //want to pick a point thats like, player.transform.lossyscale.y - 10, and if there's only background there lerp to it
        print("crouch attempt"); 
        RaycastHit2D hit = Physics2D.Raycast((Vector2)trans.position + Vector2.down*crouchcheck, Vector2.down, 1,8);
        if (hit == false && isGrounded()){
            moveAble = false;
            trans.position = Vector2.Lerp(trans.position, (Vector2)trans.position + Vector2.down*crouchcheck, 1);
            moveAble = true;
        }
        print(hit == false);
        crouched = 10;
    }

    public void Fire(InputAction.CallbackContext context){
        print("Fire attempt");
        if (lightAttackCooldown == 0){
            _lightAttackSprite.color = Color.red;
            Collider2D[] enemies = Physics2D.OverlapCircleAll(_lightAttackTrans.position, _lightAttackTrans.lossyScale.x, 32);
            if (enemies.Length > 0){
                foreach (Collider2D enemy in enemies){
                    PlayMakerFSM fsm = enemy.gameObject.GetComponent<PlayMakerFSM>();
                    fsm.SendEvent("Take Damage");
                }
            }
            lightAttackCooldown = 10;
        }
        //TODO: implement 
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

    public void GrabPlatform(Vector2 target)
    {
        if (!isGrounded() && crouched == 0)
        {
            target = target + new Vector2(0, yScale +0.1f);
            moveAble = false;
            StartCoroutine(LiftPlatform(target));
        }
    }

    private IEnumerator LiftPlatform(Vector2 target)
    {
        Debug.Log("platform lift");
        float t = 0;
        Vector2 start = transform.position;
        while (t < 0.99f)
        {
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
        return Physics2D.Raycast(trans.position, Vector2.down, yScale +0.1f, 8);
    }

    public int TakeDamage(int hit){
        hp -= hit;
        return hit;
    }

    public void Bounce(){
        rb.velocity += Vector2.up *jumpPower *1.5f;
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

        if (crouched > 0)
        {
            crouched --;
        }

        if (lightAttackCooldown > 0){
            lightAttackCooldown --;
            if (lightAttackCooldown == 2){
                _lightAttackSprite.color = Color.clear;
            }
        }
            
        sprite.flipX = rb.velocity.x < 0;
        animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
    }
}
