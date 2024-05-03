namespace Core.CrossCuttingConcerns.Exceptions.Types;

public class ValidationException : Exception
{
    //Toplamda birden fazla alanda da hata olabilir. Örneğin hem name kısmında 2 hata, hem de age kısmında 3 hata var.
    public IEnumerable<ValidationExceptionModel> Errors { get; }

    //Hata olmayabilir.
    public ValidationException()
        : base()
    {
        Errors = Array.Empty<ValidationExceptionModel>(); //Boş bir liste oluştur.
    }

    //Hata olmayabilir.
    public ValidationException(string? message)
        : base(message)
    {
        Errors = Array.Empty<ValidationExceptionModel>(); //Boş bir liste oluştur.
    }

    //Hata olmayabilir.
    public ValidationException(string? message, Exception? innerException)
        : base(message, innerException)
    {
        Errors = Array.Empty<ValidationExceptionModel>(); //Boş bir liste oluştur.
    }

    //Hataları geçtiğimiz yer.
    public ValidationException(IEnumerable<ValidationExceptionModel> errors)
        : base(BuildErrorMessage(errors)) //BuildErrorMessage => {Şu} alanı patladı, bu alanda şöyle bir problem var. 41. satır
    {
        Errors = errors; //Errors u doldur.
    }

    private static string BuildErrorMessage(IEnumerable<ValidationExceptionModel> errors)
    {
        IEnumerable<string> arr = errors.Select(
            x => $"{Environment.NewLine} -- {x.Property}: {string.Join(Environment.NewLine, values: x.Errors ?? Array.Empty<string>())}"
        );
        return $"Validation failed: {string.Join(string.Empty, arr)}";
    }
}

public class ValidationExceptionModel
{
    public string? Property { get; set; } //Hangi alanda, örneğin name alanı (olmaya da bilir. Bu yüzden nullable yapıldı)

    //Her bir alanda birden fazla hata olabilir.
    public IEnumerable<string>? Errors { get; set; } //Bu alanda hangi hatalar var (birden fazla olabilir)
}