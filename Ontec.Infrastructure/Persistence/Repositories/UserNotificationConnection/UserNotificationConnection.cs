using Dapper;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.UserNotificationConnection;
using Ontec.Core.Domain.Requests.Notification.Command;

namespace Ontec.Infrastructure.Persistence.Repositories.UserNotificationConnection
{
    public class UserNotificationConnection : IUserNotificationConnection
    {
        private readonly IGenericRepository _genericRepository;

        public UserNotificationConnection(IGenericRepository genericRepository)
        {
            _genericRepository = genericRepository;
        }

        public async Task<int> UpdateUserNotificatioConnection(UpdateUserConnectionIdQuery request)
        {
            var checkQuery = @"SELECT id FROM user_notification_connections
                             WHERE user_id=@UserId AND connection_id=@OldConnectionId ";

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", request.UserId);
            parameters.Add("@OldConnectionId", request.OldConnectionId);

            parameters.Add("@ConnectionId", request.NewConnectionId);
            parameters.Add("@LogOn", DateTime.UtcNow);
            parameters.Add("@ClientType", request.ClientType);
            try
            {
                var existId = await _genericRepository.ExecuteScalarAsync<int>(checkQuery, parameters).ConfigureAwait(false);
                if (existId > 0)
                {
                    parameters.Add("@Id", existId);

                    var updateQuery = @" UPDATE user_notification_connections 
                                 SET connection_id=@ConnectionId,log_on=@LogOn,
                                    client_type=@ClientType
                                 WHERE id=@Id;
                                SELECT id FROM
                                user_notification_connections
                                WHERE id=@Id";
                    _ = await _genericRepository.ExecuteScalarAsync<int>(updateQuery, parameters).ConfigureAwait(false);
                }
                else
                {
                    try
                    {
                        parameters.Add("@BeforeLogOn", DateTime.UtcNow.AddMinutes(-1));
                        var insertQuery = @"
                                    DELETE FROM user_notification_connections
                                    WHERE user_id=@UserId and log_on>@BeforeLogOn and log_on<=@LogOn;

                                    INSERT INTO user_notification_connections (user_id, connection_id, log_on,client_type)
                                    VALUES (@UserId,@ConnectionId, @LogOn,@ClientType)
                                     RETURNING lastval();";

                        existId = await _genericRepository.ExecuteScalarAsync<int>(insertQuery, parameters).ConfigureAwait(false);
                    }catch(Exception ex)
                    {

                    }
                }
                return existId;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public async Task<string> GetUserConnectionId(int userId)
        {
            var connectionId = "";
            var checkQuery = @"SELECT connection_id FROM user_notification_connections
                             WHERE user_id=@UserId";

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            
            try
            {
                 connectionId = await _genericRepository.ExecuteScalarAsync<string>(checkQuery, parameters).ConfigureAwait(false);
                
                return connectionId;
            }
            catch (Exception ex)
            {
                return connectionId;
            }
        }
        public async Task<int> InsertUserNotificationConnection(string connectionId, int userId,string clientType)
        {
            var checkQuery = @"SELECT id FROM user_notification_connections
                             WHERE user_id=@UserId AND connection_id=@ConnectionId ";

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@ConnectionId", connectionId);
            parameters.Add("@ClientType", clientType);
            parameters.Add("@LogOn", DateTime.UtcNow);

            var existId = await _genericRepository.ExecuteScalarAsync<int>(checkQuery, parameters);
            if (existId == 0)
            {
                var insertQuery = @"INSERT INTO user_notification_connections (user_id, connection_id, log_on,client_type)
                                    VALUES (@UserId,@ConnectionId, @LogOn,@ClientType)
                                     RETURNING lastval()";

                existId = await _genericRepository.ExecuteScalarAsync<int>(insertQuery, parameters).ConfigureAwait(false);
            }
            return existId;
        }
        public async Task DeleteUserNotificationConnectionId(string connectionId)
        {
            var sQuery = @"DELETE FROM user_notification_connections
                            WHERE connection_id=@ConnectionId OR log_on <@LogOn";

            var parameters = new DynamicParameters();
            parameters.Add("@ConnectionId", connectionId);
            parameters.Add("@LogOn", DateTime.UtcNow.AddDays(-1));

            await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
        }

        public async Task<IEnumerable<string>> GetConnectionsByUserId(int userId)
        {
            var sQuery = @"SELECT connection_id FROM public.user_notification_connections
                          WHERE user_id=@UserId";
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);

            return await _genericRepository.GetAsync<string>(sQuery, parameters).ConfigureAwait(false);
        }
    }
}
