using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Requests.Login.Command;

namespace Ontec.Core.Domain.Requests.Consumer.Queries
{
    public class GetConsumerQueryValidator : AbstractValidator<GetConsumersQuery>
    {
        public GetConsumerQueryValidator(IUserRepository _userRepository)
        {

            

        }
    }
}
