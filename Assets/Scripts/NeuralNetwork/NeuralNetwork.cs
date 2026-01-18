using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class NeuralNetwork
{
    public int[] layers = new[] { 6, 6, 6, 2 };
    public float[][] neurons;
    public float[][][] axons;

    private int x;
    private int y;
    private int z;

    public NeuralNetwork() { }

    public NeuralNetwork(int[] layersModel)
    {
        layers = new int[layersModel.Length];
        for (x = 0; x < layersModel.Length; x++)
        {
            layers[x] = layersModel[x];
        }

        InitNeurons();
        InitAxons();
    }

    private void InitNeurons()
    {
        neurons = new float[layers.Length][];
        
        for (x = 0; x < layers.Length; x++)
            neurons[x] = new float[layers[x]];
    }

    private void InitAxons()
    {
        axons = new float[layers.Length - 1][][];
        
        for (x = 0; x < layers.Length - 1; x++)
        {
            axons[x] = new float[layers[x]][];
            
            for (y = 0; y < layers[x]; y++)
            {
                axons[x][y] = new float[layers[x + 1]];

                for (z = 0; z < axons[x][y].Length; z++)
                {
                    axons[x][y][z] = Random.Range(-1f, 1f);
                }
            }
        }
    }

    private int xPreviousLayer;
    public void FeedForward(float[] inputs)
    {
        neurons[0] = inputs;

        for (x = 1; x < neurons.Length; x++)
        {
            for (y = 0; y < neurons[x].Length; y++)
            {
                neurons[x][y] = 0;
            }
            xPreviousLayer = x - 1;

            for (y = 0; y < neurons[xPreviousLayer].Length; y++)
            {
                for (z = 0; z < axons[xPreviousLayer][y].Length; z++)
                {
                    neurons[x][z] += neurons[xPreviousLayer][y] * axons[xPreviousLayer][y][z];
                }
            }
            
            for (y = 0; y < neurons[x].Length; y++)
            {
                neurons[x][y] = (float)Math.Tanh(neurons[x][y]);
            }
        }
    }
    
    public void CopyNet(NeuralNetwork netCopy)
    {
        for (x = 0; x < netCopy.axons.Length; x++)
        {
            for (y = 0; y < netCopy.axons[x].Length; y++)
            {
                for (z = 0; z < netCopy.axons[x][y].Length; z++)
                {
                    axons[x][y][z] = netCopy.axons[x][y][z];
                }
            }
        }
    }

    public void Mutate(float probability, float power)
    {
        for (x = 0; x < axons.Length; x++)
        {
            for (y = 0; y < axons[x].Length; y++)
            {
                for (z = 0; z < axons[x][y].Length; z++)
                {
                    if (Random.value < probability)
                    {
                        axons[x][y][z] += Random.Range(-power, power);

                        axons[x][y][z] = Mathf.Clamp(axons[x][y][z], -1, 1);
                    }
                }
            }
        }
    }
}