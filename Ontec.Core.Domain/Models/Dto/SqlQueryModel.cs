using Dapper;

namespace Ontec.Core.Domain.Models.Dto
{
    public class SqlQueryModel
    {
        public string? SqlQuery { get; set; }
        public string? ReturnParameterName { get; set; }
        public int ReturnParameterValue { get; set; }
        public DynamicParameters? SqlParameters { get; set; }
        public bool IsReturn { get; set; }
        public int Id { get; set; }
    }
}
