namespace XWorldTests;

using System.Numerics;
using Xunit;
using XWorldLibrary;

public class XWorldState_Metabots
{
    [Fact]
    public void CollectsMateria_Metabot()
    {
        // var mechaNode = new MechaNode();
        // var result = mechaNode.Info();
        
        // Assert.Matches(result, "Info");

        var crystal = new Materia("crystal");

        var robokirby = new RoboKirby("kirby", 100f);

        var vacuumVector = new Vector3(0);
        robokirby.SuckIn(vacuumVector);
        robokirby.Collect(crystal);

        // Assert.Matches(result, "Info");

        robokirby.BlowOut(vacuumVector);
        robokirby.Release(crystal);
    }

    [Fact]
    public void DigestMateria_Metabot() {
        var crystal = new Materia("crystal");

        var robokirby = new RoboKirby("kirby", 100f);

        Assert.Matches(robokirby.name, "kirby");

        var vacuumVector = new Vector3(0);
        robokirby.SuckIn(vacuumVector);
        robokirby.Collect(crystal);

        // Assert.Matches(result, "Info");

        robokirby.Decompose(crystal);
        robokirby.Absorb(crystal);

        // Assert.Matches(result, "Info");

        robokirby.ActivateAbsorbedPower();

        // Assert.Matches(result, "Info");

    }
}