using MediatR;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Consumer;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.Consumer;
using Ontec.Core.Domain.Models.Dto.User;
using Ontec.Core.Domain.Requests.Consumer.Queries;

namespace Ontec.Core.Application.Consumer.Queries
{

    public class ConsumerQueryHandler : IRequestHandler<GetConsumersQuery, DatatableModel<ConsumerDto>>,
                                        IRequestHandler<GetConsumerDashboardQuery, ConsumerDashboardDto>,
                                        IRequestHandler<GetConsumerMastersQuery, ConsumerMasterDto>,
                                        IRequestHandler<GetConsumerGroupQueryRequest, IEnumerable<ConsumerGroupDto>>

    {
        private readonly IUserRepository _userRepository;
        private readonly IConsumerRepository _consumerRepository;
        private readonly IWorkContext _workContext;

        public ConsumerQueryHandler(IUserRepository userRepository
                                    , IConsumerRepository consumerRepository
                                    ,IWorkContext workContext)
        {
            _userRepository = userRepository;
            _consumerRepository = consumerRepository;
            _workContext = workContext;
        }
        public async Task<DatatableModel<ConsumerDto>> Handle(GetConsumersQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var commonValidator = new GetConsumerQueryValidator(_userRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            return await _consumerRepository.GetConsumersNew(request).ConfigureAwait(false);
        }
        public async Task<ConsumerDashboardDto> Handle(GetConsumerDashboardQuery request, CancellationToken cancellationToken)
        {
            return await _consumerRepository.GetConsumerDashboard().ConfigureAwait(false);
        }

        public async Task<ConsumerMasterDto> Handle(GetConsumerMastersQuery request, CancellationToken cancellationToken)
        {
         // return await _consumerRepository.GetConsumerMasters().ConfigureAwait(false);

            return await _consumerRepository.NewGetConsumerMaster(request.EstateId).ConfigureAwait(false);
        }
        public async Task<IEnumerable<ConsumerGroupDto>> Handle(GetConsumerGroupQueryRequest request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var commonValidator = new GetConsumerGroupQueryRequestValidator(_consumerRepository,_workContext);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            return await _consumerRepository.GetConsumerGroupsById(request.UserId).ConfigureAwait(false);
        }
    }
}
