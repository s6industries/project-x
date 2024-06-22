
environment = {
    "temperature": 100
}

system = {
    "motor": 0
}

def sensor():
    return environment


def motor(speed):
    system["motor"] = speed


def notify_system():
    print('notify_system')
    
    return system
    pass