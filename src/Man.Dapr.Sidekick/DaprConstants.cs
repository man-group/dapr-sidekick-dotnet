using System;

namespace Man.Dapr.Sidekick
{
    public class DaprConstants
    {
        public const string ContentTypeApplicationJson = "application/json";
        public const string ContentTypeApplicationGrpc = "application/grpc";
        public const string ContentTypeTextPlain = "text/plain";
        public const string DefaultNamespace = "default";
        public const string LocalhostAddress = "127.0.0.1";

        // File System
        public const string DaprBinDirectory = "bin";
        public const string DaprCertsDirectory = "certs";
        public const string DaprComponentsDirectory = "components";
        public const string DaprConfigFilename = "config.yaml";
        public const string ExeExtension = ".exe";
        public const string TrustAnchorsCertificateFilename = "ca.crt";
        public const string IssuerCertificateFilename = "issuer.crt";
        public const string IssuerKeyFilename = "issuer.key";

        // Environment Variables
        public const string AppApiTokenEnvironmentVariable = "APP_API_TOKEN";
        public const string DaprApiTokenEnvironmentVariable = "DAPR_API_TOKEN";
        public const string DaprCertChainEnvironmentVariable = "DAPR_CERT_CHAIN";
        public const string DaprCertKeyEnvironmentVariable = "DAPR_CERT_KEY";
        public const string DaprGrpcPortEnvironmentVariable = "DAPR_GRPC_PORT";
        public const string DaprHttpPortEnvironmentVariable = "DAPR_HTTP_PORT";
        public const string DaprPlacementHealthPortEnvironmentVariable = "DAPR_PLACEMENT_HEALTH_PORT";
        public const string DaprPlacementMetricsPortEnvironmentVariable = "DAPR_PLACEMENT_METRICS_PORT";
        public const string DaprPlacementPortEnvironmentVariable = "DAPR_PLACEMENT_PORT";
        public const string DaprProfilePortEnvironmentVariable = "DAPR_PROFILE_PORT";
        public const string DaprSentryMetricsPortEnvironmentVariable = "DAPR_SENTRY_METRICS_PORT";
        public const string DaprTrustAnchorsEnvironmentVariable = "DAPR_TRUST_ANCHORS";
        public const string NamespaceEnvironmentVariable = "NAMESPACE";

        // HTTP Headers
        public const string DaprApiTokenHeader = "dapr-api-token";

        // Processes
        public const string DaprSidecarProcessName = "daprd";
        public const string DaprSentryProcessName = "sentry";
        public const string DaprPlacementProcessName = "placement";

        public static bool IsWindows => Environment.OSVersion.Platform == PlatformID.Win32NT;

        public static bool IsMacOs => Environment.OSVersion.Platform == PlatformID.MacOSX;
    }
}
