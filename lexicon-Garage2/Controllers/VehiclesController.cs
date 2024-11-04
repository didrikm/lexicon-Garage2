using lexicon_Garage2.Data;
using lexicon_Garage2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace lexicon_Garage2.Controllers
{
    public class VehiclesController : Controller
    {
        private readonly lexicon_Garage2Context _context;
        private const int MaxCapacity = 100;

        public VehiclesController(lexicon_Garage2Context context)
        {
            _context = context;
        }

        public int GetAvailableSpots()
        {
            int occupiedSpots = _context.Vehicle.Count();
            return MaxCapacity - occupiedSpots;
        }

        public int? GetNextAvailableSpot()
        {
            var occupiedSpots = _context
                .Vehicle.Where(v => v.ParkingSpot != null)
                .Select(v => v.ParkingSpot!.Value)
                .ToList();

            for (int i = 1; i <= MaxCapacity; i++)
            {
                if (!occupiedSpots.Contains(i))
                {
                    return i;
                }
            }
            return null; // No spots available
        }

        // GET: Vehicles
        public async Task<IActionResult> Admin()
        {
            return View(await _context.Vehicle.ToListAsync());
        }

        // GET: Filter data
        public async Task<IActionResult> Filter(string registrationNumber)
        {
            var model = string.IsNullOrWhiteSpace(registrationNumber)
                ? _context.Vehicle
                : _context.Vehicle.Where(m => m.RegistrationNumber.Contains(registrationNumber));

            return View(nameof(Garage), await model.ToListAsync());
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
            if (GetAvailableSpots() <= 0)
            {
                TempData["ErrorMessage"] = "The garage is full. No more spots are available!";
                return RedirectToAction(nameof(Garage));
            }

            vehicle.ParkingSpot = GetNextAvailableSpot();

            if (vehicle.ParkingSpot == null)
            {
                TempData["ErrorMessage"] = "No available parking spots found.";
                return RedirectToAction(nameof(Garage));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(vehicle);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Vehicle parked at spot {vehicle.ParkingSpot}.";
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
                    Console.WriteLine("Error:", ex);
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vehicle = await _context.Vehicle.FindAsync(id);

            if (vehicle != null)
            {
                // Capture the vehicle data for the receipt before deletion
                var receiptViewModel = new ReceiptViewModel
                {
                    Id = vehicle.Id,
                    VehicleType = vehicle.VehicleType,
                    RegistrationNumber = vehicle.RegistrationNumber,
                    ArrivalTime = vehicle.ParkingTime, // Use ParkingTime if ArrivalTime does not exist
                    ParkingSpot = vehicle.ParkingSpot,
                };

                // Remove the vehicle from the database
                _context.Vehicle.Remove(vehicle);
                await _context.SaveChangesAsync();

                // Set a success message with parking spot information
                TempData["SuccessMessage"] =
                    $"Vehicle with registration {vehicle.RegistrationNumber} has left. Spot {vehicle.ParkingSpot} is now available.";

                // Return the receipt view with the populated receipt model
                return View("Receipt", receiptViewModel);
            }
            else
            {
                // Handle case if the vehicle is not found
                TempData["ErrorMessage"] = "Could not find the vehicle.";
            }

            return RedirectToAction(nameof(Garage));
        }

        private bool VehicleExists(int id)
        {
            return _context.Vehicle.Any(e => e.Id == id);
        }
    }
}
