using MediatR;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Company;
using Ontec.Core.Domain.Interface.MasterApiService;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Consumption;
using Ontec.Core.Domain.Models.Dto.Meter;
using Ontec.Core.Domain.Models.Dto.Transaction;
using Ontec.Core.Domain.Requests.Purchase.Queries;

namespace Ontec.Core.Application.Purchase.Handler.Queries
{
    public class PurchaseQueryHandler : IRequestHandler<GetSTSMetersByPropertyIdQuery, IEnumerable<OntecSelectListItem>>
    {
        private readonly IMeterRepository _meterRepository;
        private readonly IWorkContext _workContext;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly MasterApiSetting _masterApiSetting;
        private readonly IMasterApiConnectService _masterApiConnectService;
        private readonly IPropertyRepository _propertyRepository;
        public PurchaseQueryHandler(IMeterRepository meterRepository,
                                    IWorkContext workContext,
                                    IUserRepository userRepository,
                                    ICompanyRepository companyRepository,
                                    MasterApiSetting masterApiSetting,
                                    IMasterApiConnectService masterApiConnectService,
                                    IPropertyRepository propertyRepository)
        {
            _meterRepository = meterRepository;
            _workContext = workContext;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _masterApiSetting = masterApiSetting;
            _masterApiConnectService = masterApiConnectService;
            _propertyRepository = propertyRepository;
        }
        public async Task<IEnumerable<OntecSelectListItem>> Handle(GetSTSMetersByPropertyIdQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            TransactionMasterDto masterDto = new();
            var commonValidator = new GetSTSMetersByPropertyIdQueryValidator(_userRepository, _companyRepository, _workContext);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var STSMeterList = new List<OntecSelectListItem>();
            bool isAdmin = false;
            IEnumerable<MeterDto> meterList = new List<MeterDto>();
            if (_workContext.CurrentRoleId == (int)RoleMasterEnum.Admin || _workContext.CurrentRoleId == (int)RoleMasterEnum.Operator)
            {
                isAdmin = true;
            }
            masterDto.PropertyList = await _propertyRepository.GetTransactionPropertyList(request.OwnerId, isAdmin);
           
            foreach (var property in masterDto.PropertyList)
            {
            //    meterList = await _meterRepository.GetMetersByPropertyId(property.PropertyId).ConfigureAwait(false);
            //}
            //foreach (var meter in meterList)
            //{

                var meterUrl = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?meter.meterNum=" + property.MeterNumber.ToUpper() + "&paging=(limit)(5)(offset)(0)";
                var meterResult = await _masterApiConnectService.GetMeter(meterUrl).ConfigureAwait(false);
                if (meterResult != null)
                {
                    var meterId = meterResult.Data[0].Meter.Id;
                    MeterType type = meterResult.Data[0].Meter.Type;

                    //if (type != null && type.Id == "STS" && type.Name == "STS Meter")
                    //{
                        STSMeterList.Add(new OntecSelectListItem
                        {
                            Id =property.MeterId,
                            Name = property.MeterNumber,
                            OtherText=type.Name
                        });
                    //}
                }
            }
            return STSMeterList;
        }
        

            
           
           
        


        }
}
