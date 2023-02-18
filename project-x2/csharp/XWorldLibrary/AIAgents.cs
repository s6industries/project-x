using Active.Core;
using static Active.Raw;

using AStar.Options;
using AStar;

namespace XWorldLibrary;

public class Soldier
{

    private WorldGrid _world;

    void SetupPathfinding()
    {
        var level =   @"XXXXXXX
                        X11X11X
                        X11111X
                        XXXXXXX";

        _world = Helper.ConvertStringToPathfinderGrid(level);

        var pathfinder = new PathFinder(_world);

        var path = pathfinder.FindPath(new Position(1, 1), new Position(2, 3));

        Helper.Print(_world, path);
    }

    public status Step()
    {
        return Attack() || Defend() || Retreat();
    }

    static status Attack()
    {
        return done;
    }

    static status Defend()
    {
        return done;

    }

    static status Retreat()
    {
        return done;
    }


}