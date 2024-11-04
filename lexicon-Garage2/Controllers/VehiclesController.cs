using lexicon_Garage2.Data;
using lexicon_Garage2.Migrations;
using lexicon_Garage2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace lexicon_Garage2.Controllers
{
    public class VehiclesController : Controller
    {
        private readonly lexicon_Garage2Context _context;

        public readonly decimal ParkingHourlyPrice = 100;
        public static decimal AccumulatedParkingRevenue { get; set; }

        private const int MaxCapacity = 100;

        public VehiclesController(lexicon_Garage2Context context)
        {
            _context = context;
        }

        public class ParkingSpotViewModel
        {
            public int SpotNumber { get; set; }
            public bool IsOccupied { get; set; }
        }

        public async Task<List<ParkingSpotViewModel>> GetParkingStatusAsync()
        {
            var occupiedSpots = await _context
                .Vehicle.Where(v => v.ParkingSpot.HasValue)
                .Select(v => v.ParkingSpot!.Value)
                .ToListAsync();

            var parkingStatus = new List<ParkingSpotViewModel>();
            for (int i = 1; i <= MaxCapacity; i++)
            {
                parkingStatus.Add(
                    new ParkingSpotViewModel
                    {
                        SpotNumber = i,
                        IsOccupied = occupiedSpots.Contains(i),
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
                .Select(v => v.ParkingSpot!.Value)
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
                _ => vehicles.OrderBy(v => v.ParkingTime),
            };

            var vehicleViewModels = await vehicles
                .Select(vehicle => new VehicleViewModel(vehicle))
                .ToListAsync();

            ViewData["CurrentSort"] = $"{sortColumn}_{sortOrder}";

            return View(vehicleViewModels);
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
            [Bind("Id,VehicleType,RegistrationNumber,Color,Brand,Model,NumberOfWheels")]
                Vehicle vehicle
        )
        {
            // Check if there are available spots
            if (GetAvailableSpots() <= 0)
            {
                TempData["ErrorMessage"] = "The garage is full. No more spots are available!";
                return RedirectToAction(nameof(Garage));
            }
            // Hämta nästa tillgängliga parkeringsplats
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
                    vehicle.ParkingSpot = nextAvailableSpot; // Tilldela den lediga platsen till fordonet
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
                    Console.WriteLine("Bigly error: ", ex);
                }
            }
            TempData["ErrorMessage"] = "Could not park the vehicle. Please check your inputs.";
            return View(vehicle);
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
                    var existingVehicle = await _context
                        .Vehicle.AsNoTracking()
                        .FirstOrDefaultAsync(v => v.Id == id);
                    if (existingVehicle == null)
                    {
                        return NotFound();
                    }

                    // Bevara originalvärdet för ArrivalTime
                    vehicle.ParkingTime = existingVehicle.ParkingTime;

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
                var receiptViewModel = new ReceiptViewModel(vehicle, ParkingHourlyPrice)
                {
                    ParkingSpotNumber =
                        parkingSpotNumber
                        ?? 0 // Pass the parking spot number
                    ,
                };

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
