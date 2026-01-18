using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    private Ninja ninjaScript;
    
    
    void Start()
    {
        ninjaScript = GetComponent<Ninja>();
    }
    
    void Update()
    {
        ninjaScript.horizontalInput = Input.GetAxisRaw("Horizontal");

        ninjaScript.attack = Input.GetKeyDown(KeyCode.Mouse0);
        ninjaScript.counter = Input.GetKeyDown(KeyCode.Mouse1);
    }
}
