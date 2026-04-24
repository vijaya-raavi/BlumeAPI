using MediatR;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Configuration;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Interface.TopUp;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.AdminDashboard;
using Ontec.Core.Domain.Models.Dto.Charts;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Requests;
using Ontec.Core.Domain.Requests.AdminDashboard.Queries;
using Ontec.Core.Domain.Requests.Dashboard.Queries;
using Ontec.Core.Domain.Requests.TopUp.Queries;
using System.Web.Mvc.Html;

namespace Ontec.Core.Application.TopUp.Queries
{
    public class GetAdminDashboardQueryHandler : IRequestHandler<GetPaymentDashboardQuery, PaymentDashboardDto>
                                                , IRequestHandler<GetAdminDashboardMasterQuery, AdminDashboardMastersDto>
                                                , IRequestHandler<GetAdminDashboardRequestQuery, AdminDashboardDto>
                                                
    {

        private readonly ITopUpRepository _topupRepository;
        private readonly IUserRepository _userRepository;
        private readonly IWorkContext _workContext;
        private readonly IMeterRepository _meterRepository;
        private readonly IConfigurationRepository _configurationRepository;
        public GetAdminDashboardQueryHandler(ITopUpRepository topupRepository,
                                        IUserRepository userRepository,
                                        IWorkContext workContext,
                                        IMeterRepository meterRepository,
                                        IConfigurationRepository configurationRepository)
        {
            _topupRepository = topupRepository;
            _userRepository = userRepository;
            _workContext = workContext;
            _meterRepository = meterRepository;
            _configurationRepository = configurationRepository;
        }


        public async Task<PaymentDashboardDto> Handle(GetPaymentDashboardQuery request, CancellationToken cancellationToken)
        {
            
            return await _topupRepository.GetPaymentDashboard(request.EstateId).ConfigureAwait(false);
        }
        public async Task<AdminDashboardMastersDto> Handle(GetAdminDashboardMasterQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new GetAdminDashboardMasterQueryValidator(_userRepository, _workContext);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            var enumList = EnumHelper.GetSelectList(typeof(DashboardPeriod));

            AdminDashboardMastersDto masterDto = new();
            var periodList = new List<OntecSelectListItem>();
            foreach (var period in enumList.ToList())
            {
                periodList.Add(new OntecSelectListItem
                {
                    Id = int.Parse(period.Value),
                    Name = period.Text,
                    //OtherText=period.
                });
            }
            masterDto.Period = periodList;
            bool isAdmin = false;

            if (_workContext.CurrentRoleId == (int)RoleMasterEnum.Admin || _workContext.CurrentRoleId == (int)RoleMasterEnum.Operator)
            {
                isAdmin = true;
            }
            masterDto.UtilityTypes = await _meterRepository.GetMeterTypes().ConfigureAwait(false);

            return masterDto;
        }

        public async Task<AdminDashboardDto> Handle(GetAdminDashboardRequestQuery request, CancellationToken cancellationToken)
        {
          
            var months = Enumerable.Range(0, 12)
                                        .Select(offset => DateTime.UtcNow.AddMonths(-offset))
                                        .Select(date => date.ToString("MMM"))
                                        .Reverse()
                                        .ToList();
            var amount = new List<double>()
            {
               0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0
            };

            var lineChartDto = new LineChartDto
            {
                XAxisdata = months,
                SeriesLineData = amount
            };
            var adminDashboardDto = new AdminDashboardDto();
            adminDashboardDto = await _topupRepository.GetAdminDashboard().ConfigureAwait(false);
            var paymenthMethodSummaries = await _topupRepository.GetPaymenthMethodSummaries().ConfigureAwait(false);
            var getpayments = new GetPaymentMethodsQuery()
            {
                StatusId = 0
            };
            var defaultPaymentMethods = await _topupRepository.GetPaymentMethods(getpayments).ConfigureAwait(false);
            var paymentMethodAmountList = new List<PaymentMethodAmount>();
            if (paymenthMethodSummaries.Any())
            {
                var monthlyAmounts = paymenthMethodSummaries
                                        .GroupBy(d => d.PaymentMethod)
                                        .Select(group => new
                                        {
                                            PaymentMethod = group.Key,
                                            MonthAmounts = months.ToDictionary(
                                                month => month,
                                                month => group.Where(g => g.Month.Trim() == month)
                                                .Sum(g => g.Amount))
                                        }).ToList();

                if (monthlyAmounts.Any() && monthlyAmounts.Count() > 0)
                {
                    foreach (var item in monthlyAmounts)
                    {
                        paymentMethodAmountList.Add(new PaymentMethodAmount
                        {
                            PaymentMethod = item.PaymentMethod,
                            MonthlWiseAmount = new LineChartDto
                            {
                                XAxisdata = item.MonthAmounts.Keys.ToList(),
                                SeriesLineData = item.MonthAmounts.Values.ToList()
                            }
                        });
                    }
                }
                else
                {

                    foreach (var item in defaultPaymentMethods)
                    {
                        paymentMethodAmountList.Add(new PaymentMethodAmount
                        {
                            PaymentMethod = item.Name,
                            MonthlWiseAmount = lineChartDto
                        });
                    }
                }
            }

            var configurations = await _configurationRepository.GetConfigurations().ConfigureAwait(false);
            
            if (configurations != null && configurations.Any(t => t.Name.ToLower().Equals("isdashboardbargraph")))
            {
                bool isDashboardBargraphValue = false;
                var isdashboardbargraph = configurations.FirstOrDefault(t => t.Name.ToLower().Equals("isdashboardbargraph"));
                if (isdashboardbargraph != null && !string.IsNullOrEmpty(isdashboardbargraph.Value))
                {

                    if (isdashboardbargraph.Value == "0")
                    {
                        isDashboardBargraphValue = false;
                    }
                    else
                    {
                        isDashboardBargraphValue = true;
                    }
                    adminDashboardDto.IsBarGraph = isDashboardBargraphValue;
                }

            }
            adminDashboardDto.MonthlyAmount = paymentMethodAmountList;
            

            return adminDashboardDto;
        }
    }
}
