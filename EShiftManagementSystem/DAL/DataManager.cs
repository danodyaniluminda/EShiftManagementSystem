using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using EShiftManagementSystem.Models;
using System.Linq;

namespace EShiftManagementSystem.DAL
{
    public class DataManager
    {
        private readonly EShiftDbContext _context;

        public DataManager(EShiftDbContext context)
        {
            _context = context;
        }

        // Customers DB Operations
        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            return await _context.Customers.ToListAsync();
        }

        public async Task<Customer> GetCustomerByEmailAsync(string email)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task AddCustomerAsync(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCustomerAsync(int customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Customer> GetCustomerByUsernameAsync(string username)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.Username == username);
        }

        public async Task<Admin> GetAdminByUsernameAsync(string username)
        {
            return await _context.Admins.FirstOrDefaultAsync(a => a.Username == username);
        }
        public async Task<List<Job>> GetJobsByCustomerIdAsync(int customerId)
        {
            return await _context.Jobs
                .Where(j => j.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task<List<Load>> GetLoadsByCustomerIdAsync(int customerId)
        {
            return await _context.Loads
                .Where(l => l.Job.CustomerId == customerId)
                .Include(l => l.Job) 
                .ToListAsync();
        }




        // Jobs DB Operations
        public async Task<List<Job>> GetAllJobsAsync()
        {
            return await _context.Jobs.Include(j => j.Customer).ToListAsync();
        }

        public async Task<List<Job>> GetJobsByCustomerEmailAsync(string email)
        {
            return await _context.Jobs
                .Include(j => j.Customer)
                .Where(j => j.Customer.Email == email)
                .ToListAsync();
        }

        public async Task AddJobAsync(Job job)
        {
            await _context.Jobs.AddAsync(job);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateJobAsync(Job job)
        {
            _context.Jobs.Update(job);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateJobStatusAsync(int jobId, string status)
        {
            var job = await _context.Jobs.FindAsync(jobId);
            if (job != null)
            {
                job.Status = status;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteJobAsync(int jobId)
        {
            var job = await _context.Jobs.FindAsync(jobId);
            if (job != null)
            {
                _context.Jobs.Remove(job);
                await _context.SaveChangesAsync();
            }
        }

        // Loads DB Operations
        public async Task<List<Load>> GetAllLoadsAsync()
        {
            return await _context.Loads.Include(l => l.Job).Include(l => l.TransportUnit).ToListAsync();
        }

        public async Task<List<Load>> GetLoadsByJobIdAsync(int jobId)
        {
            return await _context.Loads
                .Where(l => l.JobId == jobId)
                .Include(l => l.Job)
                .Include(l => l.TransportUnit)
                .ToListAsync();
        }

        public async Task AddLoadAsync(Load load)
        {
            await _context.Loads.AddAsync(load);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateLoadAsync(Load load)
        {
            _context.Loads.Update(load);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteLoadAsync(int loadId)
        {
            var load = await _context.Loads.FindAsync(loadId);
            if (load != null)
            {
                _context.Loads.Remove(load);
                await _context.SaveChangesAsync();
            }
        }

        // TransportUnits DB Operations
        public async Task<List<TransportUnit>> GetAllTransportUnitsAsync()
        {
            return await _context.TransportUnits.ToListAsync();
        }

        public async Task<List<TransportUnit>> GetAvailableTransportUnitsAsync()
        {
            return await _context.TransportUnits.Where(t => t.IsAvailable).ToListAsync();
        }

        public async Task AddTransportUnitAsync(TransportUnit unit)
        {
            await _context.TransportUnits.AddAsync(unit);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTransportUnitAsync(TransportUnit unit)
        {
            _context.TransportUnits.Update(unit);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTransportUnitAsync(int id)
        {
            var unit = await _context.TransportUnits.FindAsync(id);
            if (unit != null)
            {
                _context.TransportUnits.Remove(unit);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateTransportUnitAvailabilityAsync(int transportUnitId, bool isAvailable)
        {
            var unit = await _context.TransportUnits.FindAsync(transportUnitId);
            if (unit != null)
            {
                unit.IsAvailable = isAvailable;
                await _context.SaveChangesAsync();
            }
        }

        // Reports and Analytics
        public async Task<Dictionary<string, int>> GetDashboardStatsAsync()
        {
            var stats = new Dictionary<string, int>();

            stats["TotalCustomers"] = await _context.Customers.CountAsync();
            stats["ActiveJobs"] = await _context.Jobs.CountAsync(j =>
                j.Status == "Active" || j.Status == "Pending" || j.Status == "Accepted" || j.Status == "In Progress");
            stats["TransportUnits"] = await _context.TransportUnits.CountAsync();
            stats["CompletedJobs"] = await _context.Jobs.CountAsync(j => j.Status == "Completed");

            return stats;
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.Jobs
                .Where(j => j.Status == "Completed")
                .SumAsync(j => j.Cost);
        }


        public async Task<List<Job>> GetRecentJobsAsync()
        {
            return await _context.Jobs
                .OrderByDescending(j => j.CreatedDate)
                .Take(10)
                .ToListAsync();
        }
    }
}