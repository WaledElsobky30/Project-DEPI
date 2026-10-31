using BookifyHotel.Model;
using DAL.DataBase;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLL.Services
{
    public class ReservationCartService : BaseService<ReservationCart>
    {
        public ReservationCartService(IGenericRepository<ReservationCart> repo) : base(repo) { }

        public int GetUserReservationsCount(int userId)
        {
            return _repo.GetAll()
                .Count(rc => rc.UserProfileId == userId);
        }


    }

}
