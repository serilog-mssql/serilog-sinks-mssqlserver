#if NETSTANDARD1_6
namespace Serilog.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// ColumnCollection is a class used as a helper to implement a very limited DataTable like API.
    /// </summary>
    public class ColumnCollection : List<DataColumn>
    {
        /// <summary>
        /// Implementing Dictionary accessor.
        /// </summary>
        /// <param name="key"></param>
        public DataColumn this[string key]
        {
            get { return this.FirstOrDefault(x => x.ColumnName.Equals(key, StringComparison.Ordinal)); }
        }

        /// <summary>
        /// Implemented Dictionary like contains.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contains(string key)
        {
            return this.Any(x => x.ColumnName.Equals(key, StringComparison.Ordinal));
        }
    }
}
#endif