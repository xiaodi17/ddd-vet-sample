﻿using Ardalis.ApiEndpoints;
using AutoMapper;
using BlazorShared.Models.Room;
using FrontDesk.Core.Aggregates;
using FrontDesk.SharedKernel.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace FrontDesk.Api.RoomEndpoints
{
    public class GetById : BaseAsyncEndpoint<GetByIdRoomRequest, GetByIdRoomResponse>
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;

        public GetById(IRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet("api/rooms/{RoomId}")]
        [SwaggerOperation(
            Summary = "Get a Room by Id",
            Description = "Gets a Room by Id",
            OperationId = "rooms.GetById",
            Tags = new[] { "RoomEndpoints" })
        ]
        public override async Task<ActionResult<GetByIdRoomResponse>> HandleAsync([FromRoute] GetByIdRoomRequest request, CancellationToken cancellationToken)
        {
            var response = new GetByIdRoomResponse(request.CorrelationId());

            var room = await _repository.GetByIdAsync<Room, int>(request.RoomId);
            if (room is null) return NotFound();

            response.Room = _mapper.Map<RoomDto>(room);

            return Ok(response);
        }
    }
    

}
