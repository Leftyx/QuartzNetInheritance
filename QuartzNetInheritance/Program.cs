namespace QuartzNetInheritance
{
    using System;
    using System.IO;
    using System.Collections.Generic;

    using Quartz;
    using Quartz.Impl;

    class Program
    {
        public static StdSchedulerFactory SchedulerFactory;
        public static IScheduler Scheduler;
        
        static void Main(string[] args)
        {
            SchedulerFactory = new StdSchedulerFactory();
            Scheduler = SchedulerFactory.GetScheduler();

            Scheduler.Start();

            Console.WriteLine("Quartz.Net Started ...");

            TestInheritance(Scheduler);

            Console.WriteLine("Finished!");

            Console.ReadLine();
        }


        private static void TestInheritance(IScheduler Scheduler)
        {
            IList<BaseJob> jobs = new List<BaseJob>();

            jobs.Add(new BaseJob() { Name = "Job1", Group = "MYGROUP", Type = typeof(InheritedJob1) });
            jobs.Add(new BaseJob() { Name = "Job2", Group = "MYGROUP", Type = typeof(InheritedJob2) });
            jobs.Add(new BaseJob() { Name = "Job3", Group = "MYGROUP", Type = typeof(InheritedJob3) });

            foreach (var job in jobs)
            {
                IJobDetail quartzJob = JobBuilder.Create(job.Type)
                    .WithIdentity(job.Name, job.Group)
                    .Build();

                ITrigger quartzTrigger = TriggerBuilder.Create()
                            .WithIdentity("trg_" + job.Name, job.Group)
                            .StartNow()
                            .WithSimpleSchedule(x => x.WithIntervalInSeconds(30).RepeatForever())
                            .Build();

                Scheduler.ScheduleJob(quartzJob, quartzTrigger);
            }

        }

        private static void GetNextXFireTimes(ITrigger trigger, int triggersCount)
        {
            Console.WriteLine(string.Format("List of next {0} schedules: ", triggersCount.ToString()));

            var dt = trigger.GetNextFireTimeUtc();

            using (var file = new System.IO.StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory ,"QuartzNet.Log")))
            {
                for (int i = 0; i < triggersCount; i++)
                {
                    if (dt == null)
                        break;

                    file.WriteLine(dt.Value.ToLocalTime().ToString());

                    dt = trigger.GetFireTimeAfter(dt);
                }
            }
        }
    }
}
