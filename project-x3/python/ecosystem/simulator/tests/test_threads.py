import threading
import time

# Global variable to hold the message
message = None
# Event to signal that a message is available
message_event = threading.Event()
# Condition variable to allow interruption
interrupt_condition = threading.Condition()

# Function for the interrupting thread
def interrupt_thread():
    global message
    while True:
        message = input("Type a message to interrupt (or 'exit' to quit): ")
        if message.lower() == 'exit':
            break
        message_event.set()  # Signal that there's a message to interrupt with
        time.sleep(0.1)  # Allow some time for the other thread to be interrupted

# Function for the interrupted thread
def interrupted_thread():
    global message
    while True:
        with interrupt_condition:
            interrupt_condition.wait_for(lambda: message_event.is_set())
            print(f"Interrupted with message: {message}")
            message_event.clear()  # Reset the event
            interrupt_condition.notify()  # Notify the interrupting thread

# Create interrupting and interrupted threads
interrupting_thread = threading.Thread(target=interrupt_thread)
interrupted_thread = threading.Thread(target=interrupted_thread)

# Start both threads
interrupting_thread.start()
interrupted_thread.start()

# Wait for both threads to finish (which they won't in this case until 'exit' is typed)
interrupting_thread.join()
interrupted_thread.join()

print("Program exited.")