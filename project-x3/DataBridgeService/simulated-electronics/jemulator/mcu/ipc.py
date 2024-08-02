import os
import platform

def send_ipc_msg(msg: bytes):
    ATRI_IPC_PATH = os.environ["ATRI_IPC_PATH"]
    if platform.system() == "Windows":
        import win32file
        import win32pipe
        handle = win32file.CreateFile(ATRI_IPC_PATH, win32file.GENERIC_READ | win32file.GENERIC_WRITE,
                              0, None, win32file.OPEN_EXISTING, win32file.FILE_ATTRIBUTE_NORMAL, None)
        win32file.WriteFile(handle, msg)
    else:
        import socket
        s = socket.socket(socket.AF_UNIX, socket.SOCK_STREAM)
        s.connect(ATRI_IPC_PATH)
        s.send(msg)
        s.close()

import socket

counter = 0

def get_counter():
    return counter

# Define the path to your Unix socket
SOCKET_PATH = '/tmp/my_unix_socket'

# https://docs.python.org/3/library/socket.html#example

def read_from_socket(socket_path):
    global counter
    # Ensure the socket path exists
    if not os.path.exists(socket_path):
        print(f"Socket path {socket_path} does not exist.")
        return
    
    # Create a Unix socket
    with socket.socket(socket.AF_UNIX, socket.SOCK_STREAM) as sock:
        # Connect to the Unix socket
        sock.connect(socket_path)
        print(f"Connected to socket {socket_path}")
        sock.send(b'hi, server')
        
        try:
            while True:
                # Read data from the socket
                data = sock.recv(1024)
                
                if not data:
                    # If no data is received, the connection might be closed
                    # print("No more data received. The connection might be closed.")
                    break
                
                # Process the received data
                # print(f"Received: {data.decode('utf-8')}")
                counter = counter + 1
                sock.send(b'pong from client')
                
        except KeyboardInterrupt:
            print("Interrupted by user.")
        except Exception as e:
            print(f"An error occurred: {e}")

# if __name__ == "__main__":
#     read_from_socket(SOCKET_PATH)