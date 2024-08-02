import peripherals
import ipc

print('micropython REPL')

from threading import *
import time

# creating a function
def thread_1():      
    ipc.read_from_socket('/tmp/atricb7318103fa8cfdf0f14d0b769eed860')                
#   for i in range(5):
#     print('this is thread T')
#     time.sleep(3)

# creating a thread
T = Thread(target = thread_1) 

# change T to daemon
T.daemon = True                 

# starting of Thread T
T.start()   

