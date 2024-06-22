import { fromPromise, createActor, setup } from 'xstate';

// https://github.com/serverlessworkflow/specification/tree/main/examples#parallel-execution-example
export const workflow = setup({
  actors: {
    shortDelay: fromPromise(async () => {
      await new Promise<void>((resolve) =>
        setTimeout(() => {
          console.log('Resolved shortDelay');
          resolve();
        }, 1000)
      );
    }),
    longDelay: fromPromise(async () => {
      await new Promise<void>((resolve) =>
        setTimeout(() => {
          console.log('Resolved longDelay');
          resolve();
        }, 3000)
      );
    })
  }
}).createMachine({
  /** @xstate-layout N4IgpgJg5mDOIC5QAcCGAnVAbLYsFowAPMAYwFcAXASwHsA7AOgAUNtcsBRE0gYggZhGsSqkpC0mHHkI8qdJqykduZANoAGALqIUtWNRoNdIIogBs5gMyMrARgBMAVg0B2AJxOH711YcAaEABPRCs3RndI9wAODScAFldXJycfAF80wMl2GWIyeQYWNmkuHkYAZQALWnRKABE8VCCAIUx6UkrGVFIaADcwfkFGanpe2gBrCWKOWXyjRWm8VVIK6tqGrCbW1HbO7r6wBBGx0jEFTS0Lk2R9QwUTMwQwjVtUq2iHZ-NXePdAkIQDni8QiUUiiUc0WBGSyiwIeQo8yKyiWZQAMgwoBstm0Ol0etR+oN6EJjhMpij4XIkUocqUyIwMfQsY0Wri9gT+kdRrRTvMLlckCAbgZ5g9EN4QVZ4lZklZzHZXNEvO54v9EMD3KDIq43B8HOYnBlMiB6LQIHBrnDZoj7kKRXdjELHvh4gFgohXQ4YcLrQiCgtKctrrcxc6LOqEPEnIxflFzHE7HE3HYjSbsiUbQHkXTlqsavVWdtdiHRXbQI8EpG7FYbBp6-X3KnYrLUj6MzN-TS4XmqgXsWydnj9oSwKXHfRxQgk+6AQ4HC8XA2vBp3BpotErO2-dSFDmSr21oXNoPdowBCTx2GKxrXOZGOZoj93PL59HZxK3YxkvW7D47N85iqlu6Y7nMe60ge6KYgOxYdFe5amIgVYek8srajEGjxEBGh2NEirbpSWbdkG0HMrB7L4gcCFOje04aB+UbOLGUR3iuGiPu4hF0sREE9mRLInnBnQXmO9qhohjzYSChrmA4urytEj4aNKkbAq435xk2DFJGmsJEV2e7lOQpCkHA8DiWWtFIQgTiuJGHh2BhTiKh4kQbsaaRAA */
  id: 'parallel-execution',
  initial: 'ParallelExec',
  states: {
    ParallelExec: {
      type: 'parallel',
      states: {
        ShortDelayBranch: {
          initial: 'active',
          states: {
            active: {
              invoke: {
                src: 'shortDelay',
                onDone: 'done'
              }
            },
            done: {
              type: 'final'
            }
          }
        },
        LongDelayBranch: {
          initial: 'active',
          states: {
            active: {
              invoke: {
                src: 'longDelay',
                onDone: 'done'
              }
            },
            done: {
              type: 'final'
            }
          }
        }
      },
      onDone: 'Success'
    },
    Success: {
      type: 'final'
    }
  }
});

const actor = createActor(workflow);

actor.subscribe({
  complete() {
    console.log('workflow completed', actor.getSnapshot().output);
  }
});

actor.start();