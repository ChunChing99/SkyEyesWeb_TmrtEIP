using System;

public class SchedulerTask
{
    public string TaskName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int IntervalInMinutes { get; set; }
    public DateTime? LastRunTime { get; set; }
    public string LastRunResult { get; set; }
    public DateTime? NextRunTime { get; set; }
}

public class SchedulerSettings
{
    public bool Enabled { get; set; }
    public string[] Forced_Execution_Task { get; set; }
    public SchedulerTask[] Tasks { get; set; }
}
