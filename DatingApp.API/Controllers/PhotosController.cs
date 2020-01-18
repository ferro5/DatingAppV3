using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.DTO;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController : Controller
    {
        private readonly IDatingRepository _repository;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudConfig;
        private readonly Cloudinary _cloudinary;
        private ILogger<PhotosController> _logger;

        #region Construct
        public PhotosController(IDatingRepository repository, IMapper mapper, IOptions<CloudinarySettings> cloudConfig,
            ILogger<PhotosController> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _cloudConfig = cloudConfig;
            _logger = logger;
            Account acc = new Account(
                _cloudConfig.Value.CloudName,
                _cloudConfig.Value.ApiKey,
                _cloudConfig.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(acc);
        }
        #endregion
        #region GetPhoto
        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photoRepo = await _repository.GetPhoto(id);
            var photo = _mapper.Map<PhotoForReturnDto>(photoRepo);
            return Ok(photo);
        }
        #endregion
        #region AddPhotoForUser
        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, [FromForm] PhotoForCreationDto photoForCreationDto)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))

                return Unauthorized();

            var userRepo = await _repository.GetUser(userId);
            var file = photoForCreationDto.File;
            var uploadResult = new ImageUploadResult();
            if (file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                    };
                    uploadResult = _cloudinary.Upload(uploadParams);
                }
            }

            photoForCreationDto.Url = uploadResult.Uri.ToString();
            photoForCreationDto.PublicId = uploadResult.PublicId;
            var photo = _mapper.Map<Photo>(photoForCreationDto);
            if (!userRepo.Photos.Any(u => u.IsMain))
                photo.IsMain = true;
            userRepo.Photos.Add(photo);
            if (await _repository.SaveAll())
            {
                var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo);
                return CreatedAtRoute("GetPhoto", new { userId = userId, id = photo.Id }, photoToReturn);
            }

            return BadRequest("Could not Add the file");
        }

        #endregion
        #region SetMain
        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMain(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var user = await _repository.GetUser(userId);

            if (!user.Photos.Any(p => p.Id == id))
                return Unauthorized();

            var photoFromRepo = await _repository.GetPhoto(id);

            if (photoFromRepo.IsMain)
                return BadRequest("This is already the main photo");

            var currentMainPhoto = await _repository.GetPhotoForUser(userId);
            currentMainPhoto.IsMain = false;

            photoFromRepo.IsMain = true;

            if (await _repository.SaveAll())
                return NoContent();

            return BadRequest("Could not set photo to main");
        }
        #endregion
        #region DeletePhoto

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            var user = await _repository.GetUser(userId);
            if (!user.Photos.Any(p => p.Id == id))
                return Unauthorized();
            var photoFromRepo = await _repository.GetPhoto(id);
            if (photoFromRepo.IsMain)
                return BadRequest("You can not delete your main photo");
            if (photoFromRepo.PublicId !=null)
            {
               var deleteParams = new DeletionParams(photoFromRepo.PublicId);
               var result = _cloudinary.Destroy(deleteParams);
               if (result.Result=="ok")
               {
                   _repository.Delete(photoFromRepo);
               }
            }
            if (photoFromRepo.PublicId==null)
            {
                _repository.Delete(photoFromRepo);
            }
            if (await _repository.SaveAll())
                return Ok();

            return BadRequest("Failed to delete  photo");
        }
        #endregion

    }
}