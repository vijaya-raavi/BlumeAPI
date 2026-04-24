using Ontec.Core.Domain.Interface.User;
using Ontec.Notification.Core.Models;
using Ontec.Payment.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ontec.Payment.Core
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository=userRepository;
        }
        public async Task<UserModel> GetUserUsingPayerReferenceNumber(string payerReferenceNumber)
        {
            var user = await _userRepository.GetUserByPayerReferenceNumber(payerReferenceNumber);

            return new UserModel()
            {
                UserId  = user.Id
            };
        }
    }
}
