using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCapacityBalancing.Models
{
    public class OpenTasksUserAssociation
    {

        public User User { get; set; }
        public float Remaining { get; set; }

        public OpenTasksUserAssociation(User user, float remaining)
        {
            User = user;
            Remaining= remaining;
        }
    }
}
