using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Diagnostics;

namespace FileTakeOwnership
{
    class Program
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, ref LUID lpLuid);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool AdjustTokenPrivileges(IntPtr tokenHandle, bool disableAllPrivileges, ref TOKEN_PRIVILEGES newState, uint bufferLength, IntPtr previousState, IntPtr returnLength);

        [StructLayout(LayoutKind.Sequential)]
        private struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LUID_AND_ATTRIBUTES
        {
            public LUID Luid;
            public uint Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TOKEN_PRIVILEGES
        {
            public uint PrivilegeCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public LUID_AND_ATTRIBUTES[] Privileges;
        }

        static void Main(string[] args)
        {
            Console.WriteLine(@"



▒█▀▀▀█ █░░░█ █▀▀▄ █▀▀ █▀▀█ █▀▀ █░░█ ░▀░ █▀▀█ ▒█▀▀▀█ ▀▀█▀▀ █▀▀ █▀▀█ █░░ █▀▀ █▀▀█ 
▒█░░▒█ █▄█▄█ █░░█ █▀▀ █▄▄▀ ▀▀█ █▀▀█ ▀█▀ █░░█ ░▀▀▀▄▄ ░░█░░ █▀▀ █▄▄█ █░░ █▀▀ █▄▄▀ 
▒█▄▄▄█ ░▀░▀░ ▀░░▀ ▀▀▀ ▀░▀▀ ▀▀▀ ▀░░▀ ▀▀▀ █▀▀▀ ▒█▄▄▄█ ░░▀░░ ▀▀▀ ▀░░▀ ▀▀▀ ▀▀▀ ▀░▀▀

Author: Matan Bahar
");
            Console.WriteLine("Starting TakeOwnership program...");

            if (args.Length == 0)
            {
                Console.WriteLine("Please specify a file name.");
                return;
            }

            string fileName = args[0];
            Console.WriteLine("Attempting to take ownership of file '{0}'...", fileName);

            // Get the current process token
            IntPtr tokenHandle = IntPtr.Zero;
            Process process = Process.GetCurrentProcess(); // Declare and initialize the process variable
            if (!OpenProcessToken(process.Handle, 0x0020 | 0x0008, out tokenHandle))
            {
                Console.WriteLine("Failed to open process token.");
                return;
            }

            // Lookup the LUID for the SeTakeOwnershipPrivilege
            LUID takeOwnershipLuid = new LUID();
            if (!LookupPrivilegeValue(null, "SeTakeOwnershipPrivilege", ref takeOwnershipLuid))
            {
                Console.WriteLine("Failed to lookup privilege value.");
                return;
            }

            // Enable the SeTakeOwnershipPrivilege in the token
            TOKEN_PRIVILEGES newState = new TOKEN_PRIVILEGES();
            newState.PrivilegeCount = 1;
            newState.Privileges = new LUID_AND_ATTRIBUTES[1];
            newState.Privileges[0].Luid = takeOwnershipLuid;
            newState.Privileges[0].Attributes = 0x00000002;

            if (!AdjustTokenPrivileges(tokenHandle, false, ref newState, 0, IntPtr.Zero, IntPtr.Zero))
            {
                Console.WriteLine("Failed to adjust token privileges.");
                return;
            }

            // Take ownership of the file
            while (true)
            {
                try
                {
                    Console.WriteLine("Attempting to set ownership of file '{0}'...", fileName);
                    FileSecurity fileSecurity = new FileSecurity(fileName, AccessControlSections.Owner);
                    SecurityIdentifier securityIdentifier = WindowsIdentity.GetCurrent().User;
                    fileSecurity.SetOwner(securityIdentifier);
                    File.SetAccessControl(fileName, fileSecurity);
                    Console.WriteLine("Ownership of file '{0}' has been taken by user '{1}'.", fileName, securityIdentifier.Value);
                    break; // ownership has been successfully taken, exit the loop
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine("Failed to take ownership of file '{0}' due to insufficient permissions. Retrying...", fileName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to take ownership of file '{0}'. Exception message: {1}", fileName, ex.Message);
                    return;
                }
            }
            Console.WriteLine("FileTakeOwnership program has finished.");
        }
    }
}
