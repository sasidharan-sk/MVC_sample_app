using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MVC_sample_app.Data;
using MVC_sample_app.Models.ViewModels;

public class EmployeeController : Controller
{
    private readonly ApplicationDbContext _context;

    public EmployeeController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Display all employees
    public async Task<IActionResult> Index()
    {
        var employees = await _context.Employees
            .FromSqlRaw("EXEC spGetAllEmployees")
            .ToListAsync();

        var employeeViewModels = employees.Select(e => new EmployeeViewModel
        {
            ID = e.ID,
            Name = e.Name,
            DateOfBirth = e.DateOfBirth,
            Email = e.Email,
            Picture = e.Picture
        }).ToList();

        return View(employeeViewModels);
    }

    // Add a new employee
    [HttpPost]
    public async Task<IActionResult> Add(EmployeeViewModel model)
    {
        if (ModelState.IsValid)
        {
            byte[] pictureData = null;

            if (model.PictureFile != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await model.PictureFile.CopyToAsync(memoryStream);
                    pictureData = memoryStream.ToArray();
                }
            }

            var parameters = new[]
            {
                new SqlParameter("@Name", model.Name),
                new SqlParameter("@DateOfBirth", model.DateOfBirth),
                new SqlParameter("@Email", model.Email),
                new SqlParameter("@Picture", (object)pictureData ?? DBNull.Value)
            };

            await _context.Database.ExecuteSqlRawAsync("EXEC spAddEmployee @Name, @DateOfBirth, @Email, @Picture", parameters);
            return RedirectToAction(nameof(Index));
        }

        return View("Index");
    }

    // Edit an employee
    [HttpPost]
    public async Task<IActionResult> Edit(EmployeeViewModel model)
    {
        if (ModelState.IsValid)
        {
            byte[] pictureData = null;

            if (model.PictureFile != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await model.PictureFile.CopyToAsync(memoryStream);
                    pictureData = memoryStream.ToArray();
                }
            }

            var parameters = new[]
            {
                new SqlParameter("@ID", model.ID),
                new SqlParameter("@Name", model.Name),
                new SqlParameter("@DateOfBirth", model.DateOfBirth),
                new SqlParameter("@Email", model.Email),
                new SqlParameter("@Picture", (object)pictureData ?? DBNull.Value)
            };

            await _context.Database.ExecuteSqlRawAsync("EXEC spUpdateEmployee @ID, @Name, @DateOfBirth, @Email, @Picture", parameters);
            return RedirectToAction(nameof(Index));
        }

        return View("Index");
    }

    // Delete an employee
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _context.Database.ExecuteSqlRawAsync("EXEC spDeleteEmployee @ID", new SqlParameter("@ID", id));
        return RedirectToAction(nameof(Index));
    }
}
