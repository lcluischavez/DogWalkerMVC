using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using DogWalkerMVC.Models;
using DogWalkerMVC.Models.ViewModels;

namespace DogWalkerMVC.Controllers
{
    public class OwnersController : Controller

    {
        private readonly IConfiguration _config;

        public OwnersController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }
        // GET: Owners
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT i.Id, i.Name, i.Address, i.Phone, i.NeighborhoodId, c.Name
                                      FROM Owner i LEFT JOIN Neighborhood c 
                                      ON i.OwnerId = c.Id";

                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Owner>owners = new List<Owner>();
                    while (reader.Read())
                    {
                        Owner owner = new Owner
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Address = reader.GetString(reader.GetOrdinal("Address")),
                            Phone = reader.GetString(reader.GetOrdinal("Phone")),
                            NeighborhoodId = reader.GetInt32(reader.GetOrdinal("OwnerId")),
                            Neighborhood = new Neighborhood()
                            {
                                Name = reader.GetString(reader.GetOrdinal("Name"))
                            }

                        };

                        owners.Add(owner);
                    }

                    reader.Close();

                    return View(owners);
                }
            }

        }


        // GET: Owner/Details/1
        public ActionResult Details(int id)
        {
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT Id, Name, Adresss, NeighborhoodrId, Phone FROM Owner WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        var reader = cmd.ExecuteReader();
                        Owner owner = null;

                        if (reader.Read())
                        {
                            owner = new Owner()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Address = reader.GetString(reader.GetOrdinal("Address")),
                                Phone = reader.GetString(reader.GetOrdinal("Phone")),
                                NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId"))
                            };

                        }
                        reader.Close();
                        return View(owner);
                    }
                }
            }
        }

        // GET: Owners/Create
        public ActionResult Create()
        {
            var neighborhoodOptions = GetNeighborhoodOptions();
            var viewModel = new OwnerEditViewmodel()
            {
                NeighborhoodOptions = neighborhoodOptions

            };
            return View(viewModel);
        }

        // POST: Owners/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(OwnerEditViewmodel owner)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO Owner (Name, Address, Phone, NeighborhoodId)
                                            OUTPUT INSERTED.Id
                                            VALUES (@name, @address, @phone, @neighborhoodId)";

                        cmd.Parameters.Add(new SqlParameter("@name", owner.Name));
                        cmd.Parameters.Add(new SqlParameter("@address", owner.Address));
                        cmd.Parameters.Add(new SqlParameter("@phone", owner.Phone));
                        cmd.Parameters.Add(new SqlParameter("@neighborhoodId", owner.NeighborhoodId));

                        var id = (int)cmd.ExecuteScalar();
                        owner.Id = id;
                        // TODO: Add insert logic here

                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch
            {
                return View();
            }
        }

        // GET: Owners/Edit/1
        public ActionResult Edit(int id)
        {
            var owner = GetOwnerById(id);
            var neighborhoodOptions = GetNeighborhoodOptions();
            var viewModel = new OwnerEditViewmodel()
            {
                Id = owner.Id,
                Name = owner.Name,
                Address = owner.Address,
                NeighborhoodId = owner.NeighborhoodId,
                Phone = owner.Phone,
                NeighborhoodOptions = neighborhoodOptions


            };
            return View(viewModel);
        }

        // POST: Owners/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, OwnerEditViewmodel owner)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Owner
                                            SET Name = @name, 
                                                Address = @address, 
                                                Phone = @phone, 
                                                NeighborhoodId = @neighborhoodId
                                            WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@name", owner.Name));
                        cmd.Parameters.Add(new SqlParameter("@address", owner.Address));
                        cmd.Parameters.Add(new SqlParameter("@phone", owner.Phone));
                        cmd.Parameters.Add(new SqlParameter("@neighborhoodId", owner.NeighborhoodId));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        var rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected < 1)
                        {
                            return NotFound();
                        }
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Owners/Delete/1
        public ActionResult Delete(int id)
        {
            var owner = GetOwnerById(id);
            return View(owner);
        }


        // POST: Owners/Delete/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, Owner owner)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "DELETE FROM Owner WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        cmd.ExecuteNonQuery();
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return View();
            }
        }
        private List<SelectListItem> GetNeighborhoodOptions()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name FROM Neighborhood";



                    var reader = cmd.ExecuteReader();
                    var options = new List<SelectListItem>();

                    while (reader.Read())
                    {
                        var option = new SelectListItem()
                        {
                            Text = reader.GetString(reader.GetOrdinal("Name")),
                            Value = reader.GetInt32(reader.GetOrdinal("Id")).ToString()
                        };
                        options.Add(option);

                    }
                    reader.Close();
                    return options;
                }
            }
        }

        private Owner GetOwnerById(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name, Address, NeighborhoodId, Phone FROM Owner WHERE Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    var reader = cmd.ExecuteReader();
                    Owner owner = null;

                    if (reader.Read())
                    {
                        owner = new Owner()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Address = reader.GetString(reader.GetOrdinal("Address")),
                            Phone = reader.GetString(reader.GetOrdinal("Phone")),
                            NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId"))
                        };

                    }
                    reader.Close();
                    return owner;
                }
            }
        }
    }
}