namespace Man.Dapr.Sidekick
{
    public class MockDisposable : DaprDisposable
    {
        public bool? OnDisposingValue { get; set; }

        public bool? IsDisposingValue { get; set; }

        public void InvokeDispose(bool disposing)
        {
            Dispose(disposing);
        }

        public void InvokeEnsureNotDisposed()
        {
            EnsureNotDisposed();
        }

        protected override void OnDisposing(bool disposing)
        {
            IsDisposingValue = IsDisposing;
            OnDisposingValue = disposing;
            base.OnDisposing(disposing);
        }
    }
}
