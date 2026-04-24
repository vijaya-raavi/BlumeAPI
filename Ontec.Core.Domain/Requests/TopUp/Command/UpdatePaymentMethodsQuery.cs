using MediatR;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.TopUp.Command
{
    public  class UpdatePaymentMethodsQuery : IRequest<string>
    {
        public List<PaymentMethod> PaymentMethods { get; set; }
    }
    public class PaymentMethod
    {
        public int Id { get; set; }
        //public string Description { get; set; }
        public string DisplayName { get; set; }
       // public string Slug { get; set; }
        public double Percentage { get; set; }
        public double Discount { get; set; }
        public bool StatusId { get; set; }

    }
}
