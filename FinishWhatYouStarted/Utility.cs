using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace FinishWhatYouStarted
{
    public class Utility
    {
        public static UnfinishedThing ClosestUnfinishedThingForWorkbench(Pawn pawn, Thing billGiver)
        {
            foreach (RecipeDef recipe in billGiver.def.AllRecipes)
            {
                if (recipe.unfinishedThingDef != null)
                {
                    Predicate<Thing> validator = delegate (Thing t)
                    {
                        if (!t.IsForbidden(pawn) && ((UnfinishedThing)t).Creator == pawn && ((UnfinishedThing)t).Recipe == recipe && pawn.CanReserve(t))
                        {
                            return true;
                        }
                        return false;
                    };

                    UnfinishedThing ut = (UnfinishedThing)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(recipe.unfinishedThingDef), PathEndMode.InteractionCell, TraverseParms.For(pawn, pawn.NormalMaxDanger()), 9999f, validator);
                    if (ut != null)
                    {
                        return ut;
                    }
                }
            }
            return null;
        }

    }
}
