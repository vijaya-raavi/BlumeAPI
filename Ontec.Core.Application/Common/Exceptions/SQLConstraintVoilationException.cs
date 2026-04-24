namespace Ontec.Core.Application.Common.Exceptions
{
    public class SQLConstraintVoilationException:Exception
    {
        public string Errors { get; }
        public SQLConstraintVoilationException(string errorMessage)
        {
                Errors=errorMessage;
        }
    }
}
