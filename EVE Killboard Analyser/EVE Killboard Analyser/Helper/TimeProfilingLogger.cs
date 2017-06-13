using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;

namespace EVE_Killboard_Analyser.Helper
{
    public class TimeProfilingLogger : IDisposable
    {
        private readonly TimeRange _range;
        private readonly DateTime _start;
        private readonly ILog _logger;

        public enum TimeRange
        {
            Seconds = 0,
            Minutes
        }

        public TimeProfilingLogger(string identifier, TimeRange range = TimeRange.Seconds)
        {
            _range = range;
            _logger = LogManager.GetLogger(identifier);
            _start = DateTime.Now;
        }

        public void Dispose()
        {
            var diff = (DateTime.Now - _start);
            var message = _range == TimeRange.Seconds ? string.Format("Timing: {0}s", diff.TotalSeconds) : string.Format("Timing: {0}m", diff.TotalMinutes);
            _logger.Debug(message);
        }
    }
}