namespace Dapr.Sidekick.Process
{
    public class DaprProcessInfo
    {
        public static DaprProcessInfo Unknown => new DaprProcessInfo("unknown", null, null, DaprProcessStatus.Stopped);

        public DaprProcessInfo(string name, int? id, string version, DaprProcessStatus status)
        {
            Name = name;
            Id = id;
            Version = version;
            Status = status;
        }

        public string Name { get; }

        public int? Id { get; }

        public string Version { get; }

        public DaprProcessStatus Status { get; }

        public bool IsRunning => Status == DaprProcessStatus.Started;

        public string Description =>
            IsRunning ? (!string.IsNullOrEmpty(Version) ? $"Dapr process '{Name}' running, version {Version}" : $"Dapr process '{Name}' running, unverified version") :
            $"Dapr process '{Name}' not available, status is {Status}";

        public override string ToString() => Description;
    }
}
