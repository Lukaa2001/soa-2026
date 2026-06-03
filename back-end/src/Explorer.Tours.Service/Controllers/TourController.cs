using Explorer.BuildingBlocks.Core.UseCases;
using Explorer.Tours.API.Dtos;
using Explorer.Tours.API.Public.Author;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Explorer.API.Controllers.Author
{
    [Route("api/tour")]
    public class TourController : BaseApiController
    {
        private readonly ITourService _tourService;
        private readonly ICheckpointService _checkpointService;

        public TourController(ITourService tourService, ICheckpointService checkpointService)
        {
            _tourService = tourService;
            _checkpointService = checkpointService;
        }

        [HttpGet]
        public ActionResult<PagedResult<TourDto>> GetAll([FromQuery] int page, [FromQuery] int pageSize)
        {
            var result = _tourService.GetPaged(page, pageSize);
            return CreateResponse(result);
        }

        // Functionality 10: an author can view all of their tours.
        [HttpGet("authortours")]
        [AllowAnonymous]
        public ActionResult<PagedResult<TourDto>> GetAllAuthorTours([FromQuery] int authorId, [FromQuery] int page, [FromQuery] int pageSize)
        {
            var result = _tourService.GetByAuthorId(authorId, page, pageSize);
            return CreateResponse(result);
        }

        [HttpGet("singletour")]
        public ActionResult<TourDto> GetById([FromQuery] int tourId)
        {
            var result = _tourService.Get(tourId);
            return CreateResponse(result);
        }

        [HttpGet("singletour/{tourId:int}")]
        public ActionResult<TourDto> GetTour(int tourId)
        {
            var result = _tourService.Get(tourId);
            return CreateResponse(result);
        }

        [HttpPut("updatetour")]
        public ActionResult<TourDto> Update([FromBody] TourDto tourDto)
        {
            var result = _tourService.Update(tourDto);
            return CreateResponse(result);
        }

        // Functionality 10: create a tour (starts as draft, price 0).
        [HttpPost]
        public ActionResult<TourDto> Create([FromBody] TourDto tour)
        {
            var result = _tourService.Create(tour);
            return CreateResponse(result);
        }

        // Functionality 11: add a key point (lat/long, name, description, image).
        [HttpPost("addCheckpoint")]
        public ActionResult<CheckpointDto> AddCheckpointOnTour([FromBody] CheckpointDto checkpointDto)
        {
            var result = _checkpointService.Create(checkpointDto);
            return CreateResponse(result);
        }

        [HttpGet("getCheckpoints/{tourId:int}")]
        public ActionResult<PagedResult<CheckpointDto>> GetAllByTourId([FromQuery] int page, [FromQuery] int pageSize, int tourId)
        {
            var result = _checkpointService.GetAllByTourId(page, pageSize, tourId);
            return CreateResponse(result);
        }

        // Functionality 17 support: tourists can view public key points (on the map).
        [HttpGet("tourist/checkpoints")]
        public ActionResult<PagedResult<CheckpointDto>> GetTouristCheckpoints([FromQuery] int page, [FromQuery] int pageSize)
        {
            var result = _checkpointService.GetAllPublicCheckpoints(page, pageSize);
            return CreateResponse(result);
        }

        [HttpPut("updateCheckpoint")]
        public ActionResult<CheckpointDto> UpdateCheckpoint([FromBody] CheckpointDto checkpointDto)
        {
            var result = _checkpointService.Update(checkpointDto);
            return CreateResponse(result);
        }

        [HttpDelete("deleteCheckpoint/{checkpointId:int}")]
        public ActionResult<CheckpointDto> DeleteCheckpoint(int checkpointId)
        {
            var result = _checkpointService.Delete(checkpointId);
            return CreateResponse(result);
        }

        // Functionality 15: publish a tour (requires valid checkpoints).
        [HttpPut("publishTour")]
        public ActionResult<TourDto> PublishTour([FromBody] int tourId)
        {
            if (_checkpointService.CheckPointsAreValidForPublish(0, 0, tourId))
            {
                _tourService.PublishTour(tourId, DateTime.Now.ToUniversalTime());
                var tour = _tourService.Get(tourId);
                return CreateResponse(tour);
            }

            throw new Exception("Tour doesn't have enough checkpoints to be published ");
        }

        [HttpPut("archiveTour")]
        public ActionResult<TourDto> ArchiveTour([FromBody] int tourId)
        {
            _tourService.ArchiveTour(tourId, DateTime.Now.ToUniversalTime());
            var tour = _tourService.Get(tourId);
            return CreateResponse(tour);
        }

        // Functionality 16/17 support: tourist-facing tour listings.
        [HttpGet("shopping/{userId:int}")]
        public ActionResult<PagedResult<TourPreviewDto>> GetAllAvailableTours([FromQuery] int page, [FromQuery] int pageSize, int userId)
        {
            var result = _tourService.GetAllAvailableTours(page, pageSize, userId);
            return CreateResponse(result);
        }

        [HttpGet("touristTours/{userId:int}")]
        public ActionResult<PagedResult<TourPreviewDto>> GetTouristTours([FromQuery] int page, [FromQuery] int pageSize, int userId)
        {
            var result = _tourService.GetTouristTours(page, pageSize, userId);
            return CreateResponse(result);
        }

        [HttpGet("publishedauthortours")]
        public ActionResult<PagedResult<TourDto>> GetPublishedAuthorTours([FromQuery] int authorId, [FromQuery] int page, [FromQuery] int pageSize)
        {
            var result = _tourService.GetPublishedAuthorTours(authorId, page, pageSize);
            return CreateResponse(result);
        }
    }
}
