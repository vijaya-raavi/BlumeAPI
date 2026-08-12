using MediatR;
using Microsoft.Extensions.Configuration;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface.Company;
using Ontec.Core.Domain.Interface.Configuration;
using Ontec.Core.Domain.Models.Dto.Company;
using Ontec.Core.Domain.Requests.Company.Queries;

namespace Ontec.Core.Application.Company.Query
{
    public class GetCompanyDetailsQueryHandler : IRequestHandler<GetCompanyDetailsQuery, CompanyDto>
                                                 , IRequestHandler<GetCompanyMastersQuery, CompanyMasters>

    {
        private readonly ICompanyRepository _companyRepository;
        private readonly ICompanyHelper _companyHelper;
        private readonly IConfigurationRepository _configurationRepository;
        public GetCompanyDetailsQueryHandler(ICompanyRepository companyRepository
                                    , ICompanyHelper companyHelper
                                    , IConfigurationRepository configurationRepository)
        {
            _companyRepository = companyRepository;
            _companyHelper = companyHelper;
            _configurationRepository = configurationRepository;
        }
        public async Task<CompanyDto> Handle(GetCompanyDetailsQuery request, CancellationToken cancellationToken)
        {
            var cDto = new CompanyDto();
            try
            {
                request.TrimAllStrings();
                var commonValidator = new GetCompanyDetailsQueryValidator(_companyRepository);
                var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
                if (!validatorResult.IsValid)
                    throw new ValidationException(validatorResult.Errors);

                var companyHelper = await _companyHelper.GetCompany(request.Id);
                cDto = await _companyRepository.GetCompanyDetails(request.Id).ConfigureAwait(false);
                cDto.CompanyLogoUrl = companyHelper.Domain + cDto.CompanyLogoUrl;
                cDto.BackGroundImageUrl = companyHelper.Domain + cDto.BackGroundImageUrl;

                var configuration = await _configurationRepository.GetNotEditableConfigurations().ConfigureAwait(false);
                if (configuration != null) {
                    var stsConfig = configuration
                                    .Where(t => t.Name.Contains("stsenable", StringComparison.CurrentCultureIgnoreCase))
                                    .FirstOrDefault();

                    var topUpConfig = configuration
                                   .Where(t => t.Name.Contains("ispropertytopup", StringComparison.CurrentCultureIgnoreCase))
                                   .FirstOrDefault();
                    if (stsConfig != null)
                    {
                        string value = stsConfig.Value;
                        if (value=="1")                        
                            cDto.IsSTSEnable = true;
                        else     
                            cDto.IsSTSEnable = false;

                    }
                    if (topUpConfig != null)
                    {
                        string topUpValue = topUpConfig.Value;
                        if (topUpValue == "1")
                            cDto.IsPropertyTopUp = true;
                        else
                            cDto.IsPropertyTopUp = false;
                    }
                    var editableConfiguration = await _configurationRepository.GetConfigurations().ConfigureAwait(false);
                    if (editableConfiguration != null)
                    {
                        var paymentGateway = editableConfiguration.Where(t => t.Name.Contains("paymentgateway", StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();


                        var config = editableConfiguration.Where(t => t.Name.Contains("isbanktransferenable", StringComparison.CurrentCultureIgnoreCase))
                                      .FirstOrDefault();


                        if (config != null)
                        {
                            string value = config.Value;
                            if (value == "1")
                                cDto.IsBankTransferEnable = true;
                            else
                                cDto.IsBankTransferEnable = false;

                        }
                        if (paymentGateway != null)
                        {
                            string paymentGatewayValue = paymentGateway.Value;
                            cDto.PaymentGateWay = paymentGatewayValue;
                            PaymentGatewaysEnum gateway = PaymentGatewaysEnum.LekkaPay;
                            if (paymentGatewayValue == gateway.ToString())
                                cDto.IsLekkaPay = true;
                            else
                                cDto.IsLekkaPay = false;
                        }
                        var enabletrailvendonbanktransfer = editableConfiguration.Where(t => t.Name.Contains("enabletrailvendonbanktransfer", StringComparison.CurrentCultureIgnoreCase))
                                      .FirstOrDefault();

                        if (enabletrailvendonbanktransfer != null)
                        {
                            string value = enabletrailvendonbanktransfer.Value;
                            if (value == "1")
                                cDto.EnableTrailVendOnBankTransfer = true;
                            else
                                cDto.EnableTrailVendOnBankTransfer = false;

                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                return cDto;
            }
            return cDto;


        }
        public async Task<CompanyMasters> Handle(GetCompanyMastersQuery request, CancellationToken cancellationToken)
        {
            var companyMaster = new CompanyMasters();
            var countryList = _companyRepository.GetCountries();
            var stateList = _companyRepository.GetStates();
            var paymentGateWays = _companyRepository.GetPaymentGateWays();

            await Task.WhenAll(countryList, stateList).ConfigureAwait(false);
            companyMaster.CountryList = countryList.Result;
            companyMaster.StateList = stateList.Result;
            companyMaster.PaymentGateWays = paymentGateWays.Result;
            return companyMaster;
        }


    }
}
