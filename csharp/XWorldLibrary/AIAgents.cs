using Active.Core;
using static Active.Raw;
// https://github.com/active-logic/activelogic-cs/blob/master/Doc/QuickStart.md

using AStar.Options;
using AStar;
// https://github.com/valantonini/AStar

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