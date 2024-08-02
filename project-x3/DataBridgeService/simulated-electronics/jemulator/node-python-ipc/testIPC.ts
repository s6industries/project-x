import { runIPC } from './src/index'

console.log('runIPC')

runIPC(
	{ cmd: "ts-node", args: ["./__tests__/examples/callSendIPCMsg.ts"] },
	{
	  onClientSocketEnd(data) {
		// res(data);
		console.log('onClientSocketEnd')
		console.log(data)
	  },
	  onServerListen(ipcPath) {
		console.log('onServerListen')
		console.log(ipcPath)
	  },
	  onStdOut(data) {
		console.log('onStdOut')
		console.log(data)
	  },
	  onStdError(data) {
		console.error('onStdError')
		console.error(data)
	  }
	}
  );