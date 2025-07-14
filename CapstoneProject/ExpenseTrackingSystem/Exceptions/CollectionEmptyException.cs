namespace ExpenseTrackingSystem.Exceptions
{
    public class CollectionEmptyException : Exception
    {
        private string _message = "Duplicate entity present";
        public CollectionEmptyException(string msg)
        {
            _message = msg;
        }
        public override string Message => _message;
    }
}