using System.Net.Http;

namespace LudumDareTools;

public struct ApiResult
{
    public dynamic data;
    public HttpRequestException exception;
    public bool ok => exception is null && data is not null;
}