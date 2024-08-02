import peripherals
import ipc

print('micropython REPL')

from threading import *
import time

# creating a function
def thread_1():      
    ipc.read_from_socket('/tmp/atri17d30c27b2c612cc7830628adc966733')                
#   for i in range(5):
#     print('this is thread T')
#     time.sleep(3)

# creating a thread
T = Thread(target = thread_1) 

# change T to daemon
T.setDaemon(True)                   

# starting of Thread T
T.start()   

