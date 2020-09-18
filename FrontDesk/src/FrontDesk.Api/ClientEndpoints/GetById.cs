﻿using Ardalis.ApiEndpoints;
using AutoMapper;
using BlazorShared.Models.Client;
using FrontDesk.Core.Aggregates;
using FrontDesk.SharedKernel.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace FrontDesk.Api.ClientEndpoints
{
    public class GetById : BaseAsyncEndpoint<GetByIdClientRequest, GetByIdClientResponse>
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;

        public GetById(IRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet("api/clients/{ClientId}")]
        [SwaggerOperation(
            Summary = "Get a Client by Id",
            Description = "Gets a Client by Id",
            OperationId = "clients.GetById",
            Tags = new[] { "ClientEndpoints" })
        ]
        public override async Task<ActionResult<GetByIdClientResponse>> HandleAsync([FromRoute] GetByIdClientRequest request, CancellationToken cancellationToken)
        {
            var response = new GetByIdClientResponse(request.CorrelationId());

            var client = await _repository.GetByIdAsync<Client, int>(request.ClientId);
            if (client is null) return NotFound();

            response.Client = _mapper.Map<ClientDto>(client);

            return Ok(response);
        }
    }
    

}
