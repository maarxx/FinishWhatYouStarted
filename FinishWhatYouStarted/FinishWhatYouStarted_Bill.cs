﻿using RimWorld;
using Verse;

namespace FinishWhatYouStarted
{
    public class FinishWhatYouStarted_Bill : Bill_ProductionWithUft
    {
        public FinishWhatYouStarted_Bill master;
        public FinishWhatYouStarted_Bill()
        {
            // Intentionally left blank.
        }

        public FinishWhatYouStarted_Bill(RecipeDef recipe, Precept_ThingStyle precept = null) : base(recipe, precept)
        {
            // Intentionally left blank.
        }
    }
}
