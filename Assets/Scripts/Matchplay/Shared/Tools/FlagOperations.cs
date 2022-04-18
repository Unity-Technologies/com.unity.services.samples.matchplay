using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Matchplay.Shared.Tools
{
    public static class FlagOperation{
        public static IEnumerable<T> GetUniqueFlags<T>(this T flags) where T:Enum
        {
            ulong flag = 1;
            foreach (var value in Enum.GetValues(flags.GetType()).Cast<Enum>())
            {
                ulong bits = Convert.ToUInt64(value);
                while (flag < bits)
                {
                    flag <<= 1;
                }

                if (flag == bits && flags.HasFlag(value))
                {
                    yield return (T)value;
                }
            }
        }
    }


}
