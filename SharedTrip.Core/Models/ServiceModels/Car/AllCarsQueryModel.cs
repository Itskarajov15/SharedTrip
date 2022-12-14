using SharedTrip.Core.Models.Car;

namespace SharedTrip.Core.Models.ServiceModels.Car
{
    public class AllCarsQueryModel
    {
        public const int CarsPerPage = 6;

        public int CurrentPage { get; set; } = 1;

        public int TotalCarsCount { get; set; }

        public IEnumerable<AllCarsViewModel> Cars { get; set; } = Enumerable.Empty<AllCarsViewModel>();
    }
}