using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using Verse.AI;

namespace FinishWhatYouStarted
{
    [StaticConstructorOnStartup]
    class Main
    {
        static Main()
        {
            var harmony = new Harmony("com.github.harmony.rimworld.maarx.finishwhatyoustarted");
            Log.Message("Hello from Harmony in scope: com.github.harmony.rimworld.maarx.finishwhatyoustarted");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    // RimWorld.BillUtility
    // public static Bill MakeNewBill(this RecipeDef recipe, Precept_ThingStyle precept = null)
    [HarmonyPatch(typeof(BillUtility))]
    [HarmonyPatch("MakeNewBill")]
    class Patch_BillUtility_MakeNewBill
    {
        static bool Prefix(RecipeDef recipe, Precept_ThingStyle precept, ref Bill __result)
        {
            Log.Message("HELLO FROM Patch_BillUtility_MakeNewBill");
            if (recipe.defName == "FinishWhatYouStarted_Recipe")
            {
                __result = new FinishWhatYouStarted_Bill(recipe, precept);
                return false;
            }
            return true;
        }
    }

    // RimWorld.Bill_ProductionWithUft
    // public UnfinishedThing BoundUft => boundUftInt;
    [HarmonyPatch(typeof(Bill_ProductionWithUft))]
    [HarmonyPatch("BoundUft", MethodType.Getter)]
    class Patch_Bill_ProductionWithUft_BoundUft
    {
        static void Postfix(Bill_ProductionWithUft __instance, ref UnfinishedThing __result)
        {
            Log.Message("HELLO FROM Patch_Bill_ProductionWithUft_BoundUft");
            FinishWhatYouStarted_Bill casted = __instance as FinishWhatYouStarted_Bill;
            if (casted != null && casted.boundTick != Find.TickManager.TicksGame)
            {
                if (__result == null || __result.Creator.CurJob?.bill != __instance)
                {
                    casted.ClearBoundUft();
                    __result = null;
                }
            }
        }
    }

    // RimWorld.Bill_ProductionWithUft
    // public Pawn BoundWorker
    [HarmonyPatch(typeof(Bill_ProductionWithUft))]
    [HarmonyPatch("BoundWorker", MethodType.Getter)]
    class Patch_Bill_ProductionWithUft_BoundWorker
    {
        static void Postfix(Bill_ProductionWithUft __instance, ref Pawn __result)
        {
            Log.Message("HELLO FROM Patch_Bill_ProductionWithUft_BoundWorker");
            FinishWhatYouStarted_Bill casted = __instance as FinishWhatYouStarted_Bill;
            if (casted != null && casted.boundTick != Find.TickManager.TicksGame)
            {
                if (__result == null || __result.CurJob?.bill != __instance)
                {
                    casted.ClearBoundUft();
                    __result = null;
                }
            }
        }
    }

    // RimWorld.WorkGiver_DoBill
    // private static UnfinishedThing ClosestUnfinishedThingForBill(Pawn pawn, Bill_ProductionWithUft bill)
    [HarmonyPatch(typeof(WorkGiver_DoBill))]
    [HarmonyPatch("ClosestUnfinishedThingForBill")]
    class Patch_WorkGiver_DoBill_ClosestUnfinishedThingForBill
    {
        static bool Prefix(Pawn pawn, Bill_ProductionWithUft bill, ref UnfinishedThing __result)
        {
            Log.Message("HELLO FROM Patch_WorkGiver_DoBill_ClosestUnfinishedThingForBill");
            FinishWhatYouStarted_Bill casted = bill as FinishWhatYouStarted_Bill;
            if (casted != null)
            {
                __result = Utility.ClosestUnfinishedThingForWorkbench(pawn, (Thing)casted.billStack.billGiver);
                if (__result != null)
                {
                    if (__result.Recipe != casted.recipe)
                    {
                        FinishWhatYouStarted_Bill oldBill = casted;
                        casted = new FinishWhatYouStarted_Bill(__result.Recipe, __result.StyleSourcePrecept);
                        casted.repeatMode = oldBill.repeatMode;
                        Utility.SwitchBills(oldBill, casted);
                    }
                    casted.boundTick = Find.TickManager.TicksGame;
                    //if (__result == null) { Log.Message("WTF"); }
                    casted.SetBoundUft(__result);
                    casted.ingredientFilter.SetDisallowAll();
                }
                return false;
            }
            return true;
        }
    }

    // RimWorld.WorkGiver_DoBill
    // private static Job FinishUftJob(Pawn pawn, UnfinishedThing uft, Bill_ProductionWithUft bill)
    [HarmonyPatch(typeof(WorkGiver_DoBill))]
    [HarmonyPatch("FinishUftJob")]
    class Patch_WorkGiver_DoBill_FinishUftJob
    {
        static bool Prefix(Pawn pawn, UnfinishedThing uft, ref Bill_ProductionWithUft bill, ref Job __result)
        {
            Log.Message("HELLO FROM Patch_WorkGiver_DoBill_FinishUftJob");
            // Use reflection to bypass our quantum prefixes.
            bill = uft.GetType().GetField("boundBillInt", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(uft) as Bill_ProductionWithUft;
            return true;
        }
    }
}
