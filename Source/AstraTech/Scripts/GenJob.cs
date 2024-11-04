using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using Verse.AI;

namespace AstraTech
{
    public static class GenJob
    {
        public static Dictionary<Type, JobDef> JobDefByJobDriverType
        {
            get
            {
                if (jobDefByJobDriverType == null)
                {
                    jobDefByJobDriverType = new Dictionary<Type, JobDef>();

                    foreach (JobDef def in DefDatabase<JobDef>.AllDefsListForReading)
                    {
                        if (def.driverClass.Namespace.StartsWith("AstraTech"))
                        {
                            Log.Message("Bind job: " + def.driverClass.FullName);
                            jobDefByJobDriverType.Add(def.driverClass, def);
                        }
                    }
                }
                return jobDefByJobDriverType;
            }
        }

        private static Dictionary<Type, JobDef> jobDefByJobDriverType;
        private static FieldInfo lastJobDriverMadeField = typeof(Job).GetField("lastJobDriverMade", BindingFlags.Instance | BindingFlags.NonPublic);
        private static FieldInfo cachedDriverField = typeof(Job).GetField("cachedDriver", BindingFlags.Instance | BindingFlags.NonPublic);

        public static bool TryGiveJob<TJobDriver>(Pawn actor, Thing targetA, Thing targetB = null) where TJobDriver : JobDriver
        {
            // Create a job
            Job job = new Job(AstraDefOf.job_astra_haul_and_do);
            job.targetA = targetA;
            job.targetB = targetB;

            // Copy Job.MakeDriver logic
            TJobDriver driver = Activator.CreateInstance<TJobDriver>();
            driver.pawn = actor;
            driver.job = job;
            lastJobDriverMadeField.SetValue(job, driver);

            // Place custom driver into cache (Harmony patch will block Job.MakeDriver and force it to use cachedDriver only)
            cachedDriverField.SetValue(job, driver);

            // Give a job
            return actor.jobs.TryTakeOrderedJob(job);
        }
    }
}
