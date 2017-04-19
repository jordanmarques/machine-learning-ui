﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LinearSimple : MonoBehaviour
{

    public GameObject bluePrefab;
    public GameObject redPrefab;
    public GameObject whitePrefab;

    public List<GameObject> blueSamples = new List<GameObject>();
    public List<GameObject> redSamples = new List<GameObject>();
    public List<GameObject> whiteSamples = new List<GameObject>();

    private double[] myModelClassification;

	// Use this for initialization
	void Start () {
        myModelClassification = MyFirstDLLWrapper.linear_create_model(2);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void GenerateTestValues(int nbValues)
    {
        foreach (var blue in blueSamples)
        {
            Destroy(blue);
        }
        foreach (var red in redSamples)
        {
            Destroy(red);
        }
        foreach (var white in whiteSamples)
        {
            Destroy(white);
        }
        blueSamples.Clear();
        redSamples.Clear();
        whiteSamples.Clear();

        for (int i = 0; i < nbValues; i++)
        {
            if (i % 2 == 0)
            {   // Bleu
                blueSamples.Add(Instantiate(bluePrefab, new Vector3(Random.Range(1f, 10f), 1f, Random.Range(1f,5f)), Quaternion.identity));
            }
            else
            {   // Rouge
                redSamples.Add(Instantiate(redPrefab, new Vector3(Random.Range(1f, 10f), 1f, Random.Range(6f, 10f)), Quaternion.identity));
            }
        }
        
        for (int a = 1; a <= 10 ; a++)
        {
            for (int b = 1; b <= 10; b++)
            {
                whiteSamples.Add(Instantiate(whitePrefab, new Vector3(a, 0f, b), Quaternion.identity));
            }
        }

        Start();
    }

    public void ResolveLinear()
    {
        double[] myModel = MyFirstDLLWrapper.linear_create_model(2);
        
        double[,] inputs = new double[blueSamples.Count + redSamples.Count,2];
        double[] outputs = new double[blueSamples.Count + redSamples.Count];
        int i = 0;
        foreach (var blue in blueSamples)
        {
            inputs[i, 0] = blue.transform.position.x;
            inputs[i, 1] = blue.transform.position.z;
            outputs[i++] = 0;
        }
        foreach (var red in redSamples)
        {
            inputs[i, 0] = red.transform.position.x;
            inputs[i, 1] = red.transform.position.z;
            outputs[i++] = 1;
        }
        MyFirstDLLWrapper.linear_fit_regression(ref myModel, inputs, outputs);

       

        foreach (var white in whiteSamples)
        {
           Destroy(white);
        }
        whiteSamples.Clear();

        for (int a = 1; a <= 10; a++)
        {
            for (int b = 1; b <= 10; b++)
            {
                var rslt = MyFirstDLLWrapper.linear_predict(ref myModel, new double[] {a, b});

                var elt = Instantiate(whitePrefab, new Vector3(a, 0f, b), Quaternion.identity);
                elt.GetComponent<Renderer>().material.color = new Color((float) rslt, 0, 1 - (float)rslt);
                whiteSamples.Add(elt);
            }
        }
    }

	public void ResolveClassification()
	{
		List<GameObject> balls = new List<GameObject>();
		balls.AddRange(blueSamples);
		balls.AddRange(redSamples);
        System.Random rnd = new System.Random();
	    balls = balls.OrderBy(x => rnd.Next()).ToList();

		foreach (var ball in balls)
		{
			double[] input = new double[]{ball.transform.position.x, ball.transform.position.z};
			double[] output = new double[] { blueSamples.Contains(ball) ? 1 : -1 };

			MyFirstDLLWrapper.linear_fit_classification_rosenblatt(ref myModelClassification, input, output, 100, 0.01);

		}

		foreach (var white in whiteSamples)
		{
			Destroy(white);
		}
		whiteSamples.Clear();

		for (int a = 1; a <= 10; a++)
		{
			for (int b = 1; b <= 10; b++)
			{
				var rslt = MyFirstDLLWrapper.linear_classify(ref myModelClassification, new double[] { a, b });

				if (rslt > 0)
				{
					whiteSamples.Add(Instantiate(bluePrefab, new Vector3(a, 0, b), Quaternion.identity));
				}
				else
				{
					whiteSamples.Add(Instantiate(redPrefab, new Vector3(a, 0, b), Quaternion.identity));
				}
			}
		}
	}
}
