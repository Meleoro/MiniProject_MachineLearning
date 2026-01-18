using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kunai : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float kunaiSpeed;

    [Header("Private Infos")] 
    public bool isCountered;
    private float timer;
    
    [Header("References")] 
    private Rigidbody2D rb;
    public Ninja currentNinja;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }


    void Update()
    {
        timer += Time.deltaTime;
        
        if(!isCountered) 
            rb.velocity = transform.right * kunaiSpeed;

        if (timer >= 2)
        {
            currentNinja.currentKunai = null;
            Destroy(gameObject);
        }
    }


    public void CounterKunai()
    {
        if (isCountered)
            return;
        
        isCountered = true;
        
        rb.AddForce(-rb.velocity + Vector2.up * 2, ForceMode2D.Impulse);
        rb.AddTorque(-10, ForceMode2D.Impulse);

        rb.gravityScale = 1;
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCountered)
            return;
            
        if (other.CompareTag("Kunai"))
        {
            CounterKunai();
            other.GetComponent<Kunai>().CounterKunai();

            currentNinja.agent.fitness += 10;
        }
        
        else if (other.TryGetComponent<Ninja>(out Ninja ninja))
        {
            if(ninja != currentNinja) 
                ninja.TakeDamages(null, this, false);
        }

    }
}
