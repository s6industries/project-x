using Active.Core;                
using static Active.Raw;  

namespace XWorldLibrary;

public class Soldier{

    public status Step(){
        return Attack() || Defend() || Retreat();
    }

    static status Attack(){
        return done;
    }

    static status Defend(){
        return done;
        
    }

    static status Retreat(){
        return done;
    }


}