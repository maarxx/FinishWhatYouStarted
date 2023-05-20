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
            FinishWhatYouStarted_Bill_Slave newBill = new FinishWhatYouStarted_Bill_Slave(ut.Recipe);
            newBill.ingredientFilter.SetDisallowAll();
            newBill.SetBoundUft(ut);
            newBill.master = this;
            Utility.SwitchBills(this, newBill);
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
        public override void stopImpersonating()
        {
            Utility.SwitchBills(this, master);
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
