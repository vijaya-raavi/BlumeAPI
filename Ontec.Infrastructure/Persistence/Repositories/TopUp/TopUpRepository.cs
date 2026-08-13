using System.Globalization;
using System.Text;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Company;
using Ontec.Core.Domain.Interface.EmailTemplate;
using Ontec.Core.Domain.Interface.TopUp;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.AdminDashboard;
using Ontec.Core.Domain.Models.Dto.Debitech;
using Ontec.Core.Domain.Models.Dto.TopUp;
using Ontec.Core.Domain.Requests.Dashboard.Command;
using Ontec.Core.Domain.Requests.Debitech.Command;
using Ontec.Core.Domain.Requests.Debitech.Queries;
using Ontec.Core.Domain.Requests.TopUp.Command;
using Ontec.Core.Domain.Requests.TopUp.Queries;
using Scriban;
using SelectPdf;

//using SelectPdf;


namespace Ontec.Infrastructure.Persistence.Repositories.TopUp
{
    public class TopUpRepository(IGenericRepository genericRepository,
                IHttpContextAccessor httpContextAccessor,
                IHostingEnvironment environment,
                IUserRepository userRepository,
                IOtpService otpService,
                ICompanyRepository companyRepository,
                IWorkContext workContext,
                ICompanyHelper companyHelper,
                IEmailTemplateRepository emailTemplateRepository) : ITopUpRepository

    {
        private readonly IGenericRepository _genericRepository = genericRepository;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IHostingEnvironment _environment = environment;
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IOtpService _otpService = otpService;
        private readonly ICompanyRepository _companyRepository = companyRepository;
        private readonly ICompanyHelper _companyHelper = companyHelper;
        private readonly IWorkContext _workContext = workContext;
        private readonly IEmailTemplateRepository _emailTemplateRepository = emailTemplateRepository;

        #region TopUpTransaaction

        public async Task<int> IsTransactionNoExist(string transactionId)
        {
            var sQuery = @"SELECT id  FROM  public.ohd_top_up_transactions 
                        WHERE transaction_id=@TransactionId";
            var parameters = new DynamicParameters();
            parameters.Add("@TransactionId", transactionId);
            int id = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return id;
        }

        public async Task<int> IsNetUpTransactionGuidExist(string transactionReferenceNo)
        {
            var sQuery = @"SELECT id  FROM  public.ohd_top_up_transactions 
                        WHERE net_up_transaction=@TransactionReferenceNo";
            var parameters = new DynamicParameters();
            parameters.Add("@TransactionReferenceNo", transactionReferenceNo);
            var id = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return id;
        }
        public async Task<int> IsBankTransactionIdExist(string bankTransactionId)
        {
            var sQuery = @"SELECT id  FROM  public.ohd_top_up_transactions 
                        WHERE bank_transaction_id=@BankTransactionId";
            var parameters = new DynamicParameters();
            parameters.Add("@BankTransactionId", bankTransactionId);
            var id = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return id;
        }
        public async Task<int> AddTopupTransactions(AddTopUpTransactionQuery request,
                                                string transactionId,
                                                double transactionFee,
                                                double topUpAmount,
                                                double WalletAmountUsed,
                                                double FinalAmountToPay,
                                                string currentPaymentGateWay)
        {

            var sQuery = @" INSERT INTO public.ohd_top_up_transactions(
	                       transaction_id
                            , amount
                            , user_id
                            , use_wallet
                            , meter_id
                            , transaction_fee
                            , recharge_amount
                            , created_at
                            ,  flag
                            ,payment_method_id
                            ,wallet_amount_used
                            ,final_amount_to_pay
                           -- ,ipaymethod_id
                           , is_in_house_txn
                            ,payment_gateway)
	                        VALUES (
                            @TransactionId
                            ,@Amount
                            , @UserId
                            ,@UseWallet
                            , @MeterId
                            , @TransactionFee
                            , @TopUpAmount
                            , @Created_at
                            , @flag
                            ,@PaymentMethodId
                            ,@WalletAmountUsed
                            ,@FinalAmountToPay
                          --  , @IPayMethodId
                                ,@IsInHouseTxn
                                ,@PaymentGateway)
                            RETURNING lastval()";
            var parameters = new DynamicParameters();
            parameters.Add("@TransactionId", transactionId);
            parameters.Add("@Amount", request.Amount);
            parameters.Add("@UserId", request.UserId);
            parameters.Add("@UseWallet", request.UseWallet ? 1 : 0);
            parameters.Add("@MeterId", request.MeterId);
            parameters.Add("@TransactionFee", transactionFee);
            parameters.Add("@TopUpAmount", topUpAmount);
            parameters.Add("@Created_at", DateTime.UtcNow);
            parameters.Add("@flag", (int)PaymentStatus.InProcess);
            parameters.Add("@PaymentMethodId", request.PaymentMethodId);
            parameters.Add("@WalletAmountUsed", WalletAmountUsed);
            parameters.Add("@FinalAmountToPay", FinalAmountToPay);
            parameters.Add("@IsInHouseTxn", true);
            parameters.Add("@PaymentGateway", currentPaymentGateWay);
            // parameters.Add("@IPayMethodId", request.IPayMethod);
            try
            {
                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }

        }

        public async Task<int> UpdateTopupTransactions(PayFastModel request)
        {

            var txn = await GetTopupTransactionDetails(request.m_payment_id).ConfigureAwait(false);
            var user = await _userRepository.GetUserById(txn.UserId).ConfigureAwait(false);
            var companyDetails = await _companyHelper.GetCompany(user.CompanyId).ConfigureAwait(false);
            LogToFile(companyDetails.WWWPath, "In update topup transactions");

            var sQuery = @" UPDATE public.ohd_top_up_transactions
	                        SET pf_response=@Response
                            ,flag=@Flag
                            ,modified_at=@ModifiedAt
                            WHERE transaction_id=@TransactionId;
                            SELECT id FROM  ohd_top_up_transactions
                            WHERE transaction_id=@TransactionId;";
            var parameters = new DynamicParameters();

            parameters.Add("@TransactionId", request.m_payment_id);
            if (request.payment_status == "COMPLETE")
            {
                parameters.Add("@Flag", (int)PaymentStatus.Complete);
            }
            if (request.payment_status == "CACNCELLED")
            {
                parameters.Add("@Flag", (int)PaymentStatus.Cancelled);
            }

            parameters.Add("@Response", JsonConvert.SerializeObject(request));

            parameters.Add("@ModifiedAt", DateTime.UtcNow);
            try
            {
                LogToFile(companyDetails.WWWPath, "In update topup transactions");
                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                LogToFile(companyDetails.WWWPath, "after  update topup transaction:" + result);
                return result;
            }
            catch (Exception ex)
            {
                LogToFile(companyDetails.WWWPath, ex.Message);
                return 0;

            }

        }
        public async Task<int> UpdateWalletTopupTransactions(WalletTopUpModel request)
        {

            var sQuery = @" UPDATE public.ohd_top_up_transactions
	                        SET pf_response=@Response
                            ,flag=@Flag
                            ,modified_at=@ModifiedAt
                            WHERE transaction_id=@TransactionId;
                            SELECT id FROM  ohd_top_up_transactions
                            WHERE transaction_id=@TransactionId;";
            var parameters = new DynamicParameters();

            parameters.Add("@TransactionId", request.m_payment_id);
            if (request.payment_status == "COMPLETE")
            {
                parameters.Add("@Flag", (int)PaymentStatus.Complete);
            }
            if (request.payment_status == "CACNCELLED")
            {
                parameters.Add("@Flag", (int)PaymentStatus.Cancelled);
            }

            parameters.Add("@Response", JsonConvert.SerializeObject(request));

            parameters.Add("@ModifiedAt", DateTime.UtcNow);
            try
            {

                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);

                return result;
            }
            catch (Exception ex)
            {
                return 0;

            }

        }

        public async Task CancelTopUpTransaction(CancelTopUpTransactionQuery request)
        {
            var sQuery = @" UPDATE public.ohd_top_up_transactions
	                        SET flag=@Flag
                            ,modified_at=@ModifiedAt
                            WHERE transaction_id=@TransactionId;
                            SELECT id FROM  ohd_top_up_transactions
                            WHERE transaction_id=@TransactionId;";
            var parameters = new DynamicParameters();

            parameters.Add("@TransactionId", request.TransactionId);
            parameters.Add("@Flag", (int)PaymentStatus.Cancelled);
            parameters.Add("@ModifiedAt", DateTime.UtcNow);
            try
            {
                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw (ex);

            }
        }
        public async Task<AddTopUpTransactionsDto> GetTopUpTransaction(int Id)
        {
            var sQuery = @"SELECT tut.transaction_id AS TransactionID,
                        tut.amount AS Amount, 
                        tut.user_id AS UserId,
                        tut.use_wallet AS UseWallet,
                        tut.meter_id As MeterId, 
                        tut.transaction_fee AS TransactionFee,
                        tut.recharge_amount AS RechargeAmount,
                        tut.created_at AS CreatedAt,
						mt.meter_number AS MeterNumber,
                        tut.wallet_amount_used AS WalletAmountUsed,
                        tut.final_amount_to_pay AS FinalAmountToPay,
                        tut.trial_vend_response  AS TrailVendResponseJson
                        FROM ohd_top_up_transactions AS tut
						LEFT JOIN public.ohd_meter AS mt ON tut.meter_id =mt.id
                        WHERE tut.id=@Id;";
            var parameters = new DynamicParameters();
            parameters.Add("@Id", Id);

            var result = await _genericRepository.GetFirstOrDefaultAsync<AddTopUpTransactionsDto>(sQuery, parameters).ConfigureAwait(false);
            return result;

        }
        public async Task<DatatableModel<GetTopUpTransaction>> GetTopupTransactions(GetTopUpTransactionsQuery request)
        {
            string whereclause = "";
            var dt = new DatatableModel<GetTopUpTransaction>()
            {
                Page = request.Page,
                PageSize = request.PageSize
            };
            try
            {
                var sQuery = @"SELECT  tut.transaction_id AS TransactionId,
                               mt.meter_number AS MeterNumber,
                                mt.eft_number AS EFTNumber,	  
                                mt.meter_master_type AS MasterMeterType,
                                 mrt.unitofmeasure AS UnitOfMeasure,
                                mt.meter_type_id AS MeterTypeId,
                                 mrt.display_value AS MeterType,
                                mt.id AS MeterId,
                                tut.amount AS Amount, 
                                tut.user_id As UserId, 
                                tut.use_wallet AS UseWallet, 
                                tut.meter_id AS MeterId, 
                                tut.transaction_fee AS TransactionFee, 
                               -- (tut.recharge_amount-debt_amount) AS RechargeAmount,
                                tut.recharge_amount AS RechargeAmount,
                                tut.debt_amount as DebtAmount,                                
                               to_char(tut.created_at,'dd-MM-yyyy  HH24:MI:ss') AS   CreatedAt, 
                                tut.modified_at AS ModifiedAt, 
                                CASE WHEN tut.flag =@Complete THEN 'Completed'  END As Flag ,
                                CASE WHEN tut.flag =@Refunded THEN 'Refunded' END As Flag, 
                                CASE WHEN tut.flag =@Revoked THEN 'Revoked'  END As Flag ,
                                CASE WHEN tut.flag =@Pending THEN 'Pending'  END As Flag ,
                                CASE WHEN tut.flag =@Failed THEN 'Failed'  END As Flag ,
                                CASE WHEN tut.flag =@OnHold THEN 'OnHold'  END As Flag ,
                                CASE WHEN tut.flag =@Abandoned THEN 'Abandoned'  END As Flag ,
                                CASE WHEN tut.flag =@Preapproved THEN 'Preapproved'  END As Flag ,
                                CASE WHEN tut.flag =@Cancelled THEN 'Cancelled' END As Flag, 
                                CASE WHEN tut.flag =@Inprocess THEN 'Inprocess'  END As Flag ,
                                tut.pf_response AS PayFastResponse,
                               tut.topup_status AS TopupStatus,
                               tut.vend_response AS VendResponse,
                               tut.std_token AS StdToken,
                                tut.bsst_token AS BsstToken,
                                tut.key_change_token AS KeyChangeToken,
                                uw.balance AS WalletBalance,
                                tut.wallet_amount_used As WalletAmountUsed,
                                tut.final_amount_to_pay AS FinalAmountToPay,
                                p.name AS Property,
								p.unit_number AS UnitNumber,
                                --e.estate As Estate,
                                pm.display_name as PaymentMethod,
                                tut.receipt_number AS ReceiptNumber,
                                tut.is_in_house_txn AS IsInHouseTransaction ,
                                 tut.eft_ref_no AS EFTRefNo
	                    FROM public.ohd_top_up_transactions as tut
						LEFT JOIN ohd_meter AS mt ON  mt.id=tut.meter_id
                         LEFT JOIN public.ohd_property as p ON mt.property_id=p.id
                        LEFT JOIN public.ohd_user_wallet AS uw ON tut.user_id=uw.user_id
                        --LEFT JOIN public.ohd_estate as e ON p.estate_id=e.id
                        LEFT JOIN public.ohd_payment_methods as pm ON tut.payment_method_id=pm.id
                        WHERE tut.vend_response is not null";

                var parameters = new DynamicParameters();
                parameters.Add("@Complete", (int)PaymentStatus.Complete);
                parameters.Add("@Refunded", (int)PaymentStatus.Refunded);
                parameters.Add("@Revoked", (int)PaymentStatus.Revoked);
                parameters.Add("@Pending", (int)PaymentStatus.Pending);
                parameters.Add("@Failed", (int)PaymentStatus.Failed);
                parameters.Add("@OnHold", (int)PaymentStatus.OnHold);
                parameters.Add("@Abandoned", (int)PaymentStatus.Abandoned);
                parameters.Add("@Preapproved", (int)PaymentStatus.Preapproved);
                parameters.Add("@Inprocess", (int)PaymentStatus.InProcess);
                parameters.Add("@Cancelled", (int)PaymentStatus.Cancelled);
                if (request.TransactionId != null && request.TransactionId != "string" && request.TransactionId != "")
                {
                    whereclause += " AND tut.transaction_id=@TransactionId";
                    parameters.Add("@TransactionId", request.TransactionId);
                }
                if ((request.TransactionId == null || request.TransactionId == "string" || request.TransactionId == "") && request.UserId != null && request.UserId != 0)
                {
                    whereclause += " AND  tut.user_id=@UserId";
                    parameters.Add("@UserId", request.UserId);
                }
                if (!string.IsNullOrEmpty(request.SearchText))
                {
                    var searchText = request.SearchText.Trim();
                    var searchTerms = searchText.ToLower().Split(' ');
                    var searchConditions = new List<string>();
                    var index = 0;

                    // Add condition for the full search text
                    var fullSearchTextParam = "@SearchTextFull";
                    searchConditions.Add($@"(lower(p.name) like {fullSearchTextParam}
                           OR lower(p.unit_number) like {fullSearchTextParam}
                          --  OR lower(e.estate) like {fullSearchTextParam}
                            OR lower(mt.meter_number) like {fullSearchTextParam}
                           )");
                    parameters.Add(fullSearchTextParam, "%" + searchText.ToLower() + "%");


                    // Add conditions for each split term
                    foreach (var term in searchTerms)
                    {
                        var paramName = "@SearchText" + index;
                        searchConditions.Add($@"(lower(p.name) like {paramName}
                               OR lower(p.unit_number) like {paramName}
                             --   OR lower(e.estate) like {paramName}
                                OR lower(mt.meter_number) like {paramName}
                           )");
                        parameters.Add(paramName, "%" + term + "%");
                        index++;
                    }


                    if (whereclause != "" && searchConditions.Any())
                    {
                        whereclause += " AND (" + string.Join(" OR ", searchConditions) + ")";
                    }
                    if (whereclause == "" && searchConditions.Any())
                    {
                        whereclause += " WHERE (" + string.Join(" OR ", searchConditions) + ")";
                    }

                }
                sQuery += whereclause;
                sQuery += " Order By tut.id desc";
                var topuptransactions = await _genericRepository.GetAsync<GetTopUpTransaction>(sQuery, parameters).ConfigureAwait(false);
                var result = topuptransactions.ToList().Skip(request.Page * request.PageSize).Take(request.PageSize);
                dt.Data = result.ToList();
                dt.TotalRecords = topuptransactions.Count();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return dt;
        }





        public async Task<GetTopUpTransaction> GetTopupTransactionDetails(string transactionNo)
        {

            try
            {
                var sQuery = @"SELECT tut.id AS Id,  tut.transaction_id AS TransactionId,
                        mt.meter_number AS MeterNumber, 
                        tut.amount AS Amount, 
                        tut.user_id As UserId, 
                        tut.use_wallet AS UseWallet, 
                        tut.meter_id AS MeterId, 
                        tut.transaction_fee AS TransactionFee,
                        tut.recharge_amount AS RechargeAmount,
                        tut.payment_method_id AS PaymentMethodId,
                        tut.created_at AS CreatedAt, 
                        tut.modified_at AS ModifiedAt,
                        tut.flag As Flag,
                        tut.std_token AS StdToken,
                        tut.mrktmsg AS mrktMsg,
                        tut.customermsg  AS customerMsg,
                        tut.topup_status AS TopupStatus,
                        tut.vend_response AS VendResponse,
                        tut.receipt_number AS ReceiptNumber,
                        tut.debt_amount as Debt,
                        tut.pf_response as PayFastResponse
	                    FROM public.ohd_top_up_transactions as tut
						LEFT JOIN ohd_meter AS mt ON  mt.id=tut.meter_id 
                        WHERE transaction_id=@TransactionId";
                var parameters = new DynamicParameters();
                parameters.Add("@TransactionId", transactionNo);
                var transaction = await _genericRepository.GetFirstOrDefaultAsync<GetTopUpTransaction>(sQuery, parameters).ConfigureAwait(false);
                return transaction;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public async Task<DatatableModel<UserPaymentsDto>> GetUserPayments(GetUserPayamenstQuery request)
        {

            var dt = new DatatableModel<UserPaymentsDto>()
            {
                Page = request.Page,
                PageSize = request.PageSize
            };

            if (string.IsNullOrEmpty(request.order) || (request.order == "string"))
                request.order = "desc";
            if (string.IsNullOrEmpty(request.sort) || (request.order == "string"))
                request.sort = "ttd.created_at";
            var sortQuery = "";

            if (string.IsNullOrEmpty(request.sort))
            {
                request.sort = "Consumer";
            }
            switch (request.sort.ToLower())
            {
                case "Date":
                    request.sort = " ttd.created_at ";
                    break;
                case "Consumer":
                    request.sort = " ur.first_name , ur.last_name ";
                    break;
                case "Email":
                    request.sort = " ur.email ";
                    break;

                case "PropertyRelation":
                    request.sort = "  purid.name ";
                    break;

                case "Phone":
                    request.sort = " ur.mobile ";
                    break;

                case "MeterNumber":
                    request.sort = " mt.meter_number ";
                    break;
                case "MeterType":
                    request.sort = " emt.name ";
                    break;
                case "TopUp":
                    request.sort = " ttd.amount ";
                    break;
                case "Mode":
                    request.sort = " pm.display_name ";
                    break;


            }
            if (!string.IsNullOrEmpty(request.sort) && (request.sort != "string"))
            {
                sortQuery += " order by " + request.sort + " " + request.order;
            }
            try
            {
                var sQuery = @"SELECT 
                                ttd.transaction_id,
                                concat(ur.first_name,' ',ur.last_name) AS Consumer,
                                (CASE WHEN purid.name is null THEN 'Owner' ELSE purid.name END) AS PropertyRelation,
                                 ur.Email
                                ,ur.Mobile As Phone
                                ,mt.meter_number AS MeterNumber
								,p.unit_number AS UnitNumber
								,p.name AS Property
                                ,emt.name As MeterType
                                ,ttd.amount AS TopUp
                                ,to_char(ttd.created_at,'dd-MM-yyyy  HH24:MI:ss') AS Date
                                  ,ttd.topup_status AS TopupStatus
                                ,pm.display_name AS Mode
                               -- , e.estate As Estate
                                ,ttd.is_in_house_txn AS IsInHouseTransaction 
                                  , ttd.eft_ref_no AS EFTRefNo
	                                FROM public.ohd_top_up_transactions as ttd
	                                join public.ohd_meter AS mt on ttd.meter_id=mt.id
									LEFT JOIN public.ohd_property AS p on mt.property_id=p.id
                                    -- LEFT JOIN public.ohd_estate as e ON p.estate_id=e.id
									LEFT JOIN public.ohd_enum_meter_type AS emt ON emt.id=mt.meter_type_id
 	                                LEFT JOIN (Select epur.name,pur.property_id,pur.user_id from public.ohd_property_user_relation as pur
	                                LEFT JOIN public.ohd_enum_user_relation as epur on pur.user_relation_id=epur.id
	                                  ) as purid
	                                on  purid.property_id=mt.property_id and purid.user_id=ttd.user_id
	                                JOIN ohd_user AS ur ON ttd.user_id=ur.id 
	                                JOIN public.ohd_payment_methods AS pm ON ttd.payment_method_id=pm.id
                                    WHERE ttd.flag=@flag AND 
                                    ttd.vend_response is not null";


                var parameters = new DynamicParameters();

                if (request.EstateId > 0)
                {
                    sQuery += " AND p.estate_id=@EstateId";
                }
                if (!string.IsNullOrEmpty(request.SearchText.Trim()) && (request.SearchText.Trim() != "string"))
                {
                    var searchText = request.SearchText.Trim();
                    var searchTerms = searchText.ToLower().Split(' ');
                    var searchConditions = new List<string>();
                    var index = 0;

                    // Add condition for the full search text
                    var fullSearchTextParam = "@SearchTextFull";
                    searchConditions.Add($@"(lower(p.name) like {fullSearchTextParam}
                           OR lower(p.unit_number) like {fullSearchTextParam}
                            OR lower(ur.first_name) like {fullSearchTextParam}
                            OR lower(ur.last_name) like {fullSearchTextParam}
                           OR lower(ur.mobile) like {fullSearchTextParam}
                           OR lower(ur.email) like {fullSearchTextParam}
                            OR lower(mt.meter_number) like {fullSearchTextParam}
                           -- OR lower(e.estate ) like {fullSearchTextParam}
                           )");
                    parameters.Add(fullSearchTextParam, "%" + searchText.ToLower() + "%");


                    // Add conditions for each split term
                    foreach (var term in searchTerms)
                    {
                        var paramName = "@SearchText" + index;
                        searchConditions.Add($@"(lower(p.name) like {paramName}
                               OR lower(p.unit_number) like {paramName}
                            OR lower(ur.first_name ) like {paramName}
                            OR lower(ur.last_name) like {paramName}
                               OR lower(ur.mobile) like {paramName}
                               OR lower(ur.email) like {paramName}
                                OR lower(mt.meter_number) like {paramName}
                                  --  OR lower(e.estate ) like {paramName}
                                )");
                        parameters.Add(paramName, "%" + term + "%");
                        index++;
                    }


                    if (searchConditions.Any())
                    {
                        sQuery += " AND (" + string.Join(" OR ", searchConditions) + ")";
                    }
                }


                sQuery += sortQuery;

                parameters.Add("@flag", (int)PaymentStatus.Complete);
                parameters.Add("@EstateId", request.EstateId);
                var userPayments = await _genericRepository.GetAsync<UserPaymentsDto>(sQuery, parameters).ConfigureAwait(false);
                var result = userPayments.ToList().Skip(request.Page * request.PageSize).Take(request.PageSize);
                dt.Data = result.ToList();
                dt.TotalRecords = userPayments.Count();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return dt;
        }
        public async Task UpdateTransactionStatus(string status, int flag, string data, int id, string token,
                                                 string keyChangeToken, string bsstToken, string mrktMsg, string customerMsg, string RctNum, string tarrif, string vendReference)
        {
            var txn = await GetTopUpTransaction(id).ConfigureAwait(false);
            var user = await _userRepository.GetUserById(txn.UserId).ConfigureAwait(false);
            var companyDetails = await _companyHelper.GetCompany(user.CompanyId).ConfigureAwait(false);
            LogToFile(companyDetails.WWWPath, "In Update top up Status");

            var sQuery = @"UPDATE public.ohd_top_up_transactions
                          SET topup_status=@Status ,
                                flag=@Flag, vend_response=@Data ,
                                std_token=@Token,
                                key_change_token=@keyChangeToken,
                                bsst_token=@bsstToken,
                                mrktmsg=@mrktMsg,
                                customermsg=@customerMsg,
                                receipt_number=@RctNum,
                                tarrif=@Tariff,
                                vend_req_ref=@VendRef
                          WHERE id=@Id";

            var parameters = new DynamicParameters();
            parameters.Add("@Status", status);
            parameters.Add("@Flag", flag);
            parameters.Add("@Data", data);
            parameters.Add("@Token", token);
            parameters.Add("@keyChangeToken", keyChangeToken);
            parameters.Add("mrktMsg", mrktMsg);
            parameters.Add("customermsg", customerMsg);
            parameters.Add("@bsstToken", bsstToken);
            parameters.Add("@Id", id);
            parameters.Add("@RctNum", RctNum);
            parameters.Add("@Tariff", tarrif);
            parameters.Add("@VendRef", vendReference);
            try
            {
                await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                LogToFile(companyDetails.WWWPath, "after Update top up Status");
            }
            catch (Exception ex)
            {
                LogToFile(companyDetails.WWWPath, ex.ToString());
            }
        }
        public async Task UpdateTrailTransactionStatus(string status, int flag, string data, int id, double debt)
        {
            var sQuery = @"UPDATE public.ohd_top_up_transactions
                          SET topup_status=@Status
                            ,flag=@Flag
                            ,trial_vend_response=@Data
                            ,debt_amount=@debtAmmount
                          WHERE id=@Id";

            var parameters = new DynamicParameters();
            parameters.Add("@Status", status);
            parameters.Add("@Flag", flag);
            parameters.Add("@Data", data);
            parameters.Add("@debtAmmount", debt);
            parameters.Add("@Id", id);
            try
            {
                await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            }
            catch (Exception ex)
            {

            }
        }
        public async Task UpdateTransactionUseWallet(int id)
        {
            var sQuery = @"UPDATE public.ohd_top_up_transactions
                          SET use_wallet=1
                          WHERE id=@Id";

            var parameters = new DynamicParameters();

            parameters.Add("@Id", id);
            try
            {
                await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            }
            catch (Exception ex)
            {

            }
        }
        #endregion
        #region BankAccount
        public async Task<int> IsBankAccountExist(string accountNumber, int Id)
        {
            var sQuery = @"SELECT Id
                            FROM ohd_bank_accounts  
                          WHERE account_number=@AccountNumber AND id!=@Id";
            var parameters = new DynamicParameters();
            parameters.Add("@AccountNumber", accountNumber);
            parameters.Add("@Id", Id);
            int result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task<int> AddBankAccount(AddOrUpdateBankAccountQuery request)
        {
            var sQuery = @" INSERT INTO public.ohd_bank_accounts(
                            bank_name
	                        ,account_name
                            , account_number
                            , branch_code
                            , created_at
                            ,status_id
                            )
	                        VALUES(
                            @BankName
                            ,@AccountName
                            ,@AccountNumber
                            ,@BranchCode
                            , @Created_at
                             ,@StatusId)
                            RETURNING lastval()";
            var parameters = new DynamicParameters();
            parameters.Add("@BankName", request.BankName);
            parameters.Add("@AccountName", request.AccountName);
            parameters.Add("@AccountNumber", request.AccountNumber);
            parameters.Add("@BranchCode", request.BranchCode);
            parameters.Add("@StatusId", (int)StatusEnum.Active);
            parameters.Add("@Created_at", DateTime.UtcNow);

            try
            {
                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }

        }
        public async Task<int> UpdateBankAccount(AddOrUpdateBankAccountQuery request)
        {
            var sQuery = @" UPDATE ohd_bank_accounts
                            SET 
                                bank_name=@BankName
                                ,account_name = @AccountName
                               ,account_number = @AccountNumber
                               ,branch_code = @BranchCode 
                               ,status_id=@Status
                               ,modified_at = @ModifiedAt
                             WHERE id = @Id ;
                            Select Id From ohd_bank_accounts
                             WHERE id = @Id ";
            var parameters = new DynamicParameters();
            parameters.Add("@Id", request.Id);
            parameters.Add("@BankName", request.BankName);
            parameters.Add("@AccountName", request.AccountName);
            parameters.Add("@AccountNumber", request.AccountNumber);
            parameters.Add("@BranchCode", request.BranchCode);
            parameters.Add("@Status", request.Status ? (int)StatusEnum.Active : (int)StatusEnum.Inactive);
            parameters.Add("@ModifiedAt", DateTime.UtcNow);

            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task DeleteBankAccountById(int Id)
        {
            var sQuery = @"UPDATE public.ohd_bank_accounts
                         SET status_id=@StatusId, modified_at = @ModifiedAt
                         WHERE Id =@Id";

            var parameters = new DynamicParameters();

            parameters.Add("@Id", Id);
            parameters.Add("@ModifiedAt", DateTime.UtcNow);
            parameters.Add("@StatusId", (int)StatusEnum.Inactive);

            await _genericRepository.ExecuteScalarAsync(sQuery, parameters).ConfigureAwait(false);
        }
        public async Task<IEnumerable<BankAccountDto>> GetBankAccounts(GetBankAccountsQuery request)
        {
            var sQuery = @"SELECT id
                            ,bank_name AS BankName                         
                            ,account_name AS AccountName 
                         ,account_number AS AccountNumber
                         ,branch_code AS BranchCode  
                         ,CASE WHEN status_id = 1 THEN 'Active' ELSE 'InActive' END  AS Status
                        ,created_at As CreatedAt
                        ,modified_at As ModifiedAt    
                        FROM public.ohd_bank_accounts
                        WHERE status_id = @StatusId";



            var parameter = new DynamicParameters();
            if (request.Id > 0)
            {
                sQuery += " AND id=@Id";
                parameter.Add("@Id", request.Id);
            }
            parameter.Add("@StatusId", (int)StatusEnum.Active);
            var bankAccounts = await _genericRepository.GetAsync<BankAccountDto>(sQuery, parameter).ConfigureAwait(false);
            return bankAccounts;
        }
        public async Task<bool> IsBankAccountIdExist(int id)
        {
            var sQuery = @"SELECT count(*)
                            FROM ohd_bank_accounts  
                          WHERE id=@Id and status_id != @StatusId";
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            parameters.Add("@StatusId", (int)StatusEnum.Inactive);

            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result > 0;
        }
        #endregion

        #region PaymentMethods
        public async Task<IEnumerable<PaymentMethodsDto>> GetPaymentMethods(GetPaymentMethodsQuery request)
        {
            var sQuery = @"SELECT id As Id,
                                name as Name,
                                display_name AS DisplayName,
                                slug As Slug,
                                 lekkaPay_slug AS LekkaPaySlug,
                                percentage As Percentage, 
                                discount As Discount, 
                                CASE WHEN status_id =1 THEN true ELSE false END As StatusId,  
                                create_at As CreatedAt, 
                                modified_at As ModifiedAt, 
                                modifed_by As ModifiedBy
                                FROM public.ohd_payment_methods";

            var parameters = new DynamicParameters();
            if (request.StatusId == 1)
            {
                sQuery += @" WHERE status_id=@StatusId;";
                parameters.Add("@StatusId", request.StatusId);
            }
            if (request.StatusId == 0)
            {
                sQuery += @" WHERE status_id in (@Active,@Inactive);";
                parameters.Add("@Active", (int)StatusEnum.Active);
                parameters.Add("@Inactive", (int)StatusEnum.Inactive);
            }
            var paymentMethods = await _genericRepository.GetAsync<PaymentMethodsDto>(sQuery, parameters).ConfigureAwait(false);
            return paymentMethods;

        }


        public async Task<IEnumerable<PaymentMethodsDto>> GetAllPaymentMethods()
        {
            var sQuery = @"SELECT id As Id,
                                name as Name,
                                display_name AS DisplayName,
                                slug As Slug,
                                 lekkaPay_slug AS LekkaPaySlug,
                                percentage As Percentage, 
                                discount As Discount, 
                                CASE WHEN status_id =1 THEN true ELSE false END As StatusId,  
                                create_at As CreatedAt, 
                                modified_at As ModifiedAt, 
                                modifed_by As ModifiedBy
                                FROM public.ohd_payment_methods";

            var parameters = new DynamicParameters();
           
            var paymentMethods = await _genericRepository.GetAsync<PaymentMethodsDto>(sQuery, parameters).ConfigureAwait(false);
            return paymentMethods;

        }
        public async Task<IEnumerable<PaymentMethodsDto>> GetDebitechPaymentMethods(GetPaymentMethodsQuery request)
        {
            var sQuery = @"SELECT id As Id,
                                name as Name,
                                display_name AS DisplayName,
                                slug As Slug,
                                percentage As Percentage, 
                                discount As Discount, 
                                CASE WHEN status_id =1 THEN true ELSE false END As StatusId,  
                                create_at As CreatedAt, 
                                modified_at As ModifiedAt, 
                                modifed_by As ModifiedBy
                                FROM public.ohd_payment_methods";

            var parameters = new DynamicParameters();

            sQuery += @" WHERE status_id in (0);";

            var paymentMethods = await _genericRepository.GetAsync<PaymentMethodsDto>(sQuery, parameters).ConfigureAwait(false);
            return paymentMethods;

        }
        public async Task UpdatePaymentMethods(UpdatePaymentMethodsQuery request, int UserId)
        {
            try
            {
                foreach (var paymenthod in request.PaymentMethods)
                {
                    var sQuery = @"UPDATE public.ohd_payment_methods
                                 SET 
                                display_name=@DisplayName
                                ,discount=@Discount
                                ,percentage=@Percentage
                                ,status_id=@StatusId
                             ,modified_at=@ModifiedAt
                             ,modifed_by=@ModifiedBy
	                         WHERE id=@Id;";
                    var parameters = new DynamicParameters();
                    parameters.Add("Id", paymenthod.Id);
                    // parameters.Add("@Name", paymenthod.Name);
                    parameters.Add("@DisplayName", paymenthod.DisplayName);
                    // parameters.Add("@Slug", paymenthod.Slug);
                    parameters.Add("@Discount", paymenthod.Discount);
                    parameters.Add("@percentage", paymenthod.Percentage);
                    parameters.Add("@StatusId", paymenthod.StatusId ? (int)StatusEnum.Active : (int)StatusEnum.Inactive);
                    parameters.Add("@ModifiedAt", DateTime.UtcNow);
                    parameters.Add("@ModifiedBy", UserId);

                    var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
        #region
        public async Task<int> AddBankTransferTransaction(AddBankTransferTransactionDto request)
        {
            var sQuery = @" INSERT INTO public.ohd_top_up_transactions(
	                    transaction_id,
                        amount, 
                        user_id,
                        use_wallet, 
                        meter_id, 
                        transaction_fee, 
                        recharge_amount, 
                        created_at, 
                        flag, 
                        pf_response, 
                        payment_method_id,
                        topup_status,
                        net_up_transaction,
                        remark,
                        bank_transaction_id,
                        eft_ref_no)
	                    VALUES (@TransactionId
                                , @Amount
                                , @UserId
                                , @UseWallet
                                , @MeterId
                                , @TransactionFee
                                , @RechargeAmount
                                , @CreatedAt
                                , @Flag
                                , @PfResponse
                                , @PaymentMethodId
                                , @TopupStatus
                                , @NetUpTransactionGuid
                                , @Remark
                                , @BankTransactionId 
                                , @EFT_RefNo)
                    RETURNING lastval()";
            var parameters = new DynamicParameters();
            parameters.Add("@TransactionId", request.TransactionId);
            parameters.Add("@Amount", request.Amount);
            parameters.Add("@UserId", request.UserId);
            parameters.Add("@UseWallet", request.UseWallet);
            parameters.Add("@MeterId", request.MeterId);
            parameters.Add("@TransactionFee", request.TransactionFee);
            parameters.Add("@RechargeAmount", request.RechargeAmount);
            parameters.Add("@CreatedAt", DateTime.UtcNow);
            parameters.Add("@Flag", request.Flag);
            parameters.Add("@PfResponse", request.PfResponse);
            parameters.Add("@PaymentMethodId", request.PaymentMethodId);
            parameters.Add("@TopupStatus", request.TopupStatus);
            parameters.Add("@NetUpTransactionGuid", request.NetUpTransactionGuid);
            parameters.Add("@Remark", request.Remark);
            parameters.Add("@BankTransactionId", request.BankTransactionId);
            parameters.Add("@EFT_RefNo", request.EFTReferenceNumber);
            try
            {
                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }

        }

        public async Task<int> AddDebitechFailedTxn(AddDeitecFailedTransactionDto request)
        {
            var sQuery = @"INSERT INTO public.ohd_debitech_failed_transactions(
                            transaction_id,
                            amount,
                            reference_number,
                            created_at)
                            VALUES
                            (
                            @TxnId,
                            @Amount,
                            @RefNo,                           
                            @CreatedAt
                            ) RETURNING lastval()";
            var parameters = new DynamicParameters();
            parameters.Add("@TxnId", request.TransactionId);
            parameters.Add("@Amount", request.Amount);
            parameters.Add("@RefNo", request.ReferenceNo);
            parameters.Add("@CreatedAt", DateTime.UtcNow);

            try
            {
                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        #endregion
        #region PaymentDashboard
        public async Task<PaymentDashboardDto> GetPaymentDashboard()
        {
            PaymentDashboardDto dashboardDto = new();
            var totalTopUpAmountQuery = "";
            var totalPropertyQuery = "";
            var topUpCountquery = "";
            var totalActiveMeterQuery = "";
            var totalConsumerQuery = "";
            var totalMeterQuery = "";
            var UtilityWiseTopUpCountQuery = "";
            try
            {

                totalPropertyQuery = @"SELECT COUNT(id)
                                   FROM ohd_property
                                   WHERE status_id=@Active";

                totalActiveMeterQuery = @"SELECT COUNT(m.ID)
                                        FROM ohd_meter as m 
                                        LEFT JOIN ohd_property as p on m.property_id=p.id
                                        WHERE m.status_id =@Active";

                totalMeterQuery = @"SELECT COUNT(m.ID)
                                        FROM ohd_meter as m 
                                        LEFT JOIN ohd_property as p on m.property_id=p.id";

                totalConsumerQuery = @"SELECT COUNT(id)
                                   FROM ohd_user
                                   WHERE status_id=@Active AND role_id=@Consumer;";

                topUpCountquery = @"SELECT COUNT(tut.id)FROM
                                    public.ohd_top_up_transactions as tut
                                    LEFT JOIN ohd_meter as m on tut.meter_id=m.id
                                    WHERE tut.flag=@Complete AND  tut.vend_response is not null
									    and tut.topup_status='VendSuccess'";

                totalTopUpAmountQuery = @"SELECT SUM(tut.amount)FROM 
                                    public.ohd_top_up_transactions as tut
                                    LEFT JOIN ohd_meter as m on tut.meter_id=m.id                                    
                                   WHERE tut.flag=@Complete AND  tut.vend_response is not null
									    and tut.topup_status='VendSuccess'";

                UtilityWiseTopUpCountQuery = @"WITH last_week AS
                                        (
                                           SELECT emt.id AS meter_type_id,
                                                   emt.name AS MeterType, 
                                                  COALESCE(SUM(tut.amount), 0)::numeric AS LastWeek 
                                           FROM  public.ohd_enum_meter_type AS emt
                                           LEFT JOIN public.ohd_meter AS mt ON emt.id = mt.meter_type_id
                                           LEFT JOIN  public.ohd_top_up_transactions AS tut ON mt.id = tut.meter_id 
                                          --  LEFT JOIN public.ohd_property as p on mt.property_id=p.id
                                           AND tut.created_at > (CURRENT_DATE - EXTRACT(DOW FROM CURRENT_DATE)::INTEGER - 7)
                                           AND tut.created_at < (CURRENT_DATE - EXTRACT(DOW FROM CURRENT_DATE)::INTEGER - 1)
                                          AND tut.flag=@Complete AND  tut.vend_response is not null
									    and tut.topup_status='VendSuccess'
                                           GROUP BY emt.id, emt.name
                                       ),
                                       last_month AS 
                                       (
                                           SELECT emt.id AS meter_type_id,
                                                    emt.name AS MeterType, 
                                                   COALESCE(SUM(tut.amount), 0)::numeric AS LastMonth
                                           FROM public.ohd_enum_meter_type AS emt LEFT JOIN 
                                                 public.ohd_meter AS mt ON emt.id = mt.meter_type_id
                                           LEFT JOIN public.ohd_top_up_transactions AS tut ON mt.id = tut.meter_id 
                                          --  LEFT JOIN public.ohd_property as p on mt.property_id=p.id
                                           AND tut.created_at >= DATE_TRUNC('month', CURRENT_DATE - INTERVAL '1 month') 
                                           AND tut.created_at <= (DATE_TRUNC('month', CURRENT_DATE) - INTERVAL '1 day') 
                                           AND tut.flag=@Complete AND  tut.vend_response is not null
									    and tut.topup_status='VendSuccess'                                   
                                           GROUP BY  emt.id, emt.name
                                       ),
                                       last_year AS 
                                       (
                                           SELECT emt.id AS meter_type_id,
                                                   emt.name AS MeterType, 
                                                   COALESCE(SUM(tut.amount), 0)::numeric AS LastYear
                                           FROM public.ohd_enum_meter_type AS emt
                                           LEFT JOIN public.ohd_meter AS mt ON emt.id = mt.meter_type_id
                                           LEFT JOIN public.ohd_top_up_transactions AS tut ON mt.id = tut.meter_id 
                                         --   LEFT JOIN public.ohd_property as p on mt.property_id=p.id
                                           AND tut.created_at >= DATE_TRUNC('year', CURRENT_DATE - INTERVAL '1 year') 
                                           AND tut.created_at <= (DATE_TRUNC('year', CURRENT_DATE) - INTERVAL '1 day')
                                           AND tut.flag=@Complete AND  tut.vend_response is not null
									    and tut.topup_status='VendSuccess'
                                           GROUP BY  emt.id, emt.name
                                       )
                                       SELECT 
                                           lw.MeterType,
                                           lw.LastWeek AS LastWeekAmount,
                                           lm.LastMonth  AS LastMonthAmount,
                                           ly.LastYear As LastYearAmount
                                       FROM  last_week lw
                                       LEFT JOIN  last_month lm ON lw.meter_type_id = lm.meter_type_id
                                       LEFT JOIN last_year ly ON lw.meter_type_id = ly.meter_type_id
                                       ORDER BY lw.MeterType";



                var parameters = new DynamicParameters();
                parameters.Add("@Complete", (int)PaymentStatus.Complete);
                parameters.Add("@Active", (int)StatusEnum.Active);
                parameters.Add("@Consumer", (int)RoleMasterEnum.Customer);


                var totalProperty = _genericRepository.ExecuteScalarAsync<int>(totalPropertyQuery, parameters);

                var totalMeter = _genericRepository.ExecuteScalarAsync<int>(totalMeterQuery, parameters);

                var totalActiveMeter = _genericRepository.ExecuteScalarAsync<int>(totalActiveMeterQuery, parameters);

                var totalConsumers = _genericRepository.ExecuteScalarAsync<int>(totalConsumerQuery, parameters);

                var utilityWiseTopUpCount = _genericRepository.GetAsync<UtilityWiseTopUpAmount>(UtilityWiseTopUpCountQuery, parameters);

                var TotalTopUpcount = _genericRepository.ExecuteScalarAsync<int>(topUpCountquery, parameters);

                var TotalTopUpAmount = _genericRepository.ExecuteScalarAsync<double>(totalTopUpAmountQuery, parameters);
                await Task.WhenAll(utilityWiseTopUpCount).ConfigureAwait(false);

                dashboardDto.UtilityTopUpAmount = utilityWiseTopUpCount.Result.ToList();
                dashboardDto.TotalConsumers = totalConsumers.Result;
                dashboardDto.TotalMeters = totalMeter.Result;
                dashboardDto.TotalActiveMeters = totalActiveMeter.Result;
                dashboardDto.TotalProperties = totalProperty.Result;
                dashboardDto.TotalTopUpCount = TotalTopUpcount.Result;
                dashboardDto.TotalTopUpAmount = TotalTopUpAmount.Result;


            }
            catch (Exception ex)
            {

            }
            return dashboardDto;
        }
        #endregion
        #region AdminDashboard
        public async Task<AdminDashboardDto> GetAdminDashboard()
        {
            AdminDashboardDto adminDashboardDto = new();
            try
            {
                var topUpCountQuery = @"WITH TopUp AS ( SELECT
                                        (SELECT COUNT(ID)
                                        FROM ohd_top_up_transactions 
                                        WHERE  flag=@Complete AND  vend_response is not null
									    and topup_status='VendSuccess' AND created_at >= (date_trunc('month', now())::date - 31) 
                                        AND created_at <= (date_trunc('month', now())::date - 1)) AS LastMonthTopUpCount,
                                        (SELECT COUNT(ID)
                                        FROM ohd_top_up_transactions 
                                        WHERE flag=@Complete AND  vend_response is not null
									    and topup_status='VendSuccess' AND created_at >= date_trunc('month', current_date) 
                                        AND created_at <= date_trunc('month', current_date) + interval '1 month' - interval '1 day' 
                                        )  as ThisMonthTopUpCount
                                        )
                                        SELECT
                                       round(100*ThisMonthTopUpCount/ CASE WHEN LastMonthTopUpCount =0 THEN 1 ELSE LastMonthTopUpCount END )/100 AS Percentage,ThisMonthTopUpCount,LastMonthTopUpCount,
                                        CASE WHEN ThisMonthTopUpCount <= LastMonthTopUpCount THEN 'Down' ELSE 'Up' END AS TopUpCountPercentageFlag
                                        FROM TopUp";

                var topUpAmountQuery = @"WITH TopUp AS ( SELECT
                                        (SELECT SUM(amount)
                                        FROM ohd_top_up_transactions 
                                        WHERE flag=@Complete AND  vend_response is not null
									    and topup_status='VendSuccess' AND created_at >= (date_trunc('month', now())::date - 31) 
                                        AND created_at <= (date_trunc('month', now())::date - 1)) AS LastMonthTopUpAmount,
                                        (SELECT SUM(amount)
                                        FROM ohd_top_up_transactions 
                                        WHERE flag=@Complete AND  vend_response is not null
									    and topup_status='VendSuccess' AND created_at >= date_trunc('month', current_date) 
                                        AND created_at <= date_trunc('month', current_date) + interval '1 month' - interval '1 day'
                                        )  as ThisMonthTopUpAmount
                                        )
                                        SELECT
                                       round (  100.0 * ThisMonthTopUpAmount / CASE WHEN LastMonthTopUpAmount =0 THEN 1 ELSE LastMonthTopUpAmount END)/100 AS Percentage,ThisMonthTopUpAmount,LastMonthTopUpAmount,
                                        CASE WHEN ThisMonthTopUpAmount <= LastMonthTopUpAmount THEN 'Down' ELSE 'Up' END AS TopUpAmountPercentageFlag
                                        FROM TopUp";
                var utilityCountQuery = @"SELECT emt.name AS UtilityType,  
                                        COUNT(CASE WHEN mt.status_id IN (@StatusId,@Reject,@Pending) THEN mt.id ELSE NULL END) AS Count from 
                                    ohd_meter  AS mt
                                        LEFT JOIN public.ohd_enum_meter_type AS emt ON  mt.meter_type_id = emt.id
                                        where emt.status_id=@StatusId 
                                        GROUP BY emt.name ORDER BY emt.name";


                var parameters = new DynamicParameters();

                parameters.Add("@StatusId", (int)StatusEnum.Active);
                parameters.Add("@Complete", (int)PaymentStatus.Complete);
                parameters.Add("@Reject", (int)StatusEnum.Rejected);
                parameters.Add("@Pending", (int)StatusEnum.Pending);

                var utilityCount = _genericRepository.GetAsync<UtilityWiseCount>(utilityCountQuery, parameters);
                var topUpCount = _genericRepository.GetAsync<TopUpCount>(topUpCountQuery, parameters);
                var topUpAmount = _genericRepository.GetAsync<TopUpAmount>(topUpAmountQuery, parameters);

                await Task.WhenAll(utilityCount, topUpCount, topUpAmount).ConfigureAwait(false);
                adminDashboardDto.UtilityCount = utilityCount.Result.ToList();
                adminDashboardDto.TopUp = topUpCount.Result.ToList();
                adminDashboardDto.TopUpTotalAmount = topUpAmount.Result.ToList();

            }


            catch (Exception ex) { }
            return adminDashboardDto;
        }
        #endregion
        public async Task<IEnumerable<PaymenthMethodSummary>> GetPaymenthMethodSummaries()
        {
            var sQuery = @"SELECT pm.display_name  as PaymentMethod, TO_CHAR(tut.created_at,'Mon') as Month,sum(amount) as amount from public.ohd_top_up_transactions AS tut
                        JOIN public.ohd_payment_methods As pm ON tut.payment_method_id = pm.id
                        WHERE tut.flag=@Complete
                        GROUP BY pm.display_name,TO_CHAR(tut.created_at,'Mon')";
            var parameters = new DynamicParameters();
            parameters.Add("@Complete", (int)PaymentStatus.Complete);

            var result = await _genericRepository.GetAsync<PaymenthMethodSummary>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        #region Receipt
        public async Task<PurchaseReceiptDto> GetReceiptDetails(DownloadPurchaceRecieptPdfQuery request)
        {

            try
            {
                var sQueryNew = @"SELECT CASE 
                                    WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' IS NOT NULL THEN 
                                        (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' -> 'util' ->> '#text'
                                    WHEN (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' -> 'reprintManyRes' -> 'vendRes' IS NOT NULL THEN 
                                        (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' ->'reprintManyRes' -> 'vendRes' -> 'util' ->> '#text'
                                    ELSE NULL
                                END AS UtilName,
                                CASE 
                                    WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' IS NOT NULL THEN 
                                        (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' -> 'util' ->> '@distId'
                                    WHEN (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' -> 'reprintManyRes' -> 'vendRes' IS NOT NULL THEN 
                                        (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' ->'reprintManyRes' -> 'vendRes' -> 'util' ->> '@distId'
                                    ELSE NULL
                                END AS UtilDistId,
                                CASE 
                                    WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' IS NOT NULL THEN 
                                        (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' -> 'util' ->> '@taxRef'
                                    WHEN (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' -> 'reprintManyRes' -> 'vendRes' IS NOT NULL THEN 
                                        (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' -> 'reprintManyRes' -> 'vendRes' -> 'util' ->> '@taxRef'
                                    ELSE NULL
                                END AS UtilVATNo,
                                CASE 
                                    WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' IS NOT NULL THEN 
                                        (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' -> 'util' ->> '@addr'
                                    WHEN (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' ->'reprintManyRes' -> 'vendRes' IS NOT NULL THEN 
                                        (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' -> 'reprintManyRes' -> 'vendRes' -> 'util' ->> '@addr'
                                    ELSE NULL
                                END AS UtilAddress,
	                            CASE 
                                    WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' IS NOT NULL THEN 
		                            (tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'vendRes'->>'ref' 
	                            WHEN (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' ->'reprintManyRes' -> 'vendRes' IS NOT NULL THEN 
                                        (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' -> 'reprintManyRes' -> 'vendRes' ->> 'ref'
                                    ELSE NULL
		                            END AS ReferenceNmber,
                           CASE  WHEN ((tut.vend_response::jsonb)->'ipayMsg'->>'@time')  ~'^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2} [+-]\d{4}$'
                                THEN ((tut.vend_response::jsonb)->'ipayMsg'->>'@time') ::timestamp
                               ELSE CASE WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' IS NOT NULL THEN 
                                      ((tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'vendRes'->>'@transDate')::timestamp 
                                     ELSE 
                                      ((tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' -> 'reprintManyRes' -> 'vendRes' ->> '@transDate')::timestamp 
                                      END								
								                       
								END AS IssuedDate,
                            CASE 
                                    WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' IS NOT NULL THEN 
		                            (tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'vendRes'->>'@meterNumber'
	                            WHEN (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' ->'reprintManyRes' -> 'vendRes' IS NOT NULL THEN 
                                        (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' -> 'reprintManyRes' -> 'vendRes' ->> '@meterNumber'
                                    ELSE NULL
		                            END  AS MeterNumber,		
	                            CASE 
                                    WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' IS NOT NULL THEN 
		                            (tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'vendRes'->'tariff' 
	                            WHEN (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' ->'reprintManyRes' -> 'vendRes' IS NOT NULL THEN 
                                    (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' ->'reprintManyRes' -> 'vendRes'->'tariff' 
                                    ELSE NULL
		                            END AS  DomesticTarrif,
                            CASE 
                                    WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' IS NOT NULL THEN 
                            (tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'vendRes'->>'@tokenTechCode' 
                             WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'reprintManyRes'->'vendRes' IS NOT NULL THEN 
                            (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' ->'reprintManyRes' -> 'vendRes'->>'@tokenTechCode' 
                             ELSE NULL
		                            END AS TokenTech,

                              CASE   WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' IS NOT NULL THEN 
                             (tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'vendRes'->>'@algCode'
                             WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'reprintManyRes'->'vendRes' IS NOT NULL THEN 
                            (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' ->'reprintManyRes' -> 'vendRes'->>'@algCode'
                             ELSE NULL
		                            END AS Alg,

                             CASE    WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' IS NOT NULL THEN 
                             (tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'vendRes'->>'@supGrpRef'
                             WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' ->'reprintManyRes'-> 'vendRes' IS NOT NULL THEN 
                            (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' ->'reprintManyRes' -> 'vendRes'->>'@supGrpRef'
                             ELSE NULL
		                            END AS  SGC,
                            CASE  WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' IS NOT NULL THEN 
                              (tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'vendRes'->>'@tariffIdx'
                             WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' ->'reprintManyRes'-> 'vendRes' IS NOT NULL THEN 
                            (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' ->'reprintManyRes' -> 'vendRes'->>'@tariffIdx'
                            ELSE NULL END AS TI,
                             CASE  WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' IS NOT NULL THEN 
                                 (tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'vendRes'->>'@keyRevNum'
                             WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'reprintManyRes'->'vendRes' IS NOT NULL THEN 
                            (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' ->'reprintManyRes' -> 'vendRes'->>'@keyRevNum'
                             ELSE NULL
                            END AS KRN,
                            CASE  WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' IS NOT NULL THEN
                            (tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'vendRes'->'stdToken'->>'@units' 
                            WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' ->'reprintManyRes'-> 'vendRes' IS NOT NULL THEN 
                            (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' ->'reprintManyRes' -> 'vendRes'->'stdToken'->>'@units' 
                             ELSE NULL END AS Units,
                            CASE  WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' IS NOT NULL THEN
                            (tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'vendRes'->'stdToken'->>'@amt'
                            WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'reprintManyRes'->'vendRes' IS NOT NULL THEN 
                            (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' ->'reprintManyRes' -> 'vendRes'->'stdToken'->>'@amt' 
                             ELSE NULL END AS stdamt,
 
                             CASE  WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' IS NOT NULL THEN
                            (tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'vendRes'->'stdToken'->>'@tax'
                            WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' ->'reprintManyRes'-> 'vendRes' IS NOT NULL THEN 
                            (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' ->'reprintManyRes' -> 'vendRes'->'stdToken'->>'@tax'
                             ELSE NULL END AS stdtax, 
 
                             CASE  WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' IS NOT NULL THEN
                            (tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'vendRes'->'stdToken'->>'@rctNum'
                            WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' ->'reprintManyRes'-> 'vendRes' IS NOT NULL THEN 
                            (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' ->'reprintManyRes' -> 'vendRes'->'stdToken'->>'@rctNum'
                             ELSE NULL END AS stdReceiptId,
 
                              CASE  WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' IS NOT NULL THEN
                            (tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'reprintManyRes'->'vendRes'->'stdToken'->>'@tariff'
                            WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' ->'reprintManyRes'->'vendRes' IS NOT NULL THEN 
                            (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' ->'reprintManyRes' -> 'vendRes'->'stdToken'->>'@tariff'
                             ELSE NULL END AS StdTarrif,
                               CASE  WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' IS NOT NULL THEN
                             (tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'vendRes'->'stdToken'->>'@purchPriceInclTax'
                            WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' ->'reprintManyRes'-> 'vendRes' IS NOT NULL THEN 
                            (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' ->'reprintManyRes' -> 'vendRes'->'stdToken'->>'@purchPriceInclTax'
                             ELSE NULL END AS PurchasePriceInclTax,

                             CASE  WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' IS NOT NULL THEN
                             (tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'vendRes'->'keyChangeToken'->>'oldSupGrpRef'
                            WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' ->'reprintManyRes'-> 'vendRes' IS NOT NULL THEN 
                            (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' ->'reprintManyRes' -> 'vendRes'->'keyChangeToken'->>'oldSupGrpRef'
                             ELSE NULL END AS OldSGC,
 
                              CASE  WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' IS NOT NULL THEN
                             (tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'vendRes'->'keyChangeToken'->>'oldTariffIdx'
                            WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' ->'reprintManyRes'-> 'vendRes' IS NOT NULL THEN 
                            (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' ->'reprintManyRes' -> 'vendRes'->'keyChangeToken'->>'oldTariffIdx'
                             ELSE NULL END AS OldTI,

                              CASE  WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' IS NOT NULL THEN
                             (tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'vendRes'->'keyChangeToken'->>'oldKeyRevNum'
                            WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'reprintManyRes'->'vendRes' IS NOT NULL THEN 
                            (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' ->'reprintManyRes' -> 'vendRes'->'keyChangeToken'->>'oldKeyRevNum'
                             ELSE NULL END AS OldKRN,

                              CASE  WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' IS NOT NULL THEN
                             (tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'vendRes'->'keyChangeToken'->>'newSupGrpRef'
                            WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' ->'reprintManyRes' ->'vendRes' IS NOT NULL THEN 
                            (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' ->'reprintManyRes' -> 'vendRes'->'keyChangeToken'->>'newSupGrpRef'
                             ELSE NULL END AS NewSGC,
                             
                              CASE  WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' IS NOT NULL THEN
                             (tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'vendRes'->'keyChangeToken'->>'newKeyRevNum'
                            WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' ->'reprintManyRes'-> 'vendRes' IS NOT NULL THEN 
                            (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' ->'reprintManyRes' -> 'vendRes'->'keyChangeToken'->>'newKeyRevNum'
                             ELSE NULL END AS NewKRN,
 
                               CASE  WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' IS NOT NULL THEN
                             (tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'vendRes'->'keyChangeToken'->>'newTariffIdx'
                            WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'reprintManyRes'->'vendRes' IS NOT NULL THEN 
                            (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' ->'reprintManyRes' -> 'vendRes'->'keyChangeToken'->>'newTariffIdx'
                             ELSE NULL END AS NewSGC,
 
                               CASE  WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' IS NOT NULL THEN
                             (tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'vendRes'->'keyChangeToken'->>'newSupGrpRef'
                            WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'reprintManyRes'->'vendRes' IS NOT NULL THEN 
                            (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' ->'reprintManyRes' -> 'vendRes'->'keyChangeToken'->>'newSupGrpRef'
                             ELSE NULL END AS NewTI,
 
  
                               CASE  WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' IS NOT NULL THEN
                             (tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'vendRes'->'bsstToken'->>'@units' 
                            WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' ->'reprintManyRes'-> 'vendRes' IS NOT NULL THEN 
                            (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' ->'reprintManyRes' -> 'vendRes'->'bsstToken'->>'@units' 
                             ELSE NULL END AS BsstTokenUnits,
 
  
                               CASE  WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' IS NOT NULL THEN
                             (tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'vendRes'->'bsstToken'->>'@amt' 
                            WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' ->'reprintManyRes'-> 'vendRes' IS NOT NULL THEN 
                            (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' ->'reprintManyRes' -> 'vendRes'->'bsstToken'->>'@amt' 
                             ELSE NULL END AS BsstTokenAmount,
 
  
                               CASE  WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' IS NOT NULL THEN
                             (tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'vendRes'->'bsstToken'->>'@tax' 
                            WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' ->'reprintManyRes'-> 'vendRes' IS NOT NULL THEN 
                            (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' ->'reprintManyRes' -> 'vendRes'->'bsstToken'->>'@tax' 
                             ELSE NULL END AS BsstTokenTax,                               

                               CASE  WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' IS NOT NULL THEN
                             (tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'vendRes'->'bsstToken'->>'@rctNum' 
                            WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' ->'reprintManyRes'-> 'vendRes' IS NOT NULL THEN 
                            (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' ->'reprintManyRes' -> 'vendRes'->'bsstToken'->>'@rctNum' 
                             ELSE NULL END AS BsstReceiptId,     
                            tut.transaction_fee as Transactionfee,
                            tut.std_token as StandardTokens,
                            tut.key_change_token as KeyChangeToken,
                            tut.bsst_token as BsstToken,
                            c.name as VendorName,
                            c.companylogourl as LogoUrl,
                            u.tax_number as TaxNumber,
                            concat(u.first_name,' ',u.last_name) AS Customer,
                            u.address_line_1 AS Address,
                            tut.customermsg AS CustomerMessage,
                            tut.vend_response AS VendResponse,
                            tut.Amount AS ActualRechargeAmount,
                            tut.receipt_number AS RCTNo,
                             tut.is_in_house_txn AS IsInHouseTxn
                            FROM public.ohd_top_up_transactions  AS tut
                            LEFT JOIN ohd_user AS u ON tut.user_id=u.id
                            LEFT JOIN ohd_company AS c ON u.company_id=c.id
                            WHERE tut.transaction_id=@TransactionId";


                var parameters = new DynamicParameters();
                parameters.Add("@TransactionId", request.TransactionId);

                var receiptDetails = await _genericRepository.GetFirstOrDefaultAsync<PurchaseReceiptDto>(sQueryNew, parameters).ConfigureAwait(false);
                return receiptDetails;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        #endregion
        public async Task<PurchaceReceiptResponseModel> DownloadPurchaceRecieptPdfQuery(DownloadPurchaceRecieptPdfQuery request, int userId)
        {
            var res = new PurchaceReceiptResponseModel();
            PurchaseReceiptDto receiptDto = await GetReceiptDetails(request).ConfigureAwait(false);
            decimal totalDebtAmount = 0;
            decimal totalFixedAmunt = 0;
            decimal totalVatExcluding = 0;
            decimal totalVatIncluding = 0;
            decimal totalTax = 0;
            var debts = new List<DebtItem>();
            var fixedItems = new List<FixedItem>();
            decimal debtremainingBalance = 0;
            if (receiptDto.VendResponse != null)
            {
                JObject jsonObject = JObject.Parse(receiptDto.VendResponse);

                bool containsDebt = jsonObject["ipayMsg"]?["elecMsg"]?["vendRes"]?["debt"] != null;
                bool containsFixed = jsonObject["ipayMsg"]?["elecMsg"]?["vendRes"]?["fixed"] != null;
                if (containsDebt)
                {
                    decimal debtAmount = 0;
                    decimal debtTax = 0;
                    int debtCount = (Int32)jsonObject["ipayMsg"]["elecMsg"]["vendRes"]["debt"].Count();

                    if (debtCount != 8)
                    {
                        JArray debtArray = (JArray)jsonObject["ipayMsg"]["elecMsg"]["vendRes"]["debt"];
                        if (debtArray != null && debtArray.Count > 0)
                        {

                            foreach (var debt in debtArray)
                            {

                                if (debt != null)
                                {
                                    string text = (string)debt["#text"];
                                    string amount = (string)debt["@amt"];
                                    string remainingBalance = (string)debt["@rem"];
                                    string tax = (string)debt["@tax"];

                                    debtAmount = Convert.ToDecimal(amount);
                                    debtAmount = Math.Round(debtAmount / 100, 2);
                                    debtTax = Convert.ToDecimal(tax);
                                    debtTax = Math.Round(debtTax / 100, 2);
                                    debtremainingBalance = Convert.ToDecimal(remainingBalance);
                                    debtremainingBalance = Math.Round(debtremainingBalance / 100, 2);
                                    debts.Add(new DebtItem
                                    {
                                        Amount = debtAmount,
                                        Tax = debtTax,
                                        RemainBalance = debtremainingBalance,
                                        Text = text
                                    });
                                    totalDebtAmount += (debtAmount) + (debtTax);
                                    totalTax += debtTax;
                                }

                            }
                        }
                    }
                    else
                    {
                        decimal amount = 0;
                        decimal tax = 0;
                        decimal remainingBalance = 0;
                        amount = (decimal)jsonObject["ipayMsg"]["elecMsg"]["vendRes"]["debt"]["@amt"];
                        tax = (decimal)jsonObject["ipayMsg"]["elecMsg"]["vendRes"]["debt"]["@tax"];
                        string text = (string)jsonObject["ipayMsg"]["elecMsg"]["vendRes"]["debt"]["#text"];
                        remainingBalance = (decimal)jsonObject["ipayMsg"]["elecMsg"]["vendRes"]["debt"]["@rem"];
                        debtAmount = Convert.ToDecimal(amount);
                        debtAmount = Math.Round(debtAmount / 100, 2);
                        debtTax = Convert.ToDecimal(tax);
                        debtTax = Math.Round(debtTax / 100, 2);
                        debtremainingBalance = Convert.ToDecimal(remainingBalance);
                        debtremainingBalance = Math.Round(debtremainingBalance / 100, 2);

                        debts.Add(new DebtItem
                        {
                            Amount = debtAmount,
                            Tax = debtTax,
                            RemainBalance = debtremainingBalance,
                            Text = text

                        });
                        totalDebtAmount += (debtAmount) + (debtTax);
                        totalTax += debtTax;
                    }
                }

            }
            receiptDto.RemainingBalance = debtremainingBalance;
            //byte[] pdfBytes = await GeneratePdfwithSelectPdf(receiptDto).ConfigureAwait(false);
            byte[] pdfBytes = await GeneratePdfwithDynamicHtml(receiptDto).ConfigureAwait(false);
            MemoryStream stream = new MemoryStream();
            stream.Write(pdfBytes, 0, pdfBytes.Length);
            stream.Position = 0;
            var requestPath = _httpContextAccessor.HttpContext.Request;
            var domain = $"{requestPath.Scheme}://{requestPath.Host}";
            var absoluteUrl = domain + "/uploads/documents/TopUpReceipt/";
            string path = Path.Combine(_environment.WebRootPath, "uploads/documents/TopUpReceipt");
            string fileName = request.TransactionId.ToString();
            fileName += ".pdf";
            path = Path.Combine(path);
            absoluteUrl += fileName;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string relativePath = path + "/" + fileName;
            if (File.Exists(relativePath))
            {
                await DeleteFileAsync(path, fileName);
            }
            try
            {
                using (StreamWriter sw = new StreamWriter(relativePath, true))
                {
                    stream.CopyTo(sw.BaseStream);
                }
                if (File.Exists(relativePath))
                {
                    res.PurchaceReceiptUrl = absoluteUrl;
                    var user = await _userRepository.GetUserById(userId).ConfigureAwait(true);
                    var company = await _companyRepository.GetCompanyDetails(user.CompanyId).ConfigureAwait(false);
                    EmailModelClass obj = new()
                    {
                        title = "Purchase Receipt",
                        email = user.Email,
                        forEvent = "PurchaseReceipt",
                        subtitle = "",
                        companyId = user.CompanyId,
                        mobile = user.Mobile,
                        propertyUser = user.UserName,
                        body = "",
                        documentPath = relativePath
                    };
                    string mailContent = "";
                    if (receiptDto != null)
                    {
                        if (!string.IsNullOrEmpty(receiptDto.StandardTokens))
                        {
                            mailContent += @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'> Thank you for your payment. The transaction details are as follows:<br> " + company.CompanyName + " Meter:" + receiptDto.MeterNumber + " RCT: " + receiptDto.RCTNo + " Amt: R " + Math.Round(Convert.ToDecimal(receiptDto.stdamt / 100), 2) +
                                            "<br>Token : " + receiptDto.StandardTokens + "<br>Units : " + receiptDto.Units + "<br>Please review the attached purchase receipt for further information.</td></tr>";
                            mailContent += @"<tr><td align='left' valign='top' style='font-size: 17px; font-weight: 400; line-height: 160%; border-collapse: collapse; border-spacing: 0; margin: 0; padding: 0; padding-top: 25px; color: #000000; font-family: sans-serif;' class='paragraph'></td></tr>";
                            obj.body = mailContent;
                        }
                        else if (!string.IsNullOrEmpty(receiptDto.BsstToken))
                        {
                            mailContent += @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'> Thank you for your payment. The transaction details are as follows:<br>" +
                                           "<br> RCT: " + receiptDto.RCTNo + "<br>Amt : R " + Math.Round(Convert.ToDecimal(receiptDto.BsstTokenAmount) / 100, 2) + "<br>Please review the attached purchase receipt for further information.</td></tr>";
                            mailContent += @"<tr><td align='left' valign='top' style='font-size: 17px; font-weight: 400; line-height: 160%; border-collapse: collapse; border-spacing: 0; margin: 0; padding: 0; padding-top: 25px; color: #000000; font-family: sans-serif;' class='paragraph'></td></tr>";
                            obj.body = mailContent;
                        }
                        else
                        {
                            mailContent += @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>  Thank you for your payment. The transaction details are as follows:<br>" +
                                          "<br> RCT: " + receiptDto.RCTNo + "<br>Amt : R " + receiptDto.ActualRechargeAmount + "<br>Remaining Bal : R " + Math.Round(Convert.ToDecimal(receiptDto.RemainingBalance), 2) + " <br> Please review the attached purchase receipt for further information.</td></tr>";
                            mailContent += @"<tr><td align='left' valign='top' style='font-size: 17px; font-weight: 400; line-height: 160%; border-collapse: collapse; border-spacing: 0; margin: 0; padding: 0; padding-top: 25px; color: #000000; font-family: sans-serif;' class='paragraph'></td></tr>";
                            obj.body = mailContent;
                        }
                    }
                    res.ResponseMsg = await _otpService.SendPurchaseReceipt(obj).ConfigureAwait(false);

                    return res;
                }
            }
            catch (Exception ex)
            {

            }
            return res;
        }
        private async Task DeleteFileAsync(string path, string fileName)
        {
            await Task.Run(() => File.Delete(Path.Combine(path, fileName)));
        }

        private async Task<byte[]> GeneratePdfwithSelectPdf(PurchaseReceiptDto receiptDto)
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            decimal totalDebtAmount = 0;
            decimal totalFixedAmunt = 0;
            decimal totalVatExcluding = 0;
            decimal totalVatIncluding = 0;
            decimal totalTax = 0;
            string htmlContent = "";
            if (receiptDto != null)
            {

                if (receiptDto.IsInHouseTxn && string.IsNullOrEmpty(receiptDto.StandardTokens))
                {
                    receiptDto.stdamt = Math.Round(Convert.ToDecimal(string.Format("{0:F2}", receiptDto.stdamt)) / 100, 2);

                    receiptDto.stdtax = Math.Round(Convert.ToDecimal(string.Format("{0:F2}", receiptDto.stdtax)) / 100, 2);

                }
                if (!string.IsNullOrEmpty(receiptDto.BsstToken))
                {
                    receiptDto.Amount = Math.Round(Convert.ToDecimal(string.Format("{0:F2}", receiptDto.BsstTokenAmount)) / 100, 2);
                    receiptDto.BsstTokenTax = Math.Round(Convert.ToDecimal(string.Format("{0:F2}", receiptDto.BsstTokenTax)) / 100, 2);
                    receiptDto.BsstTokenUnits = Math.Round(Convert.ToDecimal(string.Format("{0:F2}", receiptDto.BsstTokenUnits)));
                    totalVatIncluding = (decimal)(receiptDto.Amount + receiptDto.BsstTokenTax);
                    totalTax += (decimal)receiptDto.BsstTokenTax;

                }
                if (!receiptDto.IsInHouseTxn)
                {
                    if (!string.IsNullOrEmpty(receiptDto.StandardTokens))
                    {
                        totalVatIncluding = (decimal)(receiptDto.stdamt.Value + receiptDto.stdtax);
                        totalVatExcluding = receiptDto.stdamt.Value;
                    }
                }

                receiptDto.TransactionFee = Convert.ToDecimal(receiptDto.TransactionFee);
                receiptDto.TransactionFee = Math.Round(receiptDto.TransactionFee, 2);
                receiptDto.TransactionFee = Convert.ToDecimal(string.Format("{0:F2}", receiptDto.TransactionFee));

                var requestPath = _httpContextAccessor.HttpContext.Request;
                var domain = $"{requestPath.Scheme}://{requestPath.Host}";
                string wwwPath = _environment.WebRootPath;
                //string contentPath = _environment.ContentRootPath;
                receiptDto.LogoUrl = domain + receiptDto.LogoUrl;
                string relativePath = receiptDto.LogoUrl.Replace(domain, wwwPath).Replace("/", "\\");//uri.LocalPath;
                byte[] imageByte = System.IO.File.ReadAllBytes(relativePath);
                string imgbase64 = Convert.ToBase64String(imageByte);
                string imageUrl = "data:image/png;base64, " + imgbase64;
                string logoUrl = receiptDto.LogoUrl;
                StringBuilder resultBsstToken = new StringBuilder();
                StringBuilder resultStdtoken = new StringBuilder();
                StringBuilder resultKeychangeToken = new StringBuilder();
                string cssStyles = @"
                            <style>
                                .hidden { display: none; }
                                page-break-inside: avoid !important;
                                page-break-inside: auto !important;
                                .page
                                {
                                    width:1000px;
                                    margin: 0 px auto;
                                    padding :20px;
                                    font-family:Arial, Helvetica, sans-serif;
                                    border: 1px solid #ccc; background-color: #FFFFFF;
                                    font-family: Arial, sans-serif;'
                                }
                                @page
                                    {
                                        size: auto;   /* auto is the initial value */
                                        margin: 0mm;  /* this affects the margin in the printer settings */
                                    }
                            </style>";

                htmlContent = cssStyles + @"<div class='page' style='margin: 0px auto; max-width: 1000px; padding: 20px;page-break-inside: avoid;page-break-inside: avoid; border: 1px solid #ccc; background-color: #FFFFFF; font-family: Arial, sans-serif;'>
    <div style='margin-bottom: 20px; text-align: center;'>
         <img src='" + imageUrl + @"' alt='Logo' style='max-width: 100px; margin-bottom: 10px;'/>
<h1>Purchase Receipt</h1>
<hr />
    </div>";
                if (!string.IsNullOrEmpty(receiptDto.KeyChangeToken))
                {
                    string jsonString = receiptDto.KeyChangeToken;

                    if (!string.IsNullOrEmpty(jsonString))
                    {
                        List<string> list = jsonString.Split(',').ToList();
                        if (list != null && list.Count > 0)
                        {

                            int cumulativeLength = 0; // Track cumulative length

                            foreach (string item in list)
                            {
                                int n = 4;
                                for (int i = 0; i < item.Length; i += n)
                                {
                                    if (i > 0 && cumulativeLength < 20)
                                    {
                                        resultKeychangeToken.Append(' ');
                                        cumulativeLength += 1;
                                    }

                                    string substring = item.Substring(i, Math.Min(n, item.Length - i));
                                    resultKeychangeToken.Append(substring);
                                    cumulativeLength += substring.Length;

                                    // Add a new line if cumulative length exceeds a certain limit (e.g., 20 characters)
                                    if (cumulativeLength >= 20)
                                    {
                                        resultKeychangeToken.Append("<br>");
                                        cumulativeLength = 0; // Reset cumulative length
                                    }
                                }

                                // Add a new line after processing each item, if the item length is exactly 20
                                if (item.Length == 20 && cumulativeLength != 0)
                                {
                                    resultKeychangeToken.Append("<br>");
                                    cumulativeLength = 0; // Reset cumulative length
                                }
                            }

                        }
                    }
                    htmlContent += @"<div style='margin-bottom: 20px; text-align: center;'>
<h3 style='text-align: center;color:Red;'>Key Change Tokens</h3><hr />";


                    htmlContent += @"<div style='margin-bottom:10px;margin-top:10px;text-align:left;'>
    <table style='width: 100%; border-collapse:collapse;margin-top:20px;'>
            <tr>
            <td style='width:50%;vertical-align: top;'>
            <table style='width:100%;'>
                     <tr style='padding:20px;'><td><label style='font-size:11px'>Meter Number: </label> </td></tr>
                    
            </table>
        </td>
        <td style='width: 50%;vertical-align:top;'>
            <table style='width: 100%;'>
                 <tr style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px;'>" + receiptDto.MeterNumber + @"</label></td><td style='width:5%;'></td>
                </tr>
                <tr><td></td></tr>
                
            </table>
        </td>
    </tr>
 <tr style='padding: 20px;'>
                    <td colspan='3'><label style='font-size: 11px;'>Your resource token is below, but your meter requires a key change before you enter it.</label></td>
                </tr>
                <tr style='padding: 20px;'>
                    <td><label style='font-size: 11px;'>To change your meter's key, enter the tokens listed below:</label></td>
                </tr>
    <tr><td colspan=2 style='text-align:center;margin:0 auto;width:100%;'><label style='text-align: center;'>" + resultKeychangeToken.ToString() + @"<br></label></td></tr>
</table>
<hr />
</div>";
                    htmlContent += @" <div style='margin-bottom:10px;margin-top:10px;text-align:left;'>
    <table style='width: 100%; border-collapse:collapse;margin-top:20px;'>
            <tr>
            <td style='width:50%;vertical-align: top;'>
            <table style='width:100%;'>
                <tr style='padding:20px;'>
                    <td><label style='font-size:11px'>Old SGC: </label></td></tr>
                     <tr style='padding:20px;'>
                    <td><label style='font-size:11px'>OLD TI: </label></td></tr>
                    <tr style='padding:20px;'>
                    <td><label style='font-size:11px'>OLD KRN: </label></td></tr>
                    <tr style='padding:20px;'>
                    <td><label style='font-size:11px'>New SGC: </label> </td></tr>
                     <tr style='padding:20px;'>
                    <td><label style='font-size:11px'>New TI: </label></td></tr>
                    <tr style='padding:20px;'>
                    <td><label style='font-size:11px'>New KRN: </label></td></tr>
            </table>
        </td>
        <td style='width: 50%;vertical-align:top;'>
            <table style='width: 100%;'>
                <tr style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px'>" + receiptDto.OldSGC + @"</label></td><td style='width:5%;'></td>
                </tr>
                <tr style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px'>" + receiptDto.OldTI + @"</label></td><td style='width:5%;'></td>
                </tr>
                <tr style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px'>" + receiptDto.OldKRN + @"</label></td><td style='width:5%;'></td>
                </tr>
                <tr style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px'>" + receiptDto.NewSGC + @"</label></td><td style='width:5%;'></td>
                </tr>
                 <tr style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px'>" + receiptDto.NewTI + @"</label></td><td style='width:5%;'></td>
                </tr>
                <tr style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px'>" + receiptDto.NewKRN + @"</label></td><td style='width:5%;'></td>
                </tr>
            </table>
        </td>
    </tr>
</table>
<hr />
</div>";

                }

                htmlContent += @"<div style ='margin-bottom:10px;margin-top:10px;text-align:left;'>
                            <h3 style='text-align: center;color:Red;'>Credit Vend - Tax Invoice</h3><hr /></div>";

                htmlContent += @"<div style='margin-bottom:10px;margin-top:10px;text-align:left;'>
    <table style='width: 100%; border-collapse:collapse;margin-top:20px;'>
            <tr>
            <td style='width:50%;vertical-align: top;'>
            <table style='width:100%;'>
                <tr style='padding:20px;'>
                    <td><label style='font-size:11px'>Util Name: </label> </td></tr>
                     <tr style='padding:20px;'>
                    <td><label style='font-size:11px'>Util DistId: </label></td></tr>
                    <tr style='padding:20px;'>
                    <td><label style='font-size:11px'>Util VAT: </label></td></tr>
                    <tr style='padding:20px;'>
                    <td><label style='font-size:11px'>Util Address: </label></td></tr>
            </table>
        </td>
        <td style='width: 50%;vertical-align:top;'>
            <table style='width: 100%;'>
                <tr style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px'>" + receiptDto.UtilName + @"</label></td><td style='width:5%;'></td>
                </tr>
                <tr style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px'>" + receiptDto.UtilDistId + @"</label></td><td style='width:5%;'></td>
                </tr>
                <tr style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px'>" + receiptDto.UtilVATNo + @"</label></td><td style='width:5%;'></td>
                </tr>
                <tr style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px'>" + receiptDto.UtilAddress + @"</label></td><td style='width:5%;'></td>
                </tr>
            </table>
        </td>
    </tr>
</table>
<hr />
</div>
<div style ='margin-bottom:10px;margin-top:10px;text-align:center;'>
        <h3>Vendor Name : <label>" + receiptDto.VendorName + @"</label></h3>
    </div>

 <div style='margin-bottom:10px;margin-top:10px;text-align:left;'>
<table style='width: 100%; border-collapse:collapse;margin-top:20px;'>
            <tr>
            <td style='width:50%;vertical-align:top;'>
            <table style='width:100%;'>";

                htmlContent += @"<tr style='padding:20px;'>
                    <td><label style='font-size:11px'>Issued : </label></td></tr>

                    <tr style='padding:20px;'>
                    <td><label style='font-size:11px'>Reference: </label></td></tr>

                    <tr style='padding:20px;'>
                    <td><label style='font-size:11px'>Meter Number: </label></td></tr>";
                if (string.IsNullOrEmpty(receiptDto.TokenTech))
                {
                    htmlContent += @"<tr class='hidden' style = 'padding:20px;'>
                    <td><label class='hidden' style='font-size:11px'> Token Tech: </label></td></tr>
                    

                    <tr class='hidden' style='padding:20px;'>
                     <td><label class='hidden' style='font-size:11px'>Alg: </label></td></tr>

                    <tr class='hidden' style='padding:20px;'>
                    <td><label class='hidden' style='font-size:11px'>SGC: </label></td></tr>

                    <tr class='hidden' style='padding:20px;'>
                    <td><label class='hidden' style='font-size:11px'>TI: </label></td></tr>

                    <tr class='hidden' style='padding:20px;'>
                    <td><label class='hidden' style='font-size:11px'>KRN: </label></td></tr>";
                }
                else
                {
                    htmlContent += @"<tr style = 'padding:20px;'>
                    <td><label style='font-size:11px'> Token Tech: </label></td></tr>

                    <tr style='padding:20px;'>
                     <td><label style='font-size:11px'>Alg: </label></td></tr>

                    <tr style='padding:20px;'>
                    <td><label style='font-size:11px'>SGC: </label></td></tr>

                    <tr style='padding:20px;'>
                    <td><label style='font-size:11px'>TI: </label></td></tr>

                    <tr style='padding:20px;'>
                    <td><label style='font-size:11px'>KRN: </label></td></tr>";
                }

                htmlContent += @"</table>
        </td>
        <td style='width: 50%;vertical-align:top;'>
            <table style='width: 100%;'>";

                htmlContent += @"<tr style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px'>" + receiptDto.IssuedDate + @"</label></td><td style='width:5%;'></td>
                </tr>
                <tr style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px'>" + receiptDto.ReferenceNmber + @"</label></td><td style='width:5%;'></td>
                </tr>
                <tr style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px'>" + receiptDto.MeterNumber + @"</label></td><td style='width:5%;'></td>
                </tr>";

                if (string.IsNullOrEmpty(receiptDto.TokenTech))
                {
                    htmlContent += @"<tr class='hidden' style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px'>" + receiptDto.TokenTech + @"</label></td><td style='width:5%;'></td>
                </tr>
                <tr class='hidden' style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px'>" + receiptDto.Alg + @"</label></td><td style='width:5%;'></td>
                </tr>
                <tr class='hidden' style='padding:20px;text-align:right;'>
                    <td><label style='margin-right:10px;font-size:11px'>" + receiptDto.SGC + @"</label></td><td style='width:5%;'></td>
                </tr>
                <tr class='hidden' style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px'>" + receiptDto.TI + @"</label></td><td style='width:5%;'></td>
                </tr>
                <tr class='hidden' style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px'>" + receiptDto.KRN + @"</label></td><td style='width:5%;'></td>
                </tr>";

                }
                else
                {
                    htmlContent += @"<tr style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px'>" + receiptDto.TokenTech + @"</label></td><td style='width:5%;'></td>
                </tr>
                <tr style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px'>" + receiptDto.Alg + @"</label></td><td style='width:5%;'></td>
                </tr>
                <tr style='padding:20px;text-align:right;'>
                    <td><label style='margin-right:10px;font-size:11px'>" + receiptDto.SGC + @"</label></td><td style='width:5%;'></td>
                </tr>
                <tr style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px'>" + receiptDto.TI + @"</label></td><td style='width:5%;'></td>
                </tr>
                <tr style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px'>" + receiptDto.KRN + @"</label></td><td style='width:5%;'></td>
                </tr>";
                }
                htmlContent += @"</table>
        </td>
    </tr>
</table>
</div>
<hr />
<div style='margin-bottom:10px;margin-top:10px;text-align:left;'>
      
<table style='width: 100%; border-collapse:collapse;margin-top:20px;'>
            <tr>
            <td style='width:50%;vertical-align: top;'>
            <table style='width:100%;'>";
                if (!string.IsNullOrEmpty(receiptDto.TaxNumber) && !receiptDto.TaxNumber.ToLower().Equals("undefined"))
                {
                    htmlContent += @"<tr style='padding:20px;'>
                    <td><label style='font-size:11px'>Tax Number: </label></td></tr>";
                }
                htmlContent += @"<tr style='padding:20px;'>
                    <td><label style='font-size:11px'>Customer: </label></td></tr>
                <tr style='padding:20px;'>
                    <td><label style='font-size:11px'>Address: </label></td></tr>
          </table>
        </td>
        <td style='width: 50%;vertical-align:top;'>
            <table style='width: 100%;'>";
                if (!string.IsNullOrEmpty(receiptDto.TaxNumber) && !receiptDto.TaxNumber.ToLower().Equals("undefined"))
                {
                    htmlContent += @"<tr style = 'padding:20px;text-align:right' >
                    <td> <label style = 'margin-right:10px;font-size:11px'>" + receiptDto.TaxNumber + @"</label></td ><td style = 'width:5%;'></td >
                </tr>";
                }
                htmlContent += @"<tr style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px'>" + receiptDto.Customer + @"</label></td><td style='width:5%;'></td>
                </tr>
             <tr style='padding:20px;text-align:right;'>
                    <td><label style='margin-right:10px;font-size:11px'>" + receiptDto.Address + @"</label></td><td style='width:5%;'></td>
               </tr> 
       </table>
        </td>
    </tr>
</table>
<hr />
</div>";

                htmlContent += @"<div style='margin-bottom: 10px;text-align:center;'><h2> Your Resource Tokens </h2></div>";
                if (receiptDto.VendResponse != null)
                {
                    string tariff = "";
                    JObject jsonObject = JObject.Parse(receiptDto.VendResponse);
                    //bool istariff = jsonObject["ipayMsg"]?["elecMsg"]?["vendRes"]?["tariff"] != null;

                    JToken tariffToken = jsonObject["ipayMsg"]?["elecMsg"]?["vendRes"]?["tariff"];
                    bool hasTextKey = tariffToken != null && tariffToken.Type == JTokenType.Object && tariffToken["#text"] != null;
                    if (tariffToken != null)
                    {
                        if (hasTextKey)
                        {
                            bool hasTextKeyWithValue = tariffToken != null && tariffToken.Type == JTokenType.Object && tariffToken["#text"] != null && !string.IsNullOrEmpty(tariffToken["#text"].ToString());
                            if (hasTextKeyWithValue)
                            {
                                tariff = (string)jsonObject["ipayMsg"]["elecMsg"]["vendRes"]["tariff"]["#text"];
                            }

                        }
                        else
                        {
                            tariff = (string)jsonObject["ipayMsg"]["elecMsg"]["vendRes"]["tariff"];
                        }
                    }
                    htmlContent += @"<div style='margin-bottom: 10px; text-align: center;'><h2> Tariff: " + tariff + @" </h2></div>";
                }

                if (!string.IsNullOrEmpty(receiptDto.BsstToken))
                {
                    //receiptDto.StandardTokens= string.Format(receiptDto.StandardTokens,)

                    int n = 4;
                    for (int i = 0; i < receiptDto.BsstToken.Length; i += n)
                    {
                        if (i > 0)
                        {
                            resultBsstToken.Append(' ');
                        }
                        resultBsstToken.Append(receiptDto.BsstToken.Substring(i, 4));
                    }
                    htmlContent += @"<div style='margin-bottom: 20px; text-align: center;'><h2> Free Basic Resource Tokens </h2><label style ='text-align: center;'>" + resultBsstToken.ToString() + @"</label></div>";
                    htmlContent += @"<div style='margin-bottom:10px;margin-top:10px;text-align:left;page-break-inside :avoid;page-break-inside:auto;'>
            <table style='width: 100%; border-collapse:collapse;margin-top:20px;page-break-inside :avoid;page-break-inside:auto;'>
            <tr>
            <td style='width:50%;vertical-align:top'>
            <table style='width:100%;'>
                <tr style='padding:20px;'>
                    <td><label style='font-size:11px;margin-left:10px;'>Receipt#: </label> </td></tr>          
               <tr style='padding:20px;'>
                    <td><label style='font-size:11px;margin-left:10px;'>Units : </label></td></tr>
                    <tr style='padding:20px;'>
                    <td><label  style='font-size:11px;margin-left:10px;'>Amount: </label></td></tr>
                    <tr style='padding:20px;'>
                    <td><label  style='font-size:11px;margin-left:10px;'>Tax: </label></td></tr>
            </table>
        </td>
        <td style='width: 50%;vertical-align:top;'>
                <table style='width: 100%;'>";

                    htmlContent += @"<tr style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px'>" + receiptDto.BsstReceiptId + @"</label></td><td style='width:5%;'></td>
                    
                </tr>
              
                <tr style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px'>" + Convert.ToDecimal(string.Format("{0:F2}", receiptDto.BsstTokenUnits)) + @"</label></td>
                    
                </tr>
            
                <tr style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px'> R " + Convert.ToDecimal(string.Format("{0:F2}", receiptDto.BsstTokenAmount)) + @"</label></td><td style='width:5%;'></td>
                </tr>
                <tr style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px'> R " + receiptDto.BsstTokenTax + @"</label></td><td></td><td style='width:5%;'></td>
                </tr>
                </table>
                </td>
    </tr>
</table>
</div>
<hr />";

                }

                if (!string.IsNullOrEmpty(receiptDto.StandardTokens))
                {
                    string rctNo = "";
                    if (receiptDto.VendResponse != null)
                    {
                        string tariff = "";
                        JObject jsonObject = JObject.Parse(receiptDto.VendResponse);
                        JToken stdToken = jsonObject["ipayMsg"]?["elecMsg"]?["vendRes"]?["stdToken"];
                        JToken reprintstdToken = jsonObject["ipayMsg"]?["elecMsg"]?["reprintManyRes"]?["vendRes"]?["stdToken"];
                        if (stdToken != null)
                        {
                            bool hasRctNo = stdToken != null && stdToken.Type == JTokenType.Object && stdToken["@rctNum"] != null;
                            bool hasRctNoValue = stdToken != null && stdToken.Type == JTokenType.Object && stdToken["@rctNum"] != null && !string.IsNullOrEmpty(stdToken["@rctNum"].ToString());


                            if (hasRctNoValue)
                            {
                                rctNo = (string)jsonObject["ipayMsg"]["elecMsg"]["vendRes"]["stdToken"]["@rctNum"];
                            }
                        }
                        if (reprintstdToken != null)
                        {
                            bool hasRctNo = reprintstdToken != null && reprintstdToken.Type == JTokenType.Object && reprintstdToken["@rctNum"] != null;
                            bool hasRctNoValue = reprintstdToken != null && reprintstdToken.Type == JTokenType.Object && reprintstdToken["@rctNum"] != null && !string.IsNullOrEmpty(reprintstdToken["@rctNum"].ToString());


                            if (hasRctNoValue)
                            {
                                rctNo = (string)jsonObject["ipayMsg"]["elecMsg"]["reprintManyRes"]["vendRes"]["stdToken"]["@rctNum"];
                            }
                        }
                    }
                    //receiptDto.StandardTokens= string.Format(receiptDto.StandardTokens,)

                    int n = 4;
                    for (int i = 0; i < receiptDto.StandardTokens.Length; i += n)
                    {
                        if (i > 0)
                        {
                            resultStdtoken.Append(' ');
                        }
                        resultStdtoken.Append(receiptDto.StandardTokens.Substring(i, 4));
                    }
                    //htmlContent += @"<div style='margin-bottom: 10px; text-align: center;'><h2> Tariff:" + receiptDto.StdTarrif + @" </h2></div>";


                    htmlContent += @"<div style='margin-bottom: 10px; text-align: center;'><h2> Standard Tokens </h2><label style='text-align: center;'>" + resultStdtoken.ToString() + @"</label></div>";

                    htmlContent += @"<div style='margin-bottom:10px;margin-top:10px;text-align:left;page-break-inside :avoid;page-break-inside:auto;'>
            <table style='width: 100%; border-collapse:collapse;margin-top:20px;page-break-inside :avoid;page-break-inside:auto;'>
            <tr>
            <td style='width:50%;vertical-align:top'>
            <table style='width:100%;'>
                <tr style='padding:20px;'>
                    <td><label style='font-size:11px;margin-left:10px;'>Receipt#: </label> </td></tr>          
               <tr style='padding:20px;'>
                    <td><label style='font-size:11px;margin-left:10px;'>Units : </label></td></tr>
                    <tr style='padding:20px;'>
                    <td><label  style='font-size:11px;margin-left:10px;'>Amount: </label></td></tr>
                    <tr style='padding:20px;'>
                    <td><label  style='font-size:11px;margin-left:10px;'>Tax: </label></td></tr>
            </table>
        </td>
        <td style='width: 50%;vertical-align:top;'>
            <table style='width: 100%;'>
                <tr style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px'>" + rctNo + @"</label></td><td style='width:5%;'></td>
                </tr>
                <tr style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px'>" + Convert.ToDecimal(string.Format("{0:F2}", receiptDto.Units)) + @"</label></td><td style='width:5%;'></td>
                </tr>
            <tr style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px'> R " + Convert.ToDecimal(string.Format("{0:F2}", receiptDto.stdamt)) + @"</label></td><td style='width:5%;'></td>
                </tr>
                <tr style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px'> R " + Convert.ToDecimal(string.Format("{0:F2}", receiptDto.stdtax)) + @"</label></td><td></td><td style='width:5%;'></td>
                </tr>
            </table>
        </td>
        </tr>
    </table></div><hr />";

                    totalTax += (decimal)receiptDto.stdtax;

                }
                //if (string.IsNullOrEmpty(receiptDto.StandardTokens))
                //{
                //    htmlContent += @"<div style ='margin-bottom: 10px;page-break-inside :avoid;page-break-inside:auto;text-align: center;'><h2> Tariff:" + receiptDto.DomesticTarrif + @"</h2></div>";
                //}
                if (receiptDto.VendResponse != null)
                {
                    JObject jsonObject = JObject.Parse(receiptDto.VendResponse);

                    bool containsDebt = jsonObject["ipayMsg"]?["elecMsg"]?["vendRes"]?["debt"] != null;
                    bool containsFixed = jsonObject["ipayMsg"]?["elecMsg"]?["vendRes"]?["fixed"] != null;
                    if (containsDebt)
                    {
                        decimal debtAmount = 0;
                        decimal debtTax = 0;
                        decimal debtremainingBalance = 0;
                        int debtCount = (Int32)jsonObject["ipayMsg"]["elecMsg"]["vendRes"]["debt"].Count();

                        if (debtCount != 8)
                        {
                            JArray debtArray = (JArray)jsonObject["ipayMsg"]["elecMsg"]["vendRes"]["debt"];
                            if (debtArray != null && debtArray.Count > 0)
                            {

                                htmlContent += @"<div style='margin-bottom:10px; page-break-inside :avoid;page-break-inside:auto;text-align: center;'><h2> Debt Items </h2></div>";
                                foreach (var debt in debtArray)
                                {

                                    if (debt != null)
                                    {
                                        string text = (string)debt["#text"];
                                        string amount = (string)debt["@amt"];
                                        string remainingBalance = (string)debt["@rem"];
                                        string tax = (string)debt["@tax"];

                                        debtAmount = Convert.ToDecimal(amount);
                                        debtAmount = Math.Round(debtAmount / 100, 2);
                                        debtTax = Convert.ToDecimal(tax);
                                        debtTax = Math.Round(debtTax / 100, 2);
                                        debtremainingBalance = Convert.ToDecimal(remainingBalance);
                                        debtremainingBalance = Math.Round(debtremainingBalance / 100, 2);

                                        // htmlContent += @"<div style='padding: 20px; border: 1px solid #ccc;'>";
                                        htmlContent += @"<div style='margin-bottom:10px; page-break-inside :avoid;page-break-inside:auto;text-align: center;'><h2>" + text + "</h2></div>";
                                        htmlContent += @"<div style='margin-bottom:10px;margin-top:10px;text-align:left;page-break-inside :avoid;page-break-inside:auto;'>
                     <table style='width: 100%;padding:2px;  border-collapse:collapse;margin-top:20px;page-break-inside :avoid;page-break-inside:auto;'>
                    <tr>
                    <td style='width:50%;vertical-align:top'>
                                <table style='width:100%;page-break-inside :avoid;page-break-inside:auto;'>
                                    <tr style='padding:20px;'>
                                        <td><label style='font-size:11px;margin-left:10px;'>Amount: </label> </td></tr>          
                                   <tr style='padding:20px;'>
                                        <td><label style='font-size:11px;margin-left:10px;'>Tax : </label></td></tr>
                                        <tr style='padding:20px;'>
                                        <td><label  style='font-size:11px;margin-left:10px;'>Remaining Balance: </label></td></tr>
                                </table>
                    </td>
                    <td style='width: 50%;vertical-align:top;'>
                                <table style='width: 100%;page-break-inside :avoid;page-break-inside:auto;'>
                                    <tr style='padding:20px;text-align:right'>
                                        <td><label style='margin-right:10px;font-size:11px'> R " + Convert.ToDecimal(string.Format("{0:F2}", debtAmount)) + @"</label></td><td style='width:5%;'></td>
                                    </tr>
                                    <tr style='padding:20px;text-align:right'>
                                        <td><label style='margin-right:10px;font-size:11px'> R " + Convert.ToDecimal(string.Format("{0:F2}", debtTax)) + @"</label></td><td style='width:5%;'></td>
                                    </tr>
                                    <tr style='padding:20px;text-align:right'>
                                        <td><label style='margin-right:10px;font-size:11px'> R " + Convert.ToDecimal(string.Format("{0:F2}", debtremainingBalance)) + @"</label></td><td style='width:5%;'></td>
                                    </tr>
                                   </table>
                                
        </td>
        </tr>
        </table>
</div>";
                                    }
                                    totalDebtAmount += (debtAmount) + (debtTax);
                                    totalTax += debtTax;
                                }
                            }
                        }
                        else
                        {
                            decimal amount = 0;
                            decimal tax = 0;
                            decimal remainingBalance = 0;
                            htmlContent += @"<div style='margin-bottom: 10px; text-align: center;'><h2> Debt Items </h2></div>";
                            amount = (decimal)jsonObject["ipayMsg"]["elecMsg"]["vendRes"]["debt"]["@amt"];
                            tax = (decimal)jsonObject["ipayMsg"]["elecMsg"]["vendRes"]["debt"]["@tax"];
                            string text = (string)jsonObject["ipayMsg"]["elecMsg"]["vendRes"]["debt"]["#text"];
                            remainingBalance = (decimal)jsonObject["ipayMsg"]["elecMsg"]["vendRes"]["debt"]["@rem"];
                            debtAmount = Convert.ToDecimal(amount);
                            debtAmount = Math.Round(debtAmount / 100, 2);
                            debtTax = Convert.ToDecimal(tax);
                            debtTax = Math.Round(debtTax / 100, 2);
                            debtremainingBalance = Convert.ToDecimal(remainingBalance);
                            debtremainingBalance = Math.Round(debtremainingBalance / 100, 2);


                            totalDebtAmount += (debtAmount);
                            totalTax += debtTax;
                            htmlContent += @"<div style='margin-bottom:10px; page-break-inside :avoid;page-break-inside:auto;text-align: center;'><h2>" + text + "</h2></div>";
                            htmlContent += @"<div style='margin-bottom:10px;margin-top:10px;text-align:left;page-break-inside :avoid;page-break-inside:auto;'>
                         <table style='width: 100%;padding:2px;  border-collapse:collapse;margin-top:20px;page-break-inside :avoid;page-break-inside:auto;'>
                    <tr>
                    <td style='width:50%;vertical-align:top'>
                                <table style='width:100%;page-break-inside :avoid;page-break-inside:auto;'>
                                    <tr style='padding:20px;'>
                                        <td><label style='font-size:11px;margin-left:10px;'>Amount: </label> </td></tr>          
                                   <tr style='padding:20px;'>
                                        <td><label style='font-size:11px;margin-left:10px;'>Tax : </label></td></tr>
                                        <tr style='padding:20px;'>
                                        <td><label  style='font-size:11px;margin-left:10px;'>Remaining Balance: </label></td></tr>
                                </table>
                    </td>
                    <td style='width: 50%;vertical-align:top;'>
                                <table style='width: 100%;page-break-inside :avoid;page-break-inside:auto;'>
                                    <tr style='padding:20px;text-align:right'>
                                        <td><label style='margin-right:10px;font-size:11px'> R " + Convert.ToDecimal(string.Format("{0:F2}", debtAmount)) + @"</label></td><td style='width:5%;'></td>
                                    </tr>
                                    <tr style='padding:20px;text-align:right'>
                                        <td><label style='margin-right:10px;font-size:11px'> R " + Convert.ToDecimal(string.Format("{0:F2}", debtTax)) + @"</label></td><td style='width:5%;'></td>
                                    </tr>
                                <tr style='padding:20px;text-align:right'>
                                        <td><label style='margin-right:10px;font-size:11px'> R " + Convert.ToDecimal(string.Format("{0:F2}", debtremainingBalance)) + @"</label></td><td style='width:5%;'></td>
                                    </tr>
                                   </table>
                </td></tr></table></div>";
                        }
                    }

                    if (containsFixed)
                    {
                        int fixedCount = (Int32)jsonObject["ipayMsg"]["elecMsg"]["vendRes"]["fixed"].Count();
                        decimal fixedAmount = 0;
                        decimal fixedTax = 0;
                        if (fixedCount != 5)
                        {
                            JArray fixedArray = (JArray)jsonObject["ipayMsg"]["elecMsg"]["vendRes"]["fixed"];
                            if (fixedArray != null && fixedArray.Count > 0)
                            {
                                htmlContent += @"<div style='margin-bottom: 20px; text-align: center;page-break-inside :avoid;page-break-inside:auto;'><h2> Fixed Items </h2></div>";
                                foreach (var fix in fixedArray)
                                {

                                    if (fix != null)
                                    {
                                        string text = (string)fix["#text"];
                                        string amount = (string)fix["@amt"];
                                        string tax = (string)fix["@tax"];
                                        fixedAmount = Convert.ToDecimal(amount);
                                        fixedAmount = Math.Round(fixedAmount / 100, 2);
                                        fixedTax = Convert.ToDecimal(tax);
                                        fixedTax = Math.Round(fixedTax / 100, 2);

                                        htmlContent += @"<div style='margin-bottom:10px; page-break-inside :avoid;page-break-inside:auto;text-align: center;'><h2>" + text + "</h2></div>";
                                        htmlContent += @"<div style='margin-bottom:10px;margin-top:10px;text-align:left;page-break-inside :avoid;page-break-inside:auto;'>
                                    <table style='width: 100%;padding:2px;  border-collapse:collapse;margin-top:20px;page-break-inside :avoid;page-break-inside:auto;'>
                    <tr>
                    <td style='width:50%;vertical-align:top'>
                                <table style='width:100%;page-break-inside :avoid;page-break-inside:auto;'>
                                    <tr style='padding:20px;'>
                                        <td><label style='font-size:11px;margin-left:10px;'>Amount: </label> </td></tr>          
                                   <tr style='padding:20px;'>
                                        <td><label style='font-size:11px;margin-left:10px;'>Tax : </label></td></tr>
                                </table>
                    </td>
                    <td style='width: 50%;vertical-align:top;'>
                                <table style='width: 100%;page-break-inside :avoid;page-break-inside:auto;'>
                                    <tr style='padding:20px;text-align:right'>
                                        <td><label style='margin-right:10px;font-size:11px'> R " + Convert.ToDecimal(string.Format("{0:F2}", fixedAmount)) + @"</label></td><td style='width:5%;'></td>
                                    </tr>
                                    <tr style='padding:20px;text-align:right'>
                                        <td><label style='margin-right:10px;font-size:11px'> R " + Convert.ToDecimal(string.Format("{0:F2}", fixedTax)) + @"</label></td><td style='width:5%;'></td>
                                    </tr>
                                
                                   </table>
                     </td></tr>
                   </table>
                </div>
                 <hr />";
                                    }

                                    // totalFixedAmunt += (fixedAmount) + (fixedTax);
                                    totalFixedAmunt += (fixedAmount);
                                    totalTax += fixedTax;
                                }

                            }
                        }
                        else
                        {

                            htmlContent += @"<div style='margin-bottom: 10px; text-align: center;'><h2> Fixed Items </h2></div>";
                            double amount = (double)jsonObject["ipayMsg"]["elecMsg"]["vendRes"]["fixed"]["@amt"];
                            double tax = (double)jsonObject["ipayMsg"]["elecMsg"]["vendRes"]["fixed"]["@tax"];
                            string text = (string)jsonObject["ipayMsg"]["elecMsg"]["vendRes"]["fixed"]["#text"];
                            amount = Math.Round(amount / 100, 2);
                            tax = Math.Round(tax / 100, 2);
                            fixedAmount = Convert.ToDecimal(amount);
                            fixedTax = Convert.ToDecimal(tax);
                            totalFixedAmunt = (fixedAmount) + (fixedTax);
                            totalTax += fixedTax;
                            htmlContent += @"<div style='margin-bottom:10px; page-break-inside :avoid;page-break-inside:auto;text-align: center;'><h2>" + text + "</h2></div>";
                            htmlContent += @"<div style='margin-bottom:10px;margin-top:10px;text-align:left; page-break-inside :avoid;page-break-inside:auto;'>
                    <table style='width: 100%;padding:2px;border-collapse:collapse;margin-top:20px;page-break-inside :avoid;page-break-inside:auto;'>
                    <tr>
                    <td style='width:50%;vertical-align:top'>
                                <table style='width:100%;page-break-inside :avoid;page-break-inside:auto;'>
                                    <tr style='padding:20px;'>
                                        <td><label style='font-size:11px;margin-left:10px;'>Amount: </label> </td></tr>          
                                   <tr style='padding:20px;'>
                                        <td><label style='font-size:11px;margin-left:10px;'>Tax : </label></td></tr>
                                </table>
                    </td>
                    <td style='width: 50%;vertical-align:top;'>
                                <table style='width: 100%;page-break-inside :avoid;page-break-inside:auto;'>
                                    <tr style='padding:20px;text-align:right'>
                                        <td><label style='margin-right:10px;font-size:11px'> R " + Convert.ToDecimal(string.Format("{0:F2}", fixedAmount)) + @"</label></td><td style='width:5%;'></td>
                                    </tr>
                                    <tr style='padding:20px;text-align:right'>
                                        <td><label style='margin-right:10px;font-size:11px'> R " + Convert.ToDecimal(string.Format("{0:F2}", fixedTax)) + @"</label></td><td style='width:5%;'></td>
                                    </tr>
                                   </table>
                 </td></tr>
                                    </table>
                </div>";
                        }
                    }
                }


                htmlContent += @"<div style='margin-bottom: 20px; text-align: center;page-break-inside :avoid;page-break-inside:auto;'>
<h3>UTILITY TRANSACTION FEE</h3>
 </div>
<div style='margin-bottom:10px;margin-top:10px;text-align:left;page-break-inside :avoid;page-break-inside:auto;'>
<table style='width: 100%;padding:2px;border-collapse:collapse;margin-top:20px;page-break-inside :avoid;page-break-inside:auto;'>
    <tr>
        <td style='width:50%;vertical-align:top;text-align:left'>
            <table style='width:100%;page-break-inside :avoid;page-break-inside:auto;'>
                <tr style='padding:20px;'>
                    <td><label style='font-size:11px;margin-left:10px;'>Transaction Fee: </label> 
                    </td>
                </tr>
             </table>
        </td>
        <td style='width: 50%;vertical-align:top;'>
            <table style='width: 100%;page-break-inside :avoid;page-break-inside:auto;'>
                <tr style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px'> R " + receiptDto.TransactionFee + @"</label></td><td style='width:5%;'></td>
                </tr>
            </table>
        </td>
    </tr>
</table>
<hr /></div>
";
                if (!receiptDto.IsInHouseTxn)
                {
                    receiptDto.PurchasePriceExlTax = totalVatExcluding;
                    receiptDto.PurchasePriceInclTax = totalVatIncluding;
                    receiptDto.PurchasePriceInclTax = Convert.ToDecimal(string.Format("{0:F2}", receiptDto.PurchasePriceInclTax));

                    htmlContent += @"<div class='hidden' style='margin-bottom:10px;margin-top:10px;text-align:left;page-break-inside :avoid;page-break-inside:auto;'>
<table class='hidden' style='width: 100%;padding:2px;border-collapse:collapse;margin-top:20px;page-break-inside :avoid;page-break-inside:auto;'>
            <tr>
            <td style='width:50%;vertical-align: top;text-align:left'>
            <table style='width:100%;page-break-inside :avoid;page-break-inside:auto;'>
                <tr style='padding:20px;'>
                    <td><label style='margin-left:10px;font-size:11px;'>Total (VAT Excl.): </label> </td></tr>
                    <tr style='padding:20px;'>
                    <td><label style='margin-left:10px;font-size:11px;'>Total (VAT Incl.): </label> </td></tr>
                      </table>
        </td>
        <td style='width: 50%;vertical-align:top;'>
            <table style='width: 100%;page-break-inside :avoid;page-break-inside:auto;'>
                <tr style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px;'> R " + receiptDto.PurchasePriceExlTax + @"</label></td><td style='width:5%;'></td>
                </tr>
            <tr style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px;'> R " + receiptDto.PurchasePriceInclTax + @"</label></td><td style='width:5%;'></td>
                </tr>
            </table>
        </td>
    </tr>
</table>
<hr />
</div>";
                }
                if (!string.IsNullOrEmpty(receiptDto.StandardTokens) || totalDebtAmount > 0 || totalFixedAmunt > 0 || totalTax > 0)
                {

                    // totalVatIncluding = (receiptDto.ActualRechargeAmount - receiptDto.TransactionFee);
                    //- (totalDebtAmount) - totalFixedAmunt;
                    if (receiptDto.stdamt.HasValue)
                    {
                        receiptDto.PurchasePriceExlTax = totalDebtAmount + totalFixedAmunt + receiptDto.stdamt.Value;
                    }
                    else
                    {
                        receiptDto.PurchasePriceExlTax = totalDebtAmount + totalFixedAmunt;
                    }
                    receiptDto.PurchasePriceExlTax = Math.Round(receiptDto.PurchasePriceExlTax, 2);
                    receiptDto.PurchasePriceInclTax = receiptDto.PurchasePriceExlTax + totalTax;
                    // receiptDto.PurchasePriceExlTax = (receiptDto.ActualRechargeAmount - receiptDto.TransactionFee) - (totalTax);
                    // receiptDto.PurchasePriceInclTax =  Convert.ToDecimal(string.Format("{0:F2}", totalVatIncluding));
                    receiptDto.PurchasePriceInclTax = Convert.ToDecimal(string.Format("{0:F2}", receiptDto.PurchasePriceInclTax));

                    htmlContent += @"<div style='margin-bottom:10px;margin-top:10px;text-align:left;page-break-inside :avoid;page-break-inside:auto;'>
<table style='width: 100%;padding:2px;border-collapse:collapse;margin-top:20px;page-break-inside :avoid;page-break-inside:auto;'>
            <tr>
            <td style='width:50%;vertical-align: top;text-align:left'>
            <table style='width:100%;page-break-inside :avoid;page-break-inside:auto;'>
                <tr style='padding:20px;'>
                    <td><label style='font-size:11px;margin-left:10px;'>Total (VAT Excl.): </label> </td></tr>
                    <tr style='padding:20px;'>
                    <td><label style='font-size:11px;margin-left:10px;'>Total (VAT Incl.): </label> </td></tr>
                      </table>
        </td>
        <td style='width: 50%;vertical-align:top;'>
            <table style='width: 100%;page-break-inside :avoid;page-break-inside:auto;'>
                <tr style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px'> R " + receiptDto.PurchasePriceExlTax + @"</label></td><td style='width:5%;'></td>
                </tr>
            <tr style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px'> R " + receiptDto.PurchasePriceInclTax + @"</label></td><td style='width:5%;'></td>
                </tr>
            </table>
        </td>
    </tr>
</table>
<hr />

</div>";
                }

                else
                {
                    receiptDto.PurchasePriceExlTax = totalDebtAmount + totalFixedAmunt;
                    receiptDto.PurchasePriceExlTax = Math.Round(receiptDto.PurchasePriceExlTax, 2);
                    receiptDto.PurchasePriceInclTax = receiptDto.PurchasePriceExlTax + totalTax;
                    // receiptDto.PurchasePriceExlTax = (receiptDto.ActualRechargeAmount - receiptDto.TransactionFee) - (totalTax);
                    // receiptDto.PurchasePriceInclTax =  Convert.ToDecimal(string.Format("{0:F2}", totalVatIncluding));
                    receiptDto.PurchasePriceInclTax = Convert.ToDecimal(string.Format("{0:F2}", receiptDto.PurchasePriceInclTax));

                    htmlContent += @"<div class='hidden' style='margin-bottom:10px;margin-top:10px;text-align:left;page-break-inside :avoid;page-break-inside:auto;'>
<table class='hidden' style='width: 100%;padding:2px;border-collapse:collapse;margin-top:20px;page-break-inside :avoid;page-break-inside:auto;'>
            <tr>
            <td style='width:50%;vertical-align: top;text-align:left'>
            <table style='width:100%;page-break-inside :avoid;page-break-inside:auto;'>
                <tr style='padding:20px;'>
                    <td><label style='margin-left:10px;font-size:11px;'>Total (VAT Excl.): </label> </td></tr>
                    <tr style='padding:20px;'>
                    <td><label style='margin-left:10px;font-size:11px;'>Total (VAT Incl.): </label> </td></tr>
                      </table>
        </td>
        <td style='width: 50%;vertical-align:top;'>
            <table style='width: 100%;page-break-inside :avoid;page-break-inside:auto;'>
                <tr style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px;'> R " + receiptDto.PurchasePriceExlTax + @"</label></td><td style='width:5%;'></td>
                </tr>
            <tr style='padding:20px;text-align:right'>
                    <td><label style='margin-right:10px;font-size:11px;'> R " + receiptDto.PurchasePriceInclTax + @"</label></td><td style='width:5%;'></td>
                </tr>
            </table>
        </td>
    </tr>
</table>
<hr />
</div>";
                }

                if (!string.IsNullOrEmpty(receiptDto.CustomerMessage))
                {
                    htmlContent += @"<div style='margin-bottom: 20px;page-break-inside :avoid;page-break-inside:auto; text-align: center;'>
<label style='font-size:11px;text-align:center;'> " + receiptDto.CustomerMessage + @"</label></div>
</div></div></div>";
                }

            }

            //PdfGenerator.AddPdfPages(data, htmlContent, PageSize.A4);

            var mobileView = new HtmlToPdf();
            mobileView.Options.WebPageWidth = 480;

            var tabletView = new HtmlToPdf();
            tabletView.Options.WebPageWidth = 1024;

            //var fullView = new HtmlToPdf();
            //fullView.Options.WebPageWidth = 1920;
            //var htmlToPdf = new HtmlToPdf(1000, 1414);
            //htmlToPdf.Options.DrawBackground = true;

            //PdfGenerator.AddPdfPages(data, htmlContent, PageSize.A4);
            //HtmlToPdf converter = new HtmlToPdf();
            var fullView = new HtmlToPdf();
            fullView.Options.WebPageWidth = 1024;
            fullView.Options.MinPageLoadTime = 1;
            fullView.Options.MaxPageLoadTime = 3;
            fullView.Options.WebPageFixedSize = false;
            fullView.Options.PdfPageOrientation = PdfPageOrientation.Portrait;

            fullView.Options.AutoFitWidth = HtmlToPdfPageFitMode.ShrinkOnly;
            fullView.Options.AutoFitHeight = HtmlToPdfPageFitMode.NoAdjustment;

            SelectPdf.PdfDocument doc = fullView.ConvertHtmlString(htmlContent);



            byte[] response;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    doc.Save(ms)
;
                    response = ms.ToArray();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response;


        }

        public async Task<string> GetTransactionNoFromRctNum(string rctNum)
        {
            var sQuery = @"SELECT transaction_id  FROM  public.ohd_top_up_transactions 
                        WHERE receipt_number=@RctNum";
            var parameters = new DynamicParameters();
            parameters.Add("@RctNum", rctNum);
            string rctNumber = await _genericRepository.ExecuteScalarAsync<string>(sQuery, parameters).ConfigureAwait(false);
            return rctNumber;
        }
        public async Task<int> ISVendResponseExist(string vendResponse)
        {
            var sQuery = @"SELECT id  FROM  public.ohd_top_up_transactions 
                        WHERE vend_response=@VendResponse";
            var parameters = new DynamicParameters();
            parameters.Add("@VendResponse", vendResponse);
            int id = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return id;
        }
        public async Task<int> ISRCTNoExist(string RctNo)
        {
            var sQuery = @"SELECT id  FROM  public.ohd_top_up_transactions 
                        WHERE  receipt_number=@ReceNo";
            var parameters = new DynamicParameters();
            parameters.Add("@ReceNo", RctNo);
            int id = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return id;
        }
        public async Task<double> GetTransactionNoFeeFromRctNum(string rctNum)
        {
            var sQuery = @"SELECT transaction_fee  FROM  public.ohd_top_up_transactions 
                        WHERE receipt_number=@RctNum";
            var parameters = new DynamicParameters();
            parameters.Add("@RctNum", rctNum);
            double txnFee = await _genericRepository.ExecuteScalarAsync<double>(sQuery, parameters).ConfigureAwait(false);
            return txnFee;
        }



        public async Task<int> AddTopupTransactionsFromSTSResponse(AddSTSTopUpHelper helper)
        {

            var sQuery = @" INSERT INTO public.ohd_top_up_transactions(
                                transaction_id, 
                                user_id, 
                                meter_id, 
                                created_at, 
                                std_token, 
                                vend_response, 
                                key_change_token, 
                                bsst_token, 
                                mrktmsg, 
                                tarrif,
                                customermsg, 
                                receipt_number,
                                topup_status,
                                is_in_house_txn
                                )
                                VALUES (
                                @TransactionId,
                                @UserId,
                                @MeterId,
                                @CreatedAt,
                                @StdToken,
                                @VendResponse,
                                @KeyChangeToken,
                                @BsstToken,
                                @MrktMsg,
                                @Tarrif,
                                @CustomerMsg,
                                @RCTNumber,
                                @ToupStatus,
                                @IsInHouseTxn
                                )RETURNING lastval()";
            var parameters = new DynamicParameters();
            parameters.Add("@TransactionId", helper.TransactionId);
            parameters.Add("@UserId", helper.UserId);
            parameters.Add("@MeterId", helper.MeterId);
            parameters.Add("@VendResponse", helper.vendResponse);
            if (!string.IsNullOrEmpty(helper.TxnDate))
            {
                parameters.Add("@CreatedAt", DateTime.Parse(helper.TxnDate));
            }
            else
            {
                parameters.Add("@CreatedAt", null);
            }
            parameters.Add("@StdToken", helper.StdToken);
            parameters.Add("@DebtAmount", helper.DebtAmount);
            parameters.Add("@KeyChangeToken", helper.KeyChangeToken);
            parameters.Add("@BsstToken", helper.BsstToken);
            parameters.Add("@MrktMsg", helper.MrktMsg);
            parameters.Add("@Tarrif", helper.TarrifUnits);
            parameters.Add("@CustomerMsg", helper.CustomerMsg);
            parameters.Add("@RCTNumber", helper.RCTNumber);
            parameters.Add("@ToupStatus", helper.Message);
            parameters.Add("@IsInHouseTxn", false);
            try
            {
                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }


        }


        //  public async Task<GetSTSTopUpTransactions> GetSTSTopUps(List<string> RecNum, DateTime fromDate, DateTime toDate)
        //  {
        //      var stsTxn = new GetSTSTopUpTransactions();
        //      try
        //      {
        //          var sQuery = @"SELECT  tut.transaction_id AS TransactionId,
        //                         mt.meter_number AS MeterNumber,  
        //                          tut.user_id As UserId, 
        //                          tut.meter_id AS MeterId, 
        //                          tut.debt_amount as DebtAmount,
        //                          tut.transaction_id AS TxnId,
        //                          Concat('Successfull' ,' ', tut.created_at) AS CreatedAt , 
        //                         tut.std_token AS StdToken,
        //                          tut.bsst_token AS BsstToken,
        //                          tut.key_change_token AS KeyChangeToken,
        //                           CASE  WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' IS NOT NULL THEN
        //                      (tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'vendRes'->'stdToken'->>'@units' 
        //                      WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' ->'reprintManyRes'-> 'vendRes' IS NOT NULL THEN 
        //                      (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' ->'reprintManyRes' -> 'vendRes'->'stdToken'->>'@units' 
        //                       ELSE NULL END AS stdUnits,
        //                      CASE  WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'vendRes' IS NOT NULL THEN
        //                      ((tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'vendRes'->'stdToken'->>'@amt')::NUMERIC / 100
        //                      WHEN (tut.vend_response::jsonb) -> 'ipayMsg' -> 'elecMsg' -> 'reprintManyRes'->'vendRes' IS NOT NULL THEN 
        //                      ((tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' ->'reprintManyRes' -> 'vendRes'->'stdToken'->>'@amt' )::NUMERIC / 100
        //                       ELSE NULL END AS stdAmt,
        //                        --  (tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' ->'reprintManyRes' -> 'vendRes'->'stdToken'->>'@amt' AS stdAmt,
        //                      --(tut.vend_response::jsonb) -> 'ipayMsg' ->'elecMsg' ->'reprintManyRes' -> 'vendRes'->'stdToken'->>'@units' AS stdUnits,
        //                  tut.tarrif as Tarrif,tut.is_in_house_txn AS IsInHouseTransaction
        //               FROM public.ohd_top_up_transactions as tut
        //LEFT JOIN ohd_meter AS mt ON  mt.id=tut.meter_id 
        //                  WHERE tut.receipt_number= ANY(@rctNumList) 
        //                  AND tut.created_at>=@FromDate AND tut.created_at <=@ToDate 
        //                  order by tut.created_at desc";

        //          var parameters = new DynamicParameters();
        //          parameters.Add("@rctNumList", RecNum);
        //          parameters.Add("@ToDate", toDate);
        //          parameters.Add("@FromDate", fromDate);
        //          var result = await _genericRepository.GetAsync<GetSTSTopUpTransactions.STSTopUpTransactions>(sQuery, parameters).ConfigureAwait(false);

        //          double totalPurchase = 0;
        //          if (result != null)
        //          {
        //              foreach (var transaction in result)
        //              {

        //                  transaction.TarrifUnits = new List<string>();
        //                  totalPurchase += transaction.stdUnits;
        //                  if (!string.IsNullOrEmpty(transaction.Tarrif))
        //                  {
        //                      string[] units = transaction.Tarrif.Split(":", StringSplitOptions.RemoveEmptyEntries);
        //                      foreach (string unit in units)
        //                      {
        //                          if (unit != null)
        //                          {
        //                              transaction.TarrifUnits.Add(unit);
        //                          }
        //                      }
        //                  }
        //              }
        //              stsTxn.TotalPurchase = totalPurchase;
        //              stsTxn.STSTopUpTransaction = result.ToList();


        //          }
        //      }
        //      catch (Exception ex)
        //      {
        //          throw new Exception(ex.ToString());
        //      }
        //      return stsTxn;
        //  }


        //public async Task<GetSTSTopUpTransactions> GetSTSTopUps(List<string> RecNum, DateTime fromDate, DateTime toDate)
        //{
        //    var stsTxn = new GetSTSTopUpTransactions();
        //    try
        //    {
        //        var sQuery = @"SELECT
        //                        tut.transaction_id AS TransactionId,
        //                        mt.meter_number AS MeterNumber,
        //                        tut.user_id As UserId,
        //                        tut.meter_id AS MeterId,
        //                       tut.is_in_house_txn As IsInHouseTransaction,
        //                        tut.transaction_id AS TxnId,
        //                        CONCAT('Successfull' ,' ', tut.created_at) AS CreatedAt,
        //                        tut.std_token AS StdToken,
        //                        tut.bsst_token AS BsstToken,
        //                        tut.key_change_token AS KeyChangeToken,
        //                        CASE
        //                            WHEN tut.vend_response LIKE '{%' 
        //                            THEN(tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'vendRes'->'stdToken'->>'@units'
        //                            WHEN tut.vend_response LIKE '{%' 
        //                            THEN (tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'reprintManyRes'->'vendRes'->'stdToken'->>'@units'
        //                            ELSE NULL
        //                        END AS stdUnits,
        //                        CASE
        //                            WHEN tut.vend_response LIKE '{%' 
        //                            THEN ((tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'vendRes'->'stdToken'->>'@amt')::NUMERIC / 100
        //                            WHEN tut.vend_response LIKE '{%' 
        //                            THEN ((tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'reprintManyRes'->'vendRes'->'stdToken'->>'@amt')::NUMERIC / 100
        //                            ELSE NULL
        //                        END AS stdAmt,
        //                     CASE
        //                            WHEN tut.vend_response LIKE '{%' 
        //                            THEN(tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'vendRes'->'debt'->>'@tax'
        //                            WHEN tut.vend_response LIKE '{%' 
        //                            THEN (tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'reprintManyRes'->'vendRes'->'debt'->>'@tax'
        //                            ELSE NULL
        //                        END AS debtTax,
        //                       CASE WHEN tut.is_in_house_txn THEN tut.debt_amount 
        //                       ELSE 
        //                                CASE
        //                                    WHEN tut.vend_response LIKE '{%' 
        //                                    THEN ((tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'vendRes'->'debt'->>'@amt')::NUMERIC / 100
        //                                    WHEN tut.vend_response LIKE '{%' 
        //                                    THEN ((tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'reprintManyRes'->'vendRes'->'debt'->>'@amt')::NUMERIC / 100
        //                                    END

        //                        END AS  DebtAmount,
        //                     CASE
        //                            WHEN tut.vend_response LIKE '{%' 
        //                            THEN(tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'vendRes'->'tariff'->>'#text'
        //                            WHEN tut.vend_response LIKE '{%' 
        //                            THEN (tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'reprintManyRes'->'vendRes'->'tariff'->>'#text'
        //                            ELSE NULL
        //                        END AS AMIMeterTarrif,
        //                            tut.tarrif as Tarrif,
        //                        tut.is_in_house_txn AS IsInHouseTransaction
        //                    FROM public.ohd_top_up_transactions AS tut
        //                            LEFT JOIN ohd_meter AS mt ON mt.id = tut.meter_id
        //                    WHERE tut.vend_response IS NOT NULL
        //                    AND tut.receipt_number= ANY(@rctNumList) 
        //                      AND tut.vend_response LIKE '{%'  -- only process JSON-like rows
        //                    ORDER BY tut.created_at DESC";
        //        var parameters = new DynamicParameters();

        //        parameters.Add("@rctNumList", RecNum);
        //        parameters.Add("@ToDate", toDate);
        //        parameters.Add("@FromDate", fromDate);
        //        var result = await _genericRepository.GetAsync<GetSTSTopUpTransactions.STSTopUpTransactions>(sQuery, parameters).ConfigureAwait(false);

        //        double totalPurchase = 0;
        //        if (result != null)
        //        {
        //            foreach (var transaction in result)
        //            {
        //                transaction.TarrifUnits = new List<string>();
        //                totalPurchase += transaction.stdUnits;
        //                if (!string.IsNullOrEmpty(transaction.Tarrif))
        //                {
        //                    string[] units = transaction.Tarrif.Split(":", StringSplitOptions.RemoveEmptyEntries);
        //                    foreach (string unit in units)
        //                    {
        //                        if (unit != null)
        //                        {
        //                            transaction.TarrifUnits.Add(unit);
        //                        }
        //                    }
        //                }
        //            }
        //            stsTxn.TotalPurchase = totalPurchase;
        //            stsTxn.STSTopUpTransaction = result.ToList();


        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.ToString());
        //    }
        //    return stsTxn;
        //}
        public async Task<GetSTSTopUpTransactions> GetSTSTopUps(List<string> RecNum, DateTime fromDate, DateTime toDate)
        {
            decimal totalDebtAmount = 0;
            decimal totalFixedAmunt = 0;
            decimal totalVatExcluding = 0;
            decimal totalVatIncluding = 0;
            decimal stdAmt = 0;
            string keyChangeToken = "";
            string bsstToken = "";
            string tariff = "";
            StringBuilder resultStdtoken = new StringBuilder(); ;
            decimal stdUnits = 0;

            decimal stdTax = 0;
            decimal totalTax = 0;
            var stsTxn = new GetSTSTopUpTransactions();
            try
            {
                var sQuery = @"SELECT
                                tut.transaction_id AS TransactionId,
                                mt.meter_number AS MeterNumber,
                                tut.user_id As UserId,
                                tut.meter_id AS MeterId,
                               tut.is_in_house_txn As IsInHouseTransaction,
                                tut.transaction_id AS TxnId,
                                CONCAT('Successfull' ,' ', tut.created_at) AS CreatedAt,
                                tut.std_token AS StdToken,
                                tut.bsst_token AS BsstToken,
                                tut.key_change_token AS KeyChangeToken,
                                tut.vend_response AS VendResponse,
                                transaction_fee AS TransactionFee,
	                            CASE
                                    WHEN tut.vend_response LIKE '{%' 
                                    THEN(tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'vendRes'->'tariff'->>'#text'
                                    WHEN tut.vend_response LIKE '{%' 
                                    THEN (tut.vend_response::jsonb)->'ipayMsg'->'elecMsg'->'reprintManyRes'->'vendRes'->'tariff'->>'#text'
                                    ELSE NULL
                                END AS AMIMeterTarrif,
                                    tut.tarrif as Tarrif,
                                tut.is_in_house_txn AS IsInHouseTransaction,
                                tut.payment_gateway AS PaymentGateWay
                            FROM public.ohd_top_up_transactions AS tut
                                    LEFT JOIN ohd_meter AS mt ON mt.id = tut.meter_id
                            WHERE tut.vend_response IS NOT NULL
                            AND tut.receipt_number= ANY(@rctNumList) 
                              AND tut.vend_response LIKE '{%'  -- only process JSON-like rows
                            ORDER BY tut.created_at DESC";
                var parameters = new DynamicParameters();

                parameters.Add("@rctNumList", RecNum);
                parameters.Add("@ToDate", toDate);
                parameters.Add("@FromDate", fromDate);
                var result = await _genericRepository.GetAsync<GetSTSTopUpTransactions.STSTopUpTransactions>(sQuery, parameters).ConfigureAwait(false);

                decimal totalPurchase = 0;
                if (result != null)
                {
                    foreach (var transaction in result)
                    {
                        transaction.TarrifUnits = new List<string>();
                        totalPurchase += transaction.stdUnits;
                        if (!string.IsNullOrEmpty(transaction.Tarrif))
                        {
                            string[] units = transaction.Tarrif.Split(":", StringSplitOptions.RemoveEmptyEntries);
                            foreach (string unit in units)
                            {
                                if (unit != null)
                                {
                                    transaction.TarrifUnits.Add(unit);
                                }
                            }
                        }
                        if (transaction.VendResponse != null)
                        {
                            JObject jsonObject = JObject.Parse(transaction.VendResponse);

                            var vendRes = jsonObject["ipayMsg"]?["elecMsg"]?["reprintManyRes"]?["vendRes"]
                                         ?? jsonObject["ipayMsg"]?["elecMsg"]?["vendRes"];


                            bool containsVend = vendRes != null;
                            bool containsDebt = vendRes?["debt"] != null;
                            bool containsFixed = vendRes?["fixed"] != null;
                            bool containStdToken = vendRes?["stdToken"] != null;
                            bool containbsstToken = vendRes?["bsstToken"] != null;
                            bool containkeyChangeToken = vendRes?["keyChangeToken"] != null;

                            bool isContainTariffToken = vendRes?["tariff"] != null;

                            if (isContainTariffToken)
                            {
                                JToken tariffToken = vendRes?["tariff"];
                                bool hasTextKey = tariffToken != null && tariffToken.Type == JTokenType.Object && tariffToken["#text"] != null;
                                if (hasTextKey)
                                {
                                    bool hasTextKeyWithValue = tariffToken != null && tariffToken.Type == JTokenType.Object && tariffToken["#text"] != null && !string.IsNullOrEmpty(tariffToken["#text"].ToString());
                                    if (hasTextKeyWithValue)
                                    {
                                        tariff = (string)vendRes["tariff"]["#text"];
                                    }

                                }
                                else
                                {
                                    tariff = (string)vendRes["tariff"];
                                }
                                transaction.AMIMeterTarrif = tariff;
                            }
                            if (containsDebt)
                            {
                                decimal debtAmount = 0;
                                decimal debtTax = 0;
                                int debtCount = (Int32)vendRes["debt"].Count();

                                if (debtCount != 8)
                                {
                                    JArray debtArray = (JArray)vendRes["vendRes"]["debt"];
                                    if (debtArray != null && debtArray.Count > 0)
                                    {

                                        foreach (var debt in debtArray)
                                        {

                                            if (debt != null)
                                            {
                                                string text = (string)debt["#text"];
                                                string amount = (string)debt["@amt"];
                                                string remainingBalance = (string)debt["@rem"];
                                                string tax = (string)debt["@tax"];

                                                debtAmount = Convert.ToDecimal(amount);
                                                debtAmount = Math.Round(debtAmount / 100, 2);
                                                debtTax = Convert.ToDecimal(tax);
                                                debtTax = Math.Round(debtTax / 100, 2);
                                                decimal debtremainingBalance = Convert.ToDecimal(remainingBalance);
                                                debtremainingBalance = Math.Round(debtremainingBalance / 100, 2);

                                                totalDebtAmount += (debtAmount) + (debtTax);
                                                totalTax += debtTax;
                                            }

                                        }
                                    }
                                }
                                else
                                {
                                    decimal amount = 0;
                                    decimal tax = 0;
                                    decimal remainingBalance = 0;
                                    amount = (decimal)vendRes["debt"]["@amt"];
                                    tax = (decimal)vendRes["debt"]["@tax"];
                                    string text = (string)vendRes["debt"]["#text"];
                                    remainingBalance = (decimal)vendRes["debt"]["@rem"];
                                    debtAmount = Convert.ToDecimal(amount);
                                    debtAmount = Math.Round(debtAmount / 100, 2);
                                    debtTax = Convert.ToDecimal(tax);
                                    debtTax = Math.Round(debtTax / 100, 2);
                                    decimal debtremainingBalance = Convert.ToDecimal(remainingBalance);
                                    debtremainingBalance = Math.Round(debtremainingBalance / 100, 2);


                                    totalDebtAmount += (debtAmount) + (debtTax);
                                    totalTax += debtTax;
                                }
                            }
                            if (containsFixed)
                            {
                                int fixedCount = (Int32)vendRes["fixed"].Count();
                                decimal fixedAmount = 0;
                                decimal fixedTax = 0;
                                if (fixedCount != 5)
                                {
                                    JArray fixedArray = (JArray)vendRes["fixed"];
                                    if (fixedArray != null && fixedArray.Count > 0)
                                    {
                                        foreach (var fix in fixedArray)
                                        {

                                            if (fix != null)
                                            {
                                                string text = (string)fix["#text"];
                                                string amount = (string)fix["@amt"];
                                                string tax = (string)fix["@tax"];
                                                fixedAmount = Convert.ToDecimal(amount);
                                                fixedAmount = Math.Round(fixedAmount / 100, 2);
                                                fixedTax = Convert.ToDecimal(tax);
                                                fixedTax = Math.Round(fixedTax / 100, 2);
                                                totalFixedAmunt += (fixedAmount);
                                                totalTax += fixedTax;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    double amount = (double)vendRes["fixed"]["@amt"];
                                    double tax = (double)vendRes["fixed"]["@tax"];
                                    string text = (string)vendRes["fixed"]["#text"];
                                    amount = Math.Round(amount / 100, 2);
                                    tax = Math.Round(tax / 100, 2);
                                    fixedAmount = Convert.ToDecimal(amount);
                                    fixedTax = Convert.ToDecimal(tax);

                                    totalFixedAmunt += (fixedAmount);
                                    totalTax += fixedTax;
                                }

                            }
                            if (containStdToken)
                            {
                                stdAmt = (decimal)vendRes["stdToken"]["@amt"];
                                stdUnits = (decimal)vendRes["stdToken"]["@units"];
                                stdTax = (decimal)vendRes["stdToken"]["@tax"];
                                string text = (string)vendRes["stdToken"]["#text"];
                                if (!string.IsNullOrEmpty(text))
                                {
                                    int n = 4;
                                    for (int i = 0; i < text.Length; i += n)
                                    {
                                        if (i > 0)
                                        {
                                            resultStdtoken.Append(' ');
                                        }
                                        resultStdtoken.Append(text.Substring(i, 4));
                                    }
                                }

                                totalTax += stdTax;
                                transaction.stdAmt = stdAmt / 100;
                                transaction.stdUnits = stdUnits;
                                transaction.StdToken = resultStdtoken.ToString();

                            }
                            if (vendRes?["keyChangeToken"] != null)
                            {
                                var keyChangeCodes = vendRes["keyChangeToken"]["code"].Select(c => (string)c);
                                keyChangeToken = string.Join(",", keyChangeCodes);
                            }

                            if (vendRes?["bsstToken"] != null)
                            {
                                bsstToken = (string)vendRes["bsstToken"]["#text"];
                            }
                        }

                        transaction.DebtAmount = totalDebtAmount + totalFixedAmunt + transaction.TransactionFee;
                        transaction.debtTax = totalTax;
                    }
                    stsTxn.TotalPurchase = totalPurchase;
                    stsTxn.STSTopUpTransaction = result.ToList();


                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return stsTxn;
        }


        public async Task<string> GetIPayMethodByPaymentMethodId(int id)
        {
            var sQuery = @"SELECT ipaymethod FROM 
                           public.ohd_payment_methods
                           WHERE id=@Id";
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            var result = await _genericRepository.GetFirstOrDefaultAsync<string>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }

        public async Task<int> UpdateTopupTransactionFee(double rechargeAmt, double txnFee, string TxnId)
        {

            var sQuery = @" UPDATE public.ohd_top_up_transactions
	                        SET transaction_fee=@TxnFee
                            ,recharge_amount=@RechargeAmount
                            ,modified_at=@ModifiedAt
                            WHERE transaction_id=@TransactionId;
                            SELECT id FROM  ohd_top_up_transactions
                            WHERE transaction_id=@TransactionId;";
            var parameters = new DynamicParameters();

            parameters.Add("@RechargeAmount", rechargeAmt);

            parameters.Add("@TxnFee", txnFee);
            parameters.Add("@TransactionId", TxnId);

            parameters.Add("@ModifiedAt", DateTime.UtcNow);
            try
            {
                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return 0;

            }

        }
        public async Task<int> SaveDebitechgetNetChecksumRequest(GetNetCheckSumQuery request, string checksum)
        {
            var sQuery = @"INSERT INTO public.ohd_debitech_getnetchecksum_requests(
                            request,
                            response,
                            created_at)
                            VALUES
                            (
                            @Request,
                            @CheckSum,
                            @CreatedAt
                            ) RETURNING lastval()";
            var parameters = new DynamicParameters();
            parameters.Add("@Request", JsonConvert.SerializeObject(request));
            parameters.Add("@CheckSum", checksum);
            parameters.Add("@CreatedAt", DateTime.UtcNow);

            try
            {
                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public async Task<int> SaveDebitechNotifyRequest(BankNotificationRequestQuery request)
        {
            var sQuery = @"INSERT INTO public.ohd_debitech_notify_requests(
                            request,
                            created_at)
                            VALUES
                            (
                            @Request,
                            @CreatedAt
                            ) RETURNING lastval()";
            var parameters = new DynamicParameters();
            parameters.Add("@Request", JsonConvert.SerializeObject(request));
            parameters.Add("@CreatedAt", DateTime.UtcNow);

            try
            {
                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public async Task<int> UpdateDebitechNotifyResponse(List<BankNotificationResponseDto> response, int debitechNotifyId)
        {
            var sQuery = @"Update public.ohd_debitech_notify_requests
                          SET response=@Response,
                            modified_at= @ModifiedAt
                            WHERE id=@Id;
                            SELECT id FROM public.ohd_debitech_notify_requests 
                            WHERE id=@Id;";
            var parameters = new DynamicParameters();
            parameters.Add("@Response", JsonConvert.SerializeObject(response));
            parameters.Add("@ModifiedAt", DateTime.UtcNow);
            parameters.Add("@Id", debitechNotifyId);

            try
            {
                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        static void LogToFile(string filePath, string message)
        {

            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            string fileName = "topUpError.txt";
            filePath = Path.Combine(filePath, fileName);
            if (!File.Exists(filePath))
            {
                // Create the file
                using (FileStream fs = File.Create(filePath))
                {
                    // Optionally write something to the file
                    byte[] content = new UTF8Encoding(true).GetBytes("File created successfully.");
                    fs.Write(content, 0, content.Length);
                }
                Console.WriteLine("File created.");
            }
            // Append log with timestamp
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}{Environment.NewLine}";
            File.AppendAllText(filePath, logEntry); // Append log to file
        }

        private async Task<byte[]> GeneratePdfwithDynamicHtml(PurchaseReceiptDto receiptDto)
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            decimal totalDebtAmount = 0;
            decimal totalFixedAmunt = 0;
            decimal totalVatExcluding = 0;
            decimal totalVatIncluding = 0;
            decimal totalTax = 0;
            string htmlContent = "";
            var htmlTemplate = "";
            StringBuilder resultBsstToken = new StringBuilder();
            StringBuilder resultStdtoken = new StringBuilder();
            StringBuilder resultKeychangeToken = new StringBuilder();
            //var topup = new EmailTemplateDto();
            if (receiptDto != null)
            {
                if (!string.IsNullOrEmpty(receiptDto.KeyChangeToken))
                {
                    string jsonString = receiptDto.KeyChangeToken;

                    if (!string.IsNullOrEmpty(jsonString))
                    {
                        List<string> list = jsonString.Split(',').ToList();
                        if (list != null && list.Count > 0)
                        {

                            int cumulativeLength = 0; // Track cumulative length

                            foreach (string item in list)
                            {
                                int n = 4;
                                for (int i = 0; i < item.Length; i += n)
                                {
                                    if (i > 0 && cumulativeLength < 20)
                                    {
                                        resultKeychangeToken.Append(' ');
                                        cumulativeLength += 1;
                                    }

                                    string substring = item.Substring(i, Math.Min(n, item.Length - i));
                                    resultKeychangeToken.Append(substring);
                                    cumulativeLength += substring.Length;

                                    // Add a new line if cumulative length exceeds a certain limit (e.g., 20 characters)
                                    if (cumulativeLength >= 20)
                                    {
                                        resultKeychangeToken.Append("<br>");
                                        cumulativeLength = 0; // Reset cumulative length
                                    }
                                }

                                // Add a new line after processing each item, if the item length is exactly 20
                                if (item.Length == 20 && cumulativeLength != 0)
                                {
                                    resultKeychangeToken.Append("<br>");
                                    cumulativeLength = 0; // Reset cumulative length
                                }
                            }

                        }

                    }
                }

                if (!string.IsNullOrEmpty(receiptDto.BsstToken))
                {
                    //receiptDto.StandardTokens= string.Format(receiptDto.StandardTokens,)

                    int n = 4;
                    for (int i = 0; i < receiptDto.BsstToken.Length; i += n)
                    {
                        if (i > 0)
                        {
                            resultBsstToken.Append(' ');
                        }
                        resultBsstToken.Append(receiptDto.BsstToken.Substring(i, 4));

                    }
                }
                if (receiptDto.IsInHouseTxn && !string.IsNullOrEmpty(receiptDto.StandardTokens))
                {
                    receiptDto.stdamt = Math.Round(Convert.ToDecimal(string.Format("{0:F2}", receiptDto.stdamt)) / 100, 2);

                    receiptDto.stdtax = Math.Round(Convert.ToDecimal(string.Format("{0:F2}", receiptDto.stdtax)) / 100, 2);

                }
                if (!string.IsNullOrEmpty(receiptDto.StandardTokens))
                {
                    int n = 4;
                    for (int i = 0; i < receiptDto.StandardTokens.Length; i += n)
                    {
                        if (i > 0)
                        {
                            resultStdtoken.Append(' ');
                        }
                        resultStdtoken.Append(receiptDto.StandardTokens.Substring(i, 4));
                    }
                }
                if (!string.IsNullOrEmpty(receiptDto.BsstToken))
                {
                    receiptDto.Amount = Math.Round(Convert.ToDecimal(string.Format("{0:F2}", receiptDto.BsstTokenAmount)) / 100, 2);
                    receiptDto.BsstTokenTax = Math.Round(Convert.ToDecimal(string.Format("{0:F2}", receiptDto.BsstTokenTax)) / 100, 2);
                    receiptDto.BsstTokenUnits = Math.Round(Convert.ToDecimal(string.Format("{0:F2}", receiptDto.BsstTokenUnits)));
                    totalVatIncluding = (decimal)(receiptDto.Amount + receiptDto.BsstTokenTax);
                    totalTax += (decimal)receiptDto.BsstTokenTax;

                }


                var requestPath = _httpContextAccessor.HttpContext.Request;
                var domain = $"{requestPath.Scheme}://{requestPath.Host}";
                string wwwPath = _environment.WebRootPath;
                //string contentPath = _environment.ContentRootPath;
                receiptDto.LogoUrl = domain + receiptDto.LogoUrl;
                string relativePath = receiptDto.LogoUrl.Replace(domain, wwwPath).Replace("/", "\\");//uri.LocalPath;
                byte[] imageByte = System.IO.File.ReadAllBytes(relativePath);
                string imgbase64 = Convert.ToBase64String(imageByte);
                string imageUrl = "data:image/png;base64, " + imgbase64;
                string logoUrl = receiptDto.LogoUrl;

                var templatePath = Path.Combine(_environment.WebRootPath, "DynamicHtml", "topupDynamic.html");
                if (!File.Exists(templatePath))
                    throw new FileNotFoundException("HTML Template not found at: " + templatePath);

                htmlTemplate = await File.ReadAllTextAsync(templatePath);
                var debts = new List<DebtItem>();
                var fixedItems = new List<FixedItem>();
                decimal debtremainingBalance = 0;
                if (receiptDto.VendResponse != null)
                {
                    JObject jsonObject = JObject.Parse(receiptDto.VendResponse);

                    bool containsDebt = jsonObject["ipayMsg"]?["elecMsg"]?["vendRes"]?["debt"] != null;
                    bool containsFixed = jsonObject["ipayMsg"]?["elecMsg"]?["vendRes"]?["fixed"] != null;
                    if (containsDebt)
                    {
                        decimal debtAmount = 0;
                        decimal debtTax = 0;
                        int debtCount = (Int32)jsonObject["ipayMsg"]["elecMsg"]["vendRes"]["debt"].Count();

                        if (debtCount != 8)
                        {
                            JArray debtArray = (JArray)jsonObject["ipayMsg"]["elecMsg"]["vendRes"]["debt"];
                            if (debtArray != null && debtArray.Count > 0)
                            {

                                htmlContent += @"<div style='margin-bottom:10px; page-break-inside :avoid;page-break-inside:auto;text-align: center;'><h2> Debt Items </h2></div>";
                                foreach (var debt in debtArray)
                                {

                                    if (debt != null)
                                    {
                                        string text = (string)debt["#text"];
                                        string amount = (string)debt["@amt"];
                                        string remainingBalance = (string)debt["@rem"];
                                        string tax = (string)debt["@tax"];

                                        debtAmount = Convert.ToDecimal(amount);
                                        debtAmount = Math.Round(debtAmount / 100, 2);
                                        debtTax = Convert.ToDecimal(tax);
                                        debtTax = Math.Round(debtTax / 100, 2);
                                        debtremainingBalance = Convert.ToDecimal(remainingBalance);
                                        debtremainingBalance = Math.Round(debtremainingBalance / 100, 2);
                                        debts.Add(new DebtItem
                                        {
                                            Amount = debtAmount,
                                            Tax = debtTax,
                                            RemainBalance = debtremainingBalance,
                                            Text = text
                                        });
                                        totalDebtAmount += (debtAmount) + (debtTax);
                                        totalTax += debtTax;
                                    }

                                }
                            }
                        }
                        else
                        {
                            decimal amount = 0;
                            decimal tax = 0;
                            decimal remainingBalance = 0;
                            amount = (decimal)jsonObject["ipayMsg"]["elecMsg"]["vendRes"]["debt"]["@amt"];
                            tax = (decimal)jsonObject["ipayMsg"]["elecMsg"]["vendRes"]["debt"]["@tax"];
                            string text = (string)jsonObject["ipayMsg"]["elecMsg"]["vendRes"]["debt"]["#text"];
                            remainingBalance = (decimal)jsonObject["ipayMsg"]["elecMsg"]["vendRes"]["debt"]["@rem"];
                            debtAmount = Convert.ToDecimal(amount);
                            debtAmount = Math.Round(debtAmount / 100, 2);
                            debtTax = Convert.ToDecimal(tax);
                            debtTax = Math.Round(debtTax / 100, 2);
                            debtremainingBalance = Convert.ToDecimal(remainingBalance);
                            debtremainingBalance = Math.Round(debtremainingBalance / 100, 2);

                            debts.Add(new DebtItem
                            {
                                Amount = debtAmount,
                                Tax = debtTax,
                                RemainBalance = debtremainingBalance,
                                Text = text

                            });
                            totalDebtAmount += (debtAmount) + (debtTax);
                            totalTax += debtTax;
                        }
                    }
                    if (containsFixed)
                    {
                        int fixedCount = (Int32)jsonObject["ipayMsg"]["elecMsg"]["vendRes"]["fixed"].Count();
                        decimal fixedAmount = 0;
                        decimal fixedTax = 0;
                        if (fixedCount != 5)
                        {
                            JArray fixedArray = (JArray)jsonObject["ipayMsg"]["elecMsg"]["vendRes"]["fixed"];
                            if (fixedArray != null && fixedArray.Count > 0)
                            {
                                htmlContent += @"<div style='margin-bottom: 20px; text-align: center;page-break-inside :avoid;page-break-inside:auto;'><h2> Fixed Items </h2></div>";
                                foreach (var fix in fixedArray)
                                {

                                    if (fix != null)
                                    {
                                        string text = (string)fix["#text"];
                                        string amount = (string)fix["@amt"];
                                        string tax = (string)fix["@tax"];
                                        fixedAmount = Convert.ToDecimal(amount);
                                        fixedAmount = Math.Round(fixedAmount / 100, 2);
                                        fixedTax = Convert.ToDecimal(tax);
                                        fixedTax = Math.Round(fixedTax / 100, 2);
                                        fixedItems.Add(new FixedItem
                                        {
                                            Tax = fixedTax,
                                            Amount = fixedAmount,
                                            Text = text

                                        });
                                        totalFixedAmunt += (fixedAmount);
                                        totalTax += fixedTax;
                                    }
                                }
                            }
                        }
                        else
                        {
                            double amount = (double)jsonObject["ipayMsg"]["elecMsg"]["vendRes"]["fixed"]["@amt"];
                            double tax = (double)jsonObject["ipayMsg"]["elecMsg"]["vendRes"]["fixed"]["@tax"];
                            string text = (string)jsonObject["ipayMsg"]["elecMsg"]["vendRes"]["fixed"]["#text"];
                            amount = Math.Round(amount / 100, 2);
                            tax = Math.Round(tax / 100, 2);
                            fixedAmount = Convert.ToDecimal(amount);
                            fixedTax = Convert.ToDecimal(tax);
                            fixedItems.Add(new FixedItem
                            {
                                Tax = fixedTax,
                                Amount = fixedAmount,
                                Text = text

                            });
                            totalFixedAmunt += (fixedAmount);
                            totalTax += fixedTax;
                        }

                    }

                }

                string tariff = "";
                if (!receiptDto.IsInHouseTxn)
                {
                    if (!string.IsNullOrEmpty(receiptDto.StandardTokens))
                    {
                        totalVatIncluding = (decimal)(receiptDto.stdamt.Value + receiptDto.stdtax);
                        totalVatExcluding = receiptDto.stdamt.Value;
                    }
                }

                receiptDto.TransactionFee = Convert.ToDecimal(receiptDto.TransactionFee);
                receiptDto.TransactionFee = Math.Round(receiptDto.TransactionFee, 2);
                receiptDto.TransactionFee = Convert.ToDecimal(string.Format("{0:F2}", receiptDto.TransactionFee));

                if (!receiptDto.IsInHouseTxn)
                {
                    receiptDto.PurchasePriceExlTax = totalVatExcluding;
                    receiptDto.PurchasePriceInclTax = totalVatIncluding;
                    receiptDto.PurchasePriceInclTax = Convert.ToDecimal(string.Format("{0:F2}", receiptDto.PurchasePriceInclTax));

                }
                if (!string.IsNullOrEmpty(receiptDto.StandardTokens) || totalDebtAmount > 0 || totalFixedAmunt > 0 || totalTax > 0)
                {
                    if (receiptDto.stdamt.HasValue)
                    {
                        receiptDto.PurchasePriceExlTax = totalDebtAmount + totalFixedAmunt + receiptDto.stdamt.Value + receiptDto.TransactionFee;
                    }
                    else
                    {
                        receiptDto.PurchasePriceExlTax = totalDebtAmount + totalFixedAmunt + receiptDto.TransactionFee;
                    }
                    receiptDto.PurchasePriceExlTax = Math.Round(receiptDto.PurchasePriceExlTax, 2);
                    receiptDto.PurchasePriceInclTax = receiptDto.PurchasePriceExlTax + totalTax;
                }
                else
                {
                    receiptDto.PurchasePriceExlTax = totalDebtAmount + totalFixedAmunt;
                    receiptDto.PurchasePriceExlTax = Math.Round(receiptDto.PurchasePriceExlTax, 2);
                    receiptDto.PurchasePriceInclTax = receiptDto.PurchasePriceExlTax + totalTax;
                }
                if (receiptDto.VendResponse != null)
                {

                    JObject jsonObject = JObject.Parse(receiptDto.VendResponse);
                    //bool istariff = jsonObject["ipayMsg"]?["elecMsg"]?["vendRes"]?["tariff"] != null;

                    JToken tariffToken = jsonObject["ipayMsg"]?["elecMsg"]?["vendRes"]?["tariff"];
                    bool hasTextKey = tariffToken != null && tariffToken.Type == JTokenType.Object && tariffToken["#text"] != null;
                    if (tariffToken != null)
                    {
                        if (hasTextKey)
                        {
                            bool hasTextKeyWithValue = tariffToken != null && tariffToken.Type == JTokenType.Object && tariffToken["#text"] != null && !string.IsNullOrEmpty(tariffToken["#text"].ToString());
                            if (hasTextKeyWithValue)
                            {
                                tariff = (string)jsonObject["ipayMsg"]["elecMsg"]["vendRes"]["tariff"]["#text"];
                            }

                        }
                        else
                        {
                            tariff = (string)jsonObject["ipayMsg"]["elecMsg"]["vendRes"]["tariff"];
                        }
                    }

                }

                var model = new ReceiptViewModel
                {
                    ActualRechargeAmount = receiptDto.ActualRechargeAmount,
                    LogoUrl = receiptDto.LogoUrl,
                    IssuedDate = receiptDto.IssuedDate,
                    ReferenceNmber = receiptDto.ReferenceNmber,
                    MeterNumber = receiptDto.MeterNumber,
                    UtilName = receiptDto.UtilName,
                    UtilDistId = receiptDto.UtilDistId,
                    UtilVATNo = receiptDto.UtilVATNo,
                    UtilAddress = receiptDto.UtilAddress,
                    PurchasePriceExlTax = receiptDto.PurchasePriceExlTax,
                    PurchasePriceInclTax = receiptDto.PurchasePriceInclTax,
                    KeyChangeToken = resultKeychangeToken.ToString(),
                    ShowKeyChange = !string.IsNullOrEmpty(receiptDto.KeyChangeToken),
                    DomesticTarrif = receiptDto.DomesticTarrif,
                    Amount = receiptDto.Amount,
                    Tax = receiptDto.Tax,
                    IsInHouseTxn = receiptDto.IsInHouseTxn,
                    VendorName = receiptDto.VendorName,
                    TaxNumber = receiptDto.TaxNumber,
                    Customer = receiptDto.Customer,
                    Address = receiptDto.Address,
                    Alg = receiptDto.Alg,
                    SGC = receiptDto.SGC,
                    TI = receiptDto.TI,
                    KRN = receiptDto.KRN,
                    TokenTech = receiptDto.TokenTech,
                    OldSGC = receiptDto.OldSGC,
                    OldTI = receiptDto.OldTI,
                    OldKRN = receiptDto.OldKRN,
                    NewSGC = receiptDto.NewSGC,
                    NewTI = receiptDto.NewTI,
                    NewKRN = receiptDto.NewKRN,
                    ReceiptId = receiptDto.ReceiptId,
                    ShowStdToken = !string.IsNullOrEmpty(receiptDto.StandardTokens),
                    StandardTokens = resultStdtoken.ToString(),
                    stdReceiptId = receiptDto.stdReceiptId,
                    Units = receiptDto.Units,
                    stdamt = receiptDto.stdamt,
                    stdtax = receiptDto.stdtax,
                    BsstToken = resultBsstToken.ToString(),
                    BsstReceiptId = receiptDto.BsstReceiptId,
                    BsstTokenAmount = receiptDto.BsstTokenAmount,
                    BsstTokenTax = receiptDto.BsstTokenTax,
                    BsstTokenUnits = receiptDto.BsstTokenUnits,
                    CustomerMessage = receiptDto.CustomerMessage,
                    DebtItems = debts,
                    FixedItems = fixedItems,
                    TransactionFee = receiptDto.TransactionFee,


                };
                if (!string.IsNullOrEmpty(tariff))
                {

                    model.ShowTarrifToken = true;
                    model.Tariff = tariff;
                }
                // Assign Debt
                model.ShowDebt = model.DebtItems != null && model.DebtItems.Count > 0;
                model.ShowFixed = model.FixedItems != null && model.FixedItems.Count > 0;
                model.ShowKeyChange = !string.IsNullOrEmpty(model.KeyChangeToken);
                model.ShowTaxDetails = !string.IsNullOrEmpty(model.TaxNumber) && model.TaxNumber.ToLower() != ("undefined");
                model.ShowCustomerMessage = !string.IsNullOrEmpty(model.CustomerMessage);
                model.ShowBsstToken = !string.IsNullOrEmpty(resultBsstToken.ToString());
                model.ShowTokenDetails = !string.IsNullOrEmpty(model.TokenTech);
                model.ShowStdToken = !string.IsNullOrEmpty(model.StandardTokens);

                model.ShowTxnFee = true;

                //var template = Template.Parse(topupTemplate.Html);
                //topup.Html = template.Render(model, memberRenamer: member => member.Name);

                var template = Template.Parse(htmlTemplate);

                // Render with model
                htmlContent = template.Render(model, memberRenamer: member => member.Name);

            }

            var mobileView = new HtmlToPdf();
            mobileView.Options.WebPageWidth = 480;

            var tabletView = new HtmlToPdf();
            tabletView.Options.WebPageWidth = 1024;

            //var fullView = new HtmlToPdf();
            //fullView.Options.WebPageWidth = 1920;
            //var htmlToPdf = new HtmlToPdf(1000, 1414);
            //htmlToPdf.Options.DrawBackground = true;

            //PdfGenerator.AddPdfPages(data, htmlContent, PageSize.A4);
            //HtmlToPdf converter = new HtmlToPdf();
            var fullView = new HtmlToPdf();
            fullView.Options.WebPageWidth = 1024;
            fullView.Options.MinPageLoadTime = 1;
            fullView.Options.MaxPageLoadTime = 3;
            fullView.Options.WebPageFixedSize = false;
            fullView.Options.PdfPageOrientation = PdfPageOrientation.Portrait;

            fullView.Options.AutoFitWidth = HtmlToPdfPageFitMode.ShrinkOnly;
            fullView.Options.AutoFitHeight = HtmlToPdfPageFitMode.NoAdjustment;
            SelectPdf.PdfDocument doc = fullView.ConvertHtmlString(htmlContent);


            byte[] response;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    doc.Save(ms)
;
                    response = ms.ToArray();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response;
        }

        public async Task<Dictionary<string, TransactionNoFeeFromRctNumDto>> GetTransactionFeesByReceiptNumbers(List<string> receiptNumbers)
        {
            const string query = @"  SELECT
                                    receipt_number,
                                    transaction_fee,
                                    transaction_id
                                    FROM public.ohd_top_up_transactions
                                   WHERE receipt_number = ANY(@ReceiptNumbers)";


            var result = await _genericRepository.GetAsync<TransactionNoFeeFromRctNumDto>(query,
                        new
                        {
                            ReceiptNumbers = receiptNumbers.ToArray()
                        });

            return result.ToDictionary(x => x.receipt_number, x => x);
        }
        public async Task DeleteDebitechDuplicateNotifyRequest(int id)
        {
            var sQuery = @"DELETE FROM public.ohd_debitech_notify_requests(
                            WHERE id=@Id";
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            try
            {
                var result = await _genericRepository.ExecuteScalarAsync(sQuery, parameters).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
            }
        }
        public async Task<int> SaveCredits(CaptureUsersCreditsToSaveCommandReuqest request, string imagePath, int imgCount)
        {
            var sQuery = @"INSERT INTO public.ohd_user_credits
                            (
                            credit,
                            user_id,
                            meter_id,
                            image_path_1,
                            image_path_1_date,
                            created_at,
                            image_count)
                            VALUES 
                            (@Credit,
                             @UserId, 
                             @MeterId,
                             @ImagePath, 
                             @ImagePath1,
                             @CreatedAt,
                             @ImageCount
                             )RETURNING id;";

            var parameters = new DynamicParameters();
            parameters.Add("@Credit", request.Amount);
            parameters.Add("@UserId", request.UserId);
            parameters.Add("@MeterId", request.MeterId);
            parameters.Add("@ImagePath", imagePath);
            parameters.Add("@CreatedAt", DateTime.UtcNow);
            parameters.Add("@ImagePath1", DateTime.UtcNow);
            parameters.Add("@ImageCount", imgCount);

            try
            {
                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }

        }

        public async Task<int> UpdateCreditImages(string imagePath, string columnName, int imageCount, int id)
        {
            var sQuery = @"UPDATE  public.ohd_user_credits
                            SET
                            @ColumnName=@ImagePath,
                            modified_at=@ModifiedAt,
                            image_count=@ImageCount
                            WHERE id=@Id 
                            RETURNING id;";

            var parameters = new DynamicParameters();
            parameters.Add("@ColumnName", columnName);
            parameters.Add("@ImagePath", imagePath);
            parameters.Add("@ModifiedAt", DateTime.UtcNow);
            parameters.Add("@Id", id);

            try
            {
                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }

        }

        public async Task<CreditImageDto> GetCreditsId(int meterId, int userId)
        {
            var sQuery = @"SELECT id AS Id,image_count AS ImageCount 
                           FROM public.ohd_user_credits
                           WHERE meter_id=@MeterId AND user_id=@UserId";

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@MeterId", meterId);

            try
            {
                var result = await _genericRepository.GetFirstOrDefaultAsync<CreditImageDto>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return new CreditImageDto();
            }

        }

        public async Task<CreditImagesRawDto> GetCreditImageValues(int meterId, int userId)
        {
            var sQuery = @"SELECT image_path_1 AS Image_Path_1,
                                image_path_2 AS Image_Path_2,
                                image_path_3 AS Image_Path_3,
                                image_path_4 AS Image_Path_4,
                               image_path_5 AS Image_Path_5,
                               image_path_6 AS Image_Path_6
                           FROM public.ohd_user_credits
                           WHERE meter_id=@MeterId AND user_id=@UserId";

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@MeterId", meterId);

            try
            {
                var result = await _genericRepository.GetFirstOrDefaultAsync<CreditImagesRawDto>(sQuery, parameters);
                return result;
            }
            catch (Exception ex)
            {
                return new CreditImagesRawDto();
            }

        }

        public async Task<int> UpdateAllImages(long id, string img1, string img2, string img3, string img4, string img5, string img6, int count)
        {
            var query = @" UPDATE public.ohd_user_credits
                            SET 
                                image_path_1 = @Img1,
                                image_path_2 = @Img2,
                                image_path_3 = @Img3,
                                image_path_4 = @Img4,
                                image_path_5 = @Img5,
                                image_path_6 = @Img6,
                                image_count  = @Count,
                                modified_at  = @ModifiedAt
                            WHERE id = @Id
                            RETURNING id;";

            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            parameters.Add("@Img1", img1);
            parameters.Add("@Img2", img2);
            parameters.Add("@Img3", img3);
            parameters.Add("@Img4", img4);
            parameters.Add("@Img5", img5);
            parameters.Add("@Img6", img6);
            parameters.Add("@Count", Math.Min(count, 6));
            parameters.Add("@ModifiedAt", DateTime.UtcNow);

            return await _genericRepository.ExecuteScalarAsync<int>(query, parameters).ConfigureAwait(false);
        }


        public async Task<int> SaveCreditImage(CaptureUsersCreditsToSaveCommandReuqest request, string imageUrl)
        {

            var query = @"WITH deleted AS (
                            DELETE FROM public.ohd_user_credit_images
                            WHERE id = (
                                SELECT id
                                FROM public.ohd_user_credit_images
                                WHERE meter_id = @MeterId AND user_id = @UserId
                                ORDER BY credit_image_id ASC
                                LIMIT 1
                            )
                            AND (
                                SELECT COUNT(*) 
                                FROM public.ohd_user_credit_images 
                                WHERE meter_id = @MeterId AND user_id = @UserId
                            ) = 6
                            RETURNING *
                        ),

                        shifted AS (
                            UPDATE public.ohd_user_credit_images
                            SET credit_image_id = credit_image_id - 1,
                                modified_at = NOW()
                            WHERE meter_id = @MeterId AND user_id = @UserId
                            AND EXISTS (SELECT 1 FROM deleted)
                            RETURNING *
                        )

                        INSERT INTO public.ohd_user_credit_images
                        (
                            meter_id,
                            credit_image_id,
                            image_url,
                            credit,
                            user_id,
                            created_at
                        )
                        VALUES
                        (
                            @MeterId,
                            (
                                SELECT COALESCE(MAX(credit_image_id), 0) + 1
                                FROM public.ohd_user_credit_images
                                WHERE meter_id = @MeterId AND user_id = @UserId
                            ),
                            @ImageUrl,
                            @Credit,
                            @UserId,
                            NOW()
                        )
                        RETURNING id;";

            var parameters = new DynamicParameters();
            parameters.Add("@MeterId", request.MeterId);
            parameters.Add("@UserId", request.UserId);
            parameters.Add("@ImageUrl", imageUrl);
            parameters.Add("@Credit", request.Amount);

            return await _genericRepository.ExecuteScalarAsync<int>(query, parameters);
        }
        public async Task<List<UserCreditImageDto>> GetUserImages(long meterId)
        {
            var query = @"
                            SELECT 
                                c.id AS Id,
                                c.credit_image_id AS CreditImageId,
                                c.credit AS Credit,
                                e.name AS Name,
                                e.display_name AS DisplayName,
                                c.image_url AS ImageUrl,
                                c.created_at AS CreatedAt,
                                c.modified_at AS ModifiedAt,
                                c.user_id AS UserId
                            FROM public.ohd_user_credit_images c
                            LEFT JOIN public.ohd_enum_credit_images e 
                                ON c.credit_image_id = e.id
                            WHERE c.meter_id = @MeterId
                            ORDER BY c.credit_image_id desc;";

            var parameters = new DynamicParameters();
            parameters.Add("@MeterId", meterId);

            var result = await _genericRepository.GetAsync<UserCreditImageDto>(query, parameters);


            return result.ToList();
        }
        public async Task<DatatableModel<GetTopUpTransaction>> GetTrailVendSuccessTopupTransactions(GetUserPayamenstQuery request)
        {
            string whereclause = "";
            var dt = new DatatableModel<GetTopUpTransaction>()
            {
                Page = request.Page,
                PageSize = request.PageSize
            };
            try
            {
                var sQuery = @"SELECT   concat(u.first_name,' ',u.last_name) AS Consumer,
                                tut.transaction_id AS TransactionId,
                                 u.Email AS Email,
                                u.Mobile As Phone,
                               mt.meter_number AS MeterNumber,  
                                tut.amount AS Amount, 
                                tut.user_id As UserId, 
                                tut.use_wallet AS UseWallet, 
                                tut.meter_id AS MeterId, 
                                tut.transaction_fee AS TransactionFee, 
                               -- (tut.recharge_amount-debt_amount) AS RechargeAmount,
                                tut.recharge_amount AS RechargeAmount,
                                tut.debt_amount as DebtAmount,                                
                               to_char(tut.created_at,'dd-MM-yyyy  HH24:MI:ss') AS   CreatedAt, 
                                tut.modified_at AS ModifiedAt,                                 
                                tut.pf_response AS PayFastResponse,
                               tut.topup_status AS TopupStatus,
							   tut.trial_vend_response AS TrailVendResponse,
                               tut.vend_response AS VendResponse,
                               tut.std_token AS StdToken,
                                tut.bsst_token AS BsstToken,
                                tut.key_change_token AS KeyChangeToken,
                                uw.balance AS WalletBalance,
                                tut.wallet_amount_used As WalletAmountUsed,
                                tut.final_amount_to_pay AS FinalAmountToPay,
                                p.name AS Property,
								p.unit_number AS UnitNumber,
                                e.estate As Estate,
                                pm.display_name as PaymentMethod,
                                tut.receipt_number AS ReceiptNumber,
                                tut.is_in_house_txn AS IsInHouseTransaction ,
                                tut.eft_ref_no AS EFTRefNo,
                                tut.payment_gateway AS PaymentGateWay
	                    FROM public.ohd_top_up_transactions as tut
						LEFT JOIN ohd_user u ON tut.user_id=u.id 
						LEFT JOIN ohd_meter AS mt ON  mt.id=tut.meter_id
                         LEFT JOIN public.ohd_property as p ON mt.property_id=p.id
                        LEFT JOIN public.ohd_user_wallet AS uw ON tut.user_id=uw.user_id
                        LEFT JOIN public.ohd_estate as e ON p.estate_id=e.id
                        LEFT JOIN public.ohd_payment_methods as pm ON tut.payment_method_id=pm.id
                        WHERE tut.topup_status is not null
						AND  tut.topup_status='TrailVendSuccess'
						AND FLAG=@Flag AND vend_response IS NULL
						AND pf_response IS NULL";

                var parameters = new DynamicParameters();
                parameters.Add("@Flag", (int)PaymentStatus.Complete);


                if (!string.IsNullOrEmpty(request.SearchText))
                {
                    var searchText = request.SearchText.Trim();
                    var searchTerms = searchText.ToLower().Split(' ');
                    var searchConditions = new List<string>();
                    var index = 0;

                    // Add condition for the full search text
                    var fullSearchTextParam = "@SearchTextFull";
                    searchConditions.Add($@"(lower(tut.transaction_id) like {fullSearchTextParam}
                            OR lower(u.first_name) like {fullSearchTextParam}
                                        OR lower(u.last_name) like {fullSearchTextParam}
                                        OR lower(u.email) like {fullSearchTextParam}
                                         OR lower(u.mobile) like {fullSearchTextParam}
                            OR lower(mt.meter_number) like {fullSearchTextParam}
                           )");
                    parameters.Add(fullSearchTextParam, "%" + searchText.ToLower() + "%");


                    // Add conditions for each split term
                    foreach (var term in searchTerms)
                    {
                        var paramName = "@SearchText" + index;
                        searchConditions.Add($@"(lower(tut.transaction_id) like {paramName}
                                        OR lower(u.first_name) like {paramName}
                                        OR lower(u.last_name) like {paramName}
                                        OR lower(u.email) like {paramName}
                                         OR lower(u.mobile) like {paramName}
                               OR lower(mt.meter_number) like {paramName}
                           )");
                        parameters.Add(paramName, "%" + term + "%");
                        index++;
                    }


                    if (whereclause != "" && searchConditions.Any())
                    {
                        whereclause += " AND (" + string.Join(" OR ", searchConditions) + ")";
                    }
                    if (whereclause == "" && searchConditions.Any())
                    {
                        whereclause += " WHERE (" + string.Join(" OR ", searchConditions) + ")";
                    }

                }
                sQuery += whereclause;
                sQuery += " Order By tut.id desc";
                var topuptransactions = await _genericRepository.GetAsync<GetTopUpTransaction>(sQuery, parameters).ConfigureAwait(false);
                var result = topuptransactions.ToList().Skip(request.Page * request.PageSize).Take(request.PageSize);
                dt.Data = result.ToList();
                dt.TotalRecords = topuptransactions.Count();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return dt;
        }
    }
}
