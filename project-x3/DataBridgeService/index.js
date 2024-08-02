const crypto = require('crypto');
const express = require('express');
const { createServer } = require('http');
const WebSocket = require('ws');

const app = express();
const port = 3000;

const server = createServer(app);
const wss = new WebSocket.Server({ server });

let clients = []

function detectMessageType(data) {
	let sendToEnvironmentSimulation = true
	return {
		sendToEnvironmentSimulation,
		data
	}
}

function sendToEnvironmentSimulation(msg, ws) {
	// get the ws client for environment simulation
	// client.send(msg)
}

function sendToMicrosystemSimulation(msg, ws) {
	// get the ws client for microsystem simulation
	// client.send(msg)
}

function checkDataForLED(data, ws) {

  if (data["GPIO25"] === undefined ) return;

  console.log(data.GPIO25)
  console.log(data.GPIO25 == 0)
  console.log(data.GPIO25 === 0)
  console.log(data.GPIO25 == false)
  console.log(data.GPIO25 === false)

  if (data.GPIO25 == 1) {
    console.log("on");
    wss.clients.forEach(function each(client) {
      if (client !== ws && client.readyState === WebSocket.OPEN) {
        client.send("1");
        // client.send(data, { binary: isBinary });
      }
    });
    
  } else if (data.GPIO25 == 0) {
    console.log("off");
    wss.clients.forEach(function each(client) {
      if (client !== ws && client.readyState === WebSocket.OPEN) {
        client.send("off");
        // client.send(data, { binary: isBinary });
      }
    });
  }
}

wss.on('connection', function(ws) {
  console.log("client joined.");

  clients.push(ws);
  // TODO client reports its type so server can target sends

  // send "hello world" interval
  // const textInterval = setInterval(() => ws.send("hello world!"), 100);

  // send random bytes interval
  // const binaryInterval = setInterval(() => ws.send(crypto.randomBytes(8).buffer), 110);

  ws.on('message', function(data) {
    if (typeof(data) === "string") {
      // client sent a string
      console.log("string received from client -> '" + data + "'");
      data = JSON.parse(data)
      console.log(data)
      checkDataForLED(data, ws)
      
	  let msg = detectMessageType(data)
	  if (msg.sendToEnvironmentSimulation) {
		sendToEnvironmentSimulation(msg, ws)
	  }

    } else {
      console.log("binary received from client -> " + Array.from(data).join(", ") + "");
    }
  });

  ws.on('close', function() {
    console.log("client left.");
    // clearInterval(textInterval);
    // clearInterval(binaryInterval);

    clients.splice(clients.indexOf(ws), 1);
  });
});

server.listen(port, function() {
  console.log(`Listening on http://localhost:${port}`);
});
