using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArenaManager : MonoBehaviour
{
    public static ArenaManager Instance;
    
    [Header("Public Infos")]
    public List<Arena> arenas;

    private void Awake()
    {
        Instance = this;
        
        Initialise();
    }
    
    private void Initialise()
    {
        arenas = new List<Arena>();
        arenas = GetComponentsInChildren<Arena>().ToList();
    }
}
