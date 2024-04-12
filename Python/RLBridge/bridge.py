from . import config
import socket

from . import messages

class Bridge:

    def __init__(self,behavior_name):
        try:
            self.behavior_name = behavior_name
            self.socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            self.socket.connect((config.ip,config.port))

            try:
                self.socket.send(messages.Message("begin_connect_request",messages.BeginConnectionRequestMsg(self.behavior_name).toJson()).toJson().encode())
            except Exception as e:
                print(f"Message sending error: {e}")
                exit()
            
            try:
                response = self.socket.recv(config.buffer_size).decode()
            except Exception as e:
                print(f"Message receive error: {e}")
                exit()
            
            r_msg = messages.JsonToMessage(response)

            if r_msg.title == "begin_connect_response":
                begin_connection_response_msg = messages.JsonToBeginConnectionResponseMsg(r_msg.json_content)
                self.observation_size = begin_connection_response_msg.observation_size
                self.continuous_action_size = begin_connection_response_msg.continuous_action_size
                self.discrete_action_size = begin_connection_response_msg.discrete_action_size
                self.agent_names = begin_connection_response_msg.agent_names
                
            print("Connected.")
        except Exception as e:
            print(f"Connection error: {e}")
            exit()
        
    def send_action(self,agent_name,continueous_actions,discrete_actions):
        action_msg_json = messages.ActionMsg(self.behavior_name,agent_name,continueous_actions,discrete_actions).toJson()
        msg_json = messages.Message("action",action_msg_json).toJson()

        try:
            self.socket.send(msg_json.encode())
        except Exception as e:
            print(f"Message sending error: {e}")
            return False
        
        try:
            response = self.socket.recv(config.buffer_size).decode()
        except:
            print(f"Message receive error: {e}")
            return False
        
        msg = messages.JsonToMessage(response)

        if msg.title == "status":
            status_msg = messages.JsonToStatusMsg(msg.json_content)
            return (status_msg.observations,status_msg.reward,status_msg.done)
        else:
            return False
        
    def reset(self,agent_name):
        msg_json = messages.Message("reset",messages.ResetMsg(self.behavior_name,agent_name).toJson()).toJson()

        try:
            self.socket.send(msg_json.encode())
        except Exception as e:
            print(f"Message sending error: {e}")
            return False
        
        try:
            response = self.socket.recv(config.buffer_size).decode()
        except:
            print(f"Message receive error: {e}")
            return False
        
        
        msg = messages.JsonToMessage(response)

        if msg.title == "observations":
            reset_msg = messages.JsonToStatusMsg(msg.json_content)
            return (reset_msg.observations,reset_msg.reward,reset_msg.done)
        else:
            return False
        
    def close_connection(self):
        self.socket.close()
        


        