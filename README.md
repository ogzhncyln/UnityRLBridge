# UnityRLBridge
UnityRLBridge is a package developed for the use of Unity environments in reinforcement learning applications, featuring multi-agent and environment support, and it has ease of use.. With this package, you can quickly and easily connect to a pre-made Unity environment and train your agents through this environment.
## Usage
The UnityRLBridge package operates with a server on the Unity side and one or more clients on the Python side. Each agent on the Unity side has two components of type text named `behavior_name` and `agent_name`. In contrast, each client on the Python side has one component of type text named `behavior_name`. Below is a detailed description of these components.
### behavior_name & agent_name
`behavior_name` represents a name for agents with the same type of behavior. On the other hand, `agent_name` is a name given to represent a specific agent. To illustrate with an example: Let's assume we have multiple agents in an environment, and these agents are all dogs. In this case, our `behavior_name` component would be `Dog`, and the `agent_name` component would be the name of the respective dog, such as `Max`.

In Unity, the `behavior_name` and `agent_name` components are nested under the AgentBehavior class.
```
string behavior_name = agent_behavior.behavior_name;
string agent_name = agent_behavior.agent_name;
```

On the Python side, the `behavior_name` component is located under the Bridge class. For agents with the corresponding `behavior_name` component on the Python side, their `agent_name` components are stored as a list under the `agent_name` attribute under the Bridge class.
```
behavior_name = bridge.behavior_name
agent_name = bridge.agent_names[agent_index]
```
