using MediatR;

namespace Ontec.Core.Domain.Requests.PropertyUser.Command
{
    public class DeletePropertyAssociateUserByPropertyId: IRequest<string>
    {
        public int PropertyId { get; set; }
    }
}
