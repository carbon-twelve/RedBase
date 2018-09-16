namespace Database

open System
open System.Runtime.InteropServices
open Microsoft.Win32.SafeHandles
open System.Threading
open System.IO

module Util =
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

    [<DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)>]    
    extern bool DeleteFile(
        [<MarshalAs(UnmanagedType.LPTStr)>] string fileName
    )

    [<DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)>]    
    extern bool WriteFile(
        SafeFileHandle safeFileHandle,
        byte[] buffer,
        uint32 numberOfByetsToWrite,
        uint32& numberOfByetsWritten,
        NativeOverlapped& overlapped
    )

/// <seealso cref="https://web.stanford.edu/class/cs346/2015/redbase-pf.html"/>
module PagedFile =

    let pfPageSize = 4092

    type IPageNum = interface end
    
    type IPFPageHandle =
        abstract member GetData: unit -> byte[]
        abstract member GetPageNum: unit -> IPageNum
    
    type IPFFileHandle =
        inherit IDisposable
        abstract member GetFirstPage: unit -> IPFPageHandle
        abstract member GetLastPage: unit -> IPFPageHandle
        abstract member GetNextPage: current: IPageNum -> IPFPageHandle
        abstract member GetPrevPage: current: IPageNum -> IPFPageHandle
        abstract member GetThisPage: current: IPageNum -> IPFPageHandle
        abstract member AllocatePage: unit -> Async<IPFPageHandle>
        abstract member DisposePage: pageNum: IPageNum -> unit
        abstract member MarkDarty: pageNum: IPageNum -> unit
        abstract member UnpinPage: pageNum: IPageNum -> unit
        abstract member ForcePages: pageNum: IPageNum -> unit

    type IPFManager =
        abstract member CreateFile: fileName: string -> unit
        abstract member DestroyFile: fileName: string -> unit
        abstract member OpenFile: fileName: string -> IPFFileHandle
        abstract member AllocateBlock: buffer: byte[] -> unit
        abstract member DisposeBlock: buffer: byte[] -> unit
    
    type PFFileHandle(safeFileHandle: SafeFileHandle) =
        interface IPFFileHandle with
            member __.AllocatePage(): Async<IPFPageHandle> =
                let mutable numberOfBytesWritten = 0u
                let mutable nativeOverlapped = NativeOverlapped()
                let success = Interop.WriteFile(safeFileHandle, Array.zeroCreate pfPageSize, (uint32) pfPageSize, &numberOfBytesWritten, &nativeOverlapped)
                if (not success || numberOfBytesWritten <> (uint32) pfPageSize) then
                    raise (IOException())
                
                raise (System.NotImplementedException())
            member this.Dispose(): unit = safeFileHandle.Dispose() // FIXME: Flush all dirty pages in the buffer pool

            member this.DisposePage(pageNum: IPageNum): unit = 
                raise (System.NotImplementedException())
            member this.ForcePages(pageNum: IPageNum): unit = 
                raise (System.NotImplementedException())
            member this.GetFirstPage(): IPFPageHandle = Util.undefined()
            member this.GetLastPage(): IPFPageHandle = 
                raise (System.NotImplementedException())
            member this.GetNextPage(current: IPageNum): IPFPageHandle = 
                raise (System.NotImplementedException())
            member this.GetPrevPage(current: IPageNum): IPFPageHandle = 
                raise (System.NotImplementedException())
            member this.GetThisPage(current: IPageNum): IPFPageHandle = 
                raise (System.NotImplementedException())
            member this.MarkDarty(pageNum: IPageNum): unit = 
                raise (System.NotImplementedException())
            member this.UnpinPage(pageNum: IPageNum): unit = 
                raise (System.NotImplementedException())
            
    type PFManager() =
        
        interface IPFManager with

            member __.AllocateBlock(_: byte []): unit = Util.undefined()
                
            member __.CreateFile(fileName: string): unit =
                use fileStream = FileStream(fileName, FileMode.CreateNew)
                fileStream.Dispose()

            member __.DestroyFile(fileName: string): unit = 
                let success = Interop.DeleteFile(fileName)
                if (not success) then
                    raise (System.IO.IOException())

            member __.DisposeBlock(_: byte []): unit = Util.undefined()

            member __.OpenFile(fileName: string): IPFFileHandle =
                Interop.CreateFile(fileName, FileAccess.ReadWrite, FileShare.None, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero)
                |> (fun h -> new PFFileHandle(h) :> IPFFileHandle)

module Main =
    open PagedFile

    [<EntryPoint>]
    let main argv =
        let handle = Interop.CreateFile("test.db", FileAccess.ReadWrite, FileShare.None, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero)   
        let mutable n = 0u
        let mutable overlapped = NativeOverlapped()
        let success = Interop.WriteFile(handle, [| 1uy |], 1u, &n, &overlapped)
        printf "%A,%A,%A" n overlapped success
        0
