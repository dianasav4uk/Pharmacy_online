﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Core;
using Pharmacy.Repos.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Pharmacy.Repos
{
    public class OrderRepository
    {
        public readonly PharmacyDbContext _ctx;
        private readonly UserManager<User> _userManager;
        private readonly MedicamentsRepository _medicamentsRepository;
        public OrderRepository(PharmacyDbContext ctx, UserManager<User> user, MedicamentsRepository medicamentsRepository)
        {
            _ctx = ctx;
            _userManager = user;
            _medicamentsRepository=medicamentsRepository;
        }

        

        public async Task<Order> GetOrder(string id)
        {
            return await _ctx.Order.Include(x=>x.details).ThenInclude(x=>x.Address).FirstOrDefaultAsync(x => x.OrderId == id);
        }
        public async Task<OrderDetails> GetOrderDetails(int id)
        {
            return await _ctx.OrderDetails.Include(x => x.Address).FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<List<OrderItems>> GetOrderItems(int id)
        {
            return await _ctx.OrderItems.Include(x=>x.medicaments).Where(x => x.OrderDetailsId == id).ToListAsync();
        }

        public async Task<List<Order>> GetAllOrder()
        {
            return await _ctx.Order.ToListAsync();
        }
        public async Task<List<Order>> GetSearchAllOrder(string search)
        {
            return await _ctx.Order.Where(x=>x.OrderId.Contains(search)).ToListAsync();
        }

        public async Task<Order> CreateOrder(List<ShopCartItem> cartItems,User user,OrderDetails orderDetails)
        {
            var newOrder = new Order
            {
                Total = cartItems.Sum(x => x.Quantity * x.Price),
                OrderDate = DateTime.Now,
                user = user,
                details = null,
                Status = "NEW", 
                IsPaid = false,

            };
            await _ctx.Order.AddAsync(newOrder);
            newOrder.details = orderDetails;
            await _ctx.SaveChangesAsync();

            return await _ctx.Order.Where(x => x.OrderDate==newOrder.OrderDate).FirstAsync(x => x.user == newOrder.user);
        }
        public async Task AddItems(int orderid, List<OrderItems> items, float total)
        {
            var details = await GetOrderDetails(orderid);
            details.Total = total;
          details.orderItems = items;
            await _ctx.SaveChangesAsync();
        }
        public async Task AddInfo(int orderid, string adrress, string phone, string name, string paid, string typeofdelivery)
        {
            var details = await GetOrderDetails(orderid);
            OrderAddress ad = new OrderAddress
            {
                Address = adrress,
                Phone = phone,
                Name = name,
            };
            await _ctx.OrderAddress.AddAsync(ad);
            await _ctx.SaveChangesAsync();
            details.Address = ad;
            details.Payment = paid;
            details.TypeOfDelivery = typeofdelivery;
            await _ctx.SaveChangesAsync();
        }

        public async Task<OrderDetails> CreateOrderDetails()
        {
                var newOrderDetails = new OrderDetails
                {
                    orderItems = null,
                    Address = null,
                    Payment = "nnkjn",
                    Total = null,
                };
            await _ctx.OrderDetails.AddAsync(newOrderDetails);
            await _ctx.SaveChangesAsync();

            return await _ctx.OrderDetails.OrderBy(x=>x.Id).LastAsync(x => x.Address == newOrderDetails.Address);
        }
        public async Task<Order> GetOrderUser(User us)
        {
            return await _ctx.Order.OrderBy(x=>x.OrderDate).LastAsync(x => x.user.Id == us.Id);
        }

        public async Task CreateOrderItems(List<ShopCartItem> cartItems, OrderDetails order)
        {
            foreach (var i in cartItems)
            {
                var newOrderItem = new OrderItems
                {
                    medicaments = await _medicamentsRepository.GetMedicament(i.ProductId),
                    Quantity = i.Quantity,
                    Price = i.Price,
                    OrderDetailsId = order.Id
                };
                await _ctx.OrderItems.AddAsync(newOrderItem);
            }
            await _ctx.SaveChangesAsync();
        }

        public async Task UpdateAsync(string modelId, string status, bool paid)
        {
            var or = await GetOrder(modelId);

            if (or.Status != status)
                or.Status = status;
            if(or.IsPaid != paid)
                or.IsPaid = paid;

            await _ctx.SaveChangesAsync();
        }
    }
}
