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
                __result = new FinishWhatYouStarted_BillMaster(recipe, precept);
                return false;
            }
            return true;
        }
    }

    // RimWorld.WorkGiver_DoBill
    // private Job StartOrResumeBillJob(Pawn pawn, IBillGiver giver)
    [HarmonyPatch(typeof(WorkGiver_DoBill))]
    [HarmonyPatch("StartOrResumeBillJob")]
    class Patch_WorkGiver_DoBill_StartOrResumeBillJob
    {
        static bool Prefix(WorkGiver_DoBill __instance, Pawn pawn, IBillGiver giver)
        {
            Log.Message("HELLO FROM Patch_WorkGiver_DoBill_StartOrResumeBillJob");
            FinishWhatYouStarted_BillSlave slave = null;
            bool doIt = false;
            foreach (Bill b in giver.BillStack)
            {
                if (b is FinishWhatYouStarted_BillSlave)
                {
                    slave = (FinishWhatYouStarted_BillSlave)b;
                    Pawn reserver = pawn.Map.reservationManager.FirstRespectedReserver(slave.BoundUft, pawn);
                    if (reserver != pawn)
                    {
                        //doIt = true;
                    }
                }
            }
            if (doIt)
            {
                FinishWhatYouStarted_BillMaster newMaster = new FinishWhatYouStarted_BillMaster(DefDatabase<RecipeDef>.GetNamed("FinishWhatYouStarted_Recipe"));
                Utility.SwitchBills(slave, newMaster);
            }
            return true;
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
            Log.Message("HELLO FROM Patch_WorkGiver_DoBill_TryFindBestBillIngredients");
            if (bill is FinishWhatYouStarted_BillMaster)
            {
                UnfinishedThing ut = Utility.ClosestUnfinishedThingForWorkbench(pawn, billGiver);
                if (ut != null)
                {
                    Log.Message("FOUND UT IN Patch_WorkGiver_DoBill_TryFindBestBillIngredients");
                    chosen.Add(new ThingCount(ut, 1));
                    __result = true;
                    return false;
                }
            }
            return true;
        }
    }

    // RimWorld.WorkGiver_DoBill
    // public static Job TryStartNewDoBillJob(Pawn pawn, Bill bill, IBillGiver giver, List<ThingCount> chosenIngThings, out Job haulOffJob, bool dontCreateJobIfHaulOffRequired = true)
    [HarmonyPatch(typeof(WorkGiver_DoBill))]
    [HarmonyPatch("TryStartNewDoBillJob")]
    class Patch_WorkGiver_DoBill_TryStartNewDoBillJob
    {
        static bool Prefix(Pawn pawn, Bill bill, IBillGiver giver, List<ThingCount> chosenIngThings, ref Job haulOffJob, bool dontCreateJobIfHaulOffRequired, Job __result)
        {
            Log.Message("HELLO FROM Patch_WorkGiver_DoBill_TryStartNewDoBillJob");
            if (bill is FinishWhatYouStarted_BillMaster)
            {
                __result = null;
                UnfinishedThing ut = Utility.ClosestUnfinishedThingForWorkbench(pawn, (Thing)giver);
                if (ut != null)
                {
                    Log.Message("FOUND UT IN Patch_WorkGiver_DoBill_TryStartNewDoBillJob");
                    Bill_ProductionWithUft newBill = new FinishWhatYouStarted_BillSlave(ut.Recipe, null);
                    newBill.ingredientFilter.SetDisallowAll();
                    newBill.SetBoundUft(ut);
                    Utility.SwitchBills(bill, newBill);
                    // RimWorld.WorkGiver_DoBill
                    // private static Job FinishUftJob(Pawn pawn, UnfinishedThing uft, Bill_ProductionWithUft bill)
                    MethodInfo finishUftJob = typeof(WorkGiver_DoBill).GetMethod("FinishUftJob", BindingFlags.NonPublic | BindingFlags.Static);
                    __result = (Job)finishUftJob.Invoke(null, new object[] { pawn, ut, newBill });
                }
                return false;
            }
            return true;
        }
    }
}
