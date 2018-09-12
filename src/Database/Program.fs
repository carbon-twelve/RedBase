open System
open System.IO
open System.Runtime.InteropServices
open Microsoft.Win32.SafeHandles

module Interop =
    [<DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)>]
    extern SafeFileHandle CreateFile(
        [<MarshalAs(UnmanagedType.LPTStr)>] string filename,
        [<MarshalAs(UnmanagedType.U4)>] FileAccess access,
        [<MarshalAs(UnmanagedType.U4)>] FileShare share,
        IntPtr securityAttributes,
        [<MarshalAs(UnmanagedType.U4)>] FileMode creationDisposition,
        [<MarshalAs(UnmanagedType.U4)>] FileAttributes flagsAndAttributes,
        IntPtr templateFile
    )

[<EntryPoint>]
let main argv = 
    let safeFileHandle =
        Interop.CreateFile("test.db", FileAccess.Write, FileShare.None, IntPtr.Zero, FileMode.Create, FileAttributes.Normal, IntPtr.Zero)
    safeFileHandle.Close()
    0
