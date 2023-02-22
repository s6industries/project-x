using System;
using System.Collections.Generic;
using System.Numerics;

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
// EVA
// Claptrap

// an machine comprised of MechaNodes
// AgentAvatar att
// any individual lifeform
// can implement interfaces per robot purpose / species
public class MetaBot
{

    public List<MechaNode> mechaNodes;
    public Dictionary<string, MechaNode> mechaNodesByKey;
    public List<Materia> materia;
    public string name;
    public float energy;

    public MetaBot() {

    }
    public MetaBot(string name, float _energy)
    {
        mechaNodes = new List<MechaNode>();
        materia = new List<Materia>();

        this.name = name;
        this.energy = _energy;
    }

    public string Info() {
        return "Info";
    }
}

// Kirby
// Wall-E
// BB-8

// grabs, stores, releases
public class RoboKirby : MetaBot, IVacuum, ICollector, IDigestor {

    string currentPower;
    public RoboKirby(string name, float energy) : base(name, energy)
    {
        
        // mechaNodes = new List<MechaNode>();
        // materia = new List<Materia>();

        // name = _name;
        // energy = _energy;
    }

    public string ActivateAbsorbedPower() {
        return currentPower;
    }


    // IVacuum
    public void SuckIn(Vector3 suctionVector ) {

    }
    public Materia BlowOut(Vector3 expulsionVector) {
        var detachedMateria = new Materia();
        return detachedMateria;
    }
    

    // IDigestor
    public void Decompose(Materia materia) {
        
    }
    public void Absorb(Materia materia) {

    }

    // ICollector
    public void Collect(Materia materia) {

    }
    public Materia Release(Materia materia) {
        var detachedMateria = new Materia();
        return detachedMateria;
    }
    public Materia Store(Materia materia) {
        var detachedMateria = new Materia();
        return detachedMateria;
    }
}

interface IVacuum {

    void SuckIn(Vector3 suctionVector);
    Materia BlowOut(Vector3 expulsionVector);
}

interface ICollector {
    void Collect(Materia materia);
    Materia Release(Materia materia);
    Materia Store(Materia materia);
}

interface IDigestor {
    void Decompose(Materia materia);
    void Absorb(Materia materia);
}