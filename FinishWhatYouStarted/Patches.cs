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
            //Log.Message("HELLO FROM Patch_BillUtility_MakeNewBill");
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
            //Log.Message("HELLO FROM Patch_Bill_ProductionWithUft_BoundUft");
            if (__instance is FinishWhatYouStarted_Bill)
            {
                if (__result != null && __result.Creator.CurJob?.bill != __instance)
                {
                    __instance.ClearBoundUft();
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
            //Log.Message("HELLO FROM Patch_Bill_ProductionWithUft_BoundWorker");
            if (__instance is FinishWhatYouStarted_Bill)
            {
                if (__result != null && __result.CurJob?.bill != __instance)
                {
                    __instance.ClearBoundUft();
                    __result = null;
                }
            }
        }
    }

    // RimWorld.WorkGiver_DoBill
    // private static bool TryFindBestBillIngredients(Bill bill, Pawn pawn, Thing billGiver, List<ThingCount> chosen, List<IngredientCount> missingIngredients)
    [HarmonyPatch(typeof(WorkGiver_DoBill))]
    [HarmonyPatch("TryFindBestBillIngredients")]
    class Patch_WorkGiver_DoBill_TryFindBestBillIngredients
    {
        static bool Prefix(Bill bill, Pawn pawn, Thing billGiver, List<ThingCount> chosen, List<IngredientCount> missingIngredients, ref bool __result)
        {
            //Log.Message("HELLO FROM Patch_WorkGiver_DoBill_TryFindBestBillIngredients");
            if (bill is FinishWhatYouStarted_Bill)
            {
                FinishWhatYouStarted_Bill oldBill = (FinishWhatYouStarted_Bill)bill;
                UnfinishedThing ut = Utility.ClosestUnfinishedThingForWorkbench(pawn, billGiver);
                if (ut != null)
                {
                    //Log.Message("FOUND UT IN Patch_WorkGiver_DoBill_TryFindBestBillIngredients");
                    chosen.Add(new ThingCount(ut, 1));
                    FinishWhatYouStarted_Bill newBill = new FinishWhatYouStarted_Bill(ut.Recipe, ut.StyleSourcePrecept);
                    Utility.SwitchBills(oldBill, newBill);
                    newBill.ingredientFilter.SetDisallowAll();
                    newBill.SetBoundUft(ut);
                    __result = true;
                    return false;
                }
            }
            return true;
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
            //Log.Message("HELLO FROM Patch_WorkGiver_DoBill_ClosestUnfinishedThingForBill");
            if (bill is FinishWhatYouStarted_Bill)
            {
                __result = Utility.ClosestUnfinishedThingForWorkbench(pawn, (Thing)bill.billStack.billGiver);
                return false;
            }
            return true;
        }
    }

    // RimWorld.WorkGiver_DoBill
    // public static Job TryStartNewDoBillJob(Pawn pawn, Bill bill, IBillGiver giver, List<ThingCount> chosenIngThings, out Job haulOffJob, bool dontCreateJobIfHaulOffRequired = true)
    //[HarmonyPatch(typeof(WorkGiver_DoBill))]
    //[HarmonyPatch("TryStartNewDoBillJob")]
    //class Patch_WorkGiver_DoBill_TryStartNewDoBillJob
    //{
    //    static bool Prefix(Pawn pawn, Bill bill, IBillGiver giver, List<ThingCount> chosenIngThings, ref Job haulOffJob, bool dontCreateJobIfHaulOffRequired, Job __result)
    //    {
    //        Log.Message("HELLO FROM Patch_WorkGiver_DoBill_TryStartNewDoBillJob");
    //        if (bill.recipe.defName == "FinishWhatYouStarted_Recipe")
    //        {
    //            __result = null;
    //            UnfinishedThing ut = Utility.ClosestUnfinishedThingForWorkbench(pawn, (Thing)giver);
    //            if (ut != null)
    //            {
    //                Log.Message("FOUND UT IN Patch_WorkGiver_DoBill_TryStartNewDoBillJob");
    //                Bill_ProductionWithUft newBill = new Bill_ProductionWithUft(ut.Recipe, null);
    //                newBill.ingredientFilter.SetDisallowAll();
    //                newBill.SetBoundUft(ut);
    //                giver.BillStack.AddBill(newBill);
    //                while (giver.BillStack.IndexOf(newBill) > 0)
    //                {
    //                    giver.BillStack.Reorder(newBill, -1);
    //                }
    //                // RimWorld.WorkGiver_DoBill
    //                // private static Job FinishUftJob(Pawn pawn, UnfinishedThing uft, Bill_ProductionWithUft bill)
    //                MethodInfo finishUftJob = typeof(WorkGiver_DoBill).GetMethod("FinishUftJob", BindingFlags.NonPublic | BindingFlags.Static);
    //                __result = (Job)finishUftJob.Invoke(null, new object[] { pawn, ut, newBill });
    //            }
    //            return false;
    //        }
    //        return true;
    //    }
    //}
}
