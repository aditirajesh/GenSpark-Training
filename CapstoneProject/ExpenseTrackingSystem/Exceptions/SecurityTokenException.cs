namespace ExpenseTrackingSystem.Exceptions
{
    public class SecurityTokenException : Exception
    {
        private string _message = "Invalid token";
        public SecurityTokenException(string msg)
        {
            _message = msg;
        }
        public override string Message => _message;
    }
}