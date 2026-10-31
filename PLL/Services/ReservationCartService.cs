using BookifyHotel.Model;
using DAL.DataBase;

namespace PLL.Services
{
    public class ReservationCartService : BaseService<ReservationCart>
    {
        public ReservationCartService(IGenericRepository<ReservationCart> repo) : base(repo) { }

        public int GetUserReservationsCount(int userId)
        {
            return _repo.Count(rc => rc.UserProfileId == userId);
        }
    }
}
