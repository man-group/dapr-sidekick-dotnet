// This code based on article at : https://www.codeproject.com/Articles/878605/Getting-All-Special-Folders-in-NET
// List of folder GUIDs available at : https://gitlab.com/Syroot/KnownFolders/-/blob/master/src/Syroot.KnownFolders/KnownFolderType.cs
using System;
using System.Runtime.InteropServices;

namespace Dapr.Sidekick.Native
{
    internal class NativeMethods
    {
        /// <summary>
        /// Retrieves the full path of a known folder identified by the folder's KnownFolderID.
        /// </summary>
        /// <param name="rfid">A KnownFolderID that identifies the folder.</param>
        /// <param name="dwFlags">Flags that specify special retrieval options. This value can be
        ///     0; otherwise, one or more of the KnownFolderFlag values.</param>
        /// <param name="hToken">An access token that represents a particular user. If this
        ///     parameter is NULL, which is the most common usage, the function requests the known
        ///     folder for the current user. Assigning a value of -1 indicates the Default User.
        ///     The default user profile is duplicated when any new user account is created.
        ///     Note that access to the Default User folders requires administrator privileges.
        ///     </param>
        /// <param name="ppszPath">When this method returns, contains the address of a string that
        ///     specifies the path of the known folder. The returned path does not include a
        ///     trailing backslash.</param>
        /// <returns>Returns S_OK if successful, or an error value otherwise.</returns>
        [DllImport("Shell32.dll")]
        private static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr ppszPath);

        [Flags]
#pragma warning disable RCS1135 // Declare enum member with zero value (when enum has FlagsAttribute).
        private enum KnownFolderFlags : uint
#pragma warning restore RCS1135 // Declare enum member with zero value (when enum has FlagsAttribute).
        {
            SimpleIDList = 0x00000100,
            NotParentRelative = 0x00000200,
            DefaultPath = 0x00000400,
            Init = 0x00000800,
            NoAlias = 0x00001000,
            DontUnexpand = 0x00002000,
            DontVerify = 0x00004000,
            Create = 0x00008000,
            NoAppcontainerRedirection = 0x00010000,
            AliasOnly = 0x80000000
        }

        public static string GetUserProfileFolder() => GetPath(new Guid("9274BD8D-CFD1-41C3-B35E-B13F55A758F4"));


        private static string GetPath(Guid knownFolderGuid, KnownFolderFlags flags = KnownFolderFlags.DontVerify | KnownFolderFlags.NoAlias, bool defaultUser = true)
        {
            var result = SHGetKnownFolderPath(knownFolderGuid, (uint)flags, new IntPtr(defaultUser ? -1 : 0), out var outPath);
            if (result >= 0)
            {
                var path = Marshal.PtrToStringUni(outPath);
                Marshal.FreeCoTaskMem(outPath);
                return path;
            }
            else
            {
                throw new ExternalException("Unable to retrieve the known folder path. It may not be available on this system.", result);
            }
        }
    }
}
