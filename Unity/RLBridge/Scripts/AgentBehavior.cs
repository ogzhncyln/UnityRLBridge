using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RLBridge 
{

    public abstract class AgentBehavior : MonoBehaviour
    {
        public string behaviorName;
        public int observationSize;
        public int continuousActionSize;
        public int discreteActionSize;
        public int maxSteps;

        [HideInInspector]
        public string agent_name;

        [HideInInspector]
        public float step_reward;

        [HideInInspector]
        public bool end_episode;

        [HideInInspector]
        public int steps;

        public virtual void OnConnected() { }
        public abstract void OnEpisodeBegin();
        public abstract void CollectObservations(ref double[] observations);
        public abstract void OnActionReceived(double[] continuous_actions, int[] discrete_actions);

        public void AddReward(float reward)
        {
            step_reward += reward;
        }

        public void EndEpisode()
        {
            end_episode = true;
        }
    }
}
