﻿using FedexShipping.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FedexShipping.Services
{
    public interface IPackingService
    {
        Task<PackingResponseWrapper> packingMap(List<Item> items);
    }
}