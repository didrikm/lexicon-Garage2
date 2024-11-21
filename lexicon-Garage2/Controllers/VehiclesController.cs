using lexicon_Garage2.Data;
using lexicon_Garage2.Migrations;
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

        private const int MaxCapacity = 10;

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
            var vehicles = await _context.Vehicle.Where(v => v.ParkingSpot.HasValue).ToListAsync();

            var parkingStatus = new List<ParkingSpotViewModel>();
            for (int i = 1; i <= MaxCapacity; i++)
            {
                var vehicleInSpot = vehicles.FirstOrDefault(v => v.ParkingSpot == i);

                parkingStatus.Add(
                    new ParkingSpotViewModel
                    {
                        SpotNumber = i,
                        IsOccupied = vehicleInSpot != null,
                        RegistrationNumber =
                            vehicleInSpot?.RegistrationNumber // Assign registration number if occupied
                        ,
                    }
                );
            }

            return parkingStatus;
        }

        public int GetAvailableSpots()
        {
            int occupiedSpots = _context.Vehicle.Count();
            return MaxCapacity - occupiedSpots;
        }

        // Metod för att hitta nästa lediga platsnummer
        private int? GetNextAvailableSpot()
        {
            var occupiedSpots = _context
                .Vehicle.Where(v => v.ParkingSpot.HasValue)
                .Select(v => v.ParkingSpot.Value)
                .ToList();

            for (int i = 1; i <= MaxCapacity; i++)
            {
                if (!occupiedSpots.Contains(i))
                {
                    return i; // Returnera första lediga plats
                }
            }

            return null; // Om alla platser är upptagna
        }

        public async Task<IActionResult> ParkedVehicle()
        {
            // Hämta alla fordon och inkludera relaterad användare
            var parkedVehicles = await _context
                .Vehicle.Include(v => v.ApplicationUser) // Inkludera navigeringsegenskapen för ägaren
                .ToListAsync();

            // Skapa en lista av ViewModels baserad på de hämtade fordonen
            var viewModel = parkedVehicles
                .Where(vehicle => vehicle.ApplicationUser != null) // Exkludera poster utan ägare
                .Select(vehicle => new ParkedVehicleViewModel(
                    vehicle,
                    vehicle.ApplicationUser // Skicka relaterad ApplicationUser som ägare
                ))
                .ToList();

            // Returnera vyn med ViewModel
            return View(viewModel);
        }

        public async Task<IActionResult> ParkingSpot()
        {
            ViewBag.AvailableSpots = GetAvailableSpots();
            ViewBag.ParkingStatus = await GetParkingStatusAsync();
            var parkingStatus = await GetParkingStatusAsync();
            return View(parkingStatus); // Pass the list of ParkingSpotViewModel
        }

        // GET: Vehicles
        public async Task<IActionResult> Admin()
        {
            return View(await _context.Vehicle.ToListAsync());
        }

        // GET: Garage
        public async Task<IActionResult> Garage(
            string? searchTerm = null,
            string sortColumn = "ArrivalTime",
            string sortOrder = "asc",
            string? timeFilter = null
        )
        {
            ViewBag.AvailableSpots = GetAvailableSpots();
            ViewBag.ParkingStatus = await GetParkingStatusAsync(); // Lägger till status för lediga/upptagna platser

            IQueryable<Vehicle> vehicles = _context.Vehicle;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                if (Enum.TryParse<VehicleType>(searchTerm, out var vehicleTypeEnum))
                {
                    vehicles = vehicles.Where(v => v.VehicleType == vehicleTypeEnum);
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
                    ? vehicles.OrderBy(v => v.ParkingSpot)
                    : vehicles.OrderByDescending(v => v.ParkingSpot),
                _ => vehicles.OrderBy(v => v.ParkingTime),
            };

            var vehicleViewModels = await vehicles
                .Select(vehicle => new VehicleViewModel(vehicle))
                .ToListAsync();

            ViewData["CurrentSort"] = $"{sortColumn}_{sortOrder}";

            return View(vehicleViewModels);
        }

        public async Task<IActionResult> Filter(string registrationNumber)
        {
            var model = string.IsNullOrWhiteSpace(registrationNumber)
                ? _context.Vehicle
                : _context.Vehicle.Where(m => m.RegistrationNumber.Contains(registrationNumber));

            return View(nameof(Admin), await model.ToListAsync());
        }

        // GET: Vehicles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicle.FirstOrDefaultAsync(m => m.Id == id);
            if (vehicle == null)
            {
                return NotFound();
            }

            return View(vehicle);
        }

        // GET: Vehicles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Vehicles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("VehicleType,RegistrationNumber,Color,Brand,Model,NumberOfWheels")]
                VehicleViewModel vehicleViewModel
        )
        {
            // Check if there are available spots
            if (GetAvailableSpots() <= 0)
            {
                TempData["ErrorMessage"] = "The garage is full. No more spots are available!";
                return RedirectToAction(nameof(Garage));
            }

            // Get the next available parking spot
            var nextAvailableSpot = GetNextAvailableSpot();
            if (nextAvailableSpot == null)
            {
                TempData["ErrorMessage"] = "The garage is full - no available spots.";
                return RedirectToAction(nameof(Garage));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the currently logged-in user
                    var currentUser = await _userManager.GetUserAsync(User);

                    if (currentUser == null)
                    {
                        TempData["ErrorMessage"] = "User not found.";
                        return RedirectToAction(nameof(Garage));
                    }

                    // Map the VehicleViewModel to the Vehicle entity
                    var vehicle = new Vehicle
                    {
                        VehicleType = vehicleViewModel.VehicleType,
                        RegistrationNumber = vehicleViewModel.RegistrationNumber,
                        Color = vehicleViewModel.Color,
                        Brand = vehicleViewModel.Brand,
                        Model = vehicleViewModel.Model,
                        NumberOfWheels = vehicleViewModel.NumberOfWheels,
                        ApplicationUserId = currentUser.Id, // Set the UserId (Foreign Key)
                        ApplicationUser = currentUser, // Set the navigation property
                        ParkingSpot = nextAvailableSpot,
                        ParkingTime = DateTime.Now,
                    };

                    // Add the vehicle to the context and save changes
                    _context.Add(vehicle);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Parking has started.";
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
                    // Log or handle any other exception
                    Console.WriteLine("Error: ", ex);
                    TempData["ErrorMessage"] = "An unexpected error occurred.";
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

            var vehicle = await _context.Vehicle.FindAsync(id);
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
                        .Vehicle.AsNoTracking()
                        .FirstOrDefaultAsync(v => v.Id == id);

                    if (existingVehicle == null)
                    {
                        return NotFound();
                    }

                    // Preserve original ParkingTime and ParkingSpot
                    vehicle.ParkingTime = existingVehicle.ParkingTime;
                    vehicle.ParkingSpot = existingVehicle.ParkingSpot;

                    _context.Update(vehicle);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "The vehicle has been updated.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VehicleExists(vehicle.Id))
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

            var vehicle = await _context.Vehicle.FirstOrDefaultAsync(m => m.Id == id);
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
            var vehicle = await _context.Vehicle.FindAsync(id);
            if (vehicle != null)
            {
                // Store the parking spot number before removing the vehicle
                int? parkingSpotNumber = vehicle.ParkingSpot;

                _context.Vehicle.Remove(vehicle);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Parking has ended.";

                // Create the receipt view model with the parking spot number
                var receiptViewModel = new ReceiptViewModel(vehicle, ParkingHourlyPrice);
                receiptViewModel.ParkingSpotNumber = parkingSpotNumber ?? 0;

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
            var vehicles = await _context.Vehicle.ToListAsync();
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

        private bool VehicleExists(int id)
        {
            return _context.Vehicle.Any(e => e.Id == id);
        }
    }
}
