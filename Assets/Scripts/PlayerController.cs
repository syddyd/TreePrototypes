using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.iOS;
using HutongGames.PlayMaker;


public class PlayerController : MonoBehaviour
{
    public void Move(InputAction.CallbackContext context){
        Debug.Log("moved");
        if (Input.GetKeyDown(KeyCode.A)){
            Debug.Log("left");
        } else if (Input.GetKeyDown(KeyCode.D)){
            Debug.Log("right");
        }
    }
}
