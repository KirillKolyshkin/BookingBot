using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBot.Core.Entities
{
    public class User
    {
        public User(long id, string firstName)
        {
            Id = id;
            FirstName = firstName;
            //LastName = lastName;
        }

        //public int ChatId { get; set; } // 
        public long Id { get; set; }
        public string FirstName { get; private set; }
        //public string LastName { get; private set; }

        public override string ToString() => String.Format(FirstName );
    }
}
