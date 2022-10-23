namespace WebApplication1.Exceptions;

public class NotFoundEntityException: Exception
{
    public NotFoundEntityException()
        : base()
    {
    }

    public NotFoundEntityException(string message)
        : base(message)
    {
    }
}