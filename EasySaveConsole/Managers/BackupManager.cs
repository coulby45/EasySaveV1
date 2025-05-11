using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EasySaveConsole.config;
using EasySaveConsole.Models;
using EasySaveLogging.Logger;
using System.Text.Json;

namespace EasySaveConsole.Managers
{
    public class BackupManager
    {
        private const int MaxJobs = 5;
        private readonly List<Backup> _jobs;
        private readonly Logger _logger;
        private readonly string _stateFile;

        public BackupManager()
        {
            _jobs = Config.LoadJobs();
            _logger = Logger.GetInstance();
            _stateFile = Config.GetStateFilePath();
        }

        public IReadOnlyList<Backup> Jobs => _jobs;

        public bool AddJob(Backup job)
        {
            if (_jobs.Count >= MaxJobs) return false;
            _jobs.Add(job);
            Config.SaveJobs(_jobs);
            return true;
        }

        public bool RemoveJob(string name)
        {
            var job = _jobs.FirstOrDefault(b => b.Name == name);
            if (job == null) return false;
            _jobs.Remove(job);
            Config.SaveJobs(_jobs);
            return true;
        }

        public bool UpdateJob(string name, Backup updated)
        {
            var idx = _jobs.FindIndex(b => b.Name == name);
            if (idx < 0) return false;
            _jobs[idx] = updated;
            Config.SaveJobs(_jobs);
            return true;
        }

        public Backup GetJob(string name) =>
            _jobs.FirstOrDefault(b => b.Name == name);

        public void ExecuteJobsByIndices(IEnumerable<int> indices)
        {
            foreach (var i in indices)
                if (i >= 1 && i <= _jobs.Count)
                    RunBackup(_jobs[i - 1]);
        }

        private void RunBackup(Backup job)
        {
            var allFiles = Directory
                .EnumerateFiles(job.SourcePath, "*", SearchOption.AllDirectories)
                .ToList();

            long totalBytes = allFiles.Sum(f => new FileInfo(f).Length);
            int totalFiles = allFiles.Count;

            // État initial
            var state = new
            {
                job.Name,
                LastActionTime = DateTime.Now,
                Status = "Active",
                TotalFiles = totalFiles,
                TotalBytes = totalBytes,
                FilesRemaining = totalFiles,
                BytesRemaining = totalBytes,
                CurrentSource = "",
                CurrentTarget = ""
            };
            File.WriteAllText(_stateFile,
                JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true }));

            foreach (var src in allFiles)
            {
                var rel = Path.GetRelativePath(job.SourcePath, src);
                var dst = Path.Combine(job.TargetPath, rel);
                Directory.CreateDirectory(Path.GetDirectoryName(dst));
                var sw = System.Diagnostics.Stopwatch.StartNew();

                try
                {
                    File.Copy(src, dst, true);
                    sw.Stop();
                    _logger.CreateLog(
                        job.Name,
                        sw.Elapsed,
                        new FileInfo(dst).Length,
                        DateTime.Now,
                        src,
                        dst,
                        "INFO"
                    );
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    _logger.CreateLog(
                        job.Name,
                        sw.Elapsed.Negate(),
                        0,
                        DateTime.Now,
                        src,
                        dst,
                        "ERROR"
                    );
                }

                // Mise à jour état
                totalBytes -= new FileInfo(src).Length;
                totalFiles -= 1;
                state = new
                {
                    job.Name,
                    LastActionTime = DateTime.Now,
                    Status = "Active",
                    TotalFiles = totalFiles,
                    TotalBytes = totalBytes,
                    FilesRemaining = totalFiles,
                    BytesRemaining = totalBytes,
                    CurrentSource = src,
                    CurrentTarget = dst
                };
                File.WriteAllText(_stateFile,
                    JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true }));
            }
        }

        public void ShowLogs()
        {
            _logger.DisplayLogs();
        }
    }
}
