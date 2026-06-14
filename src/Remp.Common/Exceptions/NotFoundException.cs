namespace Remp.Common.Exceptions;

public abstract class NotFoundException: AppException
{
    public NotFoundException(string message): base (message, 404){}
}