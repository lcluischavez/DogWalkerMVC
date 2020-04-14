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
    public class DogsController : Controller

    {
        private readonly IConfiguration _config;

        public DogsController(IConfiguration config)
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
        // GET: Dogs
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT i.Id, i.Name, i.Breed, i.Notes, i.ImageUrl, i.OwnerId, c.Name
                                      FROM Dog i LEFT JOIN Owner c 
                                      ON i.OwnerId = c.Id";

                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Dog> dogs = new List<Dog>();
                    while (reader.Read())
                    {
                        Dog dog = new Dog
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Breed = reader.GetString(reader.GetOrdinal("Breed")),
                            Notes = reader.GetString(reader.GetOrdinal("Notes")),
                            ImageUrl = reader.GetString(reader.GetOrdinal("ImageUrl")),
                            OwnerId = reader.GetInt32(reader.GetOrdinal("OwnerId")),
                            Owner = new Owner()
                            {
                                Name = reader.GetString(reader.GetOrdinal("Name"))
                            }

                        };

                        dogs.Add(dog);
                    }

                    reader.Close();

                    return View(dogs);
                }
            }

        }


        // GET: Dogs/Details/1
        public ActionResult Details(int id)
        {
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT Id, Name, Breed, OwmerId, Notes, ImageUrl FROM Dog WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        var reader = cmd.ExecuteReader();
                        Dog dog = null;

                        if (reader.Read())
                        {
                            dog = new Dog()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Breed = reader.GetString(reader.GetOrdinal("Breed")),
                                Notes = reader.GetString(reader.GetOrdinal("Notes")),
                                ImageUrl = reader.GetString(reader.GetOrdinal("ImageUrl")),
                                OwnerId = reader.GetInt32(reader.GetOrdinal("OwnerId"))
                            };

                        }
                        reader.Close();
                        return View(dog);
                    }
                }
            }
        }

        // GET: Dogs/Create
        public ActionResult Create()
        {
            var ownerOptions = GetOwnerOptions();
            var viewModel = new DogEditViewmodel()
            {
                OwnerOptions = ownerOptions

            };
            return View(viewModel);
        }

        // POST: Dogs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DogEditViewmodel dog)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO Dog (Name, Breed, Notes, ImageUrl, OwnerId)
                                            OUTPUT INSERTED.Id
                                            VALUES (@name, @breed, @notes, @imageUrl, @ownertId)";

                        cmd.Parameters.Add(new SqlParameter("@name", dog.Name));
                        cmd.Parameters.Add(new SqlParameter("@breed", dog.Breed));
                        cmd.Parameters.Add(new SqlParameter("@notes", dog.Notes));
                        cmd.Parameters.Add(new SqlParameter("@imageUrl", dog.ImageUrl));
                        cmd.Parameters.Add(new SqlParameter("@ownerId", dog.OwnerId));

                        var id = (int)cmd.ExecuteScalar();
                        dog.Id = id;
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

        // GET: Dogs/Edit/1
        public ActionResult Edit(int id)
        {
            var dog = GetDogById(id);
            var ownerOptions = GetOwnerOptions();
            var viewModel = new DogEditViewmodel()
            {
                Id = dog.Id,
                Name = dog.Name,
                Breed = dog.Breed,
                OwnerId = dog.OwnerId,
                Notes = dog.Notes,
                ImageUrl = dog.ImageUrl,
                OwnerOptions = ownerOptions


            };
            return View(viewModel);
        }

        // POST: Dogs/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, DogEditViewmodel dog)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Dog 
                                            SET Name = @name, 
                                                Breed = @breed, 
                                                Notes = @notes, 
                                                ImageUrl = @imageUrl, 
                                                OwnerId = @ownerId
                                            WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@name", dog.Name));
                        cmd.Parameters.Add(new SqlParameter("@breed", dog.Breed));
                        cmd.Parameters.Add(new SqlParameter("@notes", dog.Notes));
                        cmd.Parameters.Add(new SqlParameter("@imageUrl", dog.ImageUrl));
                        cmd.Parameters.Add(new SqlParameter("@ownerId", dog.OwnerId));
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

        // GET: Dogs/Delete/1
        public ActionResult Delete(int id)
        {
            var dog = GetDogById(id);
            return View(dog);
        }


        // POST: Dogs/Delete/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, Dog dog)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "DELETE FROM Dog WHERE Id = @id";
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
        private List<SelectListItem> GetOwnerOptions()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name FROM Owner";



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

        private Dog GetDogById(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name, Breed, OwnerId, Breed, ImageUrl FROM Dog WHERE Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    var reader = cmd.ExecuteReader();
                    Dog dog = null;

                    if (reader.Read())
                    {
                        dog = new Dog()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Breed = reader.GetString(reader.GetOrdinal("Breed")),
                            Notes = reader.GetString(reader.GetOrdinal("Notes")),
                            ImageUrl = reader.GetString(reader.GetOrdinal("ImageUrl")),
                            OwnerId = reader.GetInt32(reader.GetOrdinal("OwnerId"))
                        };

                    }
                    reader.Close();
                    return dog;
                }
            }
        }
    }
}