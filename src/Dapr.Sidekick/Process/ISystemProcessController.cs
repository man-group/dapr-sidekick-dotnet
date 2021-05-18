namespace Dapr.Sidekick.Process
{
    public interface ISystemProcessController
    {
        /// <summary>
        /// Immediately stops the system process.
        /// </summary>
        void Kill();

        /// <summary>
        /// Starts the system process.
        /// </summary>
        /// <returns><c>true</c> if the associated process was started; otherwise, <c>false</c>.</returns>
        bool Start();

        /// <summary>
        /// Instructs the System Process to wait indefinitely for the associated process to exit.
        /// </summary>
        void WaitForExit();

        /// <summary>
        /// Instructs the System Process to wait the specified number of milliseconds for the associated process to exit.
        /// </summary>
        /// <param name="milliseconds"> The amount of time, in milliseconds, to wait for the associated process to exit.</param>
        /// <returns><c>true</c> if the associated process has exited; otherwise, <c>false</c>.</returns>
        bool WaitForExit(int milliseconds);
    }
}
