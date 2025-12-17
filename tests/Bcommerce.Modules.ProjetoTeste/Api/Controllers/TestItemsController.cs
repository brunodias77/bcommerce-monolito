
using Bcommerce.BuildingBlocks.Application.Models;
using Bcommerce.BuildingBlocks.Web.Controllers;
using Bcommerce.BuildingBlocks.Web.Models;
using Bcommerce.Modules.ProjetoTeste.Application.Commands.CreateTestItem;
using Bcommerce.Modules.ProjetoTeste.Application.Queries.GetTestItem;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bcommerce.Modules.ProjetoTeste.Api.Controllers;

public class TestItemsController : ApiControllerBase
{
    public TestItemsController(ISender sender) : base(sender)
    {
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTestItemRequest request)
    {
        var command = new CreateTestItemCommand(request.Name, request.Description, request.Value);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, ApiResponse<Guid>.Ok(result.Value));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<TestItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    // [Authorize] // Uncomment to test auth
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetTestItemQuery(id);
        var result = await _sender.Send(query);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return OkResponse(result.Value);
    }
}

public record CreateTestItemRequest(string Name, string Description, decimal Value);
