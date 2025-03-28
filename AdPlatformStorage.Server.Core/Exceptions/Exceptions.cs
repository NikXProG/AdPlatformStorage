namespace AdPlatformStorage.Server.Core.Exceptions
{
    
    public static class RestException
    {
        
        public class RestErrorException : ApplicationException
        {
            public RestErrorException(string message, string? typeCallContext = null, Exception? innerException = null)
                : base(message, innerException)
            {
                TypeCallContext = typeCallContext;
            }

            public string? TypeCallContext { get; set; }
            
        }
    
        public class NotFoundStorageErrorException : RestErrorException
        {
            public NotFoundStorageErrorException(string message, string? typeCallContext = null, Exception? innerException = null)
                : base(message, typeCallContext, innerException) { }
        }

        public class InvalidFormatFileErrorException : RestErrorException
        {
            public InvalidFormatFileErrorException(string message, string? typeCallContext = null, Exception? innerException = null)
                : base(message, typeCallContext, innerException) { }
        }

        public class NotFoundFileOrNullErrorException : RestErrorException
        {
            public NotFoundFileOrNullErrorException(string message, string? typeCallContext = null, Exception? innerException = null)
                : base(message, typeCallContext, innerException) { }
        }
        
        
    }
    
}
