using System.Collections.Generic;

namespace EcoXShopBitirme.Models
{
	public class ShoppingCartVM
	{
		public IEnumerable<ShoppingCart> ListCart { get; set; }
		public OrderHeader OrderHeader { get; set; }
	}
}
