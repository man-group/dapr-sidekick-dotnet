namespace Man.Dapr.Sidekick.Process
{
    internal class DaprProcessLogRecord
    {
        public string Msg { get; set; }

        public string Level { get; set; }

        public string Ver { get; set; }

        public string App_id { get; set; }

        public string Instance { get; set; }

        public string Scope { get; set; }

        public string Time { get; set; }

        public string Type { get; set; }
    }
}
