/* Copyright (C) 2012 cloudbase.io
 
 This program is free software; you can redistribute it and/or modify it under
 the terms of the GNU General Public License, version 2, as published by
 the Free Software Foundation.
 
 This program is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
 or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License
 for more details.
 
 You should have received a copy of the GNU General Public License
 along with this program; see the file COPYING.  If not, write to the Free
 Software Foundation, 59 Temple Place - Suite 330, Boston, MA
 02111-1307, USA.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloudbase
{
    /// <summary>
    /// This is the bill object for the PayPal digital goods payments APIs. A bill object must contain
    /// at least one BillItem.
    ///
    /// The description of the bill should also contain the total amount as PayPal does not always display
    /// the amount in the checkout page.
    /// </summary>
    public class CBPayPalBill
    {
        /// <summary>
        /// a name for the purchase
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        ///  a description of the bill item.
	    /// this should also contain the price for the bill as PayPal will not always display the amount field.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// this is a user generated unique identifier for the transaction.
        /// </summary>
        public string InvoiceNumber { get; set; }
        /// <summary>
        /// The 3 letter ISO code for the transaction currency. If not specified this will automatically
	    /// be USD
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// This is the code of a CloudFunction to be executed once the payment is completed
        /// </summary>
        public string PaymentCompletedFunction { get; set; }
        /// <summary>
        /// This is the name of a CloudFunction to be executed if the payment is cancelled
        /// </summary>
        public string PaymentCancelledFunction { get; set; }
        /// <summary>
        /// By default the express checkout process will return to the cloudbase APIs. if you want to override 
	    /// this behaviour and return to a page you own once the payment is completed set this property to the url
        /// </summary>
        public string PaymentCompletedUrl { get; set; }
        /// <summary>
        /// By default the express checkout process will return to the cloudbase APIs. if you want to override
	    /// this behaviour and return to a page you own once the payment has been cancelled set this property to the url
        /// </summary>
        public string PaymentCancelledUrl { get; set; }

        private List<CBPayPalBillItem> _items;
        /// <summary>
        /// this is a list of BillItems. Each CBPayPalBill must have at least one BillItem
        /// </summary>
        public List<CBPayPalBillItem> Items
        {
            get { return _items; }
        }

        /// <summary>
        /// Add a new CBPayPalBillItem to the current bill
        /// </summary>
        /// <param name="newItem">The item to be added</param>
        public void AddNewItem(CBPayPalBillItem newItem)
        {
            if (this.Items == null)
                this._items = new List<CBPayPalBillItem>();

            this._items.Add(newItem);
        }

        /// <summary>
        /// This method is used internally to generate the NSMutableDictionary to be serialised
	    /// for the calls to the cloudbase.io APIs
        /// </summary>
        /// <returns>The Dictionary representation of the Bill object</returns>
        public Dictionary<string, object> serializePurchase()
        {
            if (this.Items == null || this.Items.Count == 0)
                return null;

            double totalPrice = 0.0;
            List<Dictionary<string, string>> items = new List<Dictionary<string, string>>();
            foreach (CBPayPalBillItem curItem in this.Items)
            {
                Dictionary<string, string> newItem = new Dictionary<string, string>();
                newItem.Add("item_name", curItem.Name);
                newItem.Add("item_description", curItem.Description);
                newItem.Add("item_amount", Convert.ToString(curItem.Amount));
                newItem.Add("item_tax", Convert.ToString(curItem.Tax));
                newItem.Add("item_quantity", Convert.ToString(curItem.Quantity));

                totalPrice += curItem.Amount + (curItem.Tax <= 0 ? 0.0 : curItem.Tax);

                items.Add(newItem);
            }

            Dictionary<string, object> purchase = new Dictionary<string, object>();
            purchase.Add("name", this.Name);
            purchase.Add("description", this.Description);
            purchase.Add("amount", Convert.ToString(totalPrice));
            purchase.Add("invoice_number", this.InvoiceNumber);
            purchase.Add("items", items);

            return purchase;
        }
    }

    /// <summary>
    ///  this object represents a single item within a CBPayPalBill object.
    /// </summary>
    public class CBPayPalBillItem
    {
        /// <summary>
        ///  The name of the item for the transaction
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// An extended description of the item. This should also contain the amount as
	    /// PayPal does not always display it.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// The amount of the transaction
        /// </summary>
        public double Amount { get; set; }
        /// <summary>
        /// additional taxes to be added to the amount
        /// </summary>
        public double Tax { get; set; }
        /// <summary>
        /// a quantity representing the number of items involved in the transaction.
	    /// for example 100 poker chips
        /// </summary>
        public int Quantity { get; set; }
    }
}
