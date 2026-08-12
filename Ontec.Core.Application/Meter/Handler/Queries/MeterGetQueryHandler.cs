using MediatR;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.MasterApiService;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Consumption;
using Ontec.Core.Domain.Models.Dto.Meter;
using Ontec.Core.Domain.Requests.Meter.Queries;

namespace Ontec.Core.Application.Meter.Handler.Queries
{
    public class MeterGetQueryHandler : IRequestHandler<GetMetersByPropertyIdQuery, PropertyMetersDto>
                                        , IRequestHandler<GetMeterByIdQuery, UpdateMeterDto>
                                        , IRequestHandler<GetMeterMastersByOwnerIdQuery, EditMeterMasters>
                                        , IRequestHandler<GetMeterRequestQuery, DatatableModel<MeterRequestDto>>
                                        , IRequestHandler<VerifyMeterQuery, AddUpdateResultDto>
                                        , IRequestHandler<GetMeterExpiringListRequest, IEnumerable<MeterExpiringDto>>
                                        , IRequestHandler<GetMetersByMeterNumberQuery, IEnumerable<MetersUtilityDto>>
                                        , IRequestHandler<FetchMeterFromCustAggmentCommandRequest, MasterPropertyAndMeterDetails>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMeterRepository _meterRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IMasterApiConnectService _masterApiConnectService;
        private readonly MasterApiSetting _masterApiSetting;
        private readonly IWorkContext _workContext;
        private readonly ICompanyHelper _companyHelper;
        private readonly IGenericRepository _genericRepository;
        public MeterGetQueryHandler(IMeterRepository meterRepository
                                    , IPropertyRepository propertyRepository
                                    , IWorkContext workContext
                                    , IUserRepository userRepository
                                    , IMasterApiConnectService masterApiConnectService
                                    , MasterApiSetting masterApiSetting
                                    , ICompanyHelper companyHelper,
IGenericRepository genericRepository)
        {
            _meterRepository = meterRepository;
            _propertyRepository = propertyRepository;
            _userRepository = userRepository;
            _masterApiConnectService = masterApiConnectService;
            _masterApiSetting = masterApiSetting;
            _workContext = workContext;
            _companyHelper = companyHelper;
            _genericRepository = genericRepository;
        }
        public async Task<PropertyMetersDto> Handle(GetMetersByPropertyIdQuery request, CancellationToken cancellationToken)
        {
            var propertyMeters = new PropertyMetersDto();

            request.TrimAllStrings();

            var commonValidator = new GetMetersByPropertyIdQueryValidator(_propertyRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            propertyMeters.Meters = await _meterRepository.GetMetersByPropertyId(request.Id).ConfigureAwait(false);
            var property = await _propertyRepository.GetPropertyById(request.Id).ConfigureAwait(!false);
            propertyMeters.PropertyName = property.Name;
            propertyMeters.PropertyId = request.Id;
            propertyMeters.SolarMeterCount = await _meterRepository.IsMeterSolar(request.Id).ConfigureAwait(false);
            propertyMeters.IsOwner = _workContext.CurrentUserId == property.OwnerId;
            propertyMeters.TotalMeter = propertyMeters.Meters?.Count() ?? 0;
            var metersToRemove = new List<MeterDto>();
            propertyMeters.InCompleteMeterCount = propertyMeters.Meters?.Count(m => m.Status == "In Complete") ?? 0;
            if (propertyMeters.Meters !=null &&propertyMeters.Meters.Count() > 0)
            {
                foreach (var m in propertyMeters.Meters)
                {
                    var meterUrl = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?meter.meterNum=" + m.MeterNumber.ToUpper() + "&paging=(limit)(5)(offset)(0)";
                    var meterResult = await _masterApiConnectService.GetMeter(meterUrl).ConfigureAwait(false);
                    if (meterResult != null && meterResult.Data.Count()>0)
                    {
                        var meterId = meterResult.Data[0].Meter.Id;
                        MeterType type = meterResult.Data[0].Meter.Type;
                        m.MasterMeterType = type.Name;
                    }
                }
                

            }

            return propertyMeters;
        }

        public async Task<UpdateMeterDto> Handle(GetMeterByIdQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new GetMeterByIdQueryValidator(_meterRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var meterdto = await _meterRepository.GetMeterById(request.Id).ConfigureAwait(false);
            var company = await _companyHelper.GetCompany(_workContext.CurrentCompanyId).ConfigureAwait(false);
            if (meterdto.MeterDocument != null)
            {

                meterdto.MeterDocument = company.Domain + meterdto.MeterDocument;
                byte[] fileBytes = null;
                fileBytes = await _genericRepository.GetDocumentAsBytesAsync(meterdto.MeterDocument).ConfigureAwait(false);
                if (fileBytes != null)
                {
                    var meterDoc = new DocumentResultDto
                    {
                        FileName = Path.GetFileName(meterdto.MeterDocument),
                        Type = Path.GetExtension(meterdto.MeterDocument),
                        Document = fileBytes
                    };


                    meterdto.MeterDoc = meterDoc;
                }
                meterdto.MeterDocument = null;

                meterdto.MeterDocument = company.Domain + meterdto.MeterDocument;
            }
            return meterdto;
        }

        public async Task<EditMeterMasters> Handle(GetMeterMastersByOwnerIdQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new GetMeterMastersByOwnerIdQueryValidator(_userRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            return await _meterRepository.GetEditMeterMasters(request.OwnerId).ConfigureAwait(false);
        }

        public async Task<DatatableModel<MeterRequestDto>> Handle(GetMeterRequestQuery request, CancellationToken cancellationToken)
        {
            var meterRequestDto = await _meterRepository.GetMeter(request);
            var company = await _companyHelper.GetCompany(_workContext.CurrentCompanyId).ConfigureAwait(false);
            if (meterRequestDto.Data != null)
            {
                foreach (var meter in meterRequestDto.Data)
                {
                    if (meter.Document != null)
                    {
                        meter.Document = company.Domain + meter.Document;
                    }
                }
            }
            return meterRequestDto;
        }

        public async Task<AddUpdateResultDto> Handle(VerifyMeterQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new VerifyMeterQueryValidator(_meterRepository, _propertyRepository, _masterApiConnectService, _masterApiSetting);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            string utilityType = "";
            string utilityName = "";
            var meterUrl = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?meter.meterNum=" + request.MeterNumber.ToUpper() + "&paging=(limit)(5)(offset)(0)";
            var meterResult = await _masterApiConnectService.GetMeter(meterUrl).ConfigureAwait(false);
            if (meterResult != null)
            {
                utilityType = meterResult.Data[0].Meter.Model.ServiceResource;
                utilityName = meterResult.Data[0].Meter.Model.Name;
            }
            var meterTypes = await _meterRepository.GetMeterType().ConfigureAwait(false);
            var utility = meterTypes.FirstOrDefault(t => t.name.ToUpper().Contains(utilityType));
            if (utilityName.Contains("SMART HOT_WATER"))
            {
                utilityType = "HOT WATER";
            }
            var result = new AddUpdateResultDto();
            if (utility != null)

            {
                result = new AddUpdateResultDto()
                {
                    Id = 0,
                    Message = CommonConstants.MeterVerifyied,
                    UtilityType = utility.name,
                    UtilityId = utility.id,
                };
            }
            else
            {
                result = new AddUpdateResultDto()
                {
                    Id = 0,
                    Message = CommonConstants.MeterVerifyied,
                    UtilityType = "",
                    UtilityId = 0,
                };
            }
            return result;
        }

        public async Task<IEnumerable<MeterExpiringDto>> Handle(GetMeterExpiringListRequest request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var commonValidator = new GetMeterExpiringListRequestValidator(_userRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            return await _meterRepository.GetMeterExpiringDtos(request.UserId).ConfigureAwait(false);
        }

        public async Task<IEnumerable<MetersUtilityDto>> Handle(GetMetersByMeterNumberQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new GetMetersByMeterNumberQueryValidator(_masterApiConnectService, _masterApiSetting, _meterRepository, _propertyRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            string customerAgrrementIdReference = "";
            var meterUrl = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?meter.meterNum=" + request.MeterNumber.ToUpper() + "&paging=(limit)(5)(offset)(0)";
            var meterResult = await _masterApiConnectService.GetMeter(meterUrl).ConfigureAwait(false);
            var masterMeterResult = new MeterDataModel();
            if (meterResult != null)
            {
                customerAgrrementIdReference = meterResult.Data[0].CustomerAgreement.AgreementRef;
            }
            var result = new List<MetersUtilityDto>();


            if (customerAgrrementIdReference != null)
            {
                var masterMetersUrl = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?customerAgreement.agreementRef=" + customerAgrrementIdReference + "&paging=(limit)(20)(offset)(0)";
                masterMeterResult = await _masterApiConnectService.GetMeter(masterMetersUrl).ConfigureAwait(false);

                if (masterMeterResult != null)
                {
                    var meterNumbers = new List<string>();
                    foreach (var m in masterMeterResult.Data)
                    {
                        if (m != null)
                        {
                            meterNumbers.Add(m.Meter.MeterNum);
                        }
                    }

                    var existingMeterList = await _meterRepository.GetAllMetersByPropertyId(request.PropertyId).ConfigureAwait(false);
                    var existingMeters = existingMeterList
                              .Where(t => t.Status.Equals("Active") || t.Status.Equals("Pending") || t.Status.Equals("Rejected") || t.Status.Equals("De active"))
                              .Select(t => t.MeterNumber)
                              .ToList();

                    var newMeters = meterNumbers.ToList();

                    var filteredNewMeters = newMeters
                                            .Where(meterNumbers => !existingMeters.Contains(meterNumbers))
                                            .ToList();

                    var AllMetersList = await _meterRepository.GetAllMeters().ConfigureAwait(false);
                    var AllExistingMeters = AllMetersList
                                            .Where(t => t.Status.Equals("Active") || t.Status.Equals("Pending") || t.Status.Equals("Rejected") || t.Status.Equals("De active"))
                                            .Select(t => t.MeterNumber).ToList();
                    var filteredFromAllMeters = filteredNewMeters
                                               .Where(filteredNewMeters => !AllExistingMeters.Contains(filteredNewMeters))
                                               .ToList();



                    var meterTypes = await _meterRepository.GetMeterType().ConfigureAwait(false);


                    //if (utility != null)
                    //{
                    if (filteredFromAllMeters.Count > 0)
                    {
                        foreach (var meterNumber in filteredFromAllMeters)
                        {

                            string utiltyType = "";
                            string utilityName = "";
                            var meterUrlUtility = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?meter.meterNum=" + meterNumber.ToUpper() + "&paging=(limit)(5)(offset)(0)";
                            var meterUtilityResult = await _masterApiConnectService.GetMeter(meterUrlUtility).ConfigureAwait(false);

                            if (meterUtilityResult != null)
                            {
                                utiltyType = meterUtilityResult.Data[0].Meter.Model.ServiceResource;
                                utilityName = meterUtilityResult.Data[0].Meter.Model.Name;
                                var utility = meterTypes.FirstOrDefault(t => t.name.ToUpper().Contains(utiltyType));
                                if (utilityName.Contains("SMART HOT_WATER"))
                                {
                                    utiltyType = "HOT WATER";
                                }
                                if (utility != null)

                                {
                                    result.Add(new MetersUtilityDto
                                    {
                                        MeterNumbers = meterNumber,
                                        MeterTypeId = utility.id,
                                        MeterUtility = utility.name,
                                        UnitOfMeasure = utility.UnitOfMeasure,
                                        MinDailyTarget = double.TryParse(utility.MinDailyTarget, out var minTarget) ? minTarget : 0,
                                        MaxDailyTarget = double.TryParse(utility.MaxDailyTarget, out var maxTarget) ? maxTarget : 0
                                    });
                                }
                                else
                                {

                                    result.Add(new MetersUtilityDto
                                    {
                                        MeterNumbers = meterNumber,
                                        MeterTypeId = 0,
                                        MeterUtility = "",
                                        UnitOfMeasure = "",
                                        MinDailyTarget = 0,
                                        MaxDailyTarget = 0
                                    });
                                }
                            }
                        }
                    }

                }
            }


            return result;
        }


        public async Task<MasterPropertyAndMeterDetails> Handle(FetchMeterFromCustAggmentCommandRequest request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new FetchMeterFromCustAggmentCommandRequestValidator();
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            string customerAgrrementReferenceNo = "";
            var customerAgrrementIdReference = "";
            var masterMetersUrl = "";
            var masterMeterResult = new MeterDataModel();
            var dto = new MasterPropertyAndMeterDetails();

            if (!string.IsNullOrEmpty(request.UnitNumber) && !string.IsNullOrEmpty(request.EstateName) && request.UnitNumber != "string" && request.EstateName != "string")
            {
                customerAgrrementReferenceNo = request.UnitNumber + "-" + request.EstateName;
                if (customerAgrrementReferenceNo != null)
                {
                    masterMetersUrl = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?customerAgreement.agreementRef=" + customerAgrrementReferenceNo + "&paging=(limit)(20)(offset)(0)";
                    masterMeterResult = await _masterApiConnectService.GetMeter(masterMetersUrl).ConfigureAwait(false);
                }
            }
            if (!string.IsNullOrEmpty(request.MeterNumber) && request.MeterNumber != "string")
            {
                masterMetersUrl = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?meter.meterNum=" + request.MeterNumber + "&paging=(limit)(20)(offset)(0)";
                masterMeterResult = await _masterApiConnectService.GetMeter(masterMetersUrl).ConfigureAwait(false);
            }
            if (!string.IsNullOrEmpty(request.CustomerAgreementNo) && request.CustomerAgreementNo != "string")
            {
                masterMetersUrl = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?customerAgreement.agreementRef=" + request.CustomerAgreementNo + "&paging=(limit)(20)(offset)(0)";
                masterMeterResult = await _masterApiConnectService.GetMeter(masterMetersUrl).ConfigureAwait(false);

                //masterMetersUrl = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?customer.customerReference=" + request.CustomerAgreementNo + "&paging=(limit)(20)(offset)(0)";
                //masterMeterResult = await _masterApiConnectService.GetMeter(masterMetersUrl).ConfigureAwait(false);

            }


            if (masterMeterResult != null)
            {
                if (masterMeterResult.Data != null)
                {
                    Location locations = masterMeterResult.Data[0].Customer.Location;

                    foreach (var m in masterMeterResult.Data)
                    {

                        if (m != null)
                        {

                            dto.Customer = m.Customer;
                            dto.Customer.Location = locations;
                            dto.Customer.AgreementRef = m.CustomerAgreement.AgreementRef;

                        }
                    }

                }
            }

            var result = new List<MetersUtilityDto>();

            if (masterMeterResult != null)
            {
                var meterNumbers = new List<string>();
                foreach (var m in masterMeterResult.Data)
                {
                    if (m != null)
                    {
                        meterNumbers.Add(m.Meter.MeterNum);
                    }
                }

                //var existingMeterList = await _meterRepository.GetAllMetersByPropertyId(request.PropertyId.Value).ConfigureAwait(false);
                var existingMeterList = await _meterRepository.GetAllMeters().ConfigureAwait(false);
                var existingMeters = existingMeterList
                          .Where(t => t.Status.Equals("Active") || t.Status.Equals("Pending") || t.Status.Equals("Rejected"))
                          .Select(t => t.MeterNumber)
                          .ToList();

                var newMeters = meterNumbers.ToList();

                var filteredNewMeters = newMeters
                                        .Where(meterNumbers => !existingMeters.Contains(meterNumbers))
                                        .ToList();

                


                var meterTypes = await _meterRepository.GetMeterType().ConfigureAwait(false);


                //if (utility != null)
                //{
                if (filteredNewMeters.Count > 0)
                {
                    foreach (var meterNumber in filteredNewMeters)
                    {

                        string utiltyType = "";
                        var meterUrlUtility = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?meter.meterNum=" + meterNumber + "&paging=(limit)(5)(offset)(0)";
                        var meterUtilityResult = await _masterApiConnectService.GetMeter(meterUrlUtility).ConfigureAwait(false);

                        if (meterUtilityResult != null)
                        {
                            utiltyType = meterUtilityResult.Data[0].Meter.Model.ServiceResource;
                            var utility = meterTypes.FirstOrDefault(t => t.name.ToUpper().Contains(utiltyType));
                            if (utility != null)

                            {
                                result.Add(new MetersUtilityDto
                                {
                                    MeterNumbers = meterNumber,
                                    MeterTypeId = utility.id,
                                    MeterUtility = utility.name,
                                    UnitOfMeasure = utility.UnitOfMeasure,
                                    MinDailyTarget = double.TryParse(utility.MinDailyTarget, out var minTarget) ? minTarget : 0,
                                    MaxDailyTarget = double.TryParse(utility.MaxDailyTarget, out var maxTarget) ? maxTarget : 0
                                });
                            }
                            else
                            {

                                result.Add(new MetersUtilityDto
                                {
                                    MeterNumbers = meterNumber,
                                    MeterTypeId = 0,
                                    MeterUtility = "",
                                    UnitOfMeasure = "",
                                    MinDailyTarget = 0,
                                    MaxDailyTarget = 0
                                });
                            }
                        }
                    }
                }

            }
            dto.MeterDetails = result;
            return dto;
        }
    }

}
