/*
 * This class is based on a Stack Overflow article: https://stackoverflow.com/a/46006415.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;

namespace Man.Dapr.Sidekick.Native
{
    internal static class NativeProcess
    {
        public const uint PROCESS_BASIC_INFORMATION = 0;

        [Flags]
        public enum OpenProcessDesiredAccessFlags : uint
        {
            PROCESS_VM_READ = 0x0010,
            PROCESS_QUERY_INFORMATION = 0x0400,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ProcessBasicInformation
        {
            public IntPtr Reserved1;
            public IntPtr PebBaseAddress;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public IntPtr[] Reserved2;
            public IntPtr UniqueProcessId;
            public IntPtr Reserved3;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct UnicodeString
        {
            public ushort Length;
            public ushort MaximumLength;
            public IntPtr Buffer;
        }

        // This is not the real struct!
        // I faked it to get ProcessParameters address.
        // Actual struct definition:
        // https://docs.microsoft.com/en-us/windows/win32/api/winternl/ns-winternl-peb
        [StructLayout(LayoutKind.Sequential)]
        public struct PEB
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public IntPtr[] Reserved;
            public IntPtr ProcessParameters;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RtlUserProcessParameters
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] Reserved1;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public IntPtr[] Reserved2;
            public UnicodeString ImagePathName;
            public UnicodeString CommandLine;
        }

        [DllImport("ntdll.dll")]
        public static extern uint NtQueryInformationProcess(IntPtr processHandle, uint processInformationClass, IntPtr processInformation, uint processInformationLength, out uint returnLength);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(
            OpenProcessDesiredAccessFlags dwDesiredAccess,
            [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
            uint dwProcessId);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, uint nSize, out uint lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("shell32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "CommandLineToArgvW")]
        public static extern IntPtr CommandLineToArgv(string lpCmdLine, out int pNumArgs);

        public static int GetCommandLine(System.Diagnostics.Process process, out string commandLine)
        {
            if (!DaprConstants.IsWindows)
            {
                // Return an empty command-line on non-Windows platforms
                commandLine = string.Empty;
                return 0;
            }

            var rc = 0;
            commandLine = null;
            var hProcess = OpenProcess(
                OpenProcessDesiredAccessFlags.PROCESS_QUERY_INFORMATION | OpenProcessDesiredAccessFlags.PROCESS_VM_READ,
                false,
                (uint)process.Id);
            if (hProcess != IntPtr.Zero)
            {
                try
                {
                    var sizePBI = Marshal.SizeOf(typeof(ProcessBasicInformation));
                    var memPBI = Marshal.AllocHGlobal(sizePBI);
                    try
                    {
                        var ret = NtQueryInformationProcess(hProcess, PROCESS_BASIC_INFORMATION, memPBI, (uint)sizePBI, out var len);
                        if (ret == 0)
                        {
                            var pbiInfo = (ProcessBasicInformation)Marshal.PtrToStructure(memPBI, typeof(ProcessBasicInformation));
                            if (pbiInfo.PebBaseAddress != IntPtr.Zero)
                            {
                                if (ReadStructFromProcessMemory<PEB>(hProcess, pbiInfo.PebBaseAddress, out var pebInfo))
                                {
                                    if (ReadStructFromProcessMemory<RtlUserProcessParameters>(
                                        hProcess, pebInfo.ProcessParameters, out var ruppInfo))
                                    {
                                        var clLen = ruppInfo.CommandLine.MaximumLength;
                                        var memCL = Marshal.AllocHGlobal(clLen);
                                        try
                                        {
                                            if (ReadProcessMemory(hProcess, ruppInfo.CommandLine.Buffer, memCL, clLen, out len))
                                            {
                                                commandLine = Marshal.PtrToStringUni(memCL);
                                                rc = 0;
                                            }
                                            else
                                            {
                                                // couldn't read command line buffer
                                                rc = -6;
                                            }
                                        }
                                        finally
                                        {
                                            Marshal.FreeHGlobal(memCL);
                                        }
                                    }
                                    else
                                    {
                                        // couldn't read ProcessParameters
                                        rc = -5;
                                    }
                                }
                                else
                                {
                                    // couldn't read PEB information
                                    rc = -4;
                                }
                            }
                            else
                            {
                                // PebBaseAddress is null
                                rc = -3;
                            }
                        }
                        else
                        {
                            // NtQueryInformationProcess failed
                            rc = -2;
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(memPBI);
                    }
                }
                finally
                {
                    CloseHandle(hProcess);
                }
            }
            else
            {
                // couldn't open process for VM read
                rc = -1;
            }

            return rc;
        }

        public static IEnumerable<string> CommandLineToArgs(string commandLine)
        {
            if (string.IsNullOrEmpty(commandLine) || !DaprConstants.IsWindows)
            {
                return new string[0];
            }

            var argv = CommandLineToArgv(commandLine, out var argc);
            if (argv == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            try
            {
                var args = new string[argc];
                for (var i = 0; i < args.Length; ++i)
                {
                    var p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
                    args[i] = Marshal.PtrToStringUni(p);
                }

                return args.ToList().AsReadOnly();
            }
            finally
            {
                Marshal.FreeHGlobal(argv);
            }
        }

        private static bool ReadStructFromProcessMemory<TStruct>(IntPtr hProcess, IntPtr lpBaseAddress, out TStruct val)
        {
            val = default;
            var structSize = Marshal.SizeOf(typeof(TStruct));
            var mem = Marshal.AllocHGlobal(structSize);
            try
            {
                if (ReadProcessMemory(
                    hProcess, lpBaseAddress, mem, (uint)structSize, out var len) &&
                    (len == structSize))
                {
                    val = (TStruct)Marshal.PtrToStructure(mem, typeof(TStruct));
                    return true;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(mem);
            }

            return false;
        }
    }
}
