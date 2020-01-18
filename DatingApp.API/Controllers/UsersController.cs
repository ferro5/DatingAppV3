using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DatingApp.API.Controllers
{
   // [Authorize]
   [Route("api/[controller]")]
   [ApiController]
   public class UsersController : ControllerBase
   {
       private readonly IDatingRepository _repository;
       private readonly IMapper _mapper;

       public UsersController(IDatingRepository repository, IMapper mapper)
       {
           _repository = repository;
           _mapper = mapper;
       }

       [HttpGet]
       public async Task<IActionResult> GetUsers()
       {
           var users = await _repository.GetUsers();
           var userToMap = _mapper.Map<IEnumerable<ListForUserDto>>(users);
           return Ok(userToMap);
       }

       [HttpGet("{id}", Name = "GetUser")]
       public async Task<IActionResult> GetUser(int id)
       {
           var user = await _repository.GetUser(id);
           var userToMap = _mapper.Map<UserForDetailDto>(user);
           return Ok(userToMap);
       }

       [HttpPut("{id}")]
       public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdateDto)
       {
           if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))

               return Unauthorized();

           var userRepo = await _repository.GetUser(id);
           _mapper.Map(userForUpdateDto, userRepo);
           if (await _repository.SaveAll())
               return NoContent();
           throw new Exception($"user with id: {id} was not saved");

       }
   }
}
