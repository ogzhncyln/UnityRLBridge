import json

class MsgObj:
    def toJson(self):
        return json.dumps(self.__dict__)

class Message(MsgObj):

    def __init__(self,title,json_content):
        self.title = title
        self.json_content=json_content

    
class ActionMsg(MsgObj):

    def __init__(self,behavior_name,agent_name,continuous_actions,dicrete_actions):
        self.behavior_name = behavior_name
        self.agent_name = agent_name
        self.continuous_actions = continuous_actions
        self.discrete_actions = dicrete_actions
    
class StatusMsg(MsgObj):

    def __init__(self,behavior_name,agent_name,observations,reward,done):
        self.behavior_name = behavior_name
        self.agent_name = agent_name
        self.observations = observations
        self.reward = reward
        self.done = done

class ResetMsg(MsgObj):

    def __init__(self,behavior_name,agent_name):
        self.behavior_name = behavior_name
        self.agent_name = agent_name

class BeginConnectionRequestMsg(MsgObj):

    def __init__(self,behavior_name):
        self.behavior_name = behavior_name

class BeginConnectionResponseMsg(MsgObj):

    def __init__(self,observation_size,continuous_action_size,discrete_action_size,agent_names):
        self.observation_size = observation_size
        self.continuous_action_size = continuous_action_size
        self.discrete_action_size = discrete_action_size
        self.agent_names = agent_names
    
def JsonToMessage(json_content):
    return Message(**json.loads(json_content))

def JsonToActionMsg(json_content):
    return ActionMsg(**json.loads(json_content))

def JsonToStatusMsg(json_content):
    return StatusMsg(**json.loads(json_content))

def JsonToResetMsg(json_content):
    return ResetMsg(**json.loads(json_content))

def JsonToBeginConnectionResponseMsg(json_content):
    return BeginConnectionResponseMsg(**json.loads(json_content))

def JsonToBeginConnectionRequestMsg(json_content):
    return BeginConnectionRequestMsg(**json.loads(json_content))
    
