#if NETSTANDARD1_6
namespace Serilog.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// DataRow is a class used as a helper to implement a very limited DataTable like API.
    /// </summary>
    public class DataRow : Dictionary<string, object>
    {
        /// <summary>
        /// Accessor for the DataTable of this row.
        /// </summary>
        public DataTable Table { get; set; }
    }
}
#endif