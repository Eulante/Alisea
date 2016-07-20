using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AliseaTorrent.Util
{
    class DebugPrinter
    {

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Print(string str)
        {
#if DEBUG
            /*StackTrace st = new StackTrace(new Exception(), false);
            StackFrame[] sf = st.GetFrames();

            Debug.Write(sf[1].GetMethod().Name);*/
            Debug.Write(str);

#endif
        }
    }
}
