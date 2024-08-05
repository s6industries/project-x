using System;
using UnityEngine;

namespace MidiPlayerTK
{
    // Unlike SoundFont effects, they applied to the whole player. On the other hand, the Unity effects parameters are rich and, obviously based on Uniy algo!
    // https://docs.unity3d.com/Manual/class-AudioEffectMixer.html
    // Only most important effect are integrated in Maestro: Reverb and Chorus. On need, others effects could be added. 
    // note
    //     - Unity effects integration modules are exclusively available with the Maestro MPTK Pro version. 
    //     - By default, these effects are disabled in Maestro. 
    //     - To enable them, you’ll need to adjust the settings from the prefab inspector: Synth Parameters / Unity Effect.
    //     - Each settings are available by script.
    // Available with version Maestro Pro 
    public partial class MPTKEffectUnity : ScriptableObject
    {

    }
}
