namespace ExpenseTrackingSystem.Exceptions
{
    public class EntityUpdateException : Exception
    {
        private string _message = "Item could not be updated";
        public EntityUpdateException(string msg)
        {
            _message = msg;
        }
        public override string Message => _message;
    }
}