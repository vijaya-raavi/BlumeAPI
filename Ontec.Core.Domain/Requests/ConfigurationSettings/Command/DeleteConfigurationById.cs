using MediatR;

namespace Ontec.Core.Domain.Requests.Configuration.Command
{
    public  class DeleteConfigurationById :IRequest<string>
    {
        public int Id { get; set; }
    }
}

