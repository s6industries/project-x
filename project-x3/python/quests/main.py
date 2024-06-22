# cd /mnt/c/Users/micha/OneDrive/Documents/GitHub/arkadia-xr/python/quests
# C:\Users\micha\OneDrive\Documents\GitHub\arkadia-xr\python\quests

from transitions import Machine

class Matter(object):
    pass

lump = Matter()

# The states
states=['solid', 'liquid', 'gas', 'plasma']

# And some transitions between states. We're lazy, so we'll leave out
# the inverse phase transitions (freezing, condensation, etc.).
transitions = [
    { 'trigger': 'melt', 'source': 'solid', 'dest': 'liquid' },
    { 'trigger': 'evaporate', 'source': 'liquid', 'dest': 'gas' },
    { 'trigger': 'sublimate', 'source': 'solid', 'dest': 'gas' },
    { 'trigger': 'ionize', 'source': 'gas', 'dest': 'plasma' }
]

# Initialize
machine = Machine(lump, states=states, transitions=transitions, initial='liquid')

# # Now lump maintains state...
print(lump.state)
# >>> 'liquid'

# # And that state can change...
# # Either calling the shiny new trigger methods
lump.evaporate()
print(lump.state)
# >>> 'gas'

# # Or by calling the trigger method directly
lump.trigger('ionize')
print(lump.state)
# >>> 'plasma'