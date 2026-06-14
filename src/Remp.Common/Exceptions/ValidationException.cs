namespace Remp.Common.Exceptions;

public abstract class ValidationException: AppException
{
    public List<string> Errors {get;}

    // single error
    public ValidationException(string message): base (message, 400)
    {
        Errors = new List<string>{message};
    }

    // multiple errors
    public ValidationException(List<string> errors): base ("One or more validation errors occurred.", 400)
    {
        Errors = errors;
    }
}