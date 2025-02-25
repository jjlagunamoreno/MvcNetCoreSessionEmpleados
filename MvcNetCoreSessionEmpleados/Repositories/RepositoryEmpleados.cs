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
        public async Task<List<Empleado>>
            GetEmpleadosAsync(List<int> ids)
        {
            var consulta = from datos in this.context.Empleados
                           where ids.Contains(datos.IdEmpleado)
                           select datos;
            if (consulta.Count() == 0) 
            {
                return null;
            }
            else
            {
                return await consulta.ToListAsync();
            }
        }

        public async Task<List<Empleado>>
            GetEmpleadosNotSessionAsync(List<int> ids)
        {
            var consulta = from datos in this.context.Empleados
                           where ids.Contains(datos.IdEmpleado) == false
                           select datos;
            if(consulta.Count() == 0)
            {
                return null;
            }
            else
            {
                return await consulta.ToListAsync();
            }
        }
    }
}
