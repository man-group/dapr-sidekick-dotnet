namespace Man.Dapr.Sidekick.Process
{
    public class DaprProcessInfo
    {
        public static DaprProcessInfo Unknown => new DaprProcessInfo("unknown", null, null, DaprProcessStatus.Stopped, false);

        public DaprProcessInfo(string name, int? id, string version, DaprProcessStatus status)
            : this(name, id, version, status, false)
        {
        }

        public DaprProcessInfo(string name, int? id, string version, DaprProcessStatus status, bool isAttached)
        {
            Name = name;
            Id = id;
            Version = version;
            Status = status;
            IsAttached = isAttached;
        }

        public string Name { get; }

        public int? Id { get; }

        public string Version { get; }

        public DaprProcessStatus Status { get; }

        public bool IsRunning => Status == DaprProcessStatus.Started;

        public bool IsAttached { get; }

        public string Description =>
            IsRunning ? (!string.IsNullOrEmpty(Version) ? $"Dapr process '{Name}' running, version {Version}" : $"Dapr process '{Name}' running, unverified version") :
            IsAttached ? (!string.IsNullOrEmpty(Version) ? $"Dapr process '{Name}' attached, version {Version}" : $"Dapr process '{Name}' attached, unverified version") :
            $"Dapr process '{Name}' not available, status is {Status}";

        public override string ToString() => Description;
    }
}
