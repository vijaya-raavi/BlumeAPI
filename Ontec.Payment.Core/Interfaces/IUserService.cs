using Ontec.Notification.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ontec.Payment.Core.Interfaces
{
    public interface IUserService
    {
        Task<UserModel> GetUserUsingPayerReferenceNumber(string payerReferenceNumber);
    }
}
