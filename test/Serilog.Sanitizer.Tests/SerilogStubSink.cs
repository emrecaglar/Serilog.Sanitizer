using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Serilog.Sanitizer.Tests
{
    class SerilogStubSink : ILogEventSink
    {
        private readonly List<LogEvent> _events;

        public SerilogStubSink(List<LogEvent> events)
        {
            _events = events;
        }

        public void Emit(LogEvent logEvent)
        {
            _events.Add(logEvent);
        }
    }
}
