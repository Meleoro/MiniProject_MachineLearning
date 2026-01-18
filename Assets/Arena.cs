using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Arena : MonoBehaviour
{
    [SerializeField] private bool isMainArena;
    
    [Header("References")] 
    public Transform spawn1;
    public Transform spawn2;
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject fond;

    private void Start()
    {
        if (!isMainArena)
        {
            anim.enabled = false;
            fond.SetActive(false);
        }
    }
}
