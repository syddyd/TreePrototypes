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
    public GameObject lightAttack;

    private Transform _lightAttackTrans;
    private SpriteRenderer _lightAttackSprite;
    private LayerMask enemyMask; 
    private LayerMask foreGround;
    
    private float moveDirection;
    private float groundMove;
    private float airMove;
    private bool moveAble;
    private float yScale;
    private float xScale;
    private int crouched = 0;
    private float lightAttackCooldown = 4;
    private bool wallGrabbing = false;

    public int hp = 10;
    [SerializeField] public float lerpRate = 4f;
    private float jumpPower = 15;
    public int jumpsLeft = 3;
    public float crouchcheck = 4;
    public float fallMultiplier = 4.5f;
    private float moveSpeed = 800;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        yScale = boxCollider.size.y/2f + Math.Abs(boxCollider.offset.y);
        xScale = boxCollider.size.x/2f +Math.Abs(boxCollider.offset.x);
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();

        //lightAttack = transform.Find("LightAtk");
        _lightAttackTrans = lightAttack.GetComponent<Transform>();
        _lightAttackSprite = lightAttack.GetComponent<SpriteRenderer>();
        trans = rb.transform;
        moveAble = true;

        enemyMask = LayerMask.GetMask("Enemy");
        foreGround = LayerMask.GetMask("Ground");
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
                print("Jump Attempt");
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

            if (Jumpable()){
                moveAble = false;
                rb.velocity += Vector2.up * jumpPower * 0.5f;
                if (Physics2D.Raycast(trans.position, Vector2.left, xScale+0.1f, foreGround)){
                    rb.velocity += Vector2.right * jumpPower * 0.2f;
                    jumpsLeft --;
                } else {
                    rb.velocity += Vector2.left * jumpPower * 0.2f;
                    jumpsLeft --;
                }
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
        if (lightAttackCooldown == 0){
            _lightAttackSprite.color = Color.red;
            Collider2D[] enemies = Physics2D.OverlapCircleAll(_lightAttackTrans.position,2.4f, enemyMask);
            if (enemies.Length > 0){
                print("Fire attempt");
                foreach (Collider2D enemy in enemies){
                    PlayMakerFSM fsm = enemy.gameObject.GetComponent<PlayMakerFSM>();
                    fsm.FsmVariables.GetFsmFloat("Atkdir").SafeAssign(groundMove); 
                    fsm.SendEvent("Take Damage");
                }
            }
            lightAttackCooldown = 10;
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

    private bool Jumpable(){
//        print(wallGrabbing && jumpsLeft > 0);
        if (wallGrabbing && jumpsLeft > 0 ){
            return true;
        }
        jumpsLeft = 3;
        return false;
    }

    public int TakeDamage(int hit){
        hp -= hit;
        return hit;
    }

    public void FixedUpdate()
    {
        if (moveAble)
        {
            //supposed to indicate touching the wall and holding key after jumping but is tautology? 
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)){
                if (Physics2D.Raycast(trans.position, Vector2.left, xScale+0.1f, foreGround) || Physics2D.Raycast(trans.position, Vector2.right, xScale+0.1f, foreGround)){
                    wallGrabbing = true;
                    rb.velocity = new Vector2(rb.velocity.x, 0);
                }
            } else {
                wallGrabbing = false;
            } 

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
                rb.velocity = new Vector2(moveDirection * moveSpeed * Time.deltaTime, rb.velocity.y + Time.deltaTime * Physics2D.gravity.y * fallMultiplier);
            }
            //the signs aren't even correct? 
            else
            {
                rb.velocity = new Vector2(moveDirection * moveSpeed * Time.deltaTime, rb.velocity.y + Physics2D.gravity.y * Time.deltaTime);
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

        //I had a problem like this on A1, I was not updating velocity.y properly, something about setting it to a 
        //really high number but not carrying over the velocity from last frame?? so just 1 frame got the jump velocity
            
        sprite.flipX = rb.velocity.x < 0;
        animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
    }
}
