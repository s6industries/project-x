using System;
using System.Collections.Generic;

namespace XWorldLibrary;


// Inventory = attachment & release
// Node has 1 or more properties:
// - collector
// - ejector
// - capacitor
// - neighbor(s)
// direction/flow of energy through neighbors when activating an ability
// abilities as side effects of energy flowing through a series of nodes
// a walking marble machine 
// a MechaNode

// Megaman
// Kirby
// Wall-E
// EVA
// Claptrap

// an agent comprised of MechaNode
// any individual lifeform
public class MetaBot
{

    public List<MechaNode> mechaNodes;

    public MetaBot()
    {

    }

    public string Info() {
        return "Info";
    }
}


// MechaNodes compose a MetaBot

public class MechaNode
{

    public List<MechaNode> neighborNodes;
    public List<Materia> collectedMateria;

    public MechaNode()
    {

    }
    
    public string Info() {
        return "Info";
    }
    public void Gather(Materia materia)
    {

    }

    public void Release(Materia materia)
    {

        // drop or shoot materia
        // on release, is there a sideeffect?
        // to the MechaNode?
        // to the materia?
        // to the MetaBot?
    }

    public void Attach(MechaNode mechaNode)
    {

    }
}

// a unit of matter / latent energy
// a rock, a potato, a tiger, a bucket of water
// can be nested
// can be converted into energy / resource
public class Materia
{
    
    public string type;

    public Materia()
    {

    }

    public string Info() {
        return "Info";
    }
}