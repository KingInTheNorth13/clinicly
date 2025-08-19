using Microsoft.EntityFrameworkCore;
using ClinicAppointmentSystem.Data;
using ClinicAppointmentSystem.Models;

namespace ClinicAppointmentSystem.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly ApplicationDbContext _context;

    public AppointmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Appointment?> GetByIdAsync(int id)
    {
        return await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Appointment?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Appointments
            .Include(a => a.Doctor)
                .ThenInclude(d => d.Clinic)
            .Include(a => a.Patient)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Appointment>> GetByDoctorIdAsync(int doctorId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Appointments
            .Include(a => a.Patient)
            .Where(a => a.DoctorId == doctorId);

        if (startDate.HasValue)
        {
            query = query.Where(a => a.DateTime >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(a => a.DateTime <= endDate.Value);
        }

        return await query
            .OrderBy(a => a.DateTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByPatientIdAsync(int patientId)
    {
        return await _context.Appointments
            .Include(a => a.Doctor)
                .ThenInclude(d => d.Clinic)
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.DateTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByClinicIdAsync(int clinicId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Appointments
            .Include(a => a.Doctor)
            .Include(a => a.Patient)
            .Where(a => a.Doctor.ClinicId == clinicId);

        if (startDate.HasValue)
        {
            query = query.Where(a => a.DateTime >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(a => a.DateTime <= endDate.Value);
        }

        return await query
            .OrderBy(a => a.DateTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsAsync(
        int? doctorId = null, 
        int? patientId = null, 
        int? clinicId = null,
        DateTime? startDate = null, 
        DateTime? endDate = null,
        AppointmentStatus? status = null,
        int page = 1, 
        int pageSize = 20)
    {
        var query = _context.Appointments
            .Include(a => a.Doctor)
                .ThenInclude(d => d.Clinic)
            .Include(a => a.Patient)
            .AsQueryable();

        // Apply filters
        if (doctorId.HasValue)
        {
            query = query.Where(a => a.DoctorId == doctorId.Value);
        }

        if (patientId.HasValue)
        {
            query = query.Where(a => a.PatientId == patientId.Value);
        }

        if (clinicId.HasValue)
        {
            query = query.Where(a => a.Doctor.ClinicId == clinicId.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(a => a.DateTime >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(a => a.DateTime <= endDate.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        return await query
            .OrderBy(a => a.DateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetAppointmentCountAsync(
        int? doctorId = null, 
        int? patientId = null, 
        int? clinicId = null,
        DateTime? startDate = null, 
        DateTime? endDate = null,
        AppointmentStatus? status = null)
    {
        var query = _context.Appointments.AsQueryable();

        // Apply filters
        if (doctorId.HasValue)
        {
            query = query.Where(a => a.DoctorId == doctorId.Value);
        }

        if (patientId.HasValue)
        {
            query = query.Where(a => a.PatientId == patientId.Value);
        }

        if (clinicId.HasValue)
        {
            query = query.Where(a => a.Doctor.ClinicId == clinicId.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(a => a.DateTime >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(a => a.DateTime <= endDate.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        return await query.CountAsync();
    }

    public async Task<bool> HasConflictAsync(int doctorId, DateTime dateTime, int? excludeAppointmentId = null)
    {
        var query = _context.Appointments
            .Where(a => a.DoctorId == doctorId && 
                       a.DateTime == dateTime && 
                       a.Status != AppointmentStatus.Cancelled);

        if (excludeAppointmentId.HasValue)
        {
            query = query.Where(a => a.Id != excludeAppointmentId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<IEnumerable<Appointment>> GetConflictingAppointmentsAsync(int doctorId, DateTime dateTime, int? excludeAppointmentId = null)
    {
        var query = _context.Appointments
            .Include(a => a.Patient)
            .Where(a => a.DoctorId == doctorId && 
                       a.DateTime == dateTime && 
                       a.Status != AppointmentStatus.Cancelled);

        if (excludeAppointmentId.HasValue)
        {
            query = query.Where(a => a.Id != excludeAppointmentId.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<Appointment> CreateAsync(Appointment appointment)
    {
        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();
        return appointment;
    }

    public async Task<Appointment> UpdateAsync(Appointment appointment)
    {
        _context.Appointments.Update(appointment);
        await _context.SaveChangesAsync();
        return appointment;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null)
            return false;

        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Appointments.AnyAsync(a => a.Id == id);
    }

    public async Task<bool> ExistsForDoctorAsync(int id, int doctorId)
    {
        return await _context.Appointments.AnyAsync(a => a.Id == id && a.DoctorId == doctorId);
    }

    public async Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(DateTime fromDate, int? doctorId = null)
    {
        var query = _context.Appointments
            .Include(a => a.Doctor)
                .ThenInclude(d => d.Clinic)
            .Include(a => a.Patient)
            .Where(a => a.DateTime >= fromDate && a.Status == AppointmentStatus.Scheduled);

        if (doctorId.HasValue)
        {
            query = query.Where(a => a.DoctorId == doctorId.Value);
        }

        return await query
            .OrderBy(a => a.DateTime)
            .ToListAsync();
    }
}