using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Service.Dtos;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Controllers
{
    [ApiController]
    [Route("items")]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository<InventoryItem> itemsRepository;
        public ItemsController(IRepository<InventoryItem> itemsRepository)
        {
            this.itemsRepository = itemsRepository;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable>> GetAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest();
            }

            var items = (await itemsRepository.GetAllAsync(item => item.UserId == userId))
                        .Select(item => item.AsDto());

            return Ok(items);
        }
        [HttpPost]
        public async Task<ActionResult> PostAsync(GrantItemsDtos grantItemsDtos)
        {

            var inventoryItem = await itemsRepository.GetAsync(
                item => item.UserId == grantItemsDtos.UserId && item.CatalogItemId == grantItemsDtos.CatalogItemId
                );

            if (inventoryItem == null)
            {
                inventoryItem = new InventoryItem
                {
                    CatalogItemId = grantItemsDtos.CatalogItemId,
                    UserId = grantItemsDtos.UserId,
                    Quantity = grantItemsDtos.Quantity,
                    AcquiredDate = DateTimeOffset.UtcNow
                };

                await itemsRepository.CreateAsync(inventoryItem);
            }
            else
            {
                inventoryItem.Quantity += grantItemsDtos.Quantity;
                await itemsRepository.UpdateAsync(inventoryItem);
            }
            return Ok();
        }
    }
}