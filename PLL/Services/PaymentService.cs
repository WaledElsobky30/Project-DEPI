using BookifyHotel.Model;
using DAL.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLL.Services
{
    public class PaymentService : BaseService<Payment>
    {
        public PaymentService(IGenericRepository<Payment> repo) : base(repo) { }
    }

}
