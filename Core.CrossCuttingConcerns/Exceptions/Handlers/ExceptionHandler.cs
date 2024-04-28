using Core.CrossCuttingConcerns.Exceptions.Types;

namespace Core.CrossCuttingConcerns.Exceptions.Handlers;


//Gelecek olan hataları handle edecek yer. İmplementasyon burada değil bu sınıfı implemente eden sınıfta gerçekleştirilecek.
public abstract class ExceptionHandler
{
    public Task HandleExceptionAsync(Exception exception) =>
        exception switch
        {
            BusinessException businessException => HandleException(businessException), //Gelen exception BusinessException türünde ise
            _ => HandleException(exception) //Değil ise (diğer durumlar için)
        };

    protected abstract Task HandleException(BusinessException businessException);
    protected abstract Task HandleException(Exception exception);
}
