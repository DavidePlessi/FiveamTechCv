using System.Net;

namespace FiveamTechCv.Entities;

public class FiveamTechCvException : Exception
{
    public string ErrorCode { get; set; } = GenericError;
    public int StatusCode { get; set; }
    public string? ErrorMessage { get; set; }
    
    
    #region ErrorCodes
    
    public const string GenericError = "msg-unknown-error";
    
    public const string NotFound = "msg-not-found";
    
    public const string Unauthorized = "msg-unauthorized";
    
    #endregion

    public FiveamTechCvException()
    {
        
    }

    public FiveamTechCvException(HttpStatusCode statusCode, string errorCode, string errorMessage)
    {
        StatusCode = (int)statusCode;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }
}