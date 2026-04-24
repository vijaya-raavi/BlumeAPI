using MediatR;

namespace Ontec.Core.Domain.Requests.Estate.Command
{
    public  class DeleteEstateRequestCommand:IRequest<string>
    {
        public int Id { get; set; }
    }
}
