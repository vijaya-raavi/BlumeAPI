using Dapper;
using Npgsql;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Infrastructure.Persistence.Configurations;
using System.Data;
namespace Ontec.Infrastructure.Persistence.Repositories.GenericRepository
{
    public class GenericRepository : IGenericRepository
    {
        private IDbConnection dbConnection = null;
        public string? ConnectionString { get; private set; }
        public GenericRepository(DatabaseSetting dbSetting)
        {
            ConnectionString = dbSetting.OntechDbConnectionString;
        }
        private IDbConnection Connection
        {
            get
            {
                return new NpgsqlConnection(ConnectionString);
            }
        }
        public async Task<int> ExecuteCommandAsync(string sQuery, object? param = null)
        {
            using (IDbConnection conn = Connection)
            {
                conn.Open();
                var result = await conn.ExecuteAsync(sQuery, param, null, 180).ConfigureAwait(false);
                conn.Close();
                return result;
            }
        }

        public async Task<int> ExecuteCommandAsync(string sQuery, IDbTransaction transaction, object? param = null)
        {
            using (IDbConnection conn = Connection)
            {
                conn.Open();
                var result = await conn.ExecuteAsync(sQuery, param, transaction, 180).ConfigureAwait(false);
                conn.Close();
                return result;
            }
        }

        public async Task<int> ExecuteMultipleCommandAsync(List<SqlQueryModel> sqlQueries, int steps = 1)
        {
            using IDbConnection conn = Connection;
            conn.Open();
            using (var transaction = conn.BeginTransaction())
            {
                var returnValue = 0;
                var index = 0;
                try
                {
                    foreach (var item in sqlQueries)
                    {
                        if (index > 0)
                        {
                            item.SqlQuery = item.SqlQuery.Replace("@REPLACEID", returnValue.ToString());
                        }
                        if (item.IsReturn)
                        {
                            if (index < steps)
                                returnValue = conn.QuerySingle<int>(item.SqlQuery, item.SqlParameters, transaction, 180);
                            else
                                item.Id = conn.QuerySingle<int>(item.SqlQuery, item.SqlParameters, transaction, 180);
                        }
                        else
                            await conn.ExecuteAsync(item.SqlQuery, item.SqlParameters, transaction, 180).ConfigureAwait(false);
                        index++;
                    }
                    transaction.Commit();

                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return 0;
                    throw;
                }
            }
            conn.Close();
            return 1;
        }

        public async Task<int> ExecuteProcedureAsync(string commandText, object? param = null)
        {
            using (IDbConnection conn = Connection)
            {
                conn.Open();
                var commandDefinition = new CommandDefinition(commandText, param, null, null, CommandType.StoredProcedure);
                var result = await conn.ExecuteScalarAsync(commandDefinition).ConfigureAwait(false);
                conn.Close();
                return (int)result;
            }
        }

        public object ExecuteScalar(string sQuery, object? param = null)
        {
            using (IDbConnection conn = Connection)
            {
                conn.Open();
                var result = conn.ExecuteScalar(sQuery, param);
                conn.Close();
                return result;
            }
        }

        public T ExecuteScalar<T>(string sQuery, object param = null)
        {
            using (IDbConnection conn = Connection)
            {
                conn.Open();
                var result = conn.ExecuteScalar<T>(sQuery, param);
                conn.Close();
                return result;
            }
        }
        public async Task<byte[]> GetDocumentAsBytesAsync(string url)
        {
            using (var httpClient = new HttpClient())
            {
                return await httpClient.GetByteArrayAsync(url);
            }
        }
        public async Task<T> ExecuteScalarAsync<T>(string sQuery, object? param = null)
        {
            using (IDbConnection conn = Connection)
            {
                conn.Open();
                var result = await conn.ExecuteScalarAsync<T>(sQuery, param).ConfigureAwait(false);
                conn.Close();
                return result;
            }
        }

        public async Task<object> ExecuteScalarAsync(string sQuery, object? param = null)
        {
            using (IDbConnection conn = Connection)
            {
                conn.Open();
                var result = await conn.ExecuteScalarAsync(sQuery, param).ConfigureAwait(false);
                conn.Close();
                return result;
            }
        }

        public bool Find(string sQuery, object? param = null)
        {
            using (IDbConnection conn = Connection)
            {
                conn.Open();
                var result = conn.ExecuteScalar(sQuery, param);
                conn.Close();
                if (result != null)
                {
                    return ((int)result) > 0;
                }
                return false;

            }
        }

        public async Task<bool> FindAsync(string sQuery, object? param = null)
        {
            using (IDbConnection conn = Connection)
            {
                conn.Open();
                var result = await conn.ExecuteScalarAsync(sQuery, param).ConfigureAwait(false);
                conn.Close();
                if (result != null)
                {
                    return ((int)result) > 0;
                }
                return false;

            }
        }

        public IEnumerable<T> Get<T>(string sQuery, object? param = null)
        {
            using (IDbConnection conn = Connection)
            {
                conn.Open();
                var result = conn.Query<T>(sQuery, param);
                conn.Close();
                return result;
            }
        }

        public async Task<IEnumerable<T>> GetAsync<T>(string sQuery, object? param = null)
        {
            using (IDbConnection conn = Connection)
            {
                conn.Open();
                var result = await conn.QueryAsync<T>(sQuery, param).ConfigureAwait(false);
                conn.Close();
                return result;
            }
        }

        public T GetFirstOrDefault<T>(string sQuery, object? param = null)
        {
            using (IDbConnection conn = Connection)
            {
                conn.Open();
                var result = conn.QueryFirstOrDefault<T>(sQuery, param);
                conn.Close();
                return result;
            }
        }

        public async Task<T> GetFirstOrDefaultAsync<T>(string sQuery, object? param = null)
        {
            using (IDbConnection conn = Connection)
            {
                conn.Open();
                var result = await conn.QueryFirstOrDefaultAsync<T>(sQuery, param).ConfigureAwait(false);
                conn.Close();
                return result;
            }
        }

        public T GetSignleOrDefault<T>(string sQuery, object? param = null)
        {
            using (IDbConnection conn = Connection)
            {
                conn.Open();
                var result = conn.QuerySingleOrDefault<T>(sQuery, param);
                conn.Close();
                return result;
            }
        }

        public async Task<T> GetSignleOrDefaultAsync<T>(string sQuery, object? param = null)
        {
            using (IDbConnection conn = Connection)
            {
                conn.Open();
                var result = await conn.QuerySingleOrDefaultAsync<T>(sQuery, param).ConfigureAwait(false);
                conn.Close();
                return result;
            }
        }

        public async Task<T> GetSignleOrDefaultAsync<T>(string sQuery, IDbTransaction transaction, object? param = null)
        {
            using (IDbConnection conn = Connection)
            {
                conn.Open();
                var result = await conn.QuerySingleOrDefaultAsync<T>(sQuery, param, transaction).ConfigureAwait(false);
                conn.Close();
                return result;
            }
        }
        public  Dictionary<string, string> ToDictionary(object obj)
        {
            return obj.GetType()
                      .GetProperties()
                      .ToDictionary(
                          prop => prop.Name,
                          prop => prop.GetValue(obj)?.ToString() ?? string.Empty
                      );
        }
        public IDbTransaction TransactionOpen()
        {
            dbConnection = Connection;
            dbConnection.Open();
            return dbConnection.BeginTransaction();
        }

        public void TransactionClose()
        {
            dbConnection.Close();
        }

    }
}
