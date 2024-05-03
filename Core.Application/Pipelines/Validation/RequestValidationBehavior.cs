using Core.CrossCuttingConcerns.Exceptions.Types;
using FluentValidation;
using MediatR;
using ValidationException = Core.CrossCuttingConcerns.Exceptions.Types.ValidationException;

namespace Core.Application.Pipelines.Validation;


//Middleware
public class RequestValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators; //Bir command in(request) validatorlarına ulaş.

    public RequestValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    //Her request için bir validator varsa bu handle ı çalıştır.
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ValidationContext<object> context = new(request);

        IEnumerable<ValidationExceptionModel> errors = _validators
            .Select(validator => validator.Validate(context)) //her bir validator için validate et
            .SelectMany(result => result.Errors) //birden fazla validator olabilir bu yüzden result ın errorlarını döndür.
            .Where(failure => failure != null)
            .GroupBy(
               keySelector: p => p.PropertyName,
               resultSelector: (propertyName, errors) =>
                  new ValidationExceptionModel { Property = propertyName, Errors = errors.Select(e => e.ErrorMessage) }
            ).ToList();

        if (errors.Any())
            throw new ValidationException(errors);
        TResponse response = await next(); //Hata yoksa command i çalıştır.
        return response;
    }
}
