namespace ExpenseTrackingSystem.Exceptions
{
    public class InvalidPasswordException : Exception
    {
        private string _message = "Invalid password";
        public InvalidPasswordException(string msg)
        {
            _message = msg;
        }
        public override string Message => _message;
    }
}