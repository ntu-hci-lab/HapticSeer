using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace HapticSeerDashboard
{
    public class ConfigSchema
    {
        public class Node
        {
            public string Type { get; set; }
            public string[] Outlets { get; set; }
            public Dictionary<string, string> Options { get; set; }
        }
        public class ExtractorSet : Node {}
        public class RawCapturer : Node {}
        public class EventDetector : Node
        {
            public string[] Inlets { get; set; }
        }
        public List<ExtractorSet> extractorSets = new List<ExtractorSet>();
        public List<RawCapturer> rawCapturers = new List<RawCapturer>();
        public List<EventDetector> eventDetectors = new List<EventDetector>();

    }
}
