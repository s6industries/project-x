# Ecosytem Simulation 

## Entity Component Systems
https://github.com/benmoran56/esper
https://github.com/ikvk/ecs_pattern


## System Architectural Digest

the interface sets the rate / specificity of the simulation publishing its state

the interface sends commands that mutate the world, or specifically conditions / entities within the simulation 
the world updates the local conditions / resources for each entity

the entities proceed with operating per their metabolism and agent's plan

daemon threads
https://www.youtube.com/watch?v=Kae9aV9DO7k
https://stackoverflow.com/questions/44908661/can-threads-create-sub-threads-in-python
https://pythonforthelab.com/blog/handling-and-sharing-data-between-threads/
https://www.youtube.com/watch?v=-bM--7W7F0Y

# provide a websocket server for interface to / rendering of simulation world

# 1 async thread for the websocket interface

# 1 async thread for the simulation debug UI 
  
# 1 async thread for the world simulation

within the world simulation thread, each entity/metabolism/agent runs its local metabolize/sense/think/act loop
could be implemented as threads per entity or ECS

# intial sync
# publish full snapshot of world state 


# incremental sync