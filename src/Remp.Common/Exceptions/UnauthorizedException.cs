namespace Remp.Common.Exceptions;

public abstract class UnauthorizedException: AppException
{
    public UnauthorizedException(string message = "Unauthorized. please log in first"): base (message, 401){}
}