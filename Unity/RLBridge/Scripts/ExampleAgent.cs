using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RLBridge;

public class ExampleAgent : AgentBehavior
{
    public override void OnConnected()
    {
        Debug.Log("Connected");
    }

    public override void CollectObservations(ref double[] observations)
    {
        observations[0] = 1f;
        observations[1] = 2f;
        observations[2] = 3f;   
    }

    public override void OnActionReceived(double[] continuous_actions, int[] discrete_actions)
    {
        Debug.Log($"action received {continuous_actions} {discrete_actions}");
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("env reseted.");
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
