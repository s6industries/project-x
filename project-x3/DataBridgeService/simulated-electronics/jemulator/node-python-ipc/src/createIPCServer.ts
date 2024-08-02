import net from "net";

import WebSocket from 'ws';



export function createIPCServer(callbacks: {
  onClientSocketEnd?: (data: string) => void;
}) {
  const { onClientSocketEnd } = callbacks;

  const ws = new WebSocket('ws://localhost:3000');

	ws.on('error', console.error);

	ws.on('open', function open() {
	ws.send('{"m":"micropython"}');
	});

	ws.on('message', function message(data) {
	console.log('received: %s', data);
	});

  const server = net.createServer((socket) => {
    let chunk = "";

	socket.on("connect", () => {
		
	})
    socket.on("data", (data) => {
		console.log('socket data chunk')
    //   chunk = chunk + data.toString();
	//   console.log(chunk)
	console.log(data.toString())

	  let mcuState = {counter: 1}
	  ws.send(JSON.stringify(mcuState));
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
