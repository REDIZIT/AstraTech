using RimWorld;
using Verse;

namespace AstraTech
{
    public static class CopyUtils
    {
        public static Thing Copy(Thing original)
        {
            if (original.def.category == ThingCategory.Item)
            {
                return CopyItem(original);
            }
            else if (original is Pawn originalPawn)
            {
                return CopyPawn(originalPawn);
            }

            throw new System.Exception("Failed to copy unknown thing - " + original.ToStringSafe());
        }

        private static Thing CopyItem(Thing original)
        {
            Thing clonedThing = ThingMaker.MakeThing(original.def, original.Stuff);
            clonedThing.stackCount = original.stackCount;

            if (original.HasComp<ThingComp_AstraBlueprint>())
            {
                var s = original.TryGetComp<ThingComp_AstraBlueprint>();
                var t = clonedThing.TryGetComp<ThingComp_AstraBlueprint>();
                t.prefab = s.prefab;
                t.prefabStuff = s.prefabStuff;
            }

            return clonedThing;
        }
        private static Pawn CopyPawn(Pawn original)
        {
            Pawn clonedPawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                    original.kindDef,
                    original.Faction,
                    forceGenerateNewPawn: false
                ));

            clonedPawn.Name = original.Name;

            clonedPawn.gender = original.gender;
            CopyAge(original, clonedPawn);


            clonedPawn.health = new Pawn_HealthTracker(clonedPawn);
            clonedPawn.health.Reset();
            foreach (Hediff hediff in original.health.hediffSet.hediffs)
            {
                clonedPawn.health.AddHediff(CloneHediff(hediff));
            }

            foreach (SkillRecord skill in original.skills.skills)
            {
                SkillRecord clonedSkill = clonedPawn.skills.GetSkill(skill.def);
                clonedSkill.Level = skill.Level;
                clonedSkill.passion = skill.passion;
            }


            CopyStory(original, clonedPawn);
            CopyRelations(original, clonedPawn);


            clonedPawn.story.traits.allTraits.Clear();
            foreach (Trait trait in original.story.traits.allTraits)
            {
                clonedPawn.story.traits.GainTrait(new Trait(trait.def, trait.Degree, false));
            }


            clonedPawn.SetStyleDef(original.StyleDef);
            clonedPawn.style.beardDef = original.style.beardDef;
            clonedPawn.style.BodyTattoo = original.style.BodyTattoo;
            clonedPawn.style.FaceTattoo = original.style.FaceTattoo;

            clonedPawn.inventory.DestroyAll();
            clonedPawn.apparel.DestroyAll();
            clonedPawn.equipment.DestroyAllEquipment();


            return clonedPawn;
        }

        private static void CopyStory(Pawn source, Pawn destination)
        {
            var s = source.story;

            destination.story = new Pawn_StoryTracker(destination)
            {
                Childhood = s.Childhood,
                Adulthood = s.Adulthood,
                HairColor = s.HairColor,
                skinColorOverride = s.skinColorOverride,
                headType = s.headType,
                bodyType = s.bodyType,
                hairDef = s.hairDef,
                traits = new TraitSet(destination),
                title = s.title,
                birthLastName = s.birthLastName,
                favoriteColor = s.favoriteColor,
                furDef = s.furDef,
                SkinColorBase = s.SkinColorBase,
            };
        }
        private static void CopyRelations(Pawn source, Pawn destination)
        {
            var s = source.relations;
            var d = new Pawn_RelationsTracker(destination);
            destination.relations = d;

            d.ClearAllRelations();
            foreach (DirectPawnRelation r in s.DirectRelations)
            {
                d.AddDirectRelation(r.def, r.otherPawn);
            }
        }
        private static void CopyAge(Pawn source, Pawn destination)
        {
            destination.ageTracker = source.ageTracker;

            //var s = source.ageTracker;
            //destination.ageTracker = new Pawn_AgeTracker(destination)
            //{
            //    //vatGrowTicks = s.vatGrowTicks,
            //    //growthPoints = s.growthPoints,
            //    //canGainGrowthPoints = s.canGainGrowthPoints,
            //    AgeChronologicalTicks = s.AgeChronologicalTicks,
            //    AgeBiologicalTicks = s.AgeBiologicalTicks,
            //};
        }
        private static Hediff CloneHediff(Hediff original)
        {
            Hediff clonedHediff = HediffMaker.MakeHediff(original.def, original.pawn);

            clonedHediff.Severity = original.Severity;
            clonedHediff.ageTicks = original.ageTicks;
            clonedHediff.Part = original.Part;
            clonedHediff.CopyFrom(original);


            return clonedHediff;
        }

    }
}
