using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WalkItAPI.Models;

namespace WalkItAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DogController : ControllerBase
    {
        private readonly IConfiguration _config;

        public DogController(IConfiguration config)
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

        //Get all dogs from the database
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT d.Id, d.DName, d.OwnerId, d.Breed, d.Notes, o.Id, o.OName, o.Address, o.NeighborhoodId, o.Phone
                        FROM Dog d
                        LEFT JOIN Owner o
                        ON d.OwnerId = o.Id";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Dog> dogs = new List<Dog>();

                    while (reader.Read())
                    {
                        Dog dog = new Dog
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("DName")),
                            OwnerId = reader.GetInt32(reader.GetOrdinal("OwnerId")),
                            Breed = reader.GetString(reader.GetOrdinal("Breed")),
                            Notes = reader.GetString(reader.GetOrdinal("Notes")),
                            Owner = new Owner
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("OwnerId")),
                                Name = reader.GetString(reader.GetOrdinal("OName")),
                                Address = reader.GetString(reader.GetOrdinal("Address")),
                                NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                Phone = reader.GetString(reader.GetOrdinal("Phone"))
                            }
                        };

                        dogs.Add(dog);
                    }
                    reader.Close();

                    return Ok(dogs);
                }
            }
        }


        //Get a single dog by Id
        [HttpGet("{id}", Name = "GetDog")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            d.Id, d.DName, d.OwnerId, d.Breed, d.Notes, o.Id, o.OName, o.Address, o.NeighborhoodId, o.Phone
                        FROM Dog d
                        LEFT JOIN Owner o
                        ON d.OwnerId = o.Id
                        WHERE d.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Dog dog = null;

                    if (reader.Read())
                    {
                        dog = new Dog
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("DName")),
                            OwnerId = reader.GetInt32(reader.GetOrdinal("OwnerId")),
                            Breed = reader.GetString(reader.GetOrdinal("Breed")),
                            Notes = reader.GetString(reader.GetOrdinal("Notes")),
                            Owner = new Owner
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("OwnerId")),
                                Name = reader.GetString(reader.GetOrdinal("OName")),
                                Address = reader.GetString(reader.GetOrdinal("Address")),
                                NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                Phone = reader.GetString(reader.GetOrdinal("Phone"))
                            }
                        };
                    }
                    else
                    {
                        return NotFound();
                    }
                    reader.Close();

                    return Ok(dog);
                }
            }
        }


        //Create a new dog
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Dog dog)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Dog (DName, OwnerId, Breed, Notes)
                                        OUTPUT INSERTED.Id
                                        VALUES (@DName, @OwnerId, @Breed, @Notes)";
                    cmd.Parameters.Add(new SqlParameter("@DName", dog.Name));
                    cmd.Parameters.Add(new SqlParameter("@OwnerId", dog.OwnerId));
                    cmd.Parameters.Add(new SqlParameter("@Breed", dog.Breed));
                    cmd.Parameters.Add(new SqlParameter("@Notes", dog.Notes));

                    int newId = (int)cmd.ExecuteScalar();
                    dog.Id = newId;
                    return CreatedAtRoute("GetDog", new { id = newId }, dog);
                }
            }
        }

        //Update a dog
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Dog dog)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Dog
                                            SET DName = @DName,
                                                OwnerId = @OwnerId,
                                                Breed = @Breed,
                                                Notes = @Notes
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@DName", dog.Name));
                        cmd.Parameters.Add(new SqlParameter("@OwnerId", dog.OwnerId));
                        cmd.Parameters.Add(new SqlParameter("@Breed", dog.Breed));
                        cmd.Parameters.Add(new SqlParameter("@Notes", dog.Notes));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!DogExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Dog WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!DogExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool DogExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, DName, OwnerId, Breed, Notes
                        FROM Dog
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }

    }
}
