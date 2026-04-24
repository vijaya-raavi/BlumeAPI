using Ontec.Core.Domain.Models.Dto;
using System.Data;

namespace Ontec.Core.Domain.Interface
{
    public interface IGenericRepository
    {
        public string? ConnectionString { get; }
        IDbTransaction TransactionOpen();
        void TransactionClose();
        Task<byte[]> GetDocumentAsBytesAsync(string url);
        Task<IEnumerable<T>> GetAsync<T>(string sQuery, object? param = null);
        Task<T> GetSignleOrDefaultAsync<T>(string sQuery, object? param = null);
        T GetSignleOrDefault<T>(string sQuery, object? param = null);
        Task<T> GetSignleOrDefaultAsync<T>(string sQuery, IDbTransaction transaction, object? param = null);
        Task<T> GetFirstOrDefaultAsync<T>(string sQuery, object? param = null);
        T GetFirstOrDefault<T>(string sQuery, object? param = null);

        Task<T> ExecuteScalarAsync<T>(string sQuery, object? param = null);
        Task<object> ExecuteScalarAsync(string sQuery, object? param = null);
        object ExecuteScalar(string sQuery, object? param = null);

        Task<int> ExecuteProcedureAsync(string commandText, object? param = null);
        Task<bool> FindAsync(string sQuery, object? param = null);
        bool Find(string sQuery, object? param = null);

        IEnumerable<T> Get<T>(string sQuery, object? param = null);
        Task<int> ExecuteCommandAsync(string sQuery, object? param = null);
        Task<int> ExecuteCommandAsync(string sQuery, IDbTransaction transaction, object? param = null);
        Task<int> ExecuteMultipleCommandAsync(List<SqlQueryModel> sqlQueries, int steps = 1);
         Dictionary<string, string> ToDictionary(object obj);

        T ExecuteScalar<T>(string sQuery, object param = null);
    }
}
