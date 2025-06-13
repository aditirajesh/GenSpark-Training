namespace ExpenseTrackingSystem.Exceptions
{
    public class EntityCreationException : Exception
    {
        private string _message = "Item could not be created";
        public EntityCreationException(string msg)
        {
            _message = msg;
        }
        public override string Message => _message;
    }
}