namespace XWorldTests;

using Xunit;
using XWorldLibrary;
public class XWorldState_CoreEntities
{
    [Fact]
    public void HasInfo_MechaNode()
    {
        var mechaNode = new MechaNode();
        var result = mechaNode.Info();
        
        Assert.Matches(result, "Info");
    }

    [Fact]
    public void IsPrime_MechaNode()
    {
        var mechaNode = new MechaNode();
        var result = mechaNode.Info();
        
        Assert.Matches(result, "1 should not be prime");
    }
}