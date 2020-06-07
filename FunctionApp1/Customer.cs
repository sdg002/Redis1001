namespace FunctionApp1
{
    public class Customer
    {
        private const int MAXDATASIZEBYTES = 1000;

        public Customer(string email, string firstName, string lastName)
        {
            Email = email;
            FirstName = firstName;
            LastName = lastName;
            SomeLargeDataItem = new string('*', MAXDATASIZEBYTES);
        }

        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SomeLargeDataItem { get; set; }
    }
}