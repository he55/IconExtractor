using System;
using System.Runtime.InteropServices;

namespace IconExtractor
{
    internal static class PInvoke
    {
        /// <summary>
        /// Creates an array of handles to icons that are extracted from a specified file.
        ///
        /// Remarks
        /// This function extracts from executable (.exe), DLL (.dll), icon (.ico), cursor (.cur),
        /// animated cursor (.ani), and bitmap (.bmp) files. Extractions from Windows 3.x 16-bit
        /// executables (.exe or .dll) are also supported.
        ///
        /// The cxIcon and cyIcon parameters specify the size of the icons to extract. Two sizes can
        /// be extracted by putting the first size in the LOWORD of the parameter and the second size
        /// in the HIWORD. For example, MAKELONG(24, 48) for both the cxIcon and cyIcon parameters
        /// would extract both 24 and 48 size icons.
        ///
        /// You must destroy all icons extracted by PrivateExtractIconsby calling the DestroyIcon
        /// function.
        ///
        /// This function was not included in the SDK headers and libraries until Windows XP Service
        /// Pack 1 (SP1) and Windows Server 2003. If you do not have a header file and import library
        /// for this function, you can call the function using LoadLibrary and GetProcAddress.
        /// </summary>
        /// <param name="szFileName">
        /// Type: LPCTSTR
        /// The path and name of the file from which the icon(s) are to be extracted.
        /// </param>
        /// <param name="nIconIndex">
        /// Type: int
        /// The zero-based index of the first icon to extract. For example, if this value is zero,
        /// the function extracts the first icon in the specified file.
        /// </param>
        /// <param name="cxIcon">
        /// Type: int
        /// The horizontal icon size wanted. See Remarks.
        /// </param>
        /// <param name="cyIcon">
        /// Type: int
        /// The vertical icon size wanted. See Remarks.
        /// </param>
        /// <param name="phicon">
        /// Type: HICON*
        /// A pointer to the returned array of icon handles.
        /// </param>
        /// <param name="piconid">
        /// Type: UINT*
        /// A pointer to a returned resource identifier for the icon that best fits the current
        /// display device. The returned identifier is 0xFFFFFFFF if the identifier is not available
        /// for this format. The returned identifier is 0 if the identifier cannot otherwise be obtained.
        /// </param>
        /// <param name="nIcons">
        /// Type: UINT
        /// The number of icons to extract from the file. This parameter is only valid when extracting
        /// from .exe and .dll files.
        /// </param>
        /// <param name="flags">
        /// Type: UINT
        /// Specifies flags that control this function. These flags are the LR_* flags used by the
        /// LoadImage function.
        /// </param>
        /// <returns>
        /// Type: UINT
        /// If the phiconparameter is NULL and this function succeeds, then the return value is the
        /// number of icons in the file. If the function fails then the return value is 0.
        /// 
        /// If the phicon parameter is not NULL and the function succeeds, then the return value is
        /// the number of icons extracted. Otherwise, the return value is 0xFFFFFFFF if the file is
        /// not found.
        /// </returns>
        [DllImport("User32.dll")]
        internal static extern uint PrivateExtractIcons(
            /* _In_reads_(MAX_PATH) */ string szFileName,
            /* _In_ */ int nIconIndex,
            /* _In_ */ int cxIcon,
            /* _In_ */ int cyIcon,
            /* _Out_writes_opt_(nIcons) */ IntPtr[] phicon,
            /* _Out_writes_opt_(nIcons) */ uint[] piconid,
            /* _In_ */ uint nIcons,
            /* _In_ */ uint flags);

        /// <summary>
        /// Destroys an icon and frees any memory the icon occupied.
        ///
        /// Remarks
        /// It is only necessary to call DestroyIcon for icons and cursors created with the following
        /// functions: CreateIconFromResourceEx (if called without the LR_SHARED flag), CreateIconIndirect,
        /// and CopyIcon. Do not use this function to destroy a shared icon. A shared icon is valid as long
        /// as the module from which it was loaded remains in memory. The following functions obtain a
        /// shared icon.
        /// </summary>
        /// <param name="hIcon">
        /// Type: HICON
        /// A handle to the icon to be destroyed. The icon must not be in use.
        /// </param>
        /// <returns>
        /// Type: BOOL
        /// If the function succeeds, the return value is nonzero.
        ///
        /// If the function fails, the return value is zero. To get extended error information,
        /// call GetLastError.
        /// </returns>
        [DllImport("User32.dll")]
        internal static extern int DestroyIcon(
            /* _In_ */ IntPtr hIcon);
    }
}
