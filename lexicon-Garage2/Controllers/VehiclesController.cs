using lexicon_Garage2.Data;

using lexicon_Garage2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace lexicon_Garage2.Controllers
{
    [Authorize]
    public class VehiclesController : Controller
    {
        private readonly lexicon_Garage2Context _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public readonly decimal ParkingHourlyPrice = 100;
        public static decimal AccumulatedParkingRevenue { get; set; }



        public VehiclesController(
            UserManager<ApplicationUser> userManager,
            lexicon_Garage2Context context
        )
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<ParkingSpotViewModel>> GetParkingStatusAsync()
        {
            List<Vehicle> vehicles = await _context.Vehicles.Where(v => v.ParkingSpots.Any()).ToListAsync();

            List<ParkingSpotViewModel> parkingStatus = new();

            List<ParkingSpot> parkingSpots = await _context.ParkingSpots.ToListAsync();

            foreach (var parkingSpot in parkingSpots)
            {
                var vehicleInSpot = vehicles.FirstOrDefault(v => v.ParkingSpots.Contains(parkingSpot));
                {
                    parkingStatus.Add(
                    new ParkingSpotViewModel
                    {
                        SpotNumber = (parkingSpots.IndexOf(parkingSpot) + 1),
                        IsOccupied = vehicleInSpot != null,
                        RegistrationNumber =
                            vehicleInSpot?.RegistrationNumber // Assign registration number if occupied
                    }
                );
                }
            }

            return parkingStatus;
        }
        public async Task<int> GetAvailableSpotsAsync()
        {
            int occupiedSpots = 0;

            foreach (var parkingSpot in await _context.ParkingSpots.ToListAsync())
            {
                if (parkingSpot.IsOccupied)
                {
                    occupiedSpots++;
                }
            }


            var totalSpots = await _context.ParkingSpots.ToListAsync();

            return totalSpots.Count() - occupiedSpots;
        }

        private async Task<ParkingSpot?> GetNextAvailableSpotAsync()
        {
            var firstAvailableSpot = await _context.ParkingSpots.FirstOrDefaultAsync(ps => ps.Vehicle == null);

            if (firstAvailableSpot == null)
            {
                return null; // Om alla platser är upptagna
            }
            return firstAvailableSpot; // Returnera första lediga plats(om det finns någon)
        }

        public async Task<IActionResult> ParkingSpot()
        {
            ViewBag.AvailableSpots = await GetAvailableSpotsAsync();
            ViewBag.ParkingStatus = await GetParkingStatusAsync();
            var parkingStatus = await GetParkingStatusAsync();
            return View(parkingStatus); // Pass the list of ParkingSpotViewModel
        }

        // GET: Vehicles
        public async Task<IActionResult> Admin()
        {
            return View(await _context.Vehicles.ToListAsync());
        }

        public async Task<IActionResult> VehicleTypeIndex()
        {
            return View(await _context.VehicleTypes.ToListAsync());
        }

        // GET: Garage
        public async Task<IActionResult> Garage(
            string? searchTerm = null,
            string sortColumn = "ArrivalTime",
            string sortOrder = "asc",
            string? timeFilter = null
        )
        {
            ViewBag.AvailableSpots = await GetAvailableSpotsAsync();
            ViewBag.ParkingStatus = await GetParkingStatusAsync(); // Lägger till status för lediga/upptagna platser

            IQueryable<Vehicle> vehicles = _context.Vehicles.Include(v => v.VehicleType);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                if (await _context.Vehicles.AnyAsync(v => v.VehicleType.TypeName == searchTerm))
                {
                    vehicles = vehicles.Where(v => v.VehicleType.TypeName == searchTerm);
                }
                else
                {
                    vehicles = vehicles.Where(v => v.RegistrationNumber.Contains(searchTerm));
                }
            }

            if (!string.IsNullOrEmpty(timeFilter))
            {
                var now = DateTime.Now;
                vehicles = timeFilter switch
                {
                    "minute" => vehicles.Where(v => v.ParkingTime >= now.AddMinutes(-1)),
                    "hour" => vehicles.Where(v => v.ParkingTime >= now.AddHours(-1)),
                    "day" => vehicles.Where(v => v.ParkingTime >= now.AddDays(-1)),
                    _ => vehicles,
                };
            }

            vehicles = sortColumn switch
            {
                "RegistrationNumber" => sortOrder == "asc"
                    ? vehicles.OrderBy(v => v.RegistrationNumber)
                    : vehicles.OrderByDescending(v => v.RegistrationNumber),
                "VehicleType" => sortOrder == "asc"
                    ? vehicles.OrderBy(v => v.VehicleType)
                    : vehicles.OrderByDescending(v => v.VehicleType),
                "ArrivalTime" => sortOrder == "asc"
                    ? vehicles.OrderBy(v => v.ParkingTime)
                    : vehicles.OrderByDescending(v => v.ParkingTime),
                "ParkingSpot" => sortOrder == "asc"
                    ? vehicles.OrderBy(v => v.ParkingSpots)
                    : vehicles.OrderByDescending(v => v.ParkingSpots),
                _ => vehicles.OrderBy(v => v.ParkingTime),
            };

            var vehicleViewModels = await vehicles
                .Select(vehicle => new VehicleViewModel(vehicle)
                {
                    VehicleType = vehicle.VehicleType.TypeName,
                })
                .ToListAsync();

            ViewData["CurrentSort"] = $"{sortColumn}_{sortOrder}";

            return View(vehicleViewModels);
        }

        public async Task<IActionResult> Filter(string registrationNumber)
        {
            var model = string.IsNullOrWhiteSpace(registrationNumber)
                ? _context.Vehicles
                : _context.Vehicles.Where(m => m.RegistrationNumber.Contains(registrationNumber));

            return View(nameof(Admin), await model.ToListAsync());
        }

        // GET: Vehicles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(m => m.Id == id);
            if (vehicle == null)
            {
                return NotFound();
            }

            return View(vehicle);
        }

        // GET: Vehicles/Create
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VehicleCreateViewmodel vehicleViewModel)
        {


            // Hämta en ledig parkeringsplats
            var availableSpot = await _context.ParkingSpots
                .FirstOrDefaultAsync(spot => !spot.IsOccupied);

            if (availableSpot == null)
            {
                TempData["ErrorMessage"] = "The garage is full. No available parking spots.";
                return RedirectToAction(nameof(Garage));
            }


            if (ModelState.IsValid)
            {
                try
                {

                    var vehicle = new Vehicle
                    {
                        Brand = vehicleViewModel.Brand,
                        Color = vehicleViewModel.Color,
                        NumberOfWheels = vehicleViewModel.NumberOfWheels,
                        Model = vehicleViewModel.Model,
                        VehicleTypeId = vehicleViewModel.VehicleTypeId,
                        RegistrationNumber = vehicleViewModel.RegistrationNumber,
                    };

                    vehicle.ParkingSpots.Add(availableSpot);
                    availableSpot.IsOccupied = true;


                    _context.Vehicles.Add(vehicle);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Vehicle parked at spot #{availableSpot.SpotNumber}.";
                    return RedirectToAction(nameof(Garage));
                }
                catch (DbUpdateException ex)
                    when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601)
                {
                    ModelState.AddModelError(
                        "RegistrationNumber",
                        "A vehicle with this registration number is already parked."
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

            TempData["ErrorMessage"] = "Could not park the vehicle. Please check your inputs.";
            return View(vehicleViewModel);
        }




        // GET: Vehicles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }
            return View(vehicle);
        }

        // POST: Vehicles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Vehicles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,VehicleType,RegistrationNumber,Color,Brand,Model,NumberOfWheels,ParkingTime")]
                Vehicle vehicle
        )
        {
            if (id != vehicle.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Retrieve the original vehicle data, including the parking spot
                    var existingVehicle = await _context
                        .Vehicles.AsNoTracking()
                        .FirstOrDefaultAsync(v => v.Id == id);

                    if (existingVehicle == null)
                    {
                        return NotFound();
                    }

                    // Preserve original ParkingTime and ParkingSpot
                    vehicle.ParkingTime = existingVehicle.ParkingTime;
                    vehicle.ParkingSpots = existingVehicle.ParkingSpots;

                    _context.Update(vehicle);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "The vehicle has been updated.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await VehicleExists(vehicle.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Garage));
            }

            TempData["ErrorMessage"] = "Could not update. Please check your inputs.";
            return View(vehicle);
        }

        // GET: Vehicles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(m => m.Id == id);
            if (vehicle == null)
            {
                return NotFound();
            }

            return View(vehicle);
        }

        // POST: Vehicles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vehicle = await _context.Vehicles.Include(v => v.ParkingSpots).FirstOrDefaultAsync(v => v.Id == id);
            if (vehicle != null)
            {
                // Create the receipt view model before the vehicle gets changed
                var receiptViewModel = new ReceiptViewModel(vehicle, ParkingHourlyPrice);

                // Remove the vehicle
                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Parking has ended.";

                AccumulatedParkingRevenue += receiptViewModel.Total;
                return View("Receipt", receiptViewModel);
            }
            else
            {
                TempData["ErrorMessage"] = "Could not find the vehicle.";
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Garage));
        }


        // GET: Statistics
        public async Task<IActionResult> Statistics()
        {
            var vehicles = await _context.Vehicles.ToListAsync();
            var vehicleStats = new StatisticsViewModel
            {
                NumberOfVehiclesParked = vehicles.Count,
                NumberOfWheelsInGarage = vehicles.Sum(v => v.NumberOfWheels),
                UnrealizedParkingRevenue =
                    vehicles.Sum(v => (decimal)(DateTime.Now - v.ParkingTime).TotalHours)
                    * ParkingHourlyPrice,
                AccumulatedParkingRevenueView = AccumulatedParkingRevenue,
            };

            return View(vehicleStats);
        }

        private async Task<bool> VehicleExists(int id)
        {
            return await _context.Vehicles.AnyAsync(v => v.Id == id);
        }

        //POST Add spot
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> CreateParkingSpot()
        {
            // Hämta det högsta befintliga SpotNumber
            var maxSpotNumber = _context.ParkingSpots.Any()
                ? _context.ParkingSpots.Max(ps => ps.SpotNumber)
                : 0;

            // Skapa en ny ParkingSpot med nästa SpotNumber
            var newSpot = new ParkingSpot
            {
                SpotNumber = maxSpotNumber + 1 // Tilldela nästa löpande nummer
            };

            // Lägg till den nya ParkingSpot i databasen
            _context.ParkingSpots.Add(newSpot);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Parking spot {newSpot.SpotNumber} created successfully.";
            return RedirectToAction(nameof(ParkingSpot));
        }



    }
}
