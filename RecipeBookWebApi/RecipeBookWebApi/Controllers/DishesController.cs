using Microsoft.AspNetCore.Mvc;
using RecipeBookWebApi.Dto;
using RecipeBookWebApi.Models;
using RecipeBookWebApi.Services;

namespace RecipeBookWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DishesController : ControllerBase
{
    private readonly IDishService _dishService;

    public DishesController(IDishService dishService)
    {
        _dishService = dishService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DishResponseDto>>> GetDishes([FromQuery] DishFilter filter)
    {
        var dishes = await _dishService.GetDishesAsync(filter);
        return Ok(dishes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DishResponseDto>> GetDish(int id)
    {
        var dish = await _dishService.GetDishByIdAsync(id);
        if (dish == null) return NotFound();
        return Ok(dish);
    }

    [HttpPost]
    public async Task<ActionResult<DishResponseDto>> CreateDish(DishRequestDto data)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var created = await _dishService.CreateDishAsync(data);
            return CreatedAtAction(nameof(GetDish), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDish(int id, DishRequestDto data)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            await _dishService.UpdateDishAsync(id, data);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDish(int id)
    {
        try
        {
            await _dishService.DeleteDishAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Рассчитывает КБЖУ на основе полученного списка ингредиентов
    /// Используется для подсчета черновых значений на фронтенде
    /// </summary>
    [HttpPost("calculate-nutrition")]
    public async Task<ActionResult<NutritionResponseDto>> CalculateNutrition(NutritionCalculateRequestDto data)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var nutrition = await _dishService.CalculateNutritionAsync(data);
            return Ok(nutrition);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}