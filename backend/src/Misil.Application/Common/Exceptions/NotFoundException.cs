using System.Net;

namespace Misil.Application.Common.Exceptions;
public class NotFoundException : Exception
{
    public HttpStatusCode StatusCode => HttpStatusCode.NotFound;

    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string name, object key) : base($"{name} ({key}) not found") { }
}
