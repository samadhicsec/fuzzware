using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace Fuzzware.Common.Interop
{
    [Guid("00020400-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IDispatch
    {
        /// <summary>
        /// Retrieves the number of type information interfaces that an object provides (either 0 or 1).
        /// </summary>
        /// <param name="Count">The number of type information interfaces provided by the object. If the object provides type 
        /// information, this number is 1; otherwise the number is 0</param>
        /// <returns>HRESULT</returns>
        [PreserveSig]
        int GetTypeInfoCount(out int Count);

        /// <summary>
        /// Retrieves the type information for an object, which can then be used to get the type information for an interface.
        /// </summary>
        /// <param name="iTInfo">The type information to return. Pass 0 to retrieve type information for the IDispatch implementation.</param>
        /// <param name="lcid">The locale identifier for the type information. An object may be able to return different type 
        /// information for different languages. This is important for classes that support localized member names. For classes that 
        /// do not support localized member names, this parameter can be ignored. </param>
        /// <param name="typeInfo">Receives a pointer to the requested type information object.</param>
        /// <returns>HRESULT</returns>
        [PreserveSig]
        int GetTypeInfo([MarshalAs(UnmanagedType.U4)] int iTInfo, [MarshalAs(UnmanagedType.U4)] int lcid, out ComTypes.ITypeInfo typeInfo);

        /// <summary>
        /// Maps a single member and an optional set of argument names to a corresponding set of integer DISPIDs, which can be used on 
        /// subsequent calls to IDispatch::Invoke. The dispatch function DispGetIDsOfNames provides a standard implementation of 
        /// GetIDsOfNames. 
        /// </summary>
        /// <param name="riid">Reserved for future use. Must be IID_NULL.</param>
        /// <param name="rgsNames">Passed-in array of names to be mapped. </param>
        /// <param name="cNames">Count of the names to be mapped. </param>
        /// <param name="lcid">The locale context in which to interpret the names.</param>
        /// <param name="rgDispId">Caller-allocated array, each element of which contains an identifier (ID) corresponding to one of 
        /// the names passed in the rgszNames array. The first element represents the member name. The subsequent elements represent 
        /// each of the member's parameters.</param>
        /// <returns>HRESULT</returns>
        [PreserveSig]
        int GetIDsOfNames(ref Guid riid, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)] string[] rgsNames, int cNames, int lcid, [MarshalAs(UnmanagedType.LPArray)] int[] rgDispId);

        /// <summary>
        /// Provides access to properties and methods exposed by an object. The dispatch function DispInvoke provides a standard 
        /// implementation of IDispatch::Invoke.
        /// </summary>
        /// <param name="dispIdMember">Identifies the member. Use IDispatch::GetIDsOfNames or the object's documentation to obtain 
        /// the dispatch identifier. </param>
        /// <param name="riid">Reserved for future use. Must be IID_NULL.</param>
        /// <param name="lcid">The locale context in which to interpret arguments. The lcid is used by the GetIDsOfNames function, 
        /// and is also passed to IDispatch::Invoke to allow the object to interpret its arguments specific to a locale. Applications 
        /// that do not support multiple national languages can ignore this parameter.</param>
        /// <param name="wFlags">Flags describing the context of the Invoke call, include: DISPATCH_METHOD; DISPATCH_PROPERTYGET; 
        /// DISPATCH_PROPERTYPUT; DISPATCH_PROPERTYPUTREF</param>
        /// <param name="pDispParams">Pointer to a DISPPARAMS structure containing an array of arguments, an array of argument DISPIDs 
        /// for named arguments, and counts for the number of elements in the arrays.</param>
        /// <param name="pVarResult">Pointer to the location where the result is to be stored, or NULL if the caller expects no 
        /// result. This argument is ignored if DISPATCH_PROPERTYPUT or DISPATCH_PROPERTYPUTREF is specified. </param>
        /// <param name="pExcepInfo">Pointer to a structure that contains exception information. This structure should be filled 
        /// in if DISP_E_EXCEPTION is returned. Can be NULL.</param>
        /// <param name="pArgErr">The index within rgvarg of the first argument that has an error. Arguments are stored in 
        /// pDispParams->rgvarg in reverse order, so the first argument is the one with the highest index in the array. This parameter 
        /// is returned only when the resulting return value is DISP_E_TYPEMISMATCH or DISP_E_PARAMNOTFOUND. This argument can be set 
        /// to null. </param>
        /// <returns>HRESULT</returns>
        [PreserveSig]
        int Invoke(int dispIdMember, ref Guid riid, uint lcid, ushort wFlags, ref ComTypes.DISPPARAMS pDispParams, out object pVarResult, ref ComTypes.EXCEPINFO pExcepInfo, IntPtr[] pArgErr);
    }
}
