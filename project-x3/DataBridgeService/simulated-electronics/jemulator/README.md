# jemulator

## Summary
Full-Stack IoT Simulation

system architecture [https://miro.com/app/board/uXjVKvZqtsU=/?share_link_id=769863731907]

our virtual microcontroller (JEM) runs micropython and has a peripheral/driver library (based on real electronics sensor & effector components) to easily implement & customize use cases. This is based on the actual implementation of JEM microcontroller firmware & micropython SDK.
https://github.com/kitlab-io/micropython

Example use cases:
- RC car
- mini FPV drone
- weather station 
- automated gardening system

To keep the simualtion learner-friendly, the user is expected to implement MCU business logic in micropython. 
To keep the simulation authenthic, the micropython execution affects simulated MCU+peripheral circuit state which streams its macro state changes to the environmental simulation/human perception layer (Unity game engine scene). We try to balance electronic authenticity and practical fidelity. For example, setting PWM duty cycle actually changes the voltage state of a GPIO pin thousands of times per second (mHz). But streaming that fidelity of GPIO state changes to the environmental simulation layer at mHz/1000s FPS rates is not practical. So effectively the human player needs to know the result of that PWM setting via the attached peripheral like a motor or buzzer, whose human-perceived peripheral state (deltas) could be streamed to the environment simulation layer at most 30-60 Hz/FPS.

## Implementation

This simulation stack is meant to run on a desktop computer running OSX or Windows, so the creator workflow and all simulation layers can be run on a single device. An advanced workflow/simulation across multiple devices, such as running the MCU+peripheral and DataBridge service on a headless Linux server while running the environmental simulation in a VR headset or mobile AR view, is also possible but not implemented.  

The user MCU firmware > MCU > MCU peripheral > environment stack is implemented as Python process(user MCU firmware > MCU) > NodeJS process (MCU > MCU peripheral) > environment (Unity runtime)

When running the stack via terminal window with (micro)python REPL, we execute an interactive python process which spawns a NodeJS process emulating the MCU + peripherals. Another Node JS process runs the DataBridge service, a websocket server which receives MCU peripheral state changes at most 30-60 Hz/FPS emitted by the emulated MCU+peripheral NodeJS process and forwards them to the environment simuation (Unity runtime process). Player input in the environment simulation is routed back through the websocket server to the MCU emulator NodeJS process which updates Python variables in the MCU/firmware emulation Python process. 

When running the stack without a terminal window UI/REPL, the DataBridge service can receive a player command/configuration to spawn a non-interactive python process which spawns a NodeJS process emulating the MCU + peripherals. This non-REPL/headless emulation is better for managing many instances of preconfigured MCU+peripheral microsystems. For example, simulating a fleet of spider bots that harvest ripe fruits on trigger from a garden automation system alert.