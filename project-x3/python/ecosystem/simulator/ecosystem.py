print("eco")


class Region:
    def __init__(self, data, world):
        self.data = data
        self.world = world
        self.entities = []
        pass


    # when lifeforms die, they release their parts / seeds into the local environment
    def receive_entity(self, entity):
        self.entities.append(entity)
        pass
    
    
    def release_entity(self, entity):
        self.entities.remove(entity)
    
    
    def receive_resource(self, giver, resource, amount):
        if resource not in self.data["resources"]:
            self.data["resources"][resource] = 0
        
        self.data["resources"][resource] += amount
        
        self.update_qualities()
        pass


    def give(self, receiver, resource, amount):
        # give as much as possible to the receiver
        extracted = {}

        if resource not in self.data["resources"]:
            return extracted

        target_resource = self.data["resources"][resource]
        
        if amount < target_resource:
            self.data["resources"][resource] -= amount
            extracted[resource] = amount
        else:
            extracted[resource] = target_resource
            self.data["resources"][resource] = 0

        receiver.resources[resource] += extracted[resource]
        
        self.update_qualities()

        return extracted
    
    
    def update_qualities(self):
        if self.data["type"] == 'ground':
            # update ground moisture from water
            if "moisture" in self.data["qualities"]:
                self.data["qualities"]["moisture"] = self.data["resources"]["water"] 
        
        # TODO update ground temperature
    

class PotatoMachine:
    def __init__(self, world=None, ground=None, air=None):
        # self.radius = radius

        self.world:World = world

        self.regions = [air, ground]
        self.regions_by_type = {"ground": ground, "air": air}

        self.lifecycle = [
            {"name": "seed"},
            {"name": "sprout"},
            {"name": "youth"},
            {"name": "adult"},
            {"name": "elder"},
        ]
        self.lifestage = 0

        self.ticks_at_lifestage = 0
        self.ticks_in_decay = 0 # self.ticks_in_death = 0
        self.ticks_until_death = 3
        self.ticks_in_stress = 0
        self.ticks_in_sprout_conditions = 0

        # metabolic rate 0 is 'death'
        # very low metabolic rate = 'dormant' like in seed stage
        self.metabolic_rate = 1
        self.metabolic_stress = False
        self.metabolic_decline_rate = 0.1
        self.senescence = False

        # initial resources in seed stage
        self.resources = {
            # extracted
            "water": 0,
            "nitrogen": 0,
            "potassium": 0,
            "phosphorous": 0,
            "CO2": 0,
            "photons": 0,
            # created
            "proteins": 0,
            "carbs": 2,
            "fats": 0,
            "sugars": 0,
        }

        self.extraction_rate = 2

        self.senses = {"can_sprout": False}

        # TODO sum the mass of all actual body parts
        self.body_mass = 1

        # count body mass to determine morphology
        self.body_plan = {
            # for a potato, a seed = tuber with eyes
            # seed
            1: {"parts": {"seed": 1, "roots": 0, "stalk": 0, "leaves": 0, "tubers": 0}},
            # sprout
            2: {"parts": {"seed": 0, "roots": 1, "stalk": 1, "leaves": 1, "tubers": 0}},
            # youth
            3: {"parts": {"seed": 0, "roots": 2, "stalk": 2, "leaves": 2, "tubers": 1}},
            # adult
            5: {"parts": {"seed": 0, "roots": 3, "stalk": 4, "leaves": 5, "tubers": 3}},
            # elder
            6: {"parts": {"seed": 0, "roots": 4, "stalk": 2, "leaves": 0, "tubers": 5}},
        }

        self.body = self.body_plan[1]

        self.bodypart_templates = {
            "seed": {"mass": 0.01, "volume": 0.1, "eyes": 1},
            "roots": {"mass": 0.01, "length": 0.1},
            "stalk": {"mass": 0.01, "length": 0.1},
            "leaves": {"mass": 0.01, "area": 0.1, "color": "green"},
            "tubers": {"mass": 0.01, "volume": 0.1, "eyes": 0},
        }
        
        
    def plant(self, ground, air):
        self.regions = [ground, air]
        self.regions_by_type = {"ground": ground, "air": air}
        ground.receive_entity(self)
        air.receive_entity(self)


    def tick(self):
        print("> tick")

        if self.metabolic_rate == 0:
            print("dead")
            self.decay()
            return

        self.ticks_at_lifestage += 1

        # automatic / metabolic
        self.extract()
        self.metabolize()
        self.build()
        self.update_body()
        self.update_lifecycle()

        # agent mind
        self.sense()
        self.think()
        self.plan()
        self.execute()

        # report for interface
        self.get_state()


    #region reproduction
    def create_child_entities(self):
        children = []
        # each tuber becomes a seed
        for c in range(0, self.body["parts"]["tubers"]):
            seed = PotatoMachine()
            children.append(seed)
        
        return children
    #endregion


    #region utilities
    def get_state(self):

        return {
            "lifestage": self.lifecycle[self.lifestage],
            "parts": self.body,
            "resources": self.resources,
            "bodymass": self.body_mass,
        }
    # endregion


    #region metabolism
    def decay(self):
        self.ticks_in_decay += 1
        # decomposition into environment
        # when the agent disconnects from the body, the entity becomes a collection of resources that can disperse into the local region

        
        if self.ticks_in_decay == self.ticks_until_death:
            # release body parts into environment
            print(f'--- convert tubers into seeds')
            seeds = self.create_child_entities()
            
            if len(seeds) > 0:
                for seed in seeds:
                    self.world.plant_entity(seed, self.regions_by_type["ground"], self.regions_by_type["air"])
                print(f'!!! --- {len(seeds)} seeds created')
            else: 
                print(f'!!! --- no seeds created')
            
            self.regions_by_type["ground"].receive_resource(self, "plant_decay", 1)
            
            # release self from world
            self.world.release_entity(self)
            print(f'!!! --- entity released itself from world')
            pass
        
      
    def extract(self):
        if self.senescence:
            print(f'!!! stop extracting')
            return

        # TODO extract at extraction_rate
        # current_stage = self.lifecycle[self.lifestage]

        # if current_stage["name"] == "sprout":
        if self.lifestage > 0:
            for region in self.regions:
                if region.data["type"] == "ground":
                    region.give(self, "nitrogen", 1)
                    region.give(self, "water", 1)
                    pass
                elif region.data["type"] == "air":
                    region.give(self, "CO2", 1)
                    region.give(self, "photons", 1)
                    pass
            pass

    def update_metabolism(self):
        
        # seed stage has dormant metabolism
        if self.lifestage == 0:
            return
        
        # check for essential resources to continue metabolism
        if (
            self.resources["nitrogen"] == 0
            or self.resources["CO2"] == 0
            or self.regions_by_type["air"].data["qualities"]["light"]["yellow"] < 30
        ):
            self.metabolic_stress = True
        print(self.body)
        # check for essential body parts  to continue metabolism
        if (
            self.body["parts"]["roots"] == 0
            or self.body["parts"]["stalk"] == 0
            or self.body["parts"]["leaves"] == 0
        ):
            self.metabolic_stress = True

            # TODO can regenerate essential body parts?
        else:
            self.metabolic_stress = False

        if self.metabolic_stress:
            self.ticks_in_stress += 1
            
        # slowly decrease the metabolic rate in senescence
        # decrease metabolic rate to 'death'
        if (
            self.senescence 
            or self.metabolic_stress
        ):
            if self.metabolic_rate >= self.metabolic_decline_rate:
                self.metabolic_rate -= self.metabolic_decline_rate
            else:
                self.metabolic_rate = 0    
            print(f'!!! decreasing metabolic rate {str(self.metabolic_rate)}')
            
        # return not self.metabolic_stress
    

    def metabolize(self):
        # factorio supply chain

        self.update_metabolism()

        # spend energy / resources
        # convert resources to energy / resources
        if self.resources["nitrogen"] > 0:
            self.resources["nitrogen"] -= 1
            self.resources["proteins"] += 1

        # conditions for photosynthesis
        if (
            self.resources["CO2"] > 0
            and self.regions_by_type["air"].data["qualities"]["light"]["yellow"] > 30
        ):
            self.resources["CO2"] -= 1
            self.resources["sugars"] += 1
        pass


    def build(self):
        # reduce building / regeneration in senescence
        if self.senescence:
            return

        # convert resources into body mass
        if self.resources["sugars"] > 1 and self.resources["proteins"] > 1:
            self.resources["sugars"] -= 1
            self.resources["proteins"] -= 1

            self.body_mass += 1

        # TODO alternative implementation:
        # build step adds mass / creates new individual body parts

        pass


    def update_body(self):

        # print(self.body_mass)
        # set the body parts from the total mass
        for key in self.body_plan.keys():
            # print(key)
            if key <= self.body_mass:
                self.body = self.body_plan[key]
            elif key > self.body_mass:
                continue

        # TODO alternative implementation:
        # build step adds mass / creates new individual body parts
        # then sum the mass of all actual body parts

        pass

    def update_lifecycle(self):
        print(self.lifestage)
        print(self.ticks_at_lifestage)

        if self.senescence:
            return

        # finite state machine
        current_stage = self.lifecycle[self.lifestage]

        # if seed, detect if conditions to sprout
        # seed > sprout
        if current_stage["name"] == "seed":
            if self.senses["can_sprout"]:
                print(f'!!! seed found conditions to sprout')
                self.advance_lifestage()

        # lifestage determined by body maturity
        # sprout > youth
        if self.lifestage == 1:
            if self.body["parts"]["tubers"] >= 1:
                self.advance_lifestage()

        # youth > adult
        if self.lifestage == 2:
            if self.ticks_at_lifestage >= 2:
                # if self.body["roots"] >= 4:
                self.advance_lifestage()

        # adult > elder
        if self.lifestage == 3:
            if self.ticks_at_lifestage >= 2:
                # if self.body["leaves"] >= 3:
                self.advance_lifestage()

        # elder > senescence
        if self.lifestage == 4:
            if self.ticks_at_lifestage >= 3:
                self.advance_lifestage()
        pass
    

    def advance_lifestage(self):

        if self.lifestage < len(self.lifecycle) - 1:
            self.lifestage += 1
            self.ticks_at_lifestage = 0
        else:
            # self.metabolic_decline = True
            self.senescence = True

    #endregion


    #region agent mind
    def sense(self):
        current_stage = self.lifecycle[self.lifestage]

        # if seed, detect if conditions to sprout
        if current_stage["name"] == "seed":
            ground = self.regions_by_type["ground"]
            if (
                ground.data["qualities"]["temperature"] > 5
                and ground.data["qualities"]["moisture"] > 5
            ):
                # self.metabolic_rate = 3
                self.senses["can_sprout"] = True

        elif current_stage["name"] == "sprout":
            pass
        pass


    def think(self):
        pass


    def plan(self):
        # utility AI
        pass


    def execute(self):

        pass
    #endregion

    

# a zone = subsection volume within the simulation world. 
# assumed planetary environment with ground & water regions beneath air region
# ex. a terrarium

# a zone can have an isolated time scale/tick rate than the world
# like the movie Old https://en.wikipedia.org/wiki/Old_(film)

# a zone contains regions of types (air, ground, liquid) and entities
# an entity can exist in multiple regions simultaneously (i.e. a plant in the ground/water also in the air)
# the zone provides a container for which the simulation can be reported/rendered to the player

# a zone can have many regions. regions are local to the entities/ agents
# adding resources/entities to a zone can distribute those additions among its regions
# ex. water added to the zone diffuses into all its ground regions
# ex. CO2 added to the zone diffuses into all its air regions
class Zone:
    def __init__(self):
        self.regions = []
        self.regions_by_type = {
            "ground": [],
            "air": [],
            "liquid": []
        }
        pass
    
    def add_solid(self, resource):
        pass
    
    def add_gas(self, resource):
        pass
    
    def add_liquid(self, resource): 
        pass
    
    # for entitites that are fixed in adjacent ground/water and air regions
    def plant_entity(self, entity, ground_target=None, air_target=None):
        pass
    
    # for entities that can move between region types
    def add_entity(self, entity):
        pass
    
    # for planted or moving entity
    def remove_entity(self, entity):
        pass


# the entire absolute spacetime for simulation
# can synchronize or isolate time passing across all its zones
class World:
    def __init__(self, max_ticks=15):

        self.zones = []
        self.regions = []
        self.entities = []

        self.tickcount = 0
        self.max_ticks = max_ticks


    def setup_planet(self, ground, air):
        self.regions.append(ground)
        self.regions.append(air)


    def plant_entity(self, entity, ground, air):
        self.entities.append(entity)
        entity.world = self
        entity.plant(ground, air)
        
    
    def release_entity(self, entity):
        
        for region in entity.regions:
            region.release_entity(entity)
        
        self.entities.remove(entity)
    
    
    def start(self):
        while self.tickcount < self.max_ticks:
            self.tick()


    def tick(self):
        print("======================================")
        print(f"=== WORLD tick {str(self.tickcount)}")
        # print(tickcount)
        self.tickcount += 1

        # for each entity
        # external updates to body from environment
        # each entity senses
        for entity in self.entities:
            entity.tick()

            # check entity state
            state = entity.get_state()
            print(state)
        
        self.get_state()
    
    
    def get_state(self):
        print(f'=== world state: ')
        for region in self.regions:
            print(region.data)



def setup_simulation(max_ticks=15):
    
    world = World(max_ticks)

    ground = Region(
        {
            "type": "ground",
            "resources": {
                "water": 10,
                "nitrogen": 10,
                "potassium": 10,
                "phosphorous": 10,
                "plant_decay": 0
            },
            "qualities": {"temperature": 10, "moisture": 10, "pH": 10},
            "onTick": {"resources": {"water": -1}},
            "entities": [],
            # "entities_by_type": {"potato": {}},
        }, 
        world
    )

    air = Region(
        {
            "type": "air",
            "resources": {"CO2": 10, "photons": 10},
            "qualities": {"temperature": 10, "humidity": 10, "light": {"yellow": 100}},
            "entities": [],
            # "entities_by_type": {"potato": {}},
        }, 
        world
    )
    
    world.setup_planet(ground, air)
    # potato = PotatoMachine(ground, air)
    potato = PotatoMachine()

    world.plant_entity(potato, ground, air)
    
    return world


def main():
    # setup sim
    # world = setup_simulation(max_ticks=20) # 1 potato died with seeds
    world = setup_simulation(max_ticks=38) # seeds died without resources
    world.start()
    

if __name__ == "__main__":
#    print("executed directly")
   main()
else:
#    print("imported")
    pass

