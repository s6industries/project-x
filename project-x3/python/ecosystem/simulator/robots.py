# simulate robots with microcontrollers running micropython


# for simulating micropython REPL, spawn a separate python process executing the micropython code
# that process dedicated to REPL also binds to the spacetime simulation process synced to microcontroller/micropython execution environments as subthreads (not dedicated python processes)

# reference implementation of dedicated process for REPL: https://stackoverflow.com/questions/11712629/opening-a-python-thread-in-a-new-console-window

# zone is the absolute spacetime
# environment is the agent's relative model of the regions it inhabits

zone = {
    "regions": [
        {
            "qualities": {
                "temperature": 10,
                "humidity": 5,
                "atmospheric_pressure": 7,
            },
            "location": [0, 0, 0],
            "area": [1, 1],
        },
        {
            "qualities": {
                "temperature": 8,
                "humidity": 4,
                "atmospheric_pressure": 9,
            },
            "location": [1, 0, 0],
            "area": [1, 1],
        },
    ],
    "entities": {"robots": [{"location": [0, 0, 0]}]},
}

# goal: enabling UGC sandbox environment where the players update code for embedded systems in real time
# injecting and executing containerized code for little robots is like micro-patching the game client

# the absolute spacetime rendering/interface for the player has complete knowledge of the player's robot, but does not know the robot's environment model / mind executing the player's code
# for each robot, the absolute spacetime simulation pushes sensor/command data (relative to its robot body/sensor range) into its environment model (which is processed on the next loop/frame of the containerized microconroller code execution)
# the spacetime simulation has bindings for each of the robot's sensors & actuators, so it can push the appropriate data (or deltas) into the robot's environment model

# each robot pushes commands to its body/actuators in the absolute spactime simulation, but does not know the effects the commands have on its body/environment.

# ex. via spactime/microcontroller binding, a command from microncontroller code to set the robot's motor_1 speed to 10 will move the wheels in the spacetime simulation. the likely effect is to move the robot's body in absolute space which also changes the absolute environment qualities and robot body orientation deteted by the corresponding sensors.  

# for robot[0]
# relative to its body / sensors. sampled from zone
# by default, the agent does not know it is in a zone or it's absolute position in that zone
# its environment is sampled by the spacetime simulation (from the vicinity of its body) in the zone, and the latest environment model is available on each frame of its agent mind/execution loop of the simulated microcontroller

environment = {
    "commands": {},
    "sensors": {
        "temperature": {},
        "humidity": {},
        "atmospheric_pressure": {},
        "orientation": {}
    },
    "actuators": {
        "motors": [
            { "function": "wheel1"},
            { "function": "wheel2"},
            { "function": "wheel3"},
            { "function": "wheel4"}, 
        ], 
        "buzzer": {},
        "LED": {}    
    },
    "qualities": {"temperature": 10, "humidity": 5, "atmospheric_pressure": 7},
}

import threading

def main():
    
    spacetime_thread = setup_instance_spacetime()
    
    microcontroller_thread = setup_instance_embedded_system()
    
    duplex_channel = connect_to_spacetime_simulation(spacetime_thread)
    
    bind_embedded_system_to_spacetime_simulation(microcontroller_thread, duplex_channel)
    

def setup_instance_spacetime():
    pass


def setup_instance_embedded_system():
    
    # create an easily inspectable environment model of the embedded system in the main thread that the sub thread per microcontroller instance can read and write
    
    # create a inbound message queue to simulate the network messages sent to the microcontroller
    
    # create an outbound message queue to simulate the network messages sent from the microcontroller
    

    pass

# local websocket server
# spacetime simulation has a websocket client 
def connect_to_spacetime_simulation():
    pass

def bind_embedded_system_to_spacetime_simulation():
    
        # when the microcontroller code changes the state of the microntroller (ex. GPIO changes) notify the spacetime simulation to update corresponding robot peripherals (ex. acutators, emitters connected to the changed GPIO). The player would finally see changes to the robot's physical simulation (ex. wheels turning, sound from speakers, light from LEDs/lasers) on the next frames of the spacetime simulation/game engine.
    pass