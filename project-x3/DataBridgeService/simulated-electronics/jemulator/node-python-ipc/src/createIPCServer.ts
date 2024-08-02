import net from "net";

export function createIPCServer(callbacks: {
  onClientSocketEnd?: (data: string) => void;
}) {
  const { onClientSocketEnd } = callbacks;

  const server = net.createServer((socket) => {
    let chunk = "";

	socket.on("connect", () => {
		
	})
    socket.on("data", (data) => {
		console.log('socket data chunk')
      chunk = chunk + data.toString();
	  console.log(chunk)
    });
    socket.on("end", function () {
      onClientSocketEnd?.(chunk);
	  // immediately close server after client disconnects
    //   server.close();
    });

	socket.write('hello, client')

	setInterval(()=> {
		let msg = 'ping from server'
		console.log(msg)
		socket.write(msg)
	}, 2000)
  });

  return server;
}
