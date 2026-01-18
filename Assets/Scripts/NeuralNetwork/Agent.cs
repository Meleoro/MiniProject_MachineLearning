using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    [Header("Parameters")] 
    public Ninja ninjaScript;
    
    [Header("Public Infos")]
    public NeuralNetwork net = new NeuralNetwork();
    public float fitness;
    public float[] inputs;
    public Ninja opponentScript;

    [Header("Private Infos")] 
    private bool stopFitness;
    private Rigidbody2D rb;
    private Transform currentArena;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }


    public void ResetAgent()
    {
        inputs = new float[net.layers[0]];
        
        fitness = 0;
        stopFitness = false;

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0;

        GetComponent<SpriteRenderer>().enabled = true;
        ninjaScript.currentState = NinjaState.none;
        ninjaScript.noControl = false;
        ninjaScript.StopAllCoroutines();
        
        ninjaScript.won = false;
        ninjaScript.usedKunai = false;

        ninjaScript.attackCollider.enabled = false;
        ninjaScript.heavyAttackCollider.enabled = false;
    }

    public void SetupOpponent(Transform startTr, Ninja opponent, Arena arena)
    {
        transform.position = startTr.position;
        transform.rotation = startTr.rotation;

        opponentScript = opponent;

        currentArena = arena.transform;
    }
    

    private void FixedUpdate()
    {
        InputUpdate(); 
        OutputUpdate();
        FitnessUpdate();
    }

    private void InputUpdate()
    {
        Vector2 dirOpponent = opponentScript.transform.position - transform.position;
        Vector2 dirArena = currentArena.transform.position - transform.position;
        bool isFacingOpponent = Vector2.Angle(opponentScript.transform.right , dirOpponent) < 10;
        
        inputs[0] = Mathf.Clamp(dirOpponent.x / 5f, 0, 1);
        inputs[1] = isFacingOpponent ? 0 : 1;
        inputs[2] = opponentScript.noControl ? 0 : 1;
        inputs[3] = Mathf.Clamp(dirArena.x / 5f, 0, 1);

        switch (opponentScript.currentState)
        {
            case NinjaState.none :
                inputs[4] = 0;
                break;
            
            case NinjaState.isPreparingAttack :
                inputs[4] = 0.15f;
                break;
            
            case NinjaState.isAttacking :
                inputs[4] = 0.3f;
                break;
            
            case NinjaState.isStunned :
                inputs[4] = 0.45f;
                break;
            
            case NinjaState.isCountering :
                inputs[4] = 0.6f;
                break;
            
            case NinjaState.isPreparingHeavyAttack :
                inputs[4] = 0.8f;
                break;
            
            case NinjaState.isAttackingHeavy :
                inputs[4] = 1;
                break;
        }

        
        if (opponentScript.currentKunai is not null)
            inputs[5] = Mathf.Clamp(Vector2.Distance(transform.position, opponentScript.currentKunai.transform.position) / 5, -1, 1);
        
        else
            inputs[5] = 1;

        inputs[6] = ninjaScript.currentKunai is null ? 0 : 1;
        //inputs[7] = AgentManager.Instance.timeLeft / 7;
    }

    private void OutputUpdate()
    {
        net.FeedForward(inputs);
        
        ninjaScript.horizontalInput = net.neurons[net.neurons.Length - 1][0];
        
        ninjaScript.attack = false;
        ninjaScript.counter = false;
        ninjaScript.throwKunai = false;
        ninjaScript.heavyAttack = false;
        
        if (net.neurons[net.neurons.Length - 1][1] > 0.8f)
            ninjaScript.attack = true;
        
        else if(net.neurons[net.neurons.Length - 1][1] > 0.6f)
            ninjaScript.counter = true;
        
        else if(net.neurons[net.neurons.Length - 1][1] > 0.4f)
            ninjaScript.throwKunai = true;
        
        else if(net.neurons[net.neurons.Length - 1][1] > 0.2f)
            ninjaScript.heavyAttack = true;
        
            
        /*ninjaScript.attack = net.neurons[net.neurons.Length - 1][1] > 0.75f;
        ninjaScript.counter = net.neurons[net.neurons.Length - 1][2] > 0.75f;
        ninjaScript.throwKunai = net.neurons[net.neurons.Length - 1][2] > 0.75f;*/
    }

    private void FitnessUpdate()
    {
        if (stopFitness)
            return;
        
        fitness += Time.deltaTime * 5;
        fitness -= Vector2.Distance(transform.position, opponentScript.transform.position) * Time.deltaTime * 2;
        fitness -= Vector2.Distance(transform.position, currentArena.transform.position + Vector3.down * 2) * Time.deltaTime;

        
        /*if (opponentScript.currentState == NinjaState.isStunned)
            fitness += Time.deltaTime * 5;*/
        
        
        if (ninjaScript.currentState == NinjaState.isDead || ninjaScript.currentState == NinjaState.isDeadByFall)
        {
            stopFitness = true;
            fitness -= 150;
            
            opponentScript.Win();
        }

        if (opponentScript.currentState == NinjaState.isDead)
        {
            if (opponentScript.killedByKunai)
            {
                stopFitness = true;
                fitness += 100;
            }

            else
            {
                stopFitness = true;
                fitness += 150;
            }
            
            

            if(AgentManager.Instance.agentsPool2.Contains(this)) 
                AgentManager.Instance.blueKills += 1;
            
            else
                AgentManager.Instance.redKills += 1;
        }
    }
}
