using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    public static AgentManager Instance;
    
    [SerializeField] private int switchRound = 10;
    [SerializeField] private int roundCounter;
    
    [SerializeField] private int populationSize = 100;
    [SerializeField] private float trainingDuration = 30;
    [SerializeField] private float mutationRate = 0.2f;
    [SerializeField] private float mutationPower = 0.1f;
    [SerializeField] private Agent agentPrefab;
    [SerializeField] private Transform agentGroup;
    [SerializeField] private TextMeshProUGUI generationCountText;
    [SerializeField] private TextMeshProUGUI timerText;

    [HideInInspector] public float timeLeft;
    private int generationCount;
    private float startingTime;
    private Agent agent;
    public List<Agent> agentsPool1 = new List<Agent>();
    public List<Agent> agentsPool2 = new List<Agent>();
    public bool pool1Turn;

    public List<NeuralNetwork> saveAgents = new List<NeuralNetwork>();

    [Header("Color Paramaters")] 
    [SerializeField] private Color colorPool1;
    [SerializeField] private Color colorPool2;
    
    [Header("Additionnal Stats")]
    public AnimationCurve victoryRedPool;
    public AnimationCurve victoryBluePool;
    public AnimationCurve victoryDifference;
    public AnimationCurve fitnessRedPool;
    public AnimationCurve fitnessBluePool;
    public AnimationCurve fitnessDifference;
    public int redKills;
    public int blueKills;


    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(Loop());
    }
    

    IEnumerator Loop()
    {
        StartNewGeneration();
        ResetTimer();
        Focus();
        
        yield return new WaitForSeconds(trainingDuration);

        StartCoroutine(Loop());
    }

    private void StartNewGeneration()
    {
        AddOrRemoveAgents();
        
        agentsPool1 = agentsPool1.OrderByDescending(a => a.fitness).ToList();
        agentsPool2 = agentsPool2.OrderByDescending(a => a.fitness).ToList();

        fitnessRedPool.AddKey(generationCount, agentsPool2[0].fitness);
        fitnessBluePool.AddKey(generationCount, agentsPool1[0].fitness);
        fitnessDifference .AddKey(generationCount, agentsPool2[0].fitness - agentsPool1[0].fitness);
        
        victoryRedPool.AddKey(generationCount, redKills);
        victoryBluePool.AddKey(generationCount, blueKills);
        victoryDifference.AddKey(generationCount, redKills - blueKills);
        
        redKills = 0;
        blueKills = 0;
        
        
        roundCounter--;
        if (roundCounter <= 0)
        {
            roundCounter = switchRound;
            pool1Turn = !pool1Turn;

            if (pool1Turn)
            {
                for (int i = 0; i < saveAgents.Count; i++)
                {
                    agentsPool1[i].net.CopyNet(saveAgents[i]);
                    saveAgents[i].CopyNet(agentsPool2[i].net);
                }
                
                for (int i = 0; i < agentsPool2.Count; i++)
                {
                    agentsPool2[i].net.CopyNet(agentsPool2[0].net);
                }
            }
            else
            {
                for (int i = 0; i < saveAgents.Count; i++)
                {
                    agentsPool2[i].net.CopyNet(saveAgents[i]);
                    saveAgents[i].CopyNet(agentsPool1[i].net);
                }
                
                for (int i = 0; i < agentsPool1.Count; i++)
                {
                    agentsPool1[i].net.CopyNet(agentsPool1[0].net);
                }
            }
        }
        
        Mutate();
        ResetAgents();
        SetMaterials();
        
        RefreshGenerationCount();
    }

    private void AddOrRemoveAgents()
    {
        if (agentsPool1.Count != populationSize)
        {
            int dif = populationSize - agentsPool1.Count;

            if (dif > 0)
            {
                for (int i = 0; i < dif; i++)
                {
                    AddAgent(agentsPool1);
                }
            }
            else
            {
                for (int i = 0; i < -dif; i++)
                {
                    RemoveAgent(agentsPool1);
                }
            }
        }
        
        if (agentsPool2.Count != populationSize)
        {
            int dif = populationSize - agentsPool2.Count;

            if (dif > 0)
            {
                for (int i = 0; i < dif; i++)
                {
                    AddAgent(agentsPool2);
                }
            }
            else
            {
                for (int i = 0; i < -dif; i++)
                {
                    RemoveAgent(agentsPool2);
                }
            }
        }
        
        if (saveAgents.Count != populationSize)
        {
            int dif = populationSize - saveAgents.Count;

            if (dif > 0)
            {
                for (int i = 0; i < dif; i++)
                {
                    saveAgents.Add(new NeuralNetwork(agentPrefab.net.layers));
                }
            }
            else
            {
                for (int i = 0; i < -dif; i++)
                {
                    saveAgents.RemoveAt(0);
                }
            }
        }
    }

    private void AddAgent(List<Agent> pool)
    {
        agent = Instantiate(agentPrefab, Vector3.zero, Quaternion.identity, agentGroup);

        agent.net = new NeuralNetwork(agentPrefab.net.layers);
        pool.Add(agent);
    }

    private void RemoveAgent(List<Agent> pool)
    {
        Destroy(pool[^1].gameObject);
        pool.RemoveAt(pool.Count - 1);
    }

    private void Mutate()
    {
        if (pool1Turn)
        {
            for (int i = agentsPool1.Count / 2; i < agentsPool1.Count; i++)
            {
                agentsPool1[i].net.CopyNet(agentsPool1[i - agentsPool1.Count / 2].net);
                agentsPool1[i].net.Mutate(mutationRate, mutationPower);
            }
            
            /*for (int i = 0; i < agentsPool2.Count; i++)
            {
                agentsPool2[i].net.CopyNet(agentsPool2[0].net);
                agentsPool2[i].GetComponent<SpriteRenderer>().color = Color.white;
            }*/
        }

        else
        {
            for (int i = agentsPool2.Count / 2; i < agentsPool2.Count; i++)
            {
                agentsPool2[i].net.CopyNet(agentsPool2[i - agentsPool2.Count / 2].net);
                agentsPool2[i].net.Mutate(mutationRate, mutationPower);
            }
            
            /*for (int i = 0; i < agentsPool1.Count; i++)
            {
                agentsPool1[i].net.CopyNet(agentsPool1[0].net);
                agentsPool1[i].GetComponent<SpriteRenderer>().color = Color.white;
            }*/
        }
    }

    private void ResetAgents()
    {
        for (int i = 0; i < agentsPool1.Count; i++)
        {
            agentsPool1[i].ResetAgent();
            agentsPool2[i].ResetAgent();
            
            agentsPool1[i].SetupOpponent(ArenaManager.Instance.arenas[i].spawn1, agentsPool2[i].ninjaScript, ArenaManager.Instance.arenas[i]);
            agentsPool2[i].SetupOpponent(ArenaManager.Instance.arenas[i].spawn2, agentsPool1[i].ninjaScript, ArenaManager.Instance.arenas[i]);
        }
    }

    private void SetMaterials()
    {
        for (int i = 0; i < agentsPool1.Count; i++)
        {
            agentsPool1[i].GetComponent<SpriteRenderer>().color = colorPool1;
            agentsPool2[i].GetComponent<SpriteRenderer>().color = colorPool2;
        }
    }

    private void RefreshGenerationCount()
    {
        generationCount++;

        generationCountText.text = "Generation n°" + generationCount;
    }

    private void ResetTimer()
    {
        startingTime = Time.time;
    }

    private void Update()
    {
        timerText.text = (trainingDuration - (Time.time - startingTime)).ToString("f0");
        timeLeft = (trainingDuration - (Time.time - startingTime));
    }

    private void Focus()
    {
        //cameraController.target = agents[0].transform;
    }
}
