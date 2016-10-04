using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace XLToolLibrary.Utilities
{

    /**
     * 
     * 对cpp的dll进行加载，使用时注意修改dll名称和EntryPoint的名称
     * 
     */
    public class CppDll
    {
        [DllImport( "EnDll.dll" ,EntryPoint = "str_encrypt_reversible" )]
        public static extern string str_encrypt_reversible(string dest, string key);
        [DllImport( "EnDll.dll" , EntryPoint = "str_encrypt2_reversible" )]
        public static extern string str_encrypt2_reversible( string dest, string key );
    }
}