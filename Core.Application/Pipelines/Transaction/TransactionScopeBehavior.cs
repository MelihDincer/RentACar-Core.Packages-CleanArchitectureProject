using MediatR;
using System.Transactions;

namespace Core.Application.Pipelines.Transaction;

public class TransactionScopeBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ITransactionalRequest
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled); //Async çalışmayı enable duruma getirdik.
        TResponse response;
		try
		{
			response = await next();
			transactionScope.Complete(); 
		}
		catch (Exception)
		{
			transactionScope.Dispose(); //İşlemi iptal et.
			throw;
		}
		return response;
    }
}
