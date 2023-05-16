using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

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
}
