Name : Tiny Car Controller Advance
Version : 2.1
Author : David Jalbert
Last modified : 2023-11-10
Description : Simple 3d dynamic vehicle controller for Unity

Support: 
- Unity Asset Store : https://assetstore.unity.com/packages/slug/198873
- WebGL demo : https://davidjalbert.itch.io/tiny-car-controller-advance-webgl-demo
- Email : jalbert.d@hotmail.com
- Mastodon: https://mastodon.gamedev.place/@davidjayindie
- Twitter: https://twitter.com/DavidJayIndie

=====================================================
Package structure
=====================================================

TinyCarControllerAdvance - Root of the package.
    Contains this help file and the documentation in pdf format.
Scripts
    Scripts/Core - Scripts to add to your game objects.
    Scripts/Helpers - Scripts that take care of input, camera, and effects.
    Scripts/Internal - Scripts used by the scripts above. Not to be used on their own.
Examples - Example scenes with different use cases.

=====================================================
Getting started
=====================================================

The basics

1) Create an empty GameObject in your scene.
2) Add the script at 'Assets/DavidJalbert/TinyCarControllerAdvance/Scripts/Core/TCCAPlayer.cs' to the GameObject.
3) Add your car model to the empty GameObject. Make sure you have one object per wheel and one for the body, and that they are separated.
4) Add the script at 'Assets/DavidJalbert/TinyCarControllerAdvance/Scripts/Core/TCCABody.cs' to the car body object.
5) Add the script at 'Assets/DavidJalbert/TinyCarControllerAdvance/Scripts/Core/TCCAWheel.cs' to each of the car's wheel objects.
6) Set the parameters of the body and wheel scripts to your liking.

For input

1) Add the script at 'Assets/DavidJalbert/TinyCarControllerAdvance/Scripts/Helpers/TCCAStandardInput.cs' to an empty GameObject.
2) Drag and drop the player object to the "Car Controller" field.
3) Set each input action to the desired key or button.

For the camera

1) Add the script at 'Assets/DavidJalbert/TinyCarControllerAdvance/Scripts/Helpers/TCCACamera.cs' to your camera GameObject.
2) Drag and drop the player object to the "Car Controller" field.
3) Adjust the parameters to define the behavior of the camera relative to the vehicle.

=====================================================
Tips
=====================================================

- Fields tooltips and contextual tips in the editor explain each parameter's functionality.
- Don't be afraid to experiment with parameter values to find the right behavior for your needs.
- Check out the example scene at 'Assets/DavidJalbert/TinyCarControllerAdvance/Examples/ExampleSandbox.unity'.
- You can open the helper scripts in your code editor to see how they work, and learn how to code your own.
- Don't forget to read documentation.pdf !

=====================================================
Version History:
=====================================================

Version 2.1 - 2023-11-10

- Improved the stability of the connection between the wheels and body at high speeds.

Version 2.0 - 2023-01-05

- Added a steering stabilizer to the body script, to prevent the vehicle from spinning out of control.
- Added the option to freeze the vehicle on the X, Y, and Z axes.
- Made changes to the roll and pitch countering smoothing function, now have less swaying.
- Added a small race track segment to the demo scene.
- Minor bug fixes.

Version 1.4.1 - 2022-08-19

- Now uses Unity 2021.3.6f1

Version 1.4.0 - 2022-05-01

- Added the possibility to extend the TCCABody script to access the collision functions.
- Changing the position and rotation of the vehicle now recenters it as well
- The vehicle can now be a child of other GameObjects
- Fixed a bug where the wheels would dealign if the car is disabled while the wheels are turning and then re-enabled.
- Fixed a bug where engine and drifting sounds would stop playing when the car is disabled and re-enabled.

Version 1.3.0 - 2022-01-25

- Updated the camera component to use physics, to stop stuttering when turning.

Version 1.2.0 - 2021-12-28

- Added functions to easily set the vehicle's position and rotation
- Added function to recenter the vehicle's body to its container
- Added function to immobilize the vehicle, setting its velocities to zero

Version 1.1.0 - 2021-08-25

- Added roll and pitch stabilization
- Added parameters for the steering and motor
- Added motorcycle in the sandbox scene

Version 1.0.1 - 2021-08-09

- Fixed broken dependencies
- Fixed physics when importing in new project

Version 1.0.0 - 2021-06-28

- Initial release.
