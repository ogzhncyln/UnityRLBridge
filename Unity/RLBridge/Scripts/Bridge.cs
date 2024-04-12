using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace RLBridge
{
    public class Bridge : MonoBehaviour
    {
        public AgentBehavior[] agents;
        private TcpListener server;
        private Client[] clients;

        public string Ip="127.0.0.1";
        public int Port=1234;
        public float timeScale=1;

        [Range(1, 100)]
        public int maxClients=1;

        public int num_clients { get { return _num_clients; } private set { _num_clients = value; } }
        private int _num_clients;

        Queue<MessageInfo> begin_requests = new Queue<MessageInfo>();
        Queue<MessageInfo> messages = new Queue<MessageInfo>();

        void Start()
        {
            server = new TcpListener(IPAddress.Parse(Ip),Port);
            server.Start();

            clients = new Client[maxClients];
            for (int i = 0; i < maxClients; i++)
                clients[i] = new Client(i, ref messages, OnDisconnected);

            for (int i = 0; i < agents.Length; i++)
            {
                agents[i].agent_name = $"Client{i}";
            }

            server.BeginAcceptTcpClient(AcceptCallback, null);
            Time.timeScale = timeScale;
        }


        void FixedUpdate()
        {
            Handle();
        }

        void Handle()
        {
            for (int i = 0; i < messages.Count; i++)
            {
                MessageInfo msg_data = messages.Dequeue();
                Message msg = JsonUtility.FromJson<Message>(msg_data.json_content);
                switch (msg.title)
                {
                    case "action":
                        ActionMsg action = JsonUtility.FromJson<ActionMsg>(msg.json_content);
                        AgentBehavior agent = GetAgent(action.behavior_name, action.agent_name);
                        if (agent == null) return;
                        agent.OnActionReceived(action.continuous_actions,action.discrete_actions);
                        double[] observations = new double[agent.observationSize];
                        float reward;
                        bool done;
                        agent.CollectObservations(ref observations);
                        reward = agent.step_reward;
                        done = agent.end_episode;
                        if (!agent.end_episode && agent.steps >= agent.maxSteps) { done = true; }
                        agent.step_reward = 0f;
                        string ret_json = new Message("status", new StatusMsg(agent.behaviorName, agent.agent_name, observations, reward, done).toJson()).toJson();
                        msg_data.client.Send(Encoding.UTF8.GetBytes(ret_json));
                        agent.steps += 1;
                        break;

                    case "reset":
                        ResetMsg reset = JsonUtility.FromJson<ResetMsg>(msg.json_content);
                        AgentBehavior agent1 = GetAgent(reset.behavior_name, reset.agent_name);
                        if (agent1 == null) return;
                        agent1.OnEpisodeBegin();
                        double[] observations1 = new double[agent1.observationSize];
                        agent1.CollectObservations(ref observations1);
                        agent1.step_reward = 0f;
                        agent1.end_episode = false;
                        agent1.steps = 0;
                        string ret_json1 = new Message("observations", new StatusMsg(agent1.behaviorName, agent1.agent_name, observations1, agent1.step_reward, agent1.end_episode).toJson()).toJson();
                        msg_data.client.Send(Encoding.UTF8.GetBytes(ret_json1));
                        break;

                    case "begin_connect_request":
                        begin_requests.Enqueue(msg_data);
                        break;
                    default:
                        break;
                }
            }
        }

        void AcceptCallback(IAsyncResult asyncResult)
        {
            TcpClient client = server.EndAcceptTcpClient(asyncResult);
            foreach (Client c in clients)
            {
                if (!c.isAvailable)
                {
                    c.Connect(client);
                    num_clients++;
                    server.BeginAcceptTcpClient(AcceptCallback, null);
                    return;
                }
            }
            client.Close();
            server.BeginAcceptTcpClient(AcceptCallback, null);
        }

        public void BeginEnv()
        {
            while(begin_requests.Count > 0)
            {
                MessageInfo msg_data = begin_requests.Dequeue();
                Message msg = JsonUtility.FromJson<Message>(msg_data.json_content);
                BeginConnectionRequestMsg req = JsonUtility.FromJson<BeginConnectionRequestMsg>(msg.json_content);
                msg_data.client.behavior_name = req.behavior_name;
                AgentBehavior[] agents = GetAgents(req.behavior_name);
                if (agents.Length == 0) return;
                List<string> agent_names = new List<string>();
                foreach (AgentBehavior agent in agents)
                {
                    agent_names.Add(agent.agent_name);
                }
                string response_msg = new BeginConnectResponseMsg(agents[0].observationSize, agents[0].continuousActionSize, agents[0].discreteActionSize,agent_names.ToArray()).toJson();
                msg_data.client.Send(Encoding.UTF8.GetBytes(new Message("begin_connect_response", response_msg).toJson()));
                foreach (AgentBehavior agent in agents)
                {
                    agent.OnConnected();
                }
            }
        }

        private void OnDisconnected(Client client)
        {
            num_clients--;
        }

        public AgentBehavior[] GetAgents(string behavior_name)
        {
            List<AgentBehavior> agents = new List<AgentBehavior>();
            foreach (AgentBehavior agent in this.agents)
            {
                if (agent.behaviorName == behavior_name)
                {
                   agents.Add(agent);
                }
            }

            return agents.ToArray();
        }

        public AgentBehavior GetAgent(string behavior_name,string agent_name)
        {
            foreach (AgentBehavior agent in agents)
            {
                if(agent.behaviorName == behavior_name && agent.agent_name == agent_name)
                {
                    return agent;
                }
            }

            return null;
        }
    }

    public class Client
    {
        public TcpClient client;
        private NetworkStream stream;
        private Queue<MessageInfo> q;
        private byte[] data_buffer = new byte[1024];
        public delegate void OnDisconnected(Client client);
        private OnDisconnected onDisconnected;

        public string behavior_name;
        public int id;

        public bool isAvailable { get { return client != null; } }

        public Client(int id, ref Queue<MessageInfo> q,OnDisconnected onDisconnected)
        {
            this.id = id;
            this.q = q;
            this.onDisconnected = onDisconnected;
        }

        public void Connect(TcpClient client)
        {
            this.client = client;
            stream = client.GetStream();
            stream.BeginRead(data_buffer, 0, 1024, ReadCallback, null);
        }

        public void Disconnect() 
        {
            client.Close();
            client = null;
            stream.Close();
            stream = null;
            data_buffer = new byte[1024];
            onDisconnected(this);
            behavior_name = string.Empty;
        }

        public void ReadCallback(IAsyncResult asyncResult)
        {
            try
            {
                int len = stream.EndRead(asyncResult);

                if (len <= 0)
                {
                    Disconnect();
                    return;
                }

                byte[] msg_buffer = new byte[len];
                Array.Copy(data_buffer, msg_buffer, len);

                string data = Encoding.UTF8.GetString(msg_buffer);

                string[] messages = data.Split('\n');

                lock (q)
                {
                    foreach (string msg in messages)
                    {
                        if (!string.IsNullOrEmpty(msg))
                        {
                            q.Enqueue(new MessageInfo(this,msg));
                        }
                    }
                }

                data_buffer = new byte[1024];
                stream.BeginRead(data_buffer, 0, 1024, ReadCallback, null);
            }catch  { Disconnect(); return; }
        }

        public void Send(byte[] data)
        {
            try
            {
                stream.BeginWrite(data, 0, data.Length, null, null);
            }catch  { Disconnect(); return; }
        }
    }

    public struct MessageInfo
    {
        public Client client;
        public string json_content;

        public MessageInfo(Client client,string json_content)
        {
            this.client = client;
            this.json_content = json_content;
        }
    }
}
