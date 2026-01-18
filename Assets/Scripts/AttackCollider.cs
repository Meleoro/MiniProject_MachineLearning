using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCollider : MonoBehaviour
{
    [SerializeField] private bool breaksCounter;
    
    private Ninja ninjaScript;

    private void Start()
    {
        ninjaScript = GetComponentInParent<Ninja>();
    }


    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.TryGetComponent(out AttackCollider attackCollider))
        {
            if (ninjaScript is not null && attackCollider.ninjaScript is not null)
            {
                if (attackCollider.ninjaScript.currentState == NinjaState.isAttacking && ninjaScript.currentState == NinjaState.isAttacking)
                {
                    attackCollider.ninjaScript.StunCharacter();
                    ninjaScript.StunCharacter();

                    attackCollider.ninjaScript.attackCollider.enabled = false;
                    ninjaScript.attackCollider.enabled = false;
                    
                    attackCollider.ninjaScript.agent.fitness += 20f;
                    ninjaScript.agent.fitness += 20f;
                }
            
                else if (attackCollider.ninjaScript.currentState == NinjaState.isAttackingHeavy &&
                         ninjaScript.currentState == NinjaState.isAttackingHeavy)
                {
                    attackCollider.ninjaScript.StunCharacter();
                    ninjaScript.StunCharacter();

                    attackCollider.ninjaScript.heavyAttackCollider.enabled = false;
                    ninjaScript.heavyAttackCollider.enabled = false;
                }
            
                else if (attackCollider.ninjaScript.currentState == NinjaState.isAttacking && ninjaScript.currentState == NinjaState.isAttackingHeavy)
                {
                    attackCollider.ninjaScript.StunCharacter();

                    attackCollider.ninjaScript.attackCollider.enabled = false;
                    ninjaScript.heavyAttackCollider.enabled = false;

                    ninjaScript.agent.fitness += 10f;
                }
            
                else if (attackCollider.ninjaScript.currentState == NinjaState.isAttackingHeavy && ninjaScript.currentState == NinjaState.isAttacking)
                {
                    ninjaScript.StunCharacter();

                    ninjaScript.attackCollider.enabled = false;
                    attackCollider.ninjaScript.heavyAttackCollider.enabled = false;

                    attackCollider.ninjaScript.agent.fitness += 10f;
                }
                
                else if(other.TryGetComponent<Ninja>(out Ninja ninja))
                    ninja.TakeDamages(ninjaScript, null, breaksCounter);    
            }
            
            else if(other.TryGetComponent<Ninja>(out Ninja ninja))
                ninja.TakeDamages(ninjaScript, null, breaksCounter);    
        }
        
        else if(other.TryGetComponent<Ninja>(out Ninja ninja))
            ninja.TakeDamages(ninjaScript, null, breaksCounter); 
        
        
        else if (other.CompareTag("Kunai"))
        {
            if (other.GetComponent<Kunai>().isCountered)
                return;
            
            if(ninjaScript is not null)
                ninjaScript.agent.fitness += 20;
            
            other.GetComponent<Kunai>().CounterKunai();
        }
    }
}
