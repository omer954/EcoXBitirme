﻿using System.Collections.Generic;

namespace EcoXShopBitirme.Models
{
	public class OrderDetailsVM
	{

		public OrderHeader OrderHeader { get; set; }
		public IEnumerable<OrderDetails> OrderDetails { get; set; }
	}
}
