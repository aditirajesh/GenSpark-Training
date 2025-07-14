namespace ExpenseTrackingSystem.Exceptions
{
    public class DuplicateEntityException : Exception
    {
        private string _message = "Duplicate entity present";
        public DuplicateEntityException(string msg)
        {
            _message = msg;
        }
        public override string Message => _message;
    }
}