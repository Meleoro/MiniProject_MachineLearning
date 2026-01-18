using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Ninja : MonoBehaviour
{
    [Header("Parameters")] 
    [SerializeField] private float moveSpeed;
    [SerializeField] private float lightAttackDuration;
    [SerializeField] private float lightAttackAnticipationDuration;
    [SerializeField] private float counterDuration;
    [SerializeField] private float counterCooldownDuration;
    [SerializeField] private float stunDuration;

    [Header("Parameters Throw")] 
    [SerializeField] private GameObject kunaiPrefab;
    [SerializeField] private float throwDuration;
    [SerializeField] private float throwCooldown;
    
    [Header("Parameters Throw")] 
    [SerializeField] private float heavyAttackPreparationDuration;
    [SerializeField] private float heavyAttackDuration;
    [SerializeField] private float heavyAttackCooldown;
    
    [Header("Public Infos")] 
    public float horizontalInput;
    public bool attack;
    public bool counter;
    public bool throwKunai;
    public bool heavyAttack;
    public bool dash;
    public NinjaState currentState;
    public bool noControl;
    public bool won;
    public Kunai currentKunai;
    public bool usedKunai;
    public bool killedByKunai;

    [Header("Private Infos")] 
    private float timer;

    [Header("References")] 
    public BoxCollider2D attackCollider;
    public BoxCollider2D heavyAttackCollider;
    private Rigidbody2D rb;
    [HideInInspector] public Agent agent;
    [HideInInspector] public Animator anim;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        agent = GetComponent<Agent>();
    }


    private void Update()
    {
        if (!noControl && currentState != NinjaState.isDead && currentState != NinjaState.isDeadByFall && !won)
        {
            anim.SetBool("doUniqueAction", false);
            
            MoveCharacter();

            if (attack)
                DoLightAttack();

            else if (counter)
                DoCounter();
            
            else if(throwKunai && !usedKunai)
                ThrowKunai();
            
            else if(heavyAttack)
                DoHeavyAttack();
        }

        else
        {
            anim.SetBool("doUniqueAction", true);
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }
    
    

    #region Basic Movement

    private void MoveCharacter()
    {
        rb.velocity = new Vector2(horizontalInput * moveSpeed,  rb.velocity.y);

        if (Mathf.Abs(horizontalInput) > 0.2f)
            anim.SetBool("IsWalking", true);
        
        else
            anim.SetBool("IsWalking", false);

        RotateCharacter();
    }

    private void RotateCharacter()
    {
        if (rb.velocity.x > 0.1f)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (rb.velocity.x < -0.1f)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    #endregion
    

    #region Light Attack

    private void DoLightAttack()
    {
        currentState = NinjaState.isPreparingAttack;
        noControl = true;
        
        anim.SetTrigger("LightAttack");

        StartCoroutine(LightAttackCoroutine());
    }

    private IEnumerator LightAttackCoroutine()
    {
        yield return new WaitForSeconds(lightAttackAnticipationDuration);

        if(currentState != NinjaState.isDead && currentState != NinjaState.isDeadByFall)
            attackCollider.enabled = true;
        
        currentState = NinjaState.isAttacking;

        yield return new WaitForSeconds(lightAttackDuration * 0.15f);
        
        attackCollider.enabled = false;
        currentState = NinjaState.none;
        
        yield return new WaitForSeconds(lightAttackDuration * 0.85f);

        noControl = false;
    }

    #endregion
    

    #region Counter
    
    private void DoCounter()
    {
        currentState = NinjaState.isCountering;
        noControl = true;
        
        anim.SetTrigger("Counter");

        StartCoroutine(CounterCoroutine());
    }
    
    private IEnumerator CounterCoroutine()
    {
        yield return new WaitForSeconds(counterDuration);

        currentState = NinjaState.none;
        
        
        yield return new WaitForSeconds(counterCooldownDuration);
        
        noControl = false; 
    }
    
    #endregion


    #region Stun

    public void StunCharacter()
    {
        noControl = true;
        currentState = NinjaState.isStunned;
        anim.SetTrigger("Stun"); 
        
        StopAllCoroutines();
        
        attackCollider.enabled = false;
        heavyAttackCollider.enabled = false;
        
        StartCoroutine(StunCoroutine());
    }

    private IEnumerator StunCoroutine()
    {
        yield return new WaitForSeconds(stunDuration);

        currentState = NinjaState.none;
        noControl = false;
    }

    #endregion


    #region Throw Kunai

    private void ThrowKunai()
    {
        noControl = true;
        usedKunai = true;
        currentState = NinjaState.isThrowing;

        agent.fitness += 10;
        
        anim.SetTrigger("ThrowKunai");

        StartCoroutine(ThrowKunaiCoroutine());
    }


    private IEnumerator ThrowKunaiCoroutine()
    {
        yield return new WaitForSeconds(throwDuration * 0.5f);
        
        currentKunai = Instantiate(kunaiPrefab, transform.position, transform.rotation).GetComponent<Kunai>();
        currentKunai.currentNinja = this;
        
        yield return new WaitForSeconds(throwDuration * 0.5f);
        
        currentState = NinjaState.none;
        
        yield return new WaitForSeconds(throwCooldown);

        noControl = false;
    }
    

    #endregion
    
    
    #region HeavyAttack

    private void DoHeavyAttack()
    {
        noControl = true;
        currentState = NinjaState.isPreparingAttack;
        
        anim.SetTrigger("HeavyAttack");
        
        StartCoroutine(HeavyAttackCoroutine());
    }


    private IEnumerator HeavyAttackCoroutine()
    {
        yield return new WaitForSeconds(lightAttackAnticipationDuration);
        
        currentState = NinjaState.isPreparingHeavyAttack;
        
        yield return new WaitForSeconds(heavyAttackPreparationDuration - lightAttackAnticipationDuration);

        heavyAttackCollider.enabled = true;
        currentState = NinjaState.isAttackingHeavy;
        
        yield return new WaitForSeconds(heavyAttackDuration);
        
        heavyAttackCollider.enabled = false;
        currentState = NinjaState.none;
        
        yield return new WaitForSeconds(heavyAttackCooldown);

        noControl = false;
    }
    

    #endregion


    #region Others

    public void TakeDamages(Ninja attacker, Kunai kunai, bool breakCounter)
    {
        if (currentState == NinjaState.isDead)
            return;
        
        if (attacker is not null)
        {
            if (currentState == NinjaState.isCountering && Vector2.Angle(transform.right, attacker.transform.position - transform.position) < 10)
            {
                if (breakCounter)
                {
                    StunCharacter();
                    agent.fitness += 30;
                }
                else
                {
                    attacker.StunCharacter();
                    agent.fitness += 40;
                }
            }

            else
            {
                StopAllCoroutines();
                
                currentState = NinjaState.isDead;
                GetComponent<SpriteRenderer>().enabled = false;
                
                if(attacker.currentState == NinjaState.isAttacking)
                    attacker.agent.fitness += 60;

                killedByKunai = false;
            }
        }
        
        else if (kunai is not null)
        {
            if (currentState == NinjaState.isCountering &&
                Vector2.Angle(transform.right, kunai.transform.position - transform.position) < 10)
            {
                kunai.CounterKunai();
                agent.fitness += 20;
            }

            
            else
            {
                StopAllCoroutines();
                
                currentState = NinjaState.isDead;
                GetComponent<SpriteRenderer>().enabled = false;

                killedByKunai = true;
            }
        }

        else
        {
            currentState = NinjaState.isDeadByFall;
            GetComponent<SpriteRenderer>().enabled = false;
        }
    }
    
    
    public void Win()
    {
        anim.SetTrigger("Won");
        won = true;
    }

    #endregion
}

public enum NinjaState
{
    isPreparingAttack,
    isAttacking,
    isAttackingHeavy,
    isCountering,
    isThrowing,
    isPreparingHeavyAttack,
    isStunned,
    isDead,
    isDeadByFall,
    none
}
