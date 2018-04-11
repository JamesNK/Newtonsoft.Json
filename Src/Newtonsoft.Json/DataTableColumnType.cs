
namespace Newtonsoft.Json
{
    /// <summary>
    /// Specifies how data type for columns are chosen for DataTable
    /// </summary>
    public enum DataTableColumnType
    {
        /// <summary>
        /// Automatic, use the first example data's type as the column data type
        /// </summary>
        Automatic,

        /// <summary>
        /// Force as System.Object
        /// </summary>
        Object
    }
}
