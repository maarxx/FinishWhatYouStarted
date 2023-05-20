using RimWorld;
using Verse;

namespace FinishWhatYouStarted
{
    public abstract class FinishWhatYouStarted_Bill : Bill_ProductionWithUft
    {
        public virtual void stopImpersonating()
        {
            //
        }
        public virtual void startImpersonating(UnfinishedThing ut)
        {
            //
        }
        public FinishWhatYouStarted_Bill(RecipeDef recipe, Precept_ThingStyle precept = null) : base(recipe, precept)
        {
            this.precept = precept;
        }
        public FinishWhatYouStarted_Bill()
        {
            //
        }
    }

    public class FinishWhatYouStarted_Bill_Master : FinishWhatYouStarted_Bill
    {
        public override void startImpersonating(UnfinishedThing ut)
        {
            Log.Message("HELLO FROM FinishWhatYouStarted_Bill_Master.startImpersonating");
            FinishWhatYouStarted_Bill_Slave newBill = new FinishWhatYouStarted_Bill_Slave(ut);
            newBill.ingredientFilter.SetDisallowAll();
            newBill.SetBoundUft(ut);
            Utility.SwitchBills(this, newBill);
            newBill.master = this;
        }
        public FinishWhatYouStarted_Bill_Master(RecipeDef recipe, Precept_ThingStyle precept = null) : base(recipe, precept)
        {
            //
        }
        public FinishWhatYouStarted_Bill_Master() : base()
        {
            //
        }
    }

    public class FinishWhatYouStarted_Bill_Slave : FinishWhatYouStarted_Bill
    {
        public FinishWhatYouStarted_Bill_Master master;
        public int bornTick;
        public override void stopImpersonating()
        {
            Log.Message("HELLO FROM FinishWhatYouStarted_Bill_Slave.stopImpersonating");
            Utility.SwitchBills(this, master);
        }
        public FinishWhatYouStarted_Bill_Slave(UnfinishedThing ut) : base(ut.Recipe, ut.StyleSourcePrecept)
        {
            this.bornTick = Find.TickManager.TicksGame;
        }
        public FinishWhatYouStarted_Bill_Slave(RecipeDef recipe, Precept_ThingStyle precept = null) : base(recipe, precept)
        {
            //
        }
        public FinishWhatYouStarted_Bill_Slave() : base()
        {
            //
        }
    }
}
