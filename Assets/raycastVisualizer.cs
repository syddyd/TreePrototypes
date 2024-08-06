using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class raycastVisualizer : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] PlayerController controller;
    private float scale;
    private Transform trans;
    void Start()
    {
        scale = controller.crouchcheck;
        trans = GetComponent<Transform>();
        print("visualizer "+ scale);
    }

    // Update is called once per frame
    void Update()
    {
        trans.position = (Vector2)player.transform.position + Vector2.down *scale;
    }
}
