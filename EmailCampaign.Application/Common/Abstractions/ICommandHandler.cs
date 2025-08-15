using System.Threading;
using System.Threading.Tasks;

namespace EmailCampaign.Application.Common.Abstractions;

public interface ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    Task<TResult> Handle(TCommand command, CancellationToken cancellationToken = default);
}
