open System
open System.IO
open System.Runtime.InteropServices
open Microsoft.Win32.SafeHandles

let undefined() = System.NotImplementedException() |> raise

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

module PagedFile =
    type IPageNum = interface end
    
    type IPFPageHandle =
        abstract member GetData: unit -> byte[]
        abstract member GetPageNum: unit -> IPageNum
    
    type IPFFileHandle =
        abstract member GetFirstPage: unit -> IPFPageHandle
        abstract member GetLastPage: unit -> IPFPageHandle
        abstract member GetNextPage: current: IPageNum -> IPFPageHandle
        abstract member GetPrevPage: current: IPageNum -> IPFPageHandle
        abstract member GetThisPage: current: IPageNum -> IPFPageHandle
        abstract member AllocatePage: unit -> IPFPageHandle
        abstract member DisposePage: pageNum: IPageNum -> unit
        abstract member MarkDarty: pageNum: IPageNum -> unit
        abstract member UnpinPage: pageNum: IPageNum -> unit
        abstract member ForcePages: pageNum: IPageNum -> unit

    type IPFManager =
        abstract member CreateFile: fileName: string -> unit
        abstract member DestroyFile: fileName: string -> unit
        abstract member OpenFile: fileName: string -> IPFFileHandle
        abstract member CloseFile: fileHandle: IPFFileHandle -> unit
        abstract member AllocateBlock: buffer: byte[] -> unit
        abstract member DisposeBlock: buffer: byte[] -> unit
                                                    
[<EntryPoint>]
let main argv = 
    let safeFileHandle =
        Interop.CreateFile("test.db", FileAccess.Write, FileShare.None, IntPtr.Zero, FileMode.Create, FileAttributes.Normal, IntPtr.Zero)
    safeFileHandle.Close()
    0
