// RoundEvents.cs
using System;
using System.Collections.Generic;
using DemoFile;
using DemoFile.Sdk;
using Serilog;

namespace cooper_ai.Events
{
    public class RoundEvents
    {
        private readonly DemoParser _demoParser;
        private readonly List<object> _events;

        public RoundEvents(DemoParser demoParser, List<object> events)
        {
            _demoParser = demoParser;
            _events = events;
            AttachEventHandlers();
        }

        private void AttachEventHandlers()
        {
            _demoParser.Source1GameEvents.RoundStart += OnRoundStart;
            _demoParser.Source1GameEvents.RoundEnd += OnRoundEnd;
        }

        private void OnRoundStart(Source1RoundStartEvent e)
        {
            var roundStartEvent = new
            {
                EventType = "RoundStart",
                Timestamp = _demoParser.CurrentGameTime.Value,
                Timelimit = e.Timelimit,
                Fraglimit = e.Fraglimit,
                Objective = e.Objective
            };
            _events.Add(roundStartEvent);
            Log.Information("RoundStart event added: {@RoundStartEvent}", roundStartEvent);
        }

        private void OnRoundEnd(Source1RoundEndEvent e)
        {
            var roundEndEvent = new
            {
                EventType = "RoundEnd",
                Timestamp = _demoParser.CurrentGameTime.Value,
                Winner = e.Winner,
                Reason = e.Reason,
                Message = e.Message,
                Legacy = e.Legacy,
                PlayerCount = e.PlayerCount,
                Nomusic = e.Nomusic
            };
            _events.Add(roundEndEvent);
            Log.Information("RoundEnd event added: {@RoundEndEvent}", roundEndEvent);
        }
    }
}