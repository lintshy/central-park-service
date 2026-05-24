using CentralPark.Application.Common.Interfaces;
using CentralPark.Application.Common.Markers;
using MediatR;

namespace CentralPark.Application.Common.Behaviours;

public sealed class TransactionBehaviour<TRequest, TResponse>(IUnitOfWork uow)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        try
        {
            var response = await next();
            await uow.CommitAsync(ct);
            return response;
        }
        catch
        {
            await uow.RollbackAsync(ct);
            throw;
        }
    }
}
