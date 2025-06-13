namespace ExpenseTrackingSystem.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        private string _message = "Item not found";
        public EntityNotFoundException(string msg)
        {
            _message = msg;
        }
        public override string Message => _message;
    }
}