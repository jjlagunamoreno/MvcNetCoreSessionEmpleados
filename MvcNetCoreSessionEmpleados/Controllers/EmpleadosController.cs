using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MvcNetCoreSessionEmpleados.Extensions;
using MvcNetCoreSessionEmpleados.Models;
using MvcNetCoreSessionEmpleados.Repositories;

namespace MvcNetCoreSessionEmpleados.Controllers
{
    public class EmpleadosController : Controller
    {
        private RepositoryEmpleados repo;
        private IMemoryCache memoryCache; 

        public EmpleadosController
            (RepositoryEmpleados repo,
            IMemoryCache memoryCache)
        {
            this.repo = repo;
            this.memoryCache = memoryCache;
        }

        //                                          -- V5 --

        public IActionResult EliminarFavorito(int idEmpleado)
        {
            // Recuperamos la lista de empleados favoritos almacenados en cache
            List<Empleado> empleadosFavoritos = memoryCache.Get<List<Empleado>>("FAVORITOS");

            if (empleadosFavoritos != null)
            {
                // Eliminamos el empleado de la lista de favoritos
                Empleado emp = empleadosFavoritos.FirstOrDefault(e => e.IdEmpleado == idEmpleado);
                if (emp != null)
                {
                    empleadosFavoritos.Remove(emp);
                    // Si la lista queda vacía, eliminamos la clave de memoria
                    if (empleadosFavoritos.Count == 0)
                    {
                        memoryCache.Remove("FAVORITOS");
                    }
                    else
                    {
                        memoryCache.Set("FAVORITOS", empleadosFavoritos);
                    }
                }
            }

            return RedirectToAction("EmpleadosFavoritos");
        }

        public IActionResult EmpleadosFavoritos()
        {
            List<Empleado> favoritos = memoryCache.Get<List<Empleado>>("FAVORITOS") ?? new List<Empleado>();
            return View(favoritos);
        }

        public IActionResult EliminarEmpleadoV5(int idEmpleado)
        {
            // Recuperamos los IDs de empleados almacenados en Session
            List<int> idsEmpleados = HttpContext.Session.GetObject<List<int>>("IDSEMPLEADOS");

            if (idsEmpleados != null && idsEmpleados.Contains(idEmpleado))
            {
                // Eliminamos el ID del empleado de la lista
                idsEmpleados.Remove(idEmpleado);

                // Si la lista queda vacía, eliminamos la sesión
                if (idsEmpleados.Count == 0)
                {
                    HttpContext.Session.Remove("IDSEMPLEADOS");
                }
                else
                {
                    // Actualizamos la sesión con la nueva lista de empleados
                    HttpContext.Session.SetObject("IDSEMPLEADOS", idsEmpleados);
                }
            }

            // Redirigimos a la misma vista para ver los cambios reflejados
            return RedirectToAction("EmpleadosAlmacenadosV5");
        }
        
        public async Task<IActionResult> EmpleadosAlmacenadosV5()
        {
            //DEBEMOS RECUPERAR LOS IDS EMPLEADOS QUE TENGAMOS EN SESSION
            List<int> idsEmpleados =
                HttpContext.Session.GetObject<List<int>>("IDSEMPLEADOS");
            if (idsEmpleados == null)
            {
                ViewData["MENSAJE"] = "NO EXISTEN EMPLEADOS ALMACENADOS.";
                return View();
            }
            else
            {
                List<Empleado> empleados =
                    await this.repo.GetEmpleadosAsync(idsEmpleados);
                return View(empleados);
            }
        }

        public async Task<IActionResult> SessionEmpleadosV5(int? idEmpleado, int? idfavorito)
        {
            // MANEJO DE FAVORITOS
            if (idfavorito != null)
            {
                List<Empleado> empleadosFavoritos = memoryCache.Get<List<Empleado>>("FAVORITOS") ?? new List<Empleado>();

                // Verificamos si el empleado ya está en favoritos
                Empleado emp = empleadosFavoritos.FirstOrDefault(e => e.IdEmpleado == idfavorito.Value);
                if (emp != null)
                {
                    // Si ya está en favoritos, lo eliminamos
                    empleadosFavoritos.Remove(emp);
                    if (empleadosFavoritos.Count == 0)
                    {
                        memoryCache.Remove("FAVORITOS");
                    }
                    else
                    {
                        memoryCache.Set("FAVORITOS", empleadosFavoritos);
                    }
                }
                else
                {
                    // Si no está en favoritos, lo agregamos
                    emp = await repo.FindEmpleadoAsync(idfavorito.Value);
                    if (emp != null)
                    {
                        empleadosFavoritos.Add(emp);
                        memoryCache.Set("FAVORITOS", empleadosFavoritos);
                    }
                }
            }

            // MANEJO DE EMPLEADOS EN SESIÓN
            if (idEmpleado != null)
            {
                List<int> idsEmpleados = HttpContext.Session.GetObject<List<int>>("IDSEMPLEADOS") ?? new List<int>();

                // Evitar duplicados
                if (!idsEmpleados.Contains(idEmpleado.Value))
                {
                    idsEmpleados.Add(idEmpleado.Value);
                    HttpContext.Session.SetObject("IDSEMPLEADOS", idsEmpleados);
                }
            }

            List<Empleado> empleados = await repo.GetEmpleadosAsync();
            return View(empleados);
        }

        //                                          -- V4 --
        public async Task<IActionResult> EmpleadosAlmacenadosV4()
        {
            //DEBEMOS RECUPERAR LOS IDS EMPLEADOS QUE TENGAMOS EN SESSION
            List<int> idsEmpleados =
                HttpContext.Session.GetObject<List<int>>("IDSEMPLEADOS");
            if (idsEmpleados == null)
            {
                ViewData["MENSAJE"] = "NO EXISTEN EMPLEADOS ALMACENADOS.";
                return View();
            }
            else
            {
                List<Empleado> empleados =
                    await this.repo.GetEmpleadosAsync(idsEmpleados);
                return View(empleados);
            }
        }

            public async Task<IActionResult>
        SessionEmpleadosV4(int? idEmpleado)
            {
                if (idEmpleado != null)
                {
                    //ALMACENAREMOS LO MINIMO QUE PODAMOS (int)
                    List<int> idsEmpleados;
                    if (HttpContext.Session.GetObject<List<int>>("IDSEMPLEADOS") == null)
                    {
                        //NO EXISTE Y CREAMOS LA COLECCION
                        idsEmpleados = new List<int>();
                    }
                    else
                    {
                        //EXISTE Y RECUPERAMOS LA COLECCION
                        idsEmpleados =
                            HttpContext.Session.GetObject<List<int>>("IDSEMPLEADOS");
                    }
                    idsEmpleados.Add(idEmpleado.Value);
                    //REFRESCAMOS LOS DATOS DE SESSION
                    HttpContext.Session.SetObject("IDSEMPLEADOS", idsEmpleados);
                    ViewData["MENSAJE"] = "Empleados almacenados: "
                        + idsEmpleados.Count;
                }
                //COMPROBAMOS SI TENEMOS IDS EN SESSION
                List<int> ids =
                    HttpContext.Session.GetObject<List<int>>("IDSEMPLEADOS");
                if (ids == null)
                {
                    List<Empleado> empleados =
                        await this.repo.GetEmpleadosAsync();
                    return View(empleados);
                }
                else
                {
                    List<Empleado> empleados =
                        await this.repo.GetEmpleadosNotSessionAsync(ids);
                    return View(empleados);
                }
            }
        //                                          -- V3 --
        public async Task<IActionResult> EmpleadosAlmacenadosOK()
        {
            //DEBEMOS RECUPERAR LOS IDS EMPLEADOS QUE TENGAMOS EN SESSION
            List<int> idsEmpleados =
                HttpContext.Session.GetObject<List<int>>("IDSEMPLEADOS");
            if (idsEmpleados == null)
            {
                ViewData["MENSAJE"] = "NO EXISTEN EMPLEADOS ALMACENADOS.";
                return View();
            }
            else
            {
                List<Empleado> empleados =
                    await this.repo.GetEmpleadosAsync(idsEmpleados);
                return View(empleados);
            }
        }

        public async Task<IActionResult> 
            SessionEmpleadosOK(int? idEmpleado)
        {
            if(idEmpleado != null)
            {
                //ALMACENAREMOS LO MINIMO QUE PODAMOS (int)
                List<int> idsEmpleados;
                if (HttpContext.Session.GetObject<List<int>>("IDSEMPLEADOS") == null)
                {
                    //NO EXISTE Y CREAMOS LA COLECCION
                    idsEmpleados = new List<int>();
                }
                else
                {
                    //EXISTE Y RECUPERAMOS LA COLECCION
                    idsEmpleados =
                        HttpContext.Session.GetObject<List<int>>("IDSEMPLEADOS");
                }
                idsEmpleados.Add(idEmpleado.Value);
                //REFRESCAMOS LOS DATOS DE SESSION
                HttpContext.Session.SetObject("IDSEMPLEADOS", idsEmpleados);
                ViewData["MENSAJE"] = "Empleados almacenados: "
                    + idsEmpleados.Count;
            }
            List<Empleado> empleados =
                await this.repo.GetEmpleadosAsync();
            return View(empleados);
        }
        //                                          -- V2 --
        public async Task<IActionResult> SessionEmpleados(int? idEmpleado)
        {
            if (idEmpleado != null)
            {
                //BUSCAMOS AL EMPLEADO
                Empleado empleado =
                    await this.repo.FindEmpleadoAsync(idEmpleado.Value);
                //EN SESSION TENDREMOS UN CONJUNTO DE EMPLEADOS
                List<Empleado> empleadosList;
                //DEBEMOS PREGUNTAR SI TENEMOS EL CONJUNTO DE EMPLEADOS
                //ALMACENADOS EN SESSION
                if (HttpContext.Session.GetObject<List<Empleado>>("EMPLEADOS") != null)
                {
                    //RECUPERAMOS LOS EMPLEADOS QUE TENGAMOS EN SESSION
                    empleadosList =
                        HttpContext.Session.GetObject<List<Empleado>>("EMPLEADOS");
                }
                else
                {
                    //SI NO EXISTE, INSTANCIAMOS LA COLECCION
                    empleadosList = new List<Empleado>();
                }
                //ALMACENAMOS EL EMPLEADO DENTRO DE NUESTRA COLECCION
                empleadosList.Add(empleado);
                //GUARDAMOS EN SESSION LA COLECCION CON EL NUEVO EMPLEADO
                HttpContext.Session.SetObject("EMPLEADOS", empleadosList);
                ViewData["MENSAJE"] = "Empleado " + empleado.Apellido
                    + " almacenado correctamente.";
            }
            List<Empleado> empleados =
                await this.repo.GetEmpleadosAsync();
            return View(empleados);
        }

        public IActionResult EmpleadosAlmacenados()
        {
            return View();
        }
        //                                             -- V1 --
        public async Task<IActionResult> SessionSalarios(int? salario)
        {
            if (salario != null)
            {
                //NECESITAMOS ALMACENAR EL SALARIO DEL EMPLEADO 
                //Y LA SUMA TOTAL DE SALARIOS QUE TENGAMOS
                int sumaSalarial = 0;
                //PREGUNTAMOS SI YA TENEMOS LA SUMA ALMACENADA EN SESSION
                if (HttpContext.Session.GetString("SUMASALARIAL") != null)
                {
                    //SI YA EXISTE LA SUMA SALARIAL, RECUPERAMOS 
                    //SU VALOR
                    sumaSalarial =
                        HttpContext.Session.GetObject<int>("SUMASALARIAL");
                }
                //REALIZAMOS LA SUMA
                sumaSalarial += salario.Value;
                //ALMACENAMOS EL NUEVO VALOR DE LA SUMA SALARIAL
                //DENTRO DE SESSION
                HttpContext.Session.SetObject("SUMASALARIAL", sumaSalarial);
                ViewData["MENSAJE"] = "Salario almacenado: " + salario.Value;
            }
            List<Empleado> empleados =
                await this.repo.GetEmpleadosAsync();
            return View(empleados);
        }

        public IActionResult SumaSalarial()
        {
            return View();
        }
    }
}
