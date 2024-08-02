# Project X3

## Summary
Ecosystem & IoT simulator which allows high fidelity emulation of microcontrollers and biological species.

## Structure

- EcosystemSimulator is the Unity project which allows the creator/player to immersively experience the ecosystem they are designing.

- DataBridgeService manages all connections to emulated microcontrollers, biological species lifecycles, environmental conditions, and any data sources/logic that is combined in the simulated ecosystem.

## Usage

Run each layer of the stack:
- environment layer (Unity)
- embedded system data bridge (NodeJS)
- emulated microcontroller process (NodeJS)

## Notes
### On Windows
Start WSL
$ cd /mnt/c/Users/micha/OneDrive/Documents/GitHub/project-x/project-x3/DataBridgeService/simulated-electronics/rp2040js/demo
$ npm run start:micropython

npm run start:circuitpython