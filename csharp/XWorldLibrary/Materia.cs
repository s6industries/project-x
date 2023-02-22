using System;
using System.Collections.Generic;

namespace XWorldLibrary;


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

    public Materia(string type)
    {
        this.type = type;
    }

    public string Info() {
        return "Info";
    }
}