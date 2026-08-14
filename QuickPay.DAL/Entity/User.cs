using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.DAL.Entity
{
    public class User
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    
}
}
