using Microsoft.EntityFrameworkCore;
using ClinicAppointmentSystem.Data;
using ClinicAppointmentSystem.Models;

namespace ClinicAppointmentSystem.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly ApplicationDbContext _context;

    public PatientRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Patient?> GetByIdAsync(int id)
    {
        return await _context.Patients
            .Include(p => p.Clinic)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Patient?> GetByIdWithAppointmentsAsync(int id)
    {
        return await _context.Patients
            .Include(p => p.Clinic)
            .Include(p => p.Appointments)
                .ThenInclude(a => a.Doctor)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Patient>> GetByClinicIdAsync(int clinicId)
    {
        return await _context.Patients
            .Include(p => p.Clinic)
            .Where(p => p.ClinicId == clinicId)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Patient>> SearchPatientsAsync(
        int clinicId, 
        string? query = null, 
        string? name = null, 
        string? phone = null, 
        string? email = null, 
        int page = 1, 
        int pageSize = 20)
    {
        var queryable = _context.Patients
            .Include(p => p.Clinic)
            .Where(p => p.ClinicId == clinicId);

        // Apply search filters
        if (!string.IsNullOrWhiteSpace(query))
        {
            var searchTerm = query.ToLower();
            queryable = queryable.Where(p => 
                p.Name.ToLower().Contains(searchTerm) ||
                (p.Phone != null && p.Phone.Contains(searchTerm)) ||
                (p.Email != null && p.Email.ToLower().Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            queryable = queryable.Where(p => p.Name.ToLower().Contains(name.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(phone))
        {
            queryable = queryable.Where(p => p.Phone != null && p.Phone.Contains(phone));
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            queryable = queryable.Where(p => p.Email != null && p.Email.ToLower().Contains(email.ToLower()));
        }

        return await queryable
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetPatientCountAsync(
        int clinicId, 
        string? query = null, 
        string? name = null, 
        string? phone = null, 
        string? email = null)
    {
        var queryable = _context.Patients
            .Where(p => p.ClinicId == clinicId);

        // Apply search filters
        if (!string.IsNullOrWhiteSpace(query))
        {
            var searchTerm = query.ToLower();
            queryable = queryable.Where(p => 
                p.Name.ToLower().Contains(searchTerm) ||
                (p.Phone != null && p.Phone.Contains(searchTerm)) ||
                (p.Email != null && p.Email.ToLower().Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            queryable = queryable.Where(p => p.Name.ToLower().Contains(name.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(phone))
        {
            queryable = queryable.Where(p => p.Phone != null && p.Phone.Contains(phone));
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            queryable = queryable.Where(p => p.Email != null && p.Email.ToLower().Contains(email.ToLower()));
        }

        return await queryable.CountAsync();
    }

    public async Task<Patient> CreateAsync(Patient patient)
    {
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();
        return patient;
    }

    public async Task<Patient> UpdateAsync(Patient patient)
    {
        _context.Patients.Update(patient);
        await _context.SaveChangesAsync();
        return patient;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var patient = await _context.Patients.FindAsync(id);
        if (patient == null)
            return false;

        _context.Patients.Remove(patient);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Patients.AnyAsync(p => p.Id == id);
    }

    public async Task<bool> ExistsInClinicAsync(int id, int clinicId)
    {
        return await _context.Patients.AnyAsync(p => p.Id == id && p.ClinicId == clinicId);
    }
}