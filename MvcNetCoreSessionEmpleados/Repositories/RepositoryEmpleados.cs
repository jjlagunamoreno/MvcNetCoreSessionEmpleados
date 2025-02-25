using Microsoft.EntityFrameworkCore;
using MvcNetCoreSessionEmpleados.Data;
using MvcNetCoreSessionEmpleados.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MvcNetCoreSessionEmpleados.Repositories
{
    public class RepositoryEmpleados
    {
        private HospitalContext context;

        public RepositoryEmpleados(HospitalContext context)
        {
            this.context = context;
        }

        public async Task<List<Empleado>> GetEmpleadosAsync()
        {
            return await this.context.Empleados.ToListAsync();
        }

        public async Task<Empleado> FindEmpleadoAsync(int idEmpleado)
        {
            return await this.context.Empleados
                .FirstOrDefaultAsync(e => e.IdEmpleado == idEmpleado);
        }
    }
}
