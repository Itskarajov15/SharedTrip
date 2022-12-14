using Microsoft.EntityFrameworkCore;
using SharedTrip.Core.Contracts;
using SharedTrip.Core.Models.Car;
using SharedTrip.Core.Models.ServiceModels.Car;
using SharedTrip.Infrastructure.Data;
using SharedTrip.Infrastructure.Data.Entities;

namespace SharedTrip.Core.Services
{
    public class CarService : ICarService
    {
        private readonly ApplicationDbContext context;
        private readonly ICloudinaryService cloudinaryService;

        public CarService(
            ApplicationDbContext context,
            ICloudinaryService cloudinaryService)
        {
            this.context = context;
            this.cloudinaryService = cloudinaryService;
        }

        public async Task<int> CreateCarAsync(CreateCarViewModel model, string userId)
        {
            var car = new Car
            {
                BrandId = model.BrandId,
                ColourId = model.ColourId,
                Climatronic = model.Climatronic,
                CountOfSeats = model.CountOfSeats,
                DriverId = userId,
                Model = model.Model,
                Year = model.Year
            };

            await this.context.Cars.AddAsync(car);
            car.ImageUrl = await this.cloudinaryService.UploadPicture(model.Image);
            await this.context.SaveChangesAsync();

            return car.Id;
        }

        public async Task DeleteAsync(int carId)
        {
            var car = await this.context
                .Cars
                .FirstOrDefaultAsync(c => c.Id == carId);

            car.IsDeleted = true;
            await this.context.SaveChangesAsync();
        }

        public async Task EditCarAsync(EditCarViewModel model)
        {
            var car = await this.context
                .Cars
                .Where(c => c.Id == model.Id && c.IsDeleted == false)
                .FirstOrDefaultAsync();

            car.Year = model.Year;
            car.BrandId = model.BrandId;
            car.ColourId = model.ColourId;
            car.Climatronic = model.Climatronic;
            car.ImageUrl = await this.cloudinaryService.UploadPicture(model.Image);
            car.CountOfSeats = model.CountOfSeats;
            car.Model = model.Model;

            await this.context.SaveChangesAsync();
        }

        public async Task<AllCarsQueryServiceModel> GetAllCarsAsync(int currentPage = 1, int carsPerPage = 6)
        {
            var model = new AllCarsQueryServiceModel();
            var carsQuery = this.context
            .Cars
                .Where(c => c.IsDeleted == false)
                .OrderBy(c => c.Brand.Name)
                .ThenBy(c => c.Model)
                .AsQueryable();

            var cars = await carsQuery
                .Skip((currentPage - 1) * carsPerPage)
                .Take(carsPerPage)
                .Select(c => new AllCarsViewModel
                {
                    Id = c.Id,
                    Brand = c.Brand.Name,
                    Colour = c.Colour.Name,
                    Model = c.Model,
                    Year = c.Year,
                    DriverName = $"{c.Driver.FirstName} {c.Driver.LastName}"
                })
                .ToListAsync();

            model.Cars = cars;
            model.TotalCarsCount = await carsQuery.CountAsync();

            return model;
        }

        public async Task<IEnumerable<BrandViewModel>> GetBrandsAsync()
            => await this.context
                         .Brands
                         .Select(b => new BrandViewModel
                         {
                             Id = b.Id,
                             Name = b.Name
                         })
                         .ToListAsync();

        public async Task<CarDetailsViewModel> GetCarAsync(int carId)
        {
            return await this.context
                                .Cars
                                .Where(c => c.Id == carId && c.IsDeleted == false)
                                .Select(c => new CarDetailsViewModel
                                {
                                    Id = c.Id,
                                    Brand = c.Brand.Name,
                                    Climatronic = c.Climatronic,
                                    Colour = c.Colour.Name,
                                    CountOfSeats = c.CountOfSeats,
                                    DriverId = c.DriverId,
                                    ImageUrl = c.ImageUrl,
                                    Model = c.Model,
                                    Year = c.Year
                                })
                                .FirstOrDefaultAsync();
        }

        public async Task<EditCarViewModel> GetCarForEditAsync(int carId)
        {
            return await this.context
                .Cars
                .Where(c => c.Id == carId && c.IsDeleted == false)
                .Select(c => new EditCarViewModel
                {
                    Id = c.Id,
                    BrandId = c.BrandId,
                    Climatronic = c.Climatronic,
                    ColourId = c.ColourId,
                    Year = c.Year,
                    Model = c.Model,
                    CountOfSeats = c.CountOfSeats
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<CreateTripCarViewModel>> GetCarsForTripAsync(string userId)
        {
            return await this.context
                .Cars
                .Where(c => c.DriverId == userId && c.IsDeleted == false)
                .OrderBy(c => c.Brand.Name)
                .ThenBy(c => c.Model)
                .Select(c => new CreateTripCarViewModel
                {
                    Id = c.Id,
                    Model = $"{c.Brand.Name} {c.Model}"
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ColourViewModel>> GetColoursAsync()
            => await this.context
                         .Colours
                         .Select(b => new ColourViewModel
                         {
                             Id = b.Id,
                             Name = b.Name
                         })
                         .ToListAsync();

        public async Task<int> GetCountOfCarsAsync()
        {
            return await this.context
                .Cars
                .Where(c => c.IsDeleted == false)
                .CountAsync();
        }

        public async Task<CarQueryServiceModel> GetMyCarsAsync(string userId, int currentPage = 1, int carsPerPage = 3)
        {
            var model = new CarQueryServiceModel();

            var carsQuery = this.context
                .Cars
                .Where(c => c.DriverId == userId && c.IsDeleted == false)
                .OrderBy(c => c.Brand.Name)
                .ThenBy(c => c.Model)
                .AsQueryable();

            var cars = await carsQuery
                .Skip((currentPage - 1) * carsPerPage)
                .Take(carsPerPage)
                .Select(c => new ProfileCarViewModel
                {
                    Id = c.Id,
                    Brand = c.Brand.Name,
                    Colour = c.Colour.Name,
                    ImageUrl = c.ImageUrl,
                    Model = c.Model,
                    Year = c.Year
                })
                .ToListAsync();

            model.Cars = cars;
            model.TotalCarsCount = await carsQuery.CountAsync();

            return model;
        }

        public async Task<bool> IsUserOwnerOfACar(string userId, int carId)
        {
            var car = await this.context
                .Cars
                .FirstOrDefaultAsync(c => c.Id == carId);

            if (car.DriverId == userId)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}