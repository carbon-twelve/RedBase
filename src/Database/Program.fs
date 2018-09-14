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

    [<DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)>]    
    extern bool DeleteFile(
        [<MarshalAs(UnmanagedType.LPTStr)>] string fileName
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
        abstract member AllocatePage: unit -> IPFPageHandle
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
            member this.AllocatePage(): IPFPageHandle = 
                raise (System.NotImplementedException())
            member this.Dispose(): unit = safeFileHandle.Dispose() // FIXME: Flush all dirty pages in the buffer pool

            member this.DisposePage(pageNum: IPageNum): unit = 
                raise (System.NotImplementedException())
            member this.ForcePages(pageNum: IPageNum): unit = 
                raise (System.NotImplementedException())
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
            member this.GetFirstPage(): IPFPageHandle = undefined()

    type PFManager() =
        
        interface IPFManager with

            member __.AllocateBlock(buffer: byte []): unit = undefined()
                
            member __.CreateFile(fileName: string): unit =
                let safeFileHandle = Interop.CreateFile(fileName, FileAccess.Write, FileShare.None, IntPtr.Zero, FileMode.CreateNew, FileAttributes.Normal, IntPtr.Zero)
                safeFileHandle.Close()

            member __.DestroyFile(fileName: string): unit = 
                let success = Interop.DeleteFile(fileName)
                if (not success) then
                    raise (System.IO.IOException())

            member __.DisposeBlock(buffer: byte []): unit = undefined()

            member __.OpenFile(fileName: string): IPFFileHandle =
                Interop.CreateFile(fileName, FileAccess.ReadWrite, FileShare.None, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero)
                |> (fun h -> new PFFileHandle(h) :> IPFFileHandle)

open PagedFile

[<EntryPoint>]
let main argv =
    let pfManager = PFManager() :> IPFManager
    use pfFileHandle = pfManager.OpenFile("test.db")
    0
