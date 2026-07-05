using AiTaskApi.Data;
using AiTaskApi.Services;
using AiTaskApi.Shared.Models.Agent;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace AiTaskApi.Worker.Services
{
    public class JobProcessorService
    {
        private const int RetryDelaySeconds = 10;
        private readonly AppDbContext _db;
        private readonly AgentService _agent;

        public JobProcessorService(AppDbContext db, AgentService agent) 
        {
            _db = db;
            _agent = agent;
        }

        // =========================
        // JOB EXECUTION
        // =========================
        public async Task RunJob(int jobId, CancellationToken token)
        {
            var job = await _db.AgentJobs.FindAsync(new object[] { jobId }, token);

            if (job == null)
                return;


            job.Status = "Running";
            job.AttemptCount += 1;
            job.LastError = null;
            job.NextAttemptAt = null;
            await _db.SaveChangesAsync(token);

            try
            {
                var result = await _agent.RunFullPipeline(job.Prompt);

                job.Status = "Done";
                job.Result = JsonSerializer.Serialize(result);
                job.CompletedAt = DateTime.UtcNow;
                job.NextAttemptAt = null;
            }
            catch (Exception ex)
            {
                job.LastError = ex.Message;

                if (job.AttemptCount < job.MaxAttempts)
                {
                    job.Status = "Pending";
                    job.Result = $"Attempt {job.AttemptCount}/{job.MaxAttempts} failed. Retry scheduled.";
                    job.NextAttemptAt = DateTime.UtcNow.AddSeconds(RetryDelaySeconds * job.AttemptCount);
                }
                else
                {
                    job.Status = "Failed";
                    job.Result = ex.Message;
                    job.CompletedAt = DateTime.UtcNow;
                    job.NextAttemptAt = null;
                }
            }

            await _db.SaveChangesAsync(token);
        }

    }
}
