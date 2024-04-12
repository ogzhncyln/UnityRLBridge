using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RLBridge
{

    public abstract class MsgObj
    {
        public string toJson()
        {
            return JsonUtility.ToJson(this);
        }
    }

    public class Message : MsgObj
    {
        public string title;
        public string json_content;

        public Message(string title, string json_content)
        {
            this.title = title;
            this.json_content = json_content;
        }

    }

    public class BeginConnectionRequestMsg : MsgObj
    {
        public string behavior_name;

        public BeginConnectionRequestMsg(string behavior_name)
        {
            this.behavior_name = behavior_name;
        }
    }

    public class BeginConnectResponseMsg : MsgObj
    {
        public int observation_size;
        public int continuous_action_size;
        public int discrete_action_size;
        public string[] agents;

        public BeginConnectResponseMsg(int observation_size, int continuous_action_size, int discrete_action_size, string[] agents)
        {
            this.observation_size = observation_size;
            this.continuous_action_size = continuous_action_size;
            this.discrete_action_size = discrete_action_size;
            this.agents = agents;
        }
    }

    public class ActionMsg : MsgObj
    {
        public string behavior_name;
        public string agent_name;
        public double[] continuous_actions;
        public int[] discrete_actions;

        public ActionMsg(string behavior_name, string agent_name, double[] continuous_actions, int[] discrete_actions)
        {
            this.behavior_name = behavior_name;
            this.agent_name = agent_name;
            this.continuous_actions = continuous_actions;
            this.discrete_actions = discrete_actions;
        }
    }

    public class StatusMsg : MsgObj
    {
        public string behavior_name;
        public string agent_name;
        public double[] observations;
        public float reward;
        public bool done;

        public StatusMsg(string behavior_name, string agent_name, double[] observations, float reward, bool done)
        {
            this.behavior_name = behavior_name;
            this.agent_name = agent_name;
            this.observations = observations;
            this.reward = reward;
            this.done = done;
        }

    }

    public class ResetMsg : MsgObj
    {
        public string behavior_name;
        public string agent_name;

        public ResetMsg(string behavior_name, string agent_name)
        {
            this.behavior_name = behavior_name;
            this.agent_name = agent_name;
        }
    }
}
