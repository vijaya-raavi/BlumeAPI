using MediatR;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.User;
using Ontec.Core.Domain.Requests.Operator.Queries;

namespace Ontec.Core.Application.Operator.Queries
{
    public class OperatorQueryHandler : IRequestHandler<GetOperatorsQuery, DatatableModel<OperatorDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IEncryptionandDecryption _encryptionandDecryption;
        public OperatorQueryHandler(IUserRepository userRepository
                                    , IEncryptionandDecryption encryptionandDecryption)
        {
            _userRepository = userRepository;
            _encryptionandDecryption = encryptionandDecryption;
        }

        public async Task<DatatableModel<OperatorDto>> Handle(GetOperatorsQuery request, CancellationToken cancellationToken)
        {
            var dataList = await _userRepository.GetOperators(request);
            if (request.Id > 0 && dataList.Data.Count == 1)
            {
                dataList.Data[0].Password =_encryptionandDecryption.Decrypt(dataList.Data[0].Password);
            }
            return dataList;
        }

    }
}
