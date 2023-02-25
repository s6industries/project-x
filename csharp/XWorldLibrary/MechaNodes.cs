using System;
using System.Collections.Generic;

namespace XWorldLibrary;

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