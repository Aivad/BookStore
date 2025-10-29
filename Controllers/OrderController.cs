//using BookStore.Data;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using System.Security.Claims;

//[Authorize]
//public class OrderController : Controller
//{
//    private readonly AppDbContext _context;

//    public OrderController(AppDbContext context)
//    {
//        _context = context;
//    }

//    // GET: /Order/Checkout
//    public async Task<IActionResult> Checkout()
//    {
//        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//        var cartItems = await _context.CartItems
//            .Include(c => c.Book)
//            .Where(c => c.UserId == userId)
//            .ToListAsync();

//        if (!cartItems.Any())
//            return RedirectToAction("Index", "Cart");

//        var paymentMethods = await _context.PaymentMethods.ToListAsync();
//        ViewBag.PaymentMethods = paymentMethods;

//        return View(cartItems);
//    }

//    // POST: /Order/PlaceOrder
//    [HttpPost]
//    [ValidateAntiForgeryToken]
//    public async Task<IActionResult> PlaceOrder(int paymentMethodId)
//    {
//        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//        var cartItems = await _context.CartItems
//            .Include(c => c.Book)
//            .Where(c => c.UserId == userId)
//            .ToListAsync();

//        if (!cartItems.Any())
//            return RedirectToAction("Index", "Cart");

//        // Create order
//        var order = new Order
//        {
//            UserId = userId,
//            PaymentMethodId = paymentMethodId,
//            TotalAmount = cartItems.Sum(i => i.Book.Price * i.Quantity),
//            Status = "Pending"
//        };

//        _context.Orders.Add(order);
//        await _context.SaveChangesAsync();

//        // Create order items
//        foreach (var item in cartItems)
//        {
//            _context.OrderItems.Add(new OrderItem
//            {
//                OrderId = order.Id,
//                BookId = item.BookId,
//                Quantity = item.Quantity,
//                PriceAtPurchase = item.Book.Price
//            });

//            // Reduce stock
//            item.Book.Stock -= item.Quantity;
//            _context.Update(item.Book);
//        }

//        // Clear cart
//        _context.CartItems.RemoveRange(cartItems);
//        await _context.SaveChangesAsync();

//        TempData["Success"] = "Order placed successfully!";
//        return RedirectToAction("OrderDetails", new { id = order.Id });
//    }

//    // GET: /Order/OrderDetails/5
//    public async Task<IActionResult> OrderDetails(int id)
//    {
//        var order = await _context.Orders
//            .Include(o => o.PaymentMethod)
//            .Include(o => o.OrderItems)
//                .ThenInclude(oi => oi.Book)
//            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier));

//        if (order == null) return NotFound();
//        return View(order);
//    }
//}